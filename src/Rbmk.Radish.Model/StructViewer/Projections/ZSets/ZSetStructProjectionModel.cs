using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using DynamicData.Binding;
using Rbmk.Radish.Services.Redis.Projections;
using ReactiveUI;

namespace Rbmk.Radish.Model.StructViewer.Projections.ZSets
{
    public class ZSetStructProjectionModel : StructProjectionModel
    {
        public string Pattern { get; set; }
        
        public ReactiveCommand<string, IList<ZSetStructItemModel>> ScanCommand { get; set; }
        
        public ReactiveCommand<Unit, bool> AddCommand { get; set; }
        
        public ReactiveCommand<List<ZSetStructItemModel>, List<ZSetStructItemModel>> DeleteCommand { get; set; }
        
        public ReactiveCommand<ZSetStructItemModel, (ZSetStructItemModel, ZSetStructItemModel)> EditCommand { get; set; }
        
        public ReactiveCommand<Unit, List<ZSetStructItemModel>> CancelCommand { get; set; }
        
        public bool AreActionsVisible { get; set; }
        
        public bool IsEditAvailable { get; set; }
        
        public bool IsDeleteAvailable { get; set; }
        
        public ObservableCollectionExtended<ZSetStructItemModel> ZSetItems { get; set; }
            = new ObservableCollectionExtended<ZSetStructItemModel>();

        public ZSetStructProjectionModel(StructProjectionInfo structProjectionInfo)
        {   
            var controller = new ZSetStructProjectionController(this);
            
            Pattern = "*";
            
            this.WhenActivated(disposables =>
            {
                var key = structProjectionInfo.ResultProjectionInfo.Value;
                var targetInfo = structProjectionInfo.ResultProjectionInfo.Result.TargetInfo;
                
                controller.BindLength(key, targetInfo)
                    .DisposeWith(disposables);
                
                controller.BindScanCommand(key, targetInfo)
                    .DisposeWith(disposables);

                controller.BindAddCommand(key, targetInfo)
                    .DisposeWith(disposables);
                
                controller.BindDeleteCommand(key, targetInfo)
                    .DisposeWith(disposables);

                controller.BindEditCommand(key, targetInfo)
                    .DisposeWith(disposables);

                controller.BindCancelCommand()
                    .DisposeWith(disposables);
                
                controller.ExecuteScanCommand()
                    .DisposeWith(disposables);
            });
        }
    }
}