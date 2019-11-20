using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Rbmk.Utils.Reactive;
using ReactiveUI;

namespace Rbmk.Radish.Model.EditorDialog
{
    public class EditorDialogController
    {
        private readonly EditorDialogModel _model;

        public EditorDialogController(EditorDialogModel model)
        {
            _model = model;
        }

        public IDisposable BindAddCommand(Func<EditorTarget, IObservable<EditorResult>> submit)
        {
            _model.AddCommand = ReactiveCommand.CreateFromObservable<Unit, EditorResult>(
                _ => submit(CreateTarget(EditorAction.Add)), null, RxApp.MainThreadScheduler);

            return _model.AddCommand
                .SubscribeWithLog(result =>
                {
                    if (CheckResult(result))
                    {
                        _model.Close(true);
                    }
                    else
                    {
                        _model.IndexError = result.IndexError;
                        _model.KeyError = result.KeyError;
                        _model.ScoreError = result.ScoreError;
                        _model.ValueError = result.ValueError;
                    }
                });
        }

        public IDisposable BindSaveCommand(Func<EditorTarget, IObservable<EditorResult>> submit)
        {
            _model.SaveCommand = ReactiveCommand.CreateFromObservable<Unit, EditorResult>(
                _ => submit(CreateTarget(EditorAction.Save)), null, RxApp.MainThreadScheduler);

            return _model.SaveCommand
                .SubscribeWithLog(result =>
                {
                    if (CheckResult(result))
                    {
                        _model.Close(true);
                    }
                    else
                    {
                        _model.IndexError = result.IndexError;
                        _model.KeyError = result.KeyError;
                        _model.ScoreError = result.ScoreError;
                        _model.ValueError = result.ValueError;
                    }
                });
        }

        public IDisposable BindPrependCommand(Func<EditorTarget, IObservable<EditorResult>> submit)
        {
            _model.PrependCommand = ReactiveCommand.CreateFromObservable<Unit, EditorResult>(
                _ => submit(CreateTarget(EditorAction.Prepend)), null, RxApp.MainThreadScheduler);

            return _model.PrependCommand
                .SubscribeWithLog(result =>
                {
                    if (CheckResult(result))
                    {
                        _model.Close(true);
                    }
                    else
                    {
                        _model.IndexError = result.IndexError;
                        _model.KeyError = result.KeyError;
                        _model.ScoreError = result.ScoreError;
                        _model.ValueError = result.ValueError;
                    }
                });
        }

        public IDisposable BindAppendCommand(Func<EditorTarget, IObservable<EditorResult>> submit)
        {
            _model.AppendCommand = ReactiveCommand.CreateFromObservable<Unit, EditorResult>(
                _ => submit(CreateTarget(EditorAction.Append)), null, RxApp.MainThreadScheduler);

            return _model.AppendCommand
                .SubscribeWithLog(result =>
                {
                    if (CheckResult(result))
                    {
                        _model.Close(true);
                    }
                    else
                    {
                        _model.IndexError = result.IndexError;
                        _model.KeyError = result.KeyError;
                        _model.ScoreError = result.ScoreError;
                        _model.ValueError = result.ValueError;
                    }
                });
        }

        public IDisposable BindCancelCommand()
        {
            _model.CancelCommand = ReactiveCommand.Create<Unit, Unit>(
                _ => Unit.Default, null, RxApp.MainThreadScheduler);

            return _model.CancelCommand
                .SubscribeWithLog(_ =>
                {
                    _model.Close(false);
                });
        }

        public IDisposable BindFields()
        {
            var indexSubscription = _model.WhenAnyValue(m => m.Index)
                .Select(value => value ?? "")
                .SubscribeWithLog(value =>
                {
                    var newValue = new string(value.Where(char.IsDigit).ToArray());
                    if (newValue != value)
                    {
                        _model.Index = newValue;
                    }
                });

            var scoreSubscription = _model.WhenAnyValue(m => m.Score)
                .Select(value => value ?? "")
                .SubscribeWithLog(value =>
                {
                    bool separated = false;
                    var newValue = new string(value.Where(c =>
                    {
                        if (char.IsDigit(c))
                        {
                            return true;
                        }

                        if (c == '.' && !separated)
                        {
                            separated = true;
                            return true;
                        }

                        return false;
                    }).ToArray());

                    if (newValue.StartsWith("."))
                    {
                        newValue = "0" + newValue;
                    }

                    if (newValue.EndsWith("."))
                    {
                        newValue = newValue + "0";
                    }
                    
                    if (newValue != value)
                    {
                        _model.Score = newValue;
                    }
                });

            return new CompositeDisposable(
                indexSubscription,
                scoreSubscription);
        }

        private EditorTarget CreateTarget(EditorAction action)
        {
            int.TryParse(_model.Index, out var index);
            double.TryParse(_model.Score, out var score);
            
            return new EditorTarget
            {
                Index = index,
                Score = score,
                Key = _model.Key,
                Value = _model.Value,
                Action = action
            };
        }

        private bool CheckResult(EditorResult result)
        {
            return result.IndexError == null
                   && result.KeyError == null
                   && result.ScoreError == null
                   && result.ValueError == null;
        }
    }
}