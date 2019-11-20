using Avalonia.Markup.Xaml;
using Rbmk.Radish.Model.ResultViewer.Projections.Some;

namespace Rbmk.Radish.Views.ResultViewer.Projections.Some
{
    public class ArrayResultNodeControl : BaseControl<ArrayResultNodeModel>
    {
        public ArrayResultNodeControl()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}