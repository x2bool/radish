using System;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using CSRedis;
using Rbmk.Radish.Model.CommandEditor.Targets;
using Rbmk.Radish.Model.ConnectDialog;
using Rbmk.Radish.Model.Dialogs;
using Rbmk.Radish.Model.SelectTargetDialog;
using Rbmk.Radish.Services.Broadcasts.Connections;
using Rbmk.Radish.Services.Broadcasts.Results;
using Rbmk.Radish.Services.Broadcasts.Structs;
using Rbmk.Radish.Services.Broadcasts.Targets;
using Rbmk.Radish.Services.Redis;
using Rbmk.Radish.Services.Redis.Connections;
using Rbmk.Radish.Services.Redis.Executor;
using Rbmk.Radish.Services.Redis.Parser;
using Rbmk.Utils.Broadcasts;
using Rbmk.Utils.Reactive;
using ReactiveUI;
using Splat;
using RedisServerInfo = Rbmk.Radish.Services.Redis.RedisServerInfo;

namespace Rbmk.Radish.Model.CommandEditor
{
    public class CommandEditorController
    {
        private readonly IBroadcastService _broadcastService;
        private readonly IDialogManager _dialogManager;
        private readonly IConnectionProvider _connectionProvider;
        private readonly ICommandParser _commandParser;
        private readonly ICommandExecutor _commandExecutor;
        private readonly IClientAccessor _clientAccessor;

        private readonly CommandEditorModel _model;

        public CommandEditorController(
            IBroadcastService broadcastService,
            IDialogManager dialogManager,
            IConnectionProvider connectionProvider,
            ICommandParser commandParser,
            ICommandExecutor commandExecutor,
            IClientAccessor clientAccessor)
        {
            _broadcastService = broadcastService;
            _dialogManager = dialogManager;
            _connectionProvider = connectionProvider;
            _commandParser = commandParser;
            _commandExecutor = commandExecutor;
            _clientAccessor = clientAccessor;
        }

        public CommandEditorController(
            CommandEditorModel model)
            : this(
                Locator.Current.GetService<IBroadcastService>(),
                Locator.Current.GetService<IDialogManager>(),
                Locator.Current.GetService<IConnectionProvider>(),
                Locator.Current.GetService<ICommandParser>(),
                Locator.Current.GetService<ICommandExecutor>(),
                Locator.Current.GetService<IClientAccessor>())
        {
            _model = model;
        }
        
        public IDisposable BindTargetSelection()
        {
            _model.SelectTargetCommand = ReactiveCommand.CreateFromObservable(
                SelectTarget, null, RxApp.MainThreadScheduler);
            
            return _model.SelectTargetCommand
                .SelectSeq(result =>
                {
                    switch (result)
                    {
                        case SelectTargetResult.Selected selected:
                            return Observable.Return(selected);
                        
                        case SelectTargetResult.NoConnectionsAvailable _:
                            return Connect()
                                .SelectSeq(connectResult =>
                                {
                                    if (connectResult is ConnectResult.Created)
                                    {
                                        return SelectTarget();
                                    }
                                    
                                    return Observable.Return<SelectTargetResult>(null);
                                });
                    }

                    return Observable.Return<SelectTargetResult>(null);
                })
                .ObserveOn(RxApp.MainThreadScheduler)
                .SubscribeWithLog(result =>
                {
                    if (result is SelectTargetResult.Selected selected)
                    {
                        _model.SelectedConnectionInfo = selected.ConnectionInfo;
                        _model.SelectedServerInfo = selected.ServerInfo;
                        _model.SelectedDatabaseInfo = selected.DatabaseInfo;
                    }

                    if (_model.IsQuickAccessEnabled)
                    {
                        if (_model.SelectedServerInfo == null && _model.SelectedDatabaseInfo == null)
                        {
                            _model.IsQuickAccessEnabled = false;
                        }
                    }
                });
        }

        public IDisposable BindSelectedConnection()
        {
            return _model.WhenAnyValue(m => m.SelectedConnectionInfo)
                .SubscribeWithLog(connection =>
                {
                    _model.SelectedConnectionTarget = CreateConnectionTarget(connection);
                });
        }

        public IDisposable BindSelectedServer()
        {
            return _model.WhenAnyValue(m => m.SelectedServerInfo)
                .SubscribeWithLog(server =>
                {
                    _model.SelectedServerTarget = CreateServerTarget(server);
                });
        }

        public IDisposable BindSelectedDatabase()
        {
            return _model.WhenAnyValue(m => m.SelectedDatabaseInfo)
                .SubscribeWithLog(database =>
                {
                    _model.SelectedDatabaseTarget = CreateDatabaseTarget(database);
                });
        }

        public IDisposable BindFlags()
        {
            return _model.WhenAnyValue(m => m.SelectedConnectionTarget)
                .Merge<object>(_model.WhenAnyValue(m => m.SelectedDatabaseTarget))
                .Merge<object>(_model.WhenAnyValue(m => m.SelectedServerTarget))
                .SubscribeWithLog(_ =>
                {
                    _model.IsConnectionTargetSelected = _model.SelectedConnectionTarget != null;
                    _model.IsServerTargetSelected = _model.SelectedServerTarget != null;
                    _model.IsDatabaseTargetSelected = _model.SelectedDatabaseTarget != null;
                });
        }

        public IDisposable BindExecuteCommand()
        {
            var targetObservable = _model.WhenAnyValue(m => m.IsServerTargetSelected)
                .Merge(_model.WhenAnyValue(m => m.IsDatabaseTargetSelected));

            var commandObservable = _model.WhenAnyValue(m => m.CommandText)
                .Select(string.IsNullOrWhiteSpace);

            var executableObservable = targetObservable.Merge(commandObservable)
                .SelectSeq(_ =>
                {
                    if (_model.IsServerTargetSelected || _model.IsDatabaseTargetSelected)
                    {
                        return _commandParser.Parse(_model.CommandText)
                            .Select(batchInfo => batchInfo.CommandInfos.Length > 0 && batchInfo.IsValid);
                    }

                    return Observable.Return(false);
                })
                .ObserveOn(RxApp.MainThreadScheduler);
            
            _model.ExecuteCommand =
                ReactiveCommand.CreateFromObservable<string, RedisResultInfo[]>(
                    Execute, executableObservable, RxApp.MainThreadScheduler);

            return _model.ExecuteCommand
                .SubscribeWithLog(results =>
                {
                    _broadcastService.Broadcast(new ViewResultsIntent(results));
                    _broadcastService.Broadcast(new ViewStructIntent(null));
                });
        }

        public IDisposable BindDisconnects()
        {
            var resetObservable = _broadcastService.Listen<ConnectionBroadcast>(
                    b => b.Kind == ConnectionBroadcastKind.Close)
                .Select(_ => Unit.Default)
                .Merge(_broadcastService.Listen<ConnectionBroadcast>(
                        b => b.Kind == ConnectionBroadcastKind.Disconnect)
                    .Select(_ => Unit.Default));
            
            return resetObservable
                .SelectMany(_ => _connectionProvider.GetConnections().ToList())
                .ObserveOn(RxApp.MainThreadScheduler)
                .SubscribeWithLog(connections =>
                {
                    var connection = _model.SelectedConnectionTarget?.ConnectionInfo;
                    
                    if (connection != null && connections.All(c => c.Id != connection.Id))
                    {
                        _model.SelectedConnectionTarget = null;
                        _model.SelectedServerTarget = null;
                        _model.SelectedDatabaseTarget = null;
                    }
                });
        }

        public IDisposable BindTargetActivation()
        {
            return _broadcastService.Listen<ActivateTargetIntent>()
                .SubscribeWithLog(intent =>
                {
                    if (intent.ServerInfo != null)
                    {
                        _model.SelectedConnectionInfo = intent.ServerInfo.ConnectionInfo;
                        _model.SelectedServerInfo = intent.ServerInfo;
                        _model.SelectedDatabaseInfo = null;
                    }

                    if (intent.DatabaseInfo != null)
                    {
                        _model.SelectedConnectionInfo = intent.DatabaseInfo.ConnectionInfo;
                        _model.SelectedDatabaseInfo = intent.DatabaseInfo;
                        _model.SelectedServerInfo = null;
                    }
                });
        }

        public IDisposable BindQuickAccess()
        {
            return _model.WhenAnyValue(m => m.IsQuickAccessEnabled)
                .SubscribeWithLog(isEnabled =>
                {
                    if (isEnabled)
                    {
                        if (!_model.IsDatabaseTargetSelected && !_model.IsServerTargetSelected)
                        {
                            _model.SelectTargetCommand.Execute()
                                .SubscribeWithLog();
                        }
                    }
                });
        }

        public IDisposable BindQuickAccessItems()
        {
            return _model.WhenAnyValue(m => m.QuickAccessText)
                .Synchronize()
                .SelectMany(text => GetQuickAccessItems(text)
                    .Select(items => new { Items = items, Text = text }))
                .ObserveOn(RxApp.MainThreadScheduler)
                .SubscribeWithLog(item =>
                {
                    // TODO: hack - never delete items or AutoCompleteBox goes crazy, instead replace them with empty string
                    for (int i = 0; i < _model.QuickAccessItems.Count; i++)
                    {
                        var oldQuickAccessItem = _model.QuickAccessItems[i];
                        if (oldQuickAccessItem != item.Text)
                        {
                            _model.QuickAccessItems[i] = "";
                        }
                    }

                    for (int i = 0; i < item.Items.Length; i++)
                    {
                        var quickAccessItem = item.Items[i];
                        if (quickAccessItem != item.Text)
                        {
                            _model.QuickAccessItems.Add(quickAccessItem);
                        }
                    }
                });
        }

        public IDisposable BindSelectedQuickAccessItem()
        {
            return _model.WhenAnyValue(m => m.SelectedQuickAccessItem)
                .SubscribeWithLog(quickAccessItem =>
                {
                    if (!string.IsNullOrWhiteSpace(quickAccessItem))
                    {
                        _model.ExploreCommand.Execute(quickAccessItem)
                            .SubscribeWithLog();
                    }
                });
        }

        public IDisposable BindExploreCommand()
        {
            var isNotEmpty = _model.WhenAnyValue(m => m.QuickAccessText)
                .Select(text => !string.IsNullOrWhiteSpace(text));
            
            _model.ExploreCommand = ReactiveCommand.CreateFromObservable<string, RedisResultInfo[]>(
                key => Explore(key ?? _model.QuickAccessText), isNotEmpty, RxApp.MainThreadScheduler);

            return _model.ExploreCommand
                .SubscribeWithLog(results =>
                {
                    _broadcastService.Broadcast(new ViewResultsIntent(results));
                    _broadcastService.Broadcast(new ViewStructIntent(null));
                });
        }
        
        private IObservable<ConnectResult> Connect()
        {
            return _dialogManager.Open(new ConnectDialogModel());
        }

        private IObservable<SelectTargetResult> SelectTarget()
        {
            return _dialogManager.Open(new SelectTargetDialogModel
                {
                    InitialConnectionInfo = _model.SelectedConnectionTarget?.ConnectionInfo,
                    InitialServerInfo = _model.SelectedServerTarget?.ServerInfo,
                    InitialDatabaseInfo = _model.SelectedDatabaseTarget?.DatabaseInfo
                });
        }

        private DatabaseTargetModel CreateDatabaseTarget(RedisDatabaseInfo databaseInfo)
        {
            if (databaseInfo == null)
                return null;
            
            return new DatabaseTargetModel
            {
                Number = $"database {databaseInfo.Number}",
                DatabaseInfo = databaseInfo
            };
        }

        private ServerTargetModel CreateServerTarget(RedisServerInfo serverInfo)
        {
            if (serverInfo == null)
                return null;
            
            string address;
            if (serverInfo.EndPoint is DnsEndPoint dns)
            {
                address = $"{dns.Host}:{dns.Port}";
            }
            else
            {
                address = serverInfo.EndPoint.ToString();
            }
            
            return new ServerTargetModel
            {
                Address = address,
                ServerInfo = serverInfo
            };
        }

        private ConnectionTargetModel CreateConnectionTarget(RedisConnectionInfo connectionInfo)
        {
            if (connectionInfo == null)
                return null;
            
            var connectionName = string.IsNullOrWhiteSpace(connectionInfo.Name)
                ? connectionInfo.ConnectionString
                : connectionInfo.Name;

            var connectionString = string.IsNullOrWhiteSpace(connectionInfo.Name)
                ? null
                : $"({connectionInfo.ConnectionString})";
            
            return new ConnectionTargetModel
            {
                ConnectionName = connectionName,
                ConnectionString = connectionString,
                ConnectionInfo = connectionInfo
            };
        }

        private IObservable<RedisResultInfo[]> Execute(string commandText)
        {   
            if (_model.IsDatabaseTargetSelected)
            {
                return _commandParser.Parse(commandText)
                    .SelectSeq(batch => _commandExecutor.Execute(
                        _model.SelectedDatabaseTarget.DatabaseInfo, batch))
                    .ToArray();
            }

            if (_model.IsServerTargetSelected)
            {
                return _commandParser.Parse(commandText)
                    .SelectSeq(batch => _commandExecutor.Execute(
                        _model.SelectedServerTarget.ServerInfo, batch))
                    .ToArray();
            }

            return Observable.Throw<RedisResultInfo[]>(
                new Exception("No target is selected for command execution"));
        }
        
        private IObservable<RedisResultInfo[]> Explore(string key)
        {
            var commandText = $"KEYS \"{key.Replace("\"", "\\\"")}\"";
            _model.CommandText = commandText;
            
            return Execute(commandText);
        }

        private IObservable<string[]> GetQuickAccessItems(
            string text)
        {
            if (!string.IsNullOrWhiteSpace(text) && !text.Contains("*"))
            {
                text = text.Trim() + "*";
            }
            
            RedisTargetInfo targetInfo = null;
            
            if (_model.IsDatabaseTargetSelected)
            {
                targetInfo = _model.SelectedDatabaseTarget.DatabaseInfo;
            }

            if (_model.IsServerTargetSelected)
            {
                targetInfo = _model.SelectedServerTarget.ServerInfo;
            }

            if (!string.IsNullOrWhiteSpace(text) && targetInfo != null)
            {
                return _clientAccessor.With(targetInfo, client =>
                {
                    var scan = client.Scan(0, text);
                    if (scan != null)
                    {
                        var items = scan.Items
                            .ToArray();

                        return Observable.Return(items);
                    }
                    
                    return Observable.Return(new string[0]);
                });
            }

            return Observable.Return(new string[0]);
        }
    }
}