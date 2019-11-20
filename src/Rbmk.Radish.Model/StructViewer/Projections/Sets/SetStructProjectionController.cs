using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using NStack;
using Rbmk.Radish.Model.ConfirmDialog;
using Rbmk.Radish.Model.Dialogs;
using Rbmk.Radish.Model.EditorDialog;
using Rbmk.Radish.Services.Redis;
using Rbmk.Radish.Services.Redis.Connections;
using Rbmk.Utils.Reactive;
using ReactiveUI;
using Splat;

namespace Rbmk.Radish.Model.StructViewer.Projections.Sets
{
    public class SetStructProjectionController
    {
        private readonly IDialogManager _dialogManager;
        private readonly IClientAccessor _clientAccessor;
        
        private readonly SetStructProjectionModel _model;

        public SetStructProjectionController(
            SetStructProjectionModel model)
            : this (
                Locator.Current.GetService<IDialogManager>(),
                Locator.Current.GetService<IClientAccessor>())
        {
            _model = model;
        }

        private SetStructProjectionController(
            IDialogManager dialogManager,
            IClientAccessor clientAccessor)
        {
            _dialogManager = dialogManager;
            _clientAccessor = clientAccessor;
        }
        
        public IDisposable BindLength(byte[] key, RedisTargetInfo targetInfo)
        {
            return UpdateBadge(key, targetInfo);
        }

        public IDisposable BindScanCommand(byte[] key, RedisTargetInfo targetInfo)
        {
            _model.ScanCommand = ReactiveCommand.CreateFromObservable<string, IList<SetStructItemModel>>(
                pattern => ExecuteScan(key, targetInfo, pattern),
                null,
                RxApp.MainThreadScheduler);

            return _model.ScanCommand
                .SubscribeWithLog(setItems =>
                {
                    ChangeActionsMode();
                    
                    _model.SetItems.Clear();
                    _model.SetItems.AddRange(setItems);
                });
        }

        public IDisposable BindAddCommand(byte[] key, RedisTargetInfo targetInfo)
        {
            _model.AddCommand = ReactiveCommand.CreateFromObservable(
                () => ExecuteAdd(key, targetInfo),
                null,
                RxApp.MainThreadScheduler);

            return _model.AddCommand
                .SubscribeWithLog(wasAdded =>
                {
                    if (wasAdded)
                    {
                        ExecuteScanCommand();
                        UpdateBadge(key, targetInfo);
                    }
                });
        }

        public IDisposable BindDeleteCommand(byte[] key, RedisTargetInfo targetInfo)
        {
            _model.DeleteCommand = ReactiveCommand.CreateFromObservable<List<SetStructItemModel>, List<SetStructItemModel>>(
                setItems => ExecuteDelete(key, targetInfo, setItems ?? _model.SetItems.ToList()),
                _model.WhenAnyValue(m => m.IsDeleteAvailable),
                RxApp.MainThreadScheduler);

            return _model.DeleteCommand
                .SubscribeWithLog(deletedItems =>
                {
                    if (deletedItems.Count > 0)
                    {
                        ExecuteScanCommand();
                        UpdateBadge(key, targetInfo);
                    }
                });
        }

        public IDisposable BindEditCommand(byte[] key, RedisTargetInfo targetInfo)
        {
            _model.EditCommand = ReactiveCommand.CreateFromObservable<SetStructItemModel, (SetStructItemModel, SetStructItemModel)>(
                setItem => ExecuteEdit(key, targetInfo, setItem ?? _model.SetItems.First(item => item.IsChecked)),
                _model.WhenAnyValue(m => m.IsEditAvailable),
                RxApp.MainThreadScheduler);

            return _model.EditCommand
                .SubscribeWithLog(tuple =>
                {
                    var (oldSetItem, newSetItem) = tuple;

                    if (oldSetItem != null && newSetItem != null)
                    {
                        var index = _model.SetItems.IndexOf(oldSetItem);
                        
                        newSetItem.IsChecked = false;
                        _model.SetItems[index] = newSetItem;
                    }
                });
        }

        public IDisposable BindCancelCommand()
        {
            _model.CancelCommand = ReactiveCommand.Create(
                () => _model.SetItems.ToList(), null, RxApp.MainThreadScheduler);

            return _model.CancelCommand
                .SubscribeWithLog(setItems =>
                {
                    foreach (var setItem in setItems)
                    {
                        setItem.IsChecked = false;
                    }
                });
        }

        public IDisposable ExecuteScanCommand()
        {
            return _model.ScanCommand.Execute(_model.Pattern)
                .SubscribeWithLog();
        }

        private IDisposable UpdateBadge(byte[] key, RedisTargetInfo targetInfo)
        {
            return _clientAccessor.With(targetInfo, client => client.SCard(ustring.Make(key).ToString())) // TODO: async
                .SubscribeWithLog(len =>
                {
                    _model.BadgeText = $"Set ({len})";
                });
        }
        
        private IObservable<IList<SetStructItemModel>> ExecuteScan(
            byte[] key, RedisTargetInfo targetInfo, string pattern)
        {
            return _clientAccessor.With(targetInfo, client => client.SScan(ustring.Make(key).ToString(), 0, pattern)) // TODO: async
                .Select(scan =>
                {
                    return scan.Items.Select(item => 
                        new SetStructItemModel
                        {
                            Value = item,
                            IsChecked = false,
                            IsEnabled = true,
                            CheckAction = CheckItem,
                            EditAction = EditItem
                        }).ToList();
                });
        }

        private IObservable<bool> ExecuteAdd(byte[] key, RedisTargetInfo targetInfo)
        {
            return _dialogManager.Open(EditorDialogModel.AddSetItem(target =>
            {
                return _clientAccessor.With(targetInfo, client =>
                    {
                        var isValueEmpty = string.IsNullOrEmpty(target.Value);
                        var isValueDuplicate = !isValueEmpty && client.SIsMember(ustring.Make(key).ToString(), target.Value);

                        if (!isValueEmpty && !isValueDuplicate)
                        {
                            client.SAdd(ustring.Make(key).ToString(), target.Value);
                        }
                        
                        return new EditorResult
                        {
                            ValueError = isValueEmpty ? "Value is empty"
                                : (isValueDuplicate ? "Value already exists" : null),
                            Action = target.Action
                        };
                    });
            }));
        }

        private IObservable<(SetStructItemModel, SetStructItemModel)> ExecuteEdit(
            byte[] key, RedisTargetInfo targetInfo, SetStructItemModel setItem)
        {
            var editedSetItem = new SetStructItemModel
            {
                Value = setItem.Value,
                IsChecked = setItem.IsChecked,
                IsEnabled = setItem.IsEnabled,
                CheckAction = setItem.CheckAction,
                EditAction = setItem.EditAction
            };
            
            return _dialogManager.Open(EditorDialogModel.ReplaceSetItem(
                    setItem.Value, target =>
                    {
                        return _clientAccessor.With(targetInfo, client =>
                            {
                                var isValueEmpty = string.IsNullOrEmpty(target.Value);
                                var alreadyExists = !isValueEmpty
                                    && setItem.Value != target.Value
                                    && client.SIsMember(ustring.Make(key).ToString(), target.Value);

                                if (!isValueEmpty && !alreadyExists)
                                {
                                    client.Multi();
                                    client.SRem(ustring.Make(key).ToString(), setItem.Value);
                                    client.SAdd(ustring.Make(key).ToString(), target.Value);
                                    client.Exec();
                                    
                                    editedSetItem.Value = target.Value;
                                }
                                
                                return new EditorResult
                                {
                                    ValueError = isValueEmpty
                                        ? "Value is empty"
                                        : (alreadyExists ? "Value already exists" : null),
                                    Action = target.Action
                                };
                            });
                    }))
                .Select(wasEdited => wasEdited ? (setItem, editedSetItem) : (null, null));
        }

        private IObservable<List<SetStructItemModel>> ExecuteDelete(
            byte[] key, RedisTargetInfo targetInfo, List<SetStructItemModel> items)
        {
            return _dialogManager.Open(new ConfirmDialogModel
                {
                    TitleText = "Delete set entries?",
                    MessageText = "Do you want to delete selected set entries?",
                    ConfirmText = "Delete",
                    CancelText = "Cancel"
                })
                .SelectMany(confirmed =>
                {
                    if (!confirmed)
                    {
                        return Observable.Return(new List<SetStructItemModel>());
                    }

                    return _clientAccessor.With(targetInfo, client =>
                        {
                            var deletedItems = new List<SetStructItemModel>();
                            
                            client.Multi();
                            try
                            {
                                foreach (var item in items)
                                {
                                    if (item.IsChecked)
                                    {
                                        client.SRem(ustring.Make(key).ToString(), item.Value);
                                        deletedItems.Add(item);
                                    }
                                }

                                client.Exec();
                            }
                            catch
                            {
                                client.Discard();

                                throw;
                            }

                            return deletedItems;
                        });
                });
        }

        private void CheckItem(SetStructItemModel item)
        {
            ChangeActionsMode();
        }
        
        private void EditItem(SetStructItemModel item)
        {
            if (item.IsEditing)
            {
                return;
            }

            item.IsEditing = true;
            
            _model.EditCommand.Execute(item)
                .SubscribeOn(RxApp.TaskpoolScheduler)
                .ObserveOn(RxApp.MainThreadScheduler)
                .SubscribeWithLog(_ => { item.IsEditing = false; });
        }
        
        private void ChangeActionsMode()
        {
            var count = _model.SetItems.Count(item => item.IsChecked);
            
            _model.AreActionsVisible = count > 0;
            _model.IsEditAvailable = count == 1;
            _model.IsDeleteAvailable = count > 0;
        }
    }
}