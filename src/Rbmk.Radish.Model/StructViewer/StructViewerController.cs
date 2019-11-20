using System;
using System.Reactive.Linq;
using Rbmk.Radish.Model.StructViewer.Projections;
using Rbmk.Radish.Model.StructViewer.Projections.Hashes;
using Rbmk.Radish.Model.StructViewer.Projections.Lists;
using Rbmk.Radish.Model.StructViewer.Projections.None;
using Rbmk.Radish.Model.StructViewer.Projections.Sets;
using Rbmk.Radish.Model.StructViewer.Projections.Strings;
using Rbmk.Radish.Model.StructViewer.Projections.ZSets;
using Rbmk.Radish.Services.Broadcasts.Structs;
using Rbmk.Radish.Services.Redis.Projections;
using Rbmk.Utils.Broadcasts;
using Rbmk.Utils.Reactive;
using ReactiveUI;
using Splat;

namespace Rbmk.Radish.Model.StructViewer
{
    public class StructViewerController
    {
        private readonly IBroadcastService _broadcastService;
        private readonly IStructProjector _structProjector;
        private readonly StructViewerModel _model;

        public StructViewerController(
            StructViewerModel model)
            : this(
                Locator.Current.GetService<IBroadcastService>(),
                Locator.Current.GetService<IStructProjector>())
        {
            _model = model;
        }

        public StructViewerController(
            IBroadcastService broadcastService,
            IStructProjector structProjector)
        {
            _broadcastService = broadcastService;
            _structProjector = structProjector;
        }

        public IDisposable BindStructBroadcasts()
        {
            return _broadcastService.Listen<ViewStructIntent>()
                .SubscribeOn(RxApp.TaskpoolScheduler)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(intent => intent.ResultProjectionInfo)
                .SubscribeWithLog(structure =>
                {
                    _model.ResultProjectionInfo = structure;
                });
        }
        
        public IDisposable BindProjections()
        {
            return _model.WhenAnyValue(m => m.ResultProjectionInfo)
                .SelectSeq(result => _structProjector.Project(result))
                .SubscribeOn(RxApp.TaskpoolScheduler)
                .ObserveOn(RxApp.MainThreadScheduler)
                .SubscribeWithLog(projectionInfo =>
                {
                    var projectionModel = CreateProjectionModel(projectionInfo);
                    
                    _model.StructProjections.Clear();
                    _model.StructProjections.Add(projectionModel);

                    _model.SelectedStructProjection = projectionModel;
                });
        }

        private StructProjectionModel CreateProjectionModel(
            StructProjectionInfo structProjectionInfo)
        {
            switch (structProjectionInfo.Kind)
            {
                case StructProjectionKind.String:
                    return new StringStructProjectionModel(structProjectionInfo);
                
                case StructProjectionKind.List:
                    return new ListStructProjectionModel(structProjectionInfo);
                
                case StructProjectionKind.Hash:
                    return new HashStructProjectionModel(structProjectionInfo);
                
                case StructProjectionKind.Set:
                    return new SetStructProjectionModel(structProjectionInfo);
                
                case StructProjectionKind.ZSet:
                    return new ZSetStructProjectionModel(structProjectionInfo);
            }
            
            return new NoneStructProjectionModel();
        }
    }
}