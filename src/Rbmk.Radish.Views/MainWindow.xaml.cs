using Avalonia;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Rbmk.Radish.Model;
using Rbmk.Utils.Reactive;
using ReactiveUI;

namespace Rbmk.Radish.Views
{
    public class MainWindow : BaseWindow<MainWindowModel>
    {
        public MainWindow()
        {
            AvaloniaXamlLoader.Load(this);
            this.AttachDevTools(KeyGesture.Parse("Ctrl+Shift+D"));

            this.WhenActivated(disposables =>
            {
                ViewModel?.ExitCommand?.SubscribeWithLog(_ =>
                {
                    Close();
                });
            });
        }
    }
}
