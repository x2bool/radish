using System;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Rbmk.Radish.Model.WorkspacePad.Workspaces;
using Rbmk.Radish.Model.WorkspacePad.Workspaces.Commander;
using Rbmk.Radish.Services.Broadcasts.Tabs;
using Rbmk.Utils.Broadcasts;
using ReactiveUI;
using Splat;
using Rbmk.Utils.Reactive;

namespace Rbmk.Radish.Model.WorkspacePad
{
    public class WorkspacePadController
    {
        private readonly IBroadcastService _broadcastService;
        private readonly WorkspacePadModel _model;

        public WorkspacePadController(
            WorkspacePadModel model)
            : this(
                Locator.Current.GetService<IBroadcastService>())
        {
            _model = model;
        }

        public WorkspacePadController(
            IBroadcastService broadcastService)
        {
            _broadcastService = broadcastService;
        }

        public IDisposable BindOpenCommand()
        {
            _model.OpenCommand = ReactiveCommand.Create(
                () => Unit.Default, null, RxApp.MainThreadScheduler);

            return _model.OpenCommand
                .SubscribeWithLog(_ =>
                {
                    OpenTabIntent intent = null;
                    
                    if (_model.SelectedWorkspace is CommanderWorkspaceModel workspace)
                    {
                        // if server selected as a target pass it to new tab
                        var server = workspace.CommandEditor?.SelectedServerInfo;
                        if (server != null)
                        {
                            intent = new OpenTabIntent(server, "");
                        }
                        
                        // if database selected as a target pass it to new tab
                        var database = workspace.CommandEditor?.SelectedDatabaseInfo;
                        if (database != null)
                        {
                            intent = new OpenTabIntent(database, "");
                        }
                    }
                    
                    _broadcastService.Broadcast(intent ?? new OpenTabIntent());
                });
        }

        public IDisposable BindCloseCommand()
        {
            var hasMultipleTabs = Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                    h => _model.Workspaces.CollectionChanged += h,
                    h => _model.Workspaces.CollectionChanged -= h)
                .Select(e => _model.Workspaces.Count > 1);
            
            _model.CloseCommand = ReactiveCommand.Create<WorkspaceModel, WorkspaceModel>(
                workspace => workspace, hasMultipleTabs, RxApp.MainThreadScheduler);

            return _model.CloseCommand
                .SubscribeWithLog(workspace =>
                {
                    int removeIndex = _model.Workspaces.IndexOf(workspace);
                    _broadcastService.Broadcast(new CloseTabIntent(removeIndex));
                });
        }

        public IDisposable BindWorkspaceTitle()
        {
            return _model.WhenAnyValue(x => x.SelectedWorkspace.Tab.Title)
                .SubscribeWithLog(title =>
                {
                    _model.Title = title;
                });
        }

        public IDisposable BindTabTitle()
        {
            return _model.WhenAnyValue(m => m.SelectedWorkspace)
                .OfType<CommanderWorkspaceModel>()
                .Where(w => w != null)
                .SelectMany(w => w.WhenAnyValue(x => x.CommandEditor.CommandText))
                .Select(text =>
                    (text != null)
                        ? text.Trim()
                            .Replace("\r\n", " ")
                            .Replace("\n", " ")
                        : "")
                .SubscribeWithLog(text =>
                {
                    string title;
                    if (string.IsNullOrEmpty(text))
                    {
                        title = "…";
                    }
                    else if (text.Length <= 16)
                    {
                        title = text;
                    }
                    else
                    {
                        title = text.Substring(0, 16) + "…";
                    }

                    var selectedWorkspace = _model.SelectedWorkspace;
                    if (selectedWorkspace != null)
                    {
                        _model.Title = title;
                        selectedWorkspace.Tab.Title = title;
                    }
                });
        }

        public IDisposable BindOpenIntents()
        {
            return _broadcastService.Listen<OpenTabIntent>()
                .SubscribeWithLog(intent =>
                {
                    var workspace = new CommanderWorkspaceModel();
                    workspace.Tab.CloseCommand = _model.CloseCommand;

                    if (intent.DatabaseInfo != null)
                    {
                        workspace.CommandEditor.SelectedDatabaseInfo = intent.DatabaseInfo;
                        workspace.CommandEditor.SelectedConnectionInfo = intent.DatabaseInfo.ConnectionInfo;
                    }

                    if (intent.ServerInfo != null)
                    {
                        workspace.CommandEditor.SelectedServerInfo = intent.ServerInfo;
                        workspace.CommandEditor.SelectedConnectionInfo = intent.ServerInfo.ConnectionInfo;
                    }

                    if (intent.CommandText != null)
                    {
                        workspace.CommandEditor.CommandText = intent.CommandText;
                    }

                    if (intent.Index != null)
                    {
                        _model.Workspaces.Insert(intent.Index.Value, workspace);
                    }
                    else
                    {
                        _model.Workspaces.Add(workspace);
                    }
                    
                    _model.SelectedWorkspace = workspace;
                });
        }

        public IDisposable BindCloseIntents()
        {
            return _broadcastService.Listen<CloseTabIntent>()
                .SubscribeWithLog(intent =>
                {
                    _model.Workspaces.RemoveAt(intent.Index);
                });
        }

        public IDisposable OpenDefaultWorkspace()
        {
            return _model.OpenCommand.Execute()
                .SubscribeWithLog();
        }

        public IDisposable BindScrolling()
        {
            _model.ScrollLeftCommand =
                ReactiveCommand.Create(() => { }, null, RxApp.MainThreadScheduler);
            _model.ScrollRightCommand =
                ReactiveCommand.Create(() => { }, null, RxApp.MainThreadScheduler);
            
            return new CompositeDisposable(
                _model.ScrollLeftCommand.SubscribeWithLog(),
                _model.ScrollRightCommand.SubscribeWithLog());
        }
    }
}