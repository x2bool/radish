using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using DynamicData.Binding;
using Rbmk.Radish.Services.Redis.Projections;
using ReactiveUI;

namespace Rbmk.Radish.Model.StructViewer.Projections.Hashes
{
    public class HashStructProjectionModel : StructProjectionModel
    {   
        public string Pattern { get; set; }
        
        public ReactiveCommand<string, IList<HashStructItemModel>> ScanCommand { get; set; }
        
        public ReactiveCommand<Unit, bool> AddCommand { get; set; }
        
        public ReactiveCommand<List<HashStructItemModel>, List<HashStructItemModel>> DeleteCommand { get; set; }
        
        public ReactiveCommand<HashStructItemModel, HashStructItemModel> EditCommand { get; set; }
        
        public ReactiveCommand<Unit, List<HashStructItemModel>> CancelCommand { get; set; }
        
        public bool AreActionsVisible { get; set; }
        
        public bool IsEditAvailable { get; set; }
        
        public bool IsDeleteAvailable { get; set; }
        
        public ObservableCollectionExtended<HashStructItemModel> HashItems { get; set; }
            = new ObservableCollectionExtended<HashStructItemModel>();
        
        public HashStructProjectionModel(StructProjectionInfo structProjectionInfo)
        {   
            var controller = new HashStructProjectionController(this);

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