using Avalonia.Markup.Xaml;
using Rbmk.Radish.Model.ResultViewer.Projections.Some;

namespace Rbmk.Radish.Views.ResultViewer.Projections.Some
{
    public class ErrorResultNodeControl : BaseControl<ErrorResultNodeModel>
    {
        public ErrorResultNodeControl()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}