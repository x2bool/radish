using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using DynamicData.Binding;
using Rbmk.Radish.Services.Broadcasts.Structs;
using Rbmk.Radish.Services.Redis.Projections;
using Rbmk.Utils.Broadcasts;
using Rbmk.Utils.Reactive;
using ReactiveUI;
using Splat;

namespace Rbmk.Radish.Model.ResultViewer.Projections.Some
{
    public class SomeResultProjectionController
    {
        private readonly IBroadcastService _broadcastService;
        private readonly SomeResultProjectionModel _model;

        public SomeResultProjectionController(
            SomeResultProjectionModel model)
            : this(
                Locator.Current.GetService<IBroadcastService>())
        {
            _model = model;
        }

        public SomeResultProjectionController(
            IBroadcastService broadcastService)
        {
            _broadcastService = broadcastService;
        }

        public IDisposable BindNodes(
            List<ResultProjectionInfo> projectInfos)
        {
            return projectInfos.ToObservable()
                .SelectSeq(p => BuildNodeAsync(p).ToObservable())
                .ToList()
                .ObserveOn(RxApp.MainThreadScheduler)
                .SubscribeWithLog(nodes =>
                {
                    _model.Nodes.Clear();
                    _model.Nodes.AddRange(nodes);

                    var selectedNode = FindFirstSimpleNode(_model.Nodes);
                    if (selectedNode == null)
                    {
                        selectedNode = _model.Nodes.FirstOrDefault();
                    }

                    _model.SelectedNode = selectedNode;
                });
        }

        public IDisposable BindSelection()
        {
            return _model.WhenAnyValue(m => m.SelectedNode)
                .SubscribeWithLog(node =>
                {
                    switch (node)
                    {
                        case KeyResultNodeModel key:
                            _broadcastService.Broadcast(
                                new ViewStructIntent(key.ResultProjectionInfo));
                            break;
                        
                        default:
                            _broadcastService.Broadcast(
                                new ViewStructIntent(null));
                            break;
                    }
                });
        }

        private Task<ResultNodeModel> BuildNodeAsync(
            ResultProjectionInfo projectionInfo)
        {
            return Task.Run(() =>
            {
                var node = BuildNode(projectionInfo);
                // top level node should be expanded
                node.IsExpanded = true;
                return node;
            });
        }

        private ResultNodeModel BuildNode(
            ResultProjectionInfo projectionInfo)
        {
            switch (projectionInfo.Kind)
            {
                case ResultProjectionKind.Array:
                {
                    var arrayNode = new ArrayResultNodeModel(projectionInfo);
                    arrayNode.Ordinal = projectionInfo.Index;
                    
                    foreach (var child in projectionInfo.Children)
                    {
                        var childNode = BuildNode(child);
                        arrayNode.Results.Add(childNode);
                    }

                    arrayNode.IsExpanded = projectionInfo.Children.Count <= 10;

                    return arrayNode;
                }

                case ResultProjectionKind.Transaction:
                {
                    var transactionNode = new TransactionResultNodeModel(projectionInfo);
                    transactionNode.Ordinal = projectionInfo.Index;
                    
                    foreach (var child in projectionInfo.Children)
                    {
                        transactionNode.Results.Add(BuildNode(child));
                    }

                    transactionNode.IsExpanded = true;
                    
                    return transactionNode;
                }
                
                case ResultProjectionKind.Error:
                    var errorNode = new ErrorResultNodeModel(projectionInfo);
                    errorNode.Ordinal = projectionInfo.Index;
                    return errorNode;
                
                case ResultProjectionKind.Status:
                    var statusNode = new StatusResultNodeModel(projectionInfo);
                    statusNode.Ordinal = projectionInfo.Index;
                    return statusNode;
                
                case ResultProjectionKind.Key:
                case ResultProjectionKind.SKey:
                case ResultProjectionKind.ZKey:
                case ResultProjectionKind.HKey:
                    var keyNode = new KeyResultNodeModel(projectionInfo);
                    keyNode.Ordinal = projectionInfo.Index;
                    return keyNode;
                
                case ResultProjectionKind.Value:
                default:
                    var valueNode = new ValueResultNodeModel(projectionInfo);
                    valueNode.Ordinal = projectionInfo.Index;
                    return valueNode;
            }
        }

        private ResultNodeModel FindFirstSimpleNode(
            IList<ResultNodeModel> nodes)
        {
            var simpleNode = nodes.FirstOrDefault(
                n => n is KeyResultNodeModel
                     || n is ErrorResultNodeModel
                     || n is StatusResultNodeModel
                     || n is ValueResultNodeModel);

            if (simpleNode != null)
            {
                return simpleNode;
            }
            
            foreach (var node in nodes)
            {
                switch (node)
                {
                    case ArrayResultNodeModel array:
                        simpleNode = FindFirstSimpleNode(array.Results);
                        if (simpleNode != null)
                        {
                            return simpleNode;
                        }
                        break;
                    
                    case TransactionResultNodeModel transaction:
                        simpleNode = FindFirstSimpleNode(transaction.Results);
                        if (simpleNode != null)
                        {
                            return simpleNode;
                        }
                        break;
                }
            }

            return null;
        }
    }
}