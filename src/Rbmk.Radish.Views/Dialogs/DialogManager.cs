using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Rbmk.Radish.Model.Dialogs;

namespace Rbmk.Radish.Views.Dialogs
{
    public class DialogManager : IDialogManager
    {
        private readonly IDialogProvider _dialogProvider;

        public DialogManager(
            IDialogProvider dialogProvider)
        {
            _dialogProvider = dialogProvider;
        }
        
        public IObservable<string> OpenFolder(string startDir = null)
        {
            return Dispatcher.UIThread.InvokeAsync(() =>
                {
                    var topWindow = GetTopWindow();
            
                    var dialog = new OpenFolderDialog
                    {
                        Directory = startDir
                    };
                    
                    return dialog.ShowAsync(topWindow);
                })
                .ToObservable();
        }

        public IObservable<string> OpenFile(string startDir = null)
        {
            return Dispatcher.UIThread.InvokeAsync(() =>
                {
                    var topWindow = GetTopWindow();
            
                    var dialog = new OpenFileDialog
                    {
                        Directory = startDir,
                        AllowMultiple = false
                    };
                    
                    return dialog.ShowAsync(topWindow);
                })
                .ToObservable()
                .Select(files => files?.FirstOrDefault());
        }

        public IObservable<string[]> OpenFiles(string startDir = null)
        {
            return Dispatcher.UIThread.InvokeAsync(() =>
                {
                    var topWindow = GetTopWindow();
            
                    var dialog = new OpenFileDialog
                    {
                        Directory = startDir,
                        AllowMultiple = true
                    };
                    
                    return dialog.ShowAsync(topWindow);
                })
                .ToObservable()
                .Select(files => files ?? new string[0]);
        }

        public IObservable<T> Open<T>(DialogModel<T> dialogModel)
        {
            var contextType = dialogModel.GetType();
            var windowType = _dialogProvider.GetWindowType(contextType);

            return Dispatcher.UIThread.InvokeAsync(() =>
                {
                    var topWindow = GetTopWindow();
                    
                    var dialogWindow = (Window)Activator.CreateInstance(windowType);
                    dialogWindow.DataContext = dialogModel;

                    // HACK: https://github.com/AvaloniaUI/Avalonia/issues/2774
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        CenterDialog(dialogWindow, topWindow);
                    }
                    
                    return dialogWindow.ShowDialog(topWindow);
                })
                .ToObservable()
                .SelectMany(_ => dialogModel.GetResult());
        }

        private void CenterDialog(Window dialogWindow, Window parentWindow)
        {
            var toScreen = new Func<Point, PixelPoint>(parentWindow.PlatformImpl.PointToScreen);

            var topLeftParent = parentWindow.Position;
            var bottomRightParent = parentWindow.Position + toScreen(parentWindow.Bounds.TopRight);
            
            var topChild = toScreen(new Point(dialogWindow.MinWidth, dialogWindow.MinHeight));

            dialogWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            dialogWindow.Position = new PixelPoint(
                parentWindow.Position.X + (bottomRightParent.X - topLeftParent.X - topChild.X) / 2,
                parentWindow.Position.Y + 100);
        }

        private Window GetTopWindow()
        {
            var window = ((IClassicDesktopStyleApplicationLifetime) Application.Current.ApplicationLifetime)
                .Windows.Last();
            
            return window;
        }
    }
}