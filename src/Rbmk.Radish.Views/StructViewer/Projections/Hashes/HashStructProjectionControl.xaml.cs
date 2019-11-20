using Avalonia.Markup.Xaml;
using Rbmk.Radish.Model.StructViewer.Projections.Hashes;

namespace Rbmk.Radish.Views.StructViewer.Projections.Hashes
{
    public class HashStructProjectionControl : BaseControl<HashStructProjectionModel>
    {
        public HashStructProjectionControl()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}