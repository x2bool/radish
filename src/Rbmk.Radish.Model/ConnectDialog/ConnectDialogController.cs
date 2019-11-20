using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Rbmk.Radish.Services.Redis.Connections;
using Rbmk.Utils.Reactive;
using ReactiveUI;
using Splat;

namespace Rbmk.Radish.Model.ConnectDialog
{
    public class ConnectDialogController
    {
        private readonly ConnectDialogModel _model;
        
        private readonly IConnectionProvider _connectionProvider;
        private readonly IConnectionStorage _connectionStorage;
        private readonly IClientAccessor _clientAccessor;

        public ConnectDialogController(
            IConnectionProvider connectionProvider,
            IConnectionStorage connectionStorage,
            IClientAccessor clientAccessor)
        {
            _connectionProvider = connectionProvider;
            _connectionStorage = connectionStorage;
            _clientAccessor = clientAccessor;
        }

        public ConnectDialogController(
            ConnectDialogModel model)
            : this (
                Locator.Current.GetService<IConnectionProvider>(),
                Locator.Current.GetService<IConnectionStorage>(),
                Locator.Current.GetService<IClientAccessor>())
        {
            _model = model;
        }

        public IDisposable BindConnectionName()
        {
            if (_model.ConnectionInfo != null)
            {
                _model.ConnectionName = _model.ConnectionInfo.Name;
            }
            else
            {
                return GetNextSerialNumber()
                    .SubscribeWithLog(n =>
                    {
                        _model.ConnectionName = $"Connection {n}";
                    });
            }
            
            return Disposable.Empty;
        }

        public IDisposable BindConnectionString()
        {
            if (_model.ConnectionInfo != null)
            {
                _model.ConnectionString = _model.ConnectionInfo.ConnectionString;
            }
            else
            {
                _model.ConnectionString = "localhost:6379";
            }

            return Disposable.Empty;
        }
        
        public IDisposable BindConnectCommand()
        {
            _model.ConnectCommand = ReactiveCommand.CreateFromObservable(
                Connect,
                null,
                RxApp.MainThreadScheduler);

            var progressSubscription = _model.ConnectCommand.IsExecuting
                .SubscribeWithLog(inProgress =>
                {
                    _model.InProgress = inProgress;
                });
            
            var resultSubscription = _model.ConnectCommand
                .SelectMany(result =>
                {
                    switch (result)
                    {
                        case ConnectResult.Created created:
                            return Observable.Return(created.ConnectionInfo)
                                .SelectMany(connectionInfo => connectionInfo.ServerInfos)
                                .SelectMany(serverInfo => _clientAccessor.Check(serverInfo))
                                .Where(client => client != null && client.IsConnected)
                                .ToList()
                                .Select(list =>
                                {
                                    if (list.Count > 0)
                                    {
                                        return result;
                                    }

                                    return null;
                                });
                        
                        default:
                            return Observable.Return(result);
                    }
                })
                .SubscribeWithLog(result =>
                {
                    switch (result)
                    {
                        case ConnectResult.Created created:
                            _model.ConnectionInfo = created.ConnectionInfo;
                            _model.HasError = false;
                        
                            _model.Close(created);
                            break;
                        
                        default:
                            _model.ConnectionInfo = null;
                            _model.HasError = true;
                            break;
                    }
                });
            
            return new CompositeDisposable(
                progressSubscription,
                resultSubscription);
        }

        public IDisposable BindCancelCommand()
        {
            // TODO: cancel while still in progress of connecting
            var notInProgress = _model.ConnectCommand.IsExecuting
                .Select(value => !value);
            
            _model.CancelCommand = ReactiveCommand.Create(
                () => { }, notInProgress, RxApp.MainThreadScheduler);

            return _model.CancelCommand
                .SubscribeWithLog(_ =>
                {
                    _model.Close(new ConnectResult.Cancelled());
                });
        }

        private IObservable<ConnectResult> Connect()
        {
            if (_model.ConnectionInfo != null)
            {
                return _connectionProvider.Reconnect(
                        _model.ConnectionInfo, _model.ConnectionName, _model.ConnectionString)
                    .Select(connectionInfo => new ConnectResult.Created(connectionInfo))
                    .Catch<ConnectResult, Exception>(e => Observable.Return(new ConnectResult.Failed(e)));
            }
            
            return _connectionProvider.Connect(_model.ConnectionName, _model.ConnectionString)
                .Select(c => new ConnectResult.Created(c))
                .Catch<ConnectResult, Exception>(e => Observable.Return(new ConnectResult.Failed(e)));
        }
        
        private IObservable<int> GetNextSerialNumber()
        {
            return _connectionStorage.GetAllAsync()
                .ToObservable()
                .Select(list =>
                {
                    if (list.Count > 0)
                    {
                        return list.Max(c => c.Order) + 1;
                    }

                    return 1;
                });
        }
    }
}