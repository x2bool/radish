using Avalonia.Markup.Xaml;
using Rbmk.Radish.Model.ResultViewer.Projections.Some;

namespace Rbmk.Radish.Views.ResultViewer.Projections.Some
{
    public class SomeResultProjectionControl : BaseControl<SomeResultProjectionModel>
    {
        public SomeResultProjectionControl()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}