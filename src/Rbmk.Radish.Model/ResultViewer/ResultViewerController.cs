using System;
using System.Linq;
using System.Reactive.Linq;
using Rbmk.Radish.Model.ResultViewer.Projections;
using Rbmk.Radish.Model.ResultViewer.Projections.None;
using Rbmk.Radish.Model.ResultViewer.Projections.Some;
using Rbmk.Radish.Services.Broadcasts.Results;
using Rbmk.Radish.Services.Redis;
using Rbmk.Radish.Services.Redis.Projections;
using Rbmk.Utils.Broadcasts;
using Rbmk.Utils.Reactive;
using ReactiveUI;
using Splat;

namespace Rbmk.Radish.Model.ResultViewer
{
    public class ResultViewerController
    {
        private readonly IBroadcastService _broadcastService;
        private readonly IResultProjector _resultProjector;
        private readonly ResultViewerModel _model;

        public ResultViewerController(
            IBroadcastService broadcastService,
            IResultProjector resultProjector)
        {
            _broadcastService = broadcastService;
            _resultProjector = resultProjector;
        }

        public ResultViewerController(ResultViewerModel model)
            : this (
                Locator.Current.GetService<IBroadcastService>(),
                Locator.Current.GetService<IResultProjector>())
        {
            _model = model;
        }

        public IDisposable BindResultBroadcasts()
        {
            return _broadcastService.Listen<ViewResultsIntent>()
                .SubscribeOn(RxApp.TaskpoolScheduler)
                .ObserveOn(RxApp.MainThreadScheduler)
                .SubscribeWithLog(broadcast =>
                {
                    _model.CurrentResultInfos = broadcast.ResultInfos;
                });
        }

        public IDisposable BindProjections()
        {
            return _model.WhenAnyValue(m => m.CurrentResultInfos)
                .SubscribeOn(RxApp.TaskpoolScheduler)
                .ObserveOn(RxApp.MainThreadScheduler)
                .SubscribeWithLog(results =>
                {
                    _model.ResultProjections.Clear();
                    
                    if (results == null)
                    {
                        _model.ResultProjections.Add(GetProjection());
                    }
                    else if (results.Length > 1)
                    {
                        _model.ResultProjections.Add(GetProjection(results));
                    }
                    else
                    {
                        _model.ResultProjections.Add(GetProjection(results.FirstOrDefault()));
                    }

                    _model.ResultProjection = _model.ResultProjections.FirstOrDefault();
                });
        }
        
        private ResultProjectionModel GetProjection(params RedisResultInfo[] resultInfos)
        {
            if (resultInfos.Length > 0)
            {
                var projectInfos = _resultProjector.Project(resultInfos)
                    .ToList();

                return new SomeResultProjectionModel(projectInfos);
            }
            
            return new NoneResultProjectionModel();
        }
    }
}