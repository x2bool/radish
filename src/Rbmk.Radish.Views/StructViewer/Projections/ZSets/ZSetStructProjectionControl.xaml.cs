using Avalonia.Markup.Xaml;
using Rbmk.Radish.Model.StructViewer.Projections.ZSets;

namespace Rbmk.Radish.Views.StructViewer.Projections.ZSets
{
    public class ZSetStructProjectionControl : BaseControl<ZSetStructProjectionModel>
    {
        public ZSetStructProjectionControl()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}