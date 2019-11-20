using Avalonia.Markup.Xaml;
using Rbmk.Radish.Model.StructViewer;

namespace Rbmk.Radish.Views.StructViewer
{
    public class StructViewerControl : BaseControl<StructViewerModel>
    {
        public StructViewerControl()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}