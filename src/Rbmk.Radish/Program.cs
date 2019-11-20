using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.ReactiveUI;
using Rbmk.Radish.Model;
using Rbmk.Radish.Services.Persistence;
using Rbmk.Radish.Views;
using Splat;

namespace Rbmk.Radish
{
    public class Program
    {
        static int Main(string[] args)
        {
            Thread.CurrentThread.TrySetApartmentState(ApartmentState.STA);
            
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;

            var mutex = new Mutex(false, typeof(Program).FullName);
            
            try
            {
                if (mutex.WaitOne(TimeSpan.FromSeconds(5), true))
                {
                    var registry = new Registry();
                    registry.Register(Locator.CurrentMutable, Locator.Current);
                    
                    return BuildAvaloniaApp()
                        .StartWithClassicDesktopLifetime(args);
                }
            }
            finally
            {
                mutex.ReleaseMutex();
            }

            return 0;
        }
        
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<MainApplication>()
                .UsePlatformDetect()
                .With(new X11PlatformOptions { EnableMultiTouch = true })
                .With(new Win32PlatformOptions
                {
                    EnableMultitouch = true,
                    AllowEglInitialization = true
                })
                .UseSkia()
                .UseReactiveUI();

        private static void CurrentDomainOnUnhandledException(
            object sender, UnhandledExceptionEventArgs e)
        {
            var error = e.ExceptionObject.ToString();
            Console.Error.WriteLine(error);
            Console.Error.WriteLine();
            
            var file = Path.Combine(ApplicationStorage.Instance.LogDirectory, "fatal.log");
            File.AppendAllText(file, error);
            File.AppendAllText(file, Environment.NewLine);
        }

        private static void TaskSchedulerOnUnobservedTaskException(
            object sender, UnobservedTaskExceptionEventArgs e)
        {
            var error = e.Exception.ToString();
            Console.WriteLine(error);
            Console.Error.WriteLine();
            
            var file = Path.Combine(ApplicationStorage.Instance.LogDirectory, "fatal.log");
            File.AppendAllText(file, error);
            File.AppendAllText(file, Environment.NewLine);
        }
    }
}