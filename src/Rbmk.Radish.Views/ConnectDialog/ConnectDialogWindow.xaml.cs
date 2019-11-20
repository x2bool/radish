using Avalonia.Markup.Xaml;
using Rbmk.Radish.Model.ConnectDialog;
using Rbmk.Radish.Model.Dialogs;
using Rbmk.Utils.Reactive;
using ReactiveUI;

namespace Rbmk.Radish.Views.ConnectDialog
{
    [DialogWindow(typeof(ConnectDialogModel))]
    public class ConnectDialogWindow : BaseWindow<ConnectDialogModel>
    {
        private bool _resultWasSet;

        public ConnectDialogWindow()
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
                ViewModel.Close(new ConnectResult.Cancelled());
            }
            
            base.HandleClosed();
        }
    }
}
