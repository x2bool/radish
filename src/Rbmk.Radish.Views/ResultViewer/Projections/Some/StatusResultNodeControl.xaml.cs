using Avalonia.Markup.Xaml;
using Rbmk.Radish.Model.ResultViewer.Projections.Some;

namespace Rbmk.Radish.Views.ResultViewer.Projections.Some
{
    public class StatusResultNodeControl : BaseControl<StatusResultNodeModel>
    {
        public StatusResultNodeControl()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}