using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using Rbmk.Radish.Model.ConnectDialog;
using Rbmk.Radish.Model.Dialogs;
using Rbmk.Radish.Model.ExplorerPad.ConnectionExplorer.Nodes;
using Rbmk.Radish.Services.Broadcasts.Connections;
using Rbmk.Radish.Services.Redis.Connections;
using ReactiveUI;
using Rbmk.Utils.Broadcasts;
using Splat;
using Rbmk.Utils.Reactive;

namespace Rbmk.Radish.Model.ExplorerPad.ConnectionExplorer
{
    public class ConnectionExplorerController
    {
        private readonly ConnectionExplorerModel _model;
        private readonly IConnectionNodeBuilder _connectionNodeBuilder;
        
        private readonly IBroadcastService _broadcastService;
        private readonly IConnectionProvider _connectionProvider;
        private readonly IDialogManager _dialogManager;

        public ConnectionExplorerController(
            IBroadcastService broadcastService,
            IConnectionProvider connectionProvider,
            IDialogManager dialogManager)
        {
            _broadcastService = broadcastService;
            _connectionProvider = connectionProvider;
            _dialogManager = dialogManager;
        }

        public ConnectionExplorerController(
            ConnectionExplorerModel model)
            : this(
                Locator.Current.GetService<IBroadcastService>(),
                Locator.Current.GetService<IConnectionProvider>(),
                Locator.Current.GetService<IDialogManager>())
        {
            _model = model;
            _connectionNodeBuilder = new ConnectionNodeBuilder(model.Nodes);
        }

        public IDisposable BindItems()
        {   
            return _model.Nodes.Connect()
                .Bind(_model.Items)
                .SubscribeWithLog();
        }

        public IDisposable BindSelections()
        {
            return _model
                .WhenAnyValue(x => x.SelectedItem)
                .OfType<NewConnectionNodeModel>()
                .ObserveOn(RxApp.MainThreadScheduler)
                .SubscribeWithLog(result =>
                {
                    _model.SelectedItem = null;
                });
        }

        public IDisposable BindConnectBroadcasts()
        {
            return _broadcastService.Listen<ConnectionBroadcast>(
                    b => b.Kind == ConnectionBroadcastKind.Connect)
                .SelectMany(b => _connectionProvider.GetConnection(b.Id))
                .ObserveOn(RxApp.MainThreadScheduler)
                .SubscribeWithLog(connectionInfo =>
                {
                    // add node
                    _connectionNodeBuilder.HandleAction(
                        new ConnectionNodeAction.Add(connectionInfo.Id, connectionInfo));

                    // activate node
                    _model.SelectedItem = _model.Nodes.Items
                        .OfType<ConnectionNodeModel>()
                        .FirstOrDefault(node => node.ConnectionInfo == connectionInfo);
                });
        }

        public IDisposable BindCloseBroadcasts()
        {
            return _broadcastService.Listen<ConnectionBroadcast>(
                    b => b.Kind == ConnectionBroadcastKind.Close)
                .Select(b => b.Id)
                .ObserveOn(RxApp.MainThreadScheduler)
                .SubscribeWithLog(id =>
                {
                    _connectionNodeBuilder.HandleAction(
                        new ConnectionNodeAction.Delete(id));
                });
        }

        public IDisposable BindRestoreBroadcasts()
        {
            return _broadcastService.Listen<ConnectionBroadcast>(
                    b => b.Kind == ConnectionBroadcastKind.Restore)
                .SelectMany(b => _connectionProvider.GetConnection(b.Id))
                .ObserveOn(RxApp.MainThreadScheduler)
                .SubscribeWithLog(connectionInfo =>
                {
                    // add node
                    _connectionNodeBuilder.HandleAction(
                        new ConnectionNodeAction.Add(connectionInfo.Id, connectionInfo));
                });
        }

        public IDisposable BindReconnectBroadcasts()
        {
            return _broadcastService.Listen<ConnectionBroadcast>(
                    b => b.Kind == ConnectionBroadcastKind.Reconnect)
                .SelectMany(b => _connectionProvider.GetConnection(b.Id))
                .ObserveOn(RxApp.MainThreadScheduler)
                .SubscribeWithLog(connectionInfo =>
                {
                    _connectionNodeBuilder.HandleAction(
                        new ConnectionNodeAction.Update(connectionInfo.Id, connectionInfo));
                });
        }

        public IDisposable BindDisconnectBroadcasts()
        {
            return _broadcastService.Listen<ConnectionBroadcast>(
                    b => b.Kind == ConnectionBroadcastKind.Disconnect)
                .SelectMany(b => _connectionProvider.GetConnection(b.Id))
                .ObserveOn(RxApp.MainThreadScheduler)
                .SubscribeWithLog(connectionInfo =>
                {
                    _connectionNodeBuilder.HandleAction(
                        new ConnectionNodeAction.Update(connectionInfo.Id, connectionInfo));
                });
        }

        public IDisposable RestoreConnections()
        {
            var count = _model.Items
                .OfType<ConnectionNodeModel>()
                .Count();
            
            if (count == 0)
            {
                var restoreSubscription = _connectionProvider.Restore()
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .SubscribeWithLog();

                var clearSubscription = Disposable.Create(() =>
                {
                    var connections = _model.Items
                        .OfType<ConnectionNodeModel>()
                        .Select(m => m.ConnectionInfo)
                        .ToList();

                    foreach (var connection in connections)
                    {
                        _connectionNodeBuilder.HandleAction(
                            new ConnectionNodeAction.Delete(connection.Id));
                    }
                });
                
                return new CompositeDisposable(restoreSubscription, clearSubscription);
            }
            
            return Disposable.Empty;
        }
    }
}