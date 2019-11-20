using Avalonia.Markup.Xaml;
using Rbmk.Radish.Model.ResultViewer.Projections.Some;

namespace Rbmk.Radish.Views.ResultViewer.Projections.Some
{
    public class ValueResultNodeControl : BaseControl<ValueResultNodeModel>
    {
        public ValueResultNodeControl()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}