using Avalonia.Markup.Xaml;
using Rbmk.Radish.Model.ConfirmDialog;
using Rbmk.Radish.Model.Dialogs;
using Rbmk.Utils.Reactive;
using ReactiveUI;

namespace Rbmk.Radish.Views.ConfirmDialog
{
    [DialogWindow(typeof(ConfirmDialogModel))]
    public class ConfirmDialogWindow : BaseWindow<ConfirmDialogModel>
    {
        private bool _resultWasSet;
        
        public ConfirmDialogWindow()
            : base(false)
        {
            AvaloniaXamlLoader.Load(this);

            this.WhenActivated(disposables =>
            {
                ViewModel.GetResult()
                    .SubscribeWithLog(_ =>
                    {
                        _resultWasSet = true;
                        Close();
                    });
            });
        }

        protected override void HandleClosed()
        {
            if (ViewModel != null && !_resultWasSet)
            {
                ViewModel.Close(false);
            }
            
            base.HandleClosed();
        }
    }
}
