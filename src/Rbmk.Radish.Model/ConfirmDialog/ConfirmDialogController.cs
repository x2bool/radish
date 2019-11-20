using System;
using Rbmk.Utils.Reactive;
using ReactiveUI;

namespace Rbmk.Radish.Model.ConfirmDialog
{
    public class ConfirmDialogController
    {
        private readonly ConfirmDialogModel _model;

        public ConfirmDialogController(ConfirmDialogModel model)
        {
            _model = model;
        }

        public IDisposable BindConfirmCommand()
        {
            _model.ConfirmCommand = ReactiveCommand.Create(
                () => _model.Close(true), null, RxApp.MainThreadScheduler);

            return _model.ConfirmCommand
                .SubscribeWithLog();
        }

        public IDisposable BindCancelCommand()
        {
            _model.CancelCommand = ReactiveCommand.Create(
                () => _model.Close(false), null, RxApp.MainThreadScheduler);

            return _model.CancelCommand
                .SubscribeWithLog();
        }
    }
}