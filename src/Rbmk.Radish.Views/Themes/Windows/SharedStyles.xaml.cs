using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Rbmk.Radish.Views.Themes.Windows
{
    public class SharedStyles : Styles
    {
        public SharedStyles()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}