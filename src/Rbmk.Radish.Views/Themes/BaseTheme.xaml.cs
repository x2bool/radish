using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Rbmk.Radish.Views.Themes
{
    public class BaseTheme : Styles
    {
        public BaseTheme()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}