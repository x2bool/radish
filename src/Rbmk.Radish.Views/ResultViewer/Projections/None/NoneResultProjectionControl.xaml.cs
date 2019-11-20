using Avalonia.Markup.Xaml;
using Rbmk.Radish.Model.ResultViewer.Projections.None;

namespace Rbmk.Radish.Views.ResultViewer.Projections.None
{
    public class NoneResultProjectionControl : BaseControl<NoneResultProjectionModel>
    {
        public NoneResultProjectionControl()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}