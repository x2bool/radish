using Avalonia.Markup.Xaml;
using Rbmk.Radish.Model.SelectTargetDialog.Items;

namespace Rbmk.Radish.Views.SelectTargetDialog.Items
{
    public class DatabaseItemControl : BaseControl<DatabaseItemModel>
    {
        public DatabaseItemControl()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
