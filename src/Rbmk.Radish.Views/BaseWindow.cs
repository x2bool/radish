using System.Reactive.Disposables;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Rbmk.Radish.Views
{
    public class BaseWindow<TViewModel> : ReactiveWindow<TViewModel>
        where TViewModel : class
    {
        public BaseWindow(bool activate = true)
        {
            if (activate)
            {
                this.WhenActivated(disposables =>
                {
                    Disposable.Create(() => { }).DisposeWith(disposables);
                });
            }
        }
    }
}