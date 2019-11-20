using System;
using System.Reactive.Linq;
using Splat;
using ILogger = Serilog.ILogger;

namespace Rbmk.Utils.Reactive
{
    public static class ObservableExtensions
    {
        /// <summary>
        /// Like SelectMany but ordered
        /// </summary>
        public static IObservable<TResult> SelectSeq<T, TResult>(
            this IObservable<T> observable,
            Func<T, IObservable<TResult>> selector)
        {
            return observable.Select(selector).Concat();
        }
        
        /// <summary>
        /// Send errors to stderr
        /// </summary>
        public static IDisposable SubscribeWithLog<T>(this IObservable<T> observable, Action<T> onNext, Action onCompleted)
        {
            var logger = Locator.Current.GetService<ILogger>();
            
            return observable.Subscribe(
                onNext,
                e => logger.Error(e, "Unhandled exception occured on observable"),
                onCompleted);
        }
        
        /// <summary>
        /// Send errors to stderr
        /// </summary>
        public static IDisposable SubscribeWithLog<T>(this IObservable<T> observable, Action onCompleted)
        {
            var logger = Locator.Current.GetService<ILogger>();
            
            return observable.Subscribe(
                _ => { },
                e => logger.Error(e, "Unhandled exception occured on observable"),
                onCompleted);
        }
        
        /// <summary>
        /// Send errors to stderr
        /// </summary>
        public static IDisposable SubscribeWithLog<T>(this IObservable<T> observable, Action<T> onNext)
        {
            var logger = Locator.Current.GetService<ILogger>();
            
            return observable.Subscribe(
                onNext,
                e => logger.Error(e, "Unhandled exception occured on observable"));
        }
        
        /// <summary>
        /// Send errors to stderr
        /// </summary>
        public static IDisposable SubscribeWithLog<T>(this IObservable<T> observable)
        {
            var logger = Locator.Current.GetService<ILogger>();
            
            return observable.Subscribe(
                _ => { },
                e => logger.Error(e, "Unhandled exception occured on observable"));
        }
    }
}