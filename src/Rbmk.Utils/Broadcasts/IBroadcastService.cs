using System;

namespace Rbmk.Utils.Broadcasts
{
    public interface IBroadcastService
    {
        void Broadcast<T>(T intent)
            where T : Broadcast;

        IObservable<T> Listen<T>()
            where T : Broadcast;

        IObservable<T> Listen<T>(Func<T, bool> predicate)
            where T : Broadcast;
    }
}