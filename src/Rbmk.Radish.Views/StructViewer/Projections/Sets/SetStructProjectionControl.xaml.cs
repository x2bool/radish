using Avalonia.Markup.Xaml;
using Rbmk.Radish.Model.StructViewer.Projections.Sets;

namespace Rbmk.Radish.Views.StructViewer.Projections.Sets
{
    public class SetStructProjectionControl : BaseControl<SetStructProjectionModel>
    {
        public SetStructProjectionControl()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}