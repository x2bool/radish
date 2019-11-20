using Avalonia.Markup.Xaml;
using Rbmk.Radish.Model.SelectTargetDialog.Items;

namespace Rbmk.Radish.Views.SelectTargetDialog.Items
{
    public class ConnectionItemControl : BaseControl<ConnectionItemModel>
    {
        public ConnectionItemControl()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
