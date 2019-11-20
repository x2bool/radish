using Avalonia.Markup.Xaml;
using Rbmk.Radish.Model.Dialogs;
using Rbmk.Radish.Model.SelectTargetDialog;
using Rbmk.Utils.Reactive;
using ReactiveUI;

namespace Rbmk.Radish.Views.SelectTargetDialog
{
    [DialogWindow(typeof(SelectTargetDialogModel))]
    public class SelectTargetDialogWindow : BaseWindow<SelectTargetDialogModel>
    {
        private bool _resultWasSet;
        
        public SelectTargetDialogWindow()
            : base(false)
        {
            AvaloniaXamlLoader.Load(this);

            this.WhenActivated(disposables =>
            {
                ViewModel.GetResult()
                    .SubscribeWithLog(_ =>
                    {
                        if (!_resultWasSet)
                        {
                            _resultWasSet = true;
                            Close();
                        }
                    });
            });
        }

        protected override void HandleClosed()
        {
            if (ViewModel != null && !_resultWasSet)
            {
                _resultWasSet = true;
                ViewModel.Close(new SelectTargetResult.Cancelled());
            }
            
            base.HandleClosed();
        }
    }
}
