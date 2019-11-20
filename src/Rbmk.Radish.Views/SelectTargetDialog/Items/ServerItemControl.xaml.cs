using Avalonia.Markup.Xaml;
using Rbmk.Radish.Model.SelectTargetDialog.Items;

namespace Rbmk.Radish.Views.SelectTargetDialog.Items
{
    public class ServerItemControl : BaseControl<ServerItemModel>
    {
        public ServerItemControl()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
