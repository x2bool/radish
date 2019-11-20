using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Rbmk.Radish.Model.SettingsDialog.Sections;
using Rbmk.Radish.Model.SettingsDialog.Sections.Licenses;
using Rbmk.Utils.Reactive;
using Rbmk.Utils.System;
using ReactiveUI;
using Splat;

namespace Rbmk.Radish.Model.SettingsDialog
{
    public class SettingsDialogController
    {
        private readonly IWebBrowser _webBrowser;
        private readonly SettingsDialogModel _model;

        public SettingsDialogController(SettingsDialogModel model)
            : this(
                Locator.Current.GetService<IWebBrowser>())
        {
            _model = model;
        }

        private SettingsDialogController(
            IWebBrowser webBrowser)
        {
            _webBrowser = webBrowser;
        }

        public IDisposable BindSections()
        {
            var sections = new SettingsSectionModel[]
            {
                new LicenseSectionModel()
            };

            return sections.ToObservable()
                .ToList()
                .SubscribeWithLog(list =>
                {
                    _model.Sections.Clear();
                    _model.Sections.AddRange(list);

                    _model.SelectedSection = _model.Sections.FirstOrDefault();
                });
        }

        public IDisposable BindInfoCommand()
        {
            _model.InfoCommand = ReactiveCommand.Create(
                () => _webBrowser.OpenUrl("https://rbmk.io"), null, RxApp.MainThreadScheduler);

            return _model.InfoCommand
                .SubscribeWithLog();
        }

        public IDisposable BindCloseCommand()
        {
            _model.CloseCommand = ReactiveCommand.Create(
                () => { }, null, RxApp.MainThreadScheduler);

            return _model.CloseCommand
                .SubscribeWithLog(_ =>
                {
                    _model.Close(Unit.Default);
                });
        }
    }
}