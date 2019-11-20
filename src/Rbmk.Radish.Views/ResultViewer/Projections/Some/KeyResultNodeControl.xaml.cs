using Avalonia.Markup.Xaml;
using Rbmk.Radish.Model.ResultViewer.Projections.Some;

namespace Rbmk.Radish.Views.ResultViewer.Projections.Some
{
    public class KeyResultNodeControl : BaseControl<KeyResultNodeModel>
    {
        public KeyResultNodeControl()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}