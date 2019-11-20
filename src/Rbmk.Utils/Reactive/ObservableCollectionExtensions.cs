using System;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using DynamicData.Binding;

namespace Rbmk.Utils.Reactive
{
    public static class ObservableCollectionExtensions
    {
        public static IObservable<T> ObserveAdditions<T>(this IObservableCollection<T> collection)
        {
            return Observable
                .FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                    h => collection.CollectionChanged += h,
                    h => collection.CollectionChanged -= h)
                .Where(args => args.EventArgs.Action == NotifyCollectionChangedAction.Add)
                .SelectMany(args => args.EventArgs.NewItems.Cast<T>());
        }
        
        public static IObservable<T> ObserveRemovals<T>(this IObservableCollection<T> collection)
        {
            return Observable
                .FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                    h => collection.CollectionChanged += h,
                    h => collection.CollectionChanged -= h)
                .Where(args => args.EventArgs.Action == NotifyCollectionChangedAction.Remove)
                .SelectMany(args => args.EventArgs.OldItems.Cast<T>());
        }
    }
}