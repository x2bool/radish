using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using CSRedis;
using DynamicData;
using Rbmk.Radish.Model.ConfirmDialog;
using Rbmk.Radish.Model.ConnectDialog;
using Rbmk.Radish.Model.Dialogs;
using Rbmk.Radish.Services.Redis;
using Rbmk.Radish.Services.Redis.Connections;
using ReactiveUI;
using Splat;
using Rbmk.Utils.Reactive;

namespace Rbmk.Radish.Model.ExplorerPad.ConnectionExplorer.Nodes
{
    public class ConnectionNodeController
    {
        private readonly ConnectionNodeModel _model;
        
        private readonly IDialogManager _dialogManager;
        private readonly IConnectionProvider _connectionProvider;
        private readonly IClientAccessor _clientAccessor;

        public ConnectionNodeController(
            IDialogManager dialogManager,
            IConnectionProvider connectionProvider,
            IClientAccessor clientAccessor)
        {
            _dialogManager = dialogManager;
            _connectionProvider = connectionProvider;
            _clientAccessor = clientAccessor;
        }

        public ConnectionNodeController(
            ConnectionNodeModel model)
            : this(
                Locator.Current.GetService<IDialogManager>(),
                Locator.Current.GetService<IConnectionProvider>(),
                Locator.Current.GetService<IClientAccessor>())
        {
            _model = model;
        }

        public IDisposable BindDisconnectCommand()
        {
            _model.DisconnectCommand = ReactiveCommand.CreateFromObservable(
                Disconnect, null, RxApp.MainThreadScheduler);

            return _model.DisconnectCommand
                .SubscribeWithLog();
        }

        public IDisposable BindCloseCommand()
        {
            _model.CloseCommand = ReactiveCommand.CreateFromObservable(
                Close, null, RxApp.MainThreadScheduler);
            
            return _model.CloseCommand
                .SubscribeWithLog();
        }

        public IDisposable BindReconnectCommand()
        {
            _model.ReconnectCommand = ReactiveCommand.CreateFromObservable(
                Reconnect, null, RxApp.MainThreadScheduler);

            return _model.ReconnectCommand
                .SubscribeWithLog();
        }

        private IObservable<RedisConnectionInfo> Disconnect()
        {
            return _connectionProvider
                .Disconnect(_model.ConnectionInfo);
        }

        private IObservable<RedisConnectionInfo> Close()
        {
            return _dialogManager.Open(new ConfirmDialogModel
                {
                    TitleText = "Delete connection?",
                    MessageText = "Do you want to delete selected connection?",
                    ConfirmText = "Delete",
                    CancelText = "Cancel"
                })
                .SelectMany(confirmed =>
                {
                    if (!confirmed)
                    {
                        return Observable.Return<RedisConnectionInfo>(null);
                    }
                    
                    return _connectionProvider.Close(_model.ConnectionInfo);
                });
        }

        private IObservable<RedisConnectionInfo> Reconnect()
        {
            return _dialogManager.Open(new ConnectDialogModel(_model.ConnectionInfo))
                .Select(result =>
                {
                    if (result is ConnectResult.Created created)
                    {
                        return created.ConnectionInfo;
                    }

                    return null;
                });
        }
    }
}