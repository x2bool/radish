using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Rbmk.Radish.Model.SelectTargetDialog.Items;
using Rbmk.Radish.Services.Redis;
using Rbmk.Radish.Services.Redis.Connections;
using Rbmk.Utils.Reactive;
using ReactiveUI;
using Splat;

namespace Rbmk.Radish.Model.SelectTargetDialog
{
    public class SelectTargetDialogController
    {
        private readonly IConnectionProvider _connectionProvider;
        private readonly SelectTargetDialogModel _model;

        public SelectTargetDialogController(
            SelectTargetDialogModel model)
            : this(
                Locator.Current.GetService<IConnectionProvider>())
        {
            _model = model;
        }

        public SelectTargetDialogController(
            IConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider;
        }

        public IDisposable BindConnections()
        {
            return _connectionProvider.GetConnections()
                .Where(connectionInfo => connectionInfo.ServerInfos.Count > 0)
                .ToList()
                .SubscribeWithLog(openConnections =>
                {
                    if (openConnections.Count > 0)
                    {
                        _model.Connections.Clear();
                        _model.Connections.AddRange(
                            openConnections.Select(CreateConnectionItem));

                        if (_model.InitialConnectionInfo != null)
                        {
                            _model.SelectedConnection = _model.Connections
                                .FirstOrDefault(c => c.ConnectionInfo == _model.InitialConnectionInfo);
                        }
                        else
                        {
                            _model.SelectedConnection = _model.Connections
                                .FirstOrDefault();
                        }
                    }
                    else
                    {
                        _model.Close(
                            new SelectTargetResult.NoConnectionsAvailable());
                    }
                });
        }

        public IDisposable BindServers()
        {
            var connectionSubscription = _model.WhenAnyValue(m => m.SelectedConnection)
                .Select(connection => connection?.ConnectionInfo?.ServerInfos ?? new List<RedisServerInfo>())
                .ObserveOn(RxApp.MainThreadScheduler)
                .SubscribeWithLog(list =>
                {
                    _model.Servers.Clear();
                    _model.Servers.AddRange(
                        list.Select(CreateServerItem));

                    if (_model.SelectedServer != null)
                    {
                        _model.SelectedServer = _model.Servers
                            .FirstOrDefault(s => s.ServerInfo == _model.InitialServerInfo);
                    }
                    else if (_model.InitialDatabaseInfo == null)
                    {
                        _model.SelectedServer = _model.Servers
                            .FirstOrDefault();
                    }
                });

            var databaseSubscription = _model.WhenAnyValue(m => m.SelectedServer)
                .SubscribeWithLog(server =>
                {
                    if (server != null)
                    {
                        _model.SelectedDatabase = null;
                    }
                });
            
            return new CompositeDisposable(
                connectionSubscription,
                databaseSubscription);
        }

        public IDisposable BindDatabases()
        {   
            var connectionSubscription = _model.WhenAnyValue(m => m.SelectedConnection)
                .Select(connection => connection?.ConnectionInfo?.DatabaseInfos ?? new List<RedisDatabaseInfo>())
                .SubscribeWithLog(list =>
                {
                    _model.Databases.Clear();
                    _model.Databases.AddRange(
                        list.Select(CreateDatabaseItem));

                    if (_model.InitialDatabaseInfo != null)
                    {
                        _model.SelectedDatabase = _model.Databases
                            .FirstOrDefault(d => d.DatabaseInfo == _model.InitialDatabaseInfo);
                    }
                    else if (_model.InitialServerInfo == null)
                    {
                        _model.SelectedDatabase = _model.Databases
                            .FirstOrDefault();
                    }
                });

            var databaseSubscription = _model.WhenAnyValue(m => m.SelectedDatabase)
                .SubscribeWithLog(database =>
                {
                    if (database != null)
                    {
                        _model.SelectedServer = null;
                    }
                });
            
            return new CompositeDisposable(
                connectionSubscription,
                databaseSubscription);
        }

        public IDisposable BindSelect()
        {
            var isTargetSelected = _model.WhenAnyValue(m => m.SelectedServer)
                .Merge<object>(_model.WhenAnyValue(m => m.SelectedDatabase))
                .Select(_ => _model.SelectedServer != null || _model.SelectedDatabase != null);
                
            _model.SelectCommand = ReactiveCommand.Create(
                () => {}, isTargetSelected, RxApp.MainThreadScheduler);

            return _model.SelectCommand
                .SubscribeWithLog(_ =>
                {   
                    _model.Close(new SelectTargetResult.Selected(
                        _model.SelectedConnection?.ConnectionInfo,
                        _model.SelectedServer?.ServerInfo,
                        _model.SelectedDatabase?.DatabaseInfo));
                });
        }

        public IDisposable BindCancel()
        {
            _model.CancelCommand = ReactiveCommand.Create(
                () => {}, null, RxApp.MainThreadScheduler);

            return _model.CancelCommand
                .SubscribeWithLog(_ =>
                {
                    _model.Close(new SelectTargetResult.Cancelled());
                });
        }

        private ConnectionItemModel CreateConnectionItem(
            RedisConnectionInfo connectionInfo)
        {
            var connectionName = string.IsNullOrWhiteSpace(connectionInfo.Name)
                ? connectionInfo.ConnectionString
                : connectionInfo.Name;

            var connectionString = string.IsNullOrWhiteSpace(connectionInfo.Name)
                ? null
                : $"({connectionInfo.ConnectionString})";
            
            return new ConnectionItemModel
            {
                ConnectionString = connectionString,
                ConnectionName = connectionName,
                ConnectionInfo = connectionInfo
            };
        }

        private ServerItemModel CreateServerItem(
            RedisServerInfo serverInfo)
        {
            string address;
            if (serverInfo.EndPoint is DnsEndPoint dns)
            {
                address = $"{dns.Host}:{dns.Port}";
            }
            else
            {
                address = serverInfo.EndPoint.ToString();
            }
            
            return new ServerItemModel
            {
                Address = address,
                ServerInfo = serverInfo
            };
        }

        private DatabaseItemModel CreateDatabaseItem(
            RedisDatabaseInfo databaseInfo)
        {
            return new DatabaseItemModel
            {
                Number = databaseInfo.Number,
                DatabaseInfo = databaseInfo
            };
        }
    }
}