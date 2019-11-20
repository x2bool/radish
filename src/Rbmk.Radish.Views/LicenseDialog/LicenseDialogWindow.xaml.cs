using Avalonia.Markup.Xaml;
using Rbmk.Radish.Model.Dialogs;
using Rbmk.Radish.Model.LicenseDialog;
using Rbmk.Utils.Reactive;
using ReactiveUI;

namespace Rbmk.Radish.Views.LicenseDialog
{
    [DialogWindow(typeof(LicenseDialogModel))]
    public class LicenseDialogWindow : BaseWindow<LicenseDialogModel>
    {
        private bool _resultWasSet;
        
        public LicenseDialogWindow()
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
