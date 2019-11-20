using System;
using System.Linq;
using System.Net;
using DynamicData;
using Rbmk.Radish.Services.Redis;

namespace Rbmk.Radish.Model.ExplorerPad.ConnectionExplorer.Nodes
{
    public class ConnectionNodeBuilder : IConnectionNodeBuilder
    {
        private readonly SourceList<BaseNodeModel> _nodes;

        public ConnectionNodeBuilder(
            SourceList<BaseNodeModel> nodes)
        {
            _nodes = nodes;
            _nodes.Add(new NewConnectionNodeModel());
        }

        public void HandleAction(ConnectionNodeAction action)
        {
            switch (action)
            {
                case ConnectionNodeAction.Add add:
                    HandleAdd(add.ConnectionInfo);
                    break;
                
                case ConnectionNodeAction.Delete delete:
                    HandleDelete(delete.Id);
                    break;
                
                case ConnectionNodeAction.Update update:
                    HandleUpdate(update.Id, update.ConnectionInfo);
                    break;
            }
        }

        private void HandleAdd(RedisConnectionInfo connectionInfo)
        {
            var connectionNode = CreateConnectionNode(connectionInfo);
            _nodes.Insert(_nodes.Count - 1, connectionNode);
        }

        private void HandleDelete(Guid id)
        {
            var connectionNode = _nodes.Items
                .OfType<ConnectionNodeModel>()
                .FirstOrDefault(node => node.ConnectionInfo.Id == id);
            
            _nodes.Remove(connectionNode);
        }

        private void HandleUpdate(Guid id, RedisConnectionInfo connectionInfo)
        {
            var newConnectionNode = CreateConnectionNode(connectionInfo);
            var oldConnectionNode = _nodes.Items
                .OfType<ConnectionNodeModel>()
                .FirstOrDefault(node => node.ConnectionInfo.Id == id);
            
            _nodes.Replace(oldConnectionNode, newConnectionNode);
        }

        private ConnectionNodeModel CreateConnectionNode(RedisConnectionInfo connectionInfo)
        {
            var node = new ConnectionNodeModel
            {
                ConnectionName = connectionInfo.Name,
                ConnectionString = connectionInfo.ConnectionString,
                ConnectionInfo = connectionInfo
            };

            foreach (var server in connectionInfo.ServerInfos)
            {
                var serverNode = CreateServerNode(server);
                node.Nodes.Add(serverNode);
            }

            if (connectionInfo.DatabaseInfos.Count > 0)
            {
                var databaseGroupNode = CreateDatabaseGroupNode(connectionInfo);
                node.Nodes.Add(databaseGroupNode);
            }

            if (node.Nodes.Count > 0)
            {
                node.IsExpanded = true;
                node.IsOpen = true;
            }
            
            return node;
        }

        private DatabaseGroupNodeModel CreateDatabaseGroupNode(RedisConnectionInfo connectionInfo)
        {
            var node = new DatabaseGroupNodeModel();

            foreach (var database in connectionInfo.DatabaseInfos)
            {
                var databaseNode = CreateDatabaseNode(database);
                node.Nodes.Add(databaseNode);
            }

            return node;
        }

        private DatabaseNodeModel CreateDatabaseNode(RedisDatabaseInfo databaseInfo)
        {
            var node = new DatabaseNodeModel
            {
                Number = databaseInfo.Number,
                DatabaseInfo = databaseInfo
            };

            return node;
        }

        private ServerGroupNodeModel CreateServerGroupNode(RedisConnectionInfo connectionInfo)
        {
            var node = new ServerGroupNodeModel();

            foreach (var server in connectionInfo.ServerInfos)
            {
                var serverNode = CreateServerNode(server);
                node.Nodes.Add(serverNode);
            }

            return node;
        }

        private ServerNodeModel CreateServerNode(RedisServerInfo serverInfo)
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
            
            var node = new ServerNodeModel
            {
                Address = address,
                ServerInfo = serverInfo
            };

            return node;
        }
    }
}