using System;
using System.Reactive.Linq;
using Rbmk.Radish.Model.ConnectDialog;
using Rbmk.Radish.Model.Dialogs;
using Rbmk.Radish.Services.Redis;
using Rbmk.Radish.Services.Redis.Connections;
using Rbmk.Utils.Reactive;
using ReactiveUI;
using Splat;

namespace Rbmk.Radish.Model.ExplorerPad.ConnectionExplorer.Nodes
{
    public class NewConnectionNodeController
    {
        private readonly NewConnectionNodeModel _model;
        
        private readonly IDialogManager _dialogManager;
        private readonly IConnectionProvider _connectionProvider;

        public NewConnectionNodeController(
            IDialogManager dialogManager,
            IConnectionProvider connectionProvider)
        {
            _dialogManager = dialogManager;
            _connectionProvider = connectionProvider;
        }

        public NewConnectionNodeController(
            NewConnectionNodeModel model)
            : this(
                Locator.Current.GetService<IDialogManager>(),
                Locator.Current.GetService<IConnectionProvider>())
        {
            _model = model;
        }

        public IDisposable BindConnectCommand()
        {
            _model.ConnectCommand = ReactiveCommand.CreateFromObservable(
                Connect, null, RxApp.MainThreadScheduler);

            return _model.ConnectCommand
                .SubscribeWithLog();
        }
        
        private IObservable<RedisConnectionInfo> Connect()
        {
            return _dialogManager.Open(new ConnectDialogModel())
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