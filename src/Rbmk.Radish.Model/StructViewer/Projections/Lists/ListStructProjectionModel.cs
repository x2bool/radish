using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using DynamicData.Binding;
using Rbmk.Radish.Services.Redis.Projections;
using ReactiveUI;

namespace Rbmk.Radish.Model.StructViewer.Projections.Lists
{
    public class ListStructProjectionModel : StructProjectionModel
    {
        public string StartIndex { get; set; }

        public string StopIndex { get; set; }
        
        public Tuple<int, int> Range { get; set; }
        
        public ReactiveCommand<Tuple<int, int>, List<ListStructItemModel>> RangeCommand { get; set; }
        
        public ReactiveCommand<Unit, bool> AddCommand { get; set; }
        
        public ReactiveCommand<List<ListStructItemModel>, List<ListStructItemModel>> DeleteCommand { get; set; }
        
        public ReactiveCommand<ListStructItemModel, ListStructItemModel> EditCommand { get; set; }
        
        public ReactiveCommand<Unit, List<ListStructItemModel>> CancelCommand { get; set; }
        
        public bool AreActionsVisible { get; set; }
        
        public bool IsEditAvailable { get; set; }
        
        public bool IsDeleteAvailable { get; set; }
        
        public ObservableCollectionExtended<ListStructItemModel> ListItems { get; set; }
            = new ObservableCollectionExtended<ListStructItemModel>();
        
        public ListStructProjectionModel(StructProjectionInfo structProjectionInfo)
        {   
            var controller = new ListStructProjectionController(this);

            StartIndex = "0";
            StopIndex = "9";
            Range = Tuple.Create(0, 9);
            
            this.WhenActivated(disposables =>
            {
                var key = structProjectionInfo.ResultProjectionInfo.Value;
                var targetInfo = structProjectionInfo.ResultProjectionInfo.Result.TargetInfo;
                
                controller.BindLength(key, targetInfo)
                    .DisposeWith(disposables);
                
                controller.BindStartIndex()
                    .DisposeWith(disposables);

                controller.BindStopIndex()
                    .DisposeWith(disposables);

                controller.BindRange()
                    .DisposeWith(disposables);

                controller.BindRangeCommand(key, targetInfo)
                    .DisposeWith(disposables);

                controller.BindAddCommand(key, targetInfo)
                    .DisposeWith(disposables);
                
                controller.BindDeleteCommand(key, targetInfo)
                    .DisposeWith(disposables);

                controller.BindEditCommand(key, targetInfo)
                    .DisposeWith(disposables);

                controller.BindCancelCommand()
                    .DisposeWith(disposables);
                
                controller.ExecuteRangeCommand()
                    .DisposeWith(disposables);
            });
        }
    }
}