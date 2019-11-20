using Avalonia.Markup.Xaml;
using Rbmk.Radish.Model.StructViewer.Projections.Lists;

namespace Rbmk.Radish.Views.StructViewer.Projections.Lists
{
    public class ListStructProjectionControl : BaseControl<ListStructProjectionModel>
    {
        public ListStructProjectionControl()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}