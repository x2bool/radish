using System.Reactive;
using System.Reactive.Disposables;
using DynamicData.Binding;
using Rbmk.Radish.Services.Redis.Projections;
using ReactiveUI;

namespace Rbmk.Radish.Model.StructViewer.Projections.Strings
{
    public class StringStructProjectionModel : StructProjectionModel
    {   
        public string ValueText { get; set; }
        
        public ObservableCollectionExtended<MimeTypeItem> MimeTypes { get; set; }
            = new ObservableCollectionExtended<MimeTypeItem>();
        
        public MimeTypeItem SelectedMimeType { get; set; }
        
        public ReactiveCommand<string, Unit> SaveCommand { get; set; }

        public StringStructProjectionModel(StructProjectionInfo structProjectionInfo)
        {   
            var controller = new StringStructProjectionController(this);
            
            this.WhenActivated(disposables =>
            {
                var key = structProjectionInfo.ResultProjectionInfo.Value;
                var targetInfo = structProjectionInfo.ResultProjectionInfo.Result.TargetInfo;
                
                controller.BindMimeTypes()
                    .DisposeWith(disposables);
                
                controller.BindLength(key, targetInfo)
                    .DisposeWith(disposables);

                controller.BindValue(key, targetInfo)
                    .DisposeWith(disposables);

                controller.BindSaveCommand(key, targetInfo)
                    .DisposeWith(disposables);
            });
        }
    }
}