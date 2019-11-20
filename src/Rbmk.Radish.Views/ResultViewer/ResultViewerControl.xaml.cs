using Avalonia.Markup.Xaml;
using Rbmk.Radish.Model.ResultViewer;

namespace Rbmk.Radish.Views.ResultViewer
{
    public class ResultViewerControl : BaseControl<ResultViewerModel>
    {
        public ResultViewerControl()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}