using System.Reactive.Disposables;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Rbmk.Radish.Views
{
    public class BaseControl<TViewModel> : ReactiveUserControl<TViewModel>
        where TViewModel : class
    {   
        public BaseControl(bool activate = true)
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