using Avalonia.Markup.Xaml;
using Rbmk.Radish.Model.ResultViewer.Projections.Some;

namespace Rbmk.Radish.Views.ResultViewer.Projections.Some
{
    public class TransactionResultNodeControl : BaseControl<TransactionResultNodeModel>
    {
        public TransactionResultNodeControl()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}