using Avalonia.Markup.Xaml;
using Rbmk.Radish.Model.SettingsDialog.Sections.Licenses;

namespace Rbmk.Radish.Views.SettingsDialog.Sections.Licenses
{
    public class LicenseSectionControl : BaseControl<LicenseSectionModel>
    {
        public LicenseSectionControl()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
