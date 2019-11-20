using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Rbmk.Utils.Broadcasts
{
    public class BroadcastService : IBroadcastService
    {
        private readonly Subject<object> _intents
            = new Subject<object>();

        public void Broadcast<T>(T intent)
            where T : Broadcast
        {
            _intents.OnNext(intent);
        }

        public IObservable<T> Listen<T>()
            where T : Broadcast
        {
            return _intents
                .AsObservable()
                .OfType<T>();
        }

        public IObservable<T> Listen<T>(Func<T, bool> predicate)
            where T : Broadcast
        {
            return Listen<T>()
                .Where(predicate);
        }
    }
}