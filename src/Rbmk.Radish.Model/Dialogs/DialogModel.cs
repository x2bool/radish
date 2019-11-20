using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using PropertyChanged;
using ReactiveUI;

namespace Rbmk.Radish.Model.Dialogs
{
    [AddINotifyPropertyChangedInterface]
    public abstract class DialogModel<T> : IActivatableViewModel
    {
        private readonly ReplaySubject<T> _result = new ReplaySubject<T>();
        
        public IObservable<T> GetResult()
        {
            return _result.AsObservable()
                .Take(1);
        }

        public void Close(T result)
        {
            _result.OnNext(result);
        }
        
        public ViewModelActivator Activator { get; } = new ViewModelActivator();
    }
}