using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Rbmk.Radish.Views.Themes
{
    public class WindowsTheme : Styles
    {
        public WindowsTheme()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}