using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.Themes.Default;
using Rbmk.Radish.Model;
using Rbmk.Radish.Views.Themes;

namespace Rbmk.Radish.Views
{
    public class MainApplication : Application
    {   
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            ApplyStyles();
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
            {
                lifetime.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowModel()
                };
            }
            
            base.OnFrameworkInitializationCompleted();
        }

        private void ApplyStyles()
        {
            Styles.Add(new BaseTheme());

            var theme = Environment.GetEnvironmentVariable("RADISH_THEME");

            if (theme == null)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Styles.Add(new MacosTheme());
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Styles.Add(new WindowsTheme());
                }
            }
            else if (theme.Equals("windows", StringComparison.InvariantCultureIgnoreCase))
            {
                Styles.Add(new WindowsTheme());
            }
            else // if (theme.Equals("macos", StringComparison.InvariantCultureIgnoreCase))
            {
                Styles.Add(new MacosTheme());
            }
        }
    }
}
