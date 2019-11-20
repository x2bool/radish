using System.Reactive;
using Avalonia;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Rbmk.Radish.Model.Dialogs;
using Rbmk.Radish.Model.SettingsDialog;
using Rbmk.Utils.Reactive;
using ReactiveUI;

namespace Rbmk.Radish.Views.SettingsDialog
{
    [DialogWindow(typeof(SettingsDialogModel))]
    public class SettingsDialogWindow : BaseWindow<SettingsDialogModel>
    {
        private bool _resultWasSet;
        
        public SettingsDialogWindow()
            : base(false)
        {
            AvaloniaXamlLoader.Load(this);
            this.AttachDevTools(KeyGesture.Parse("Ctrl+Shift+D"));

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
                ViewModel.Close(Unit.Default);
            }
            
            base.HandleClosed();
        }
    }
}
