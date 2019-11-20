using System;
using System.Reactive;
using Rbmk.Radish.Model.Dialogs;
using Rbmk.Radish.Model.SettingsDialog;
using Rbmk.Utils.Reactive;
using ReactiveUI;
using Splat;

namespace Rbmk.Radish.Model.ExplorerPad
{
    public class ExplorerPadController
    {
        private readonly IDialogManager _dialogManager;
        private readonly ExplorerPadModel _model;

        public ExplorerPadController(
            ExplorerPadModel model)
            : this(
                Locator.Current.GetService<IDialogManager>())
        {
            _model = model;
        }

        public ExplorerPadController(
            IDialogManager dialogManager)
        {
            _dialogManager = dialogManager;
        }

        public IDisposable BindOpenSettingsCommand()
        {
            _model.OpenSettingsCommand = ReactiveCommand.CreateFromObservable<Unit, Unit>(
                _ => OpenSettings(), null, RxApp.MainThreadScheduler);

            return _model.OpenSettingsCommand
                .SubscribeWithLog();
        }

        private IObservable<Unit> OpenSettings()
        {
            return _dialogManager.Open(new SettingsDialogModel());
        }
    }
}