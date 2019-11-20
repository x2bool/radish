using Avalonia.Markup.Xaml;
using Rbmk.Radish.Model.StructViewer.Projections.None;

namespace Rbmk.Radish.Views.StructViewer.Projections.None
{
    public class NoneStructProjectionControl : BaseControl<NoneStructProjectionModel>
    {
        public NoneStructProjectionControl()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}