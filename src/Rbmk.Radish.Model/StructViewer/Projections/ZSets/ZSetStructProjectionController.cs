using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace Rbmk.Radish.Model.StructViewer.Projections.ZSets
{
    public class ZSetStructProjectionController
    {
        private readonly IDialogManager _dialogManager;
        private readonly IClientAccessor _clientAccessor;
        
        private readonly ZSetStructProjectionModel _model;

        public ZSetStructProjectionController(
            ZSetStructProjectionModel model)
            : this(
                Locator.Current.GetService<IDialogManager>(),
                Locator.Current.GetService<IClientAccessor>())
        {
            _model = model;
        }

        private ZSetStructProjectionController(
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
            _model.ScanCommand = ReactiveCommand.CreateFromObservable<string, IList<ZSetStructItemModel>>(
                pattern => ExecuteScan(key, targetInfo, pattern),
                null,
                RxApp.MainThreadScheduler);

            return _model.ScanCommand
                .SubscribeWithLog(zsetItems =>
                {
                    ChangeActionsMode();
                    
                    _model.ZSetItems.Clear();
                    _model.ZSetItems.AddRange(zsetItems);
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
            _model.DeleteCommand = ReactiveCommand.CreateFromObservable<List<ZSetStructItemModel>, List<ZSetStructItemModel>>(
                zsetItems => ExecuteDelete(key, targetInfo, zsetItems ?? _model.ZSetItems.ToList()),
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
            _model.EditCommand = ReactiveCommand.CreateFromObservable<ZSetStructItemModel, (ZSetStructItemModel, ZSetStructItemModel)>(
                zsetItem => ExecuteEdit(key, targetInfo, zsetItem ?? _model.ZSetItems.First(item => item.IsChecked)),
                _model.WhenAnyValue(m => m.IsEditAvailable),
                RxApp.MainThreadScheduler);

            return _model.EditCommand
                .SubscribeWithLog(tuple =>
                {
                    var (oldZSetItem, newZSetItem) = tuple;

                    if (oldZSetItem != null && newZSetItem != null)
                    {
                        var index = _model.ZSetItems.IndexOf(oldZSetItem);
                        
                        newZSetItem.IsChecked = false;
                        _model.ZSetItems[index] = newZSetItem;
                    }
                });
        }

        public IDisposable BindCancelCommand()
        {
            _model.CancelCommand = ReactiveCommand.Create(
                () => _model.ZSetItems.ToList(), null, RxApp.MainThreadScheduler);

            return _model.CancelCommand
                .SubscribeWithLog(zsetItems =>
                {
                    foreach (var zsetItem in zsetItems)
                    {
                        zsetItem.IsChecked = false;
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
            return _clientAccessor.With(targetInfo, client => client.ZCard(ustring.Make(key).ToString())) // TODO: async
                .SubscribeWithLog(len =>
                {
                    _model.BadgeText = $"ZSet ({len})";
                });
        }
        
        private IObservable<IList<ZSetStructItemModel>> ExecuteScan(
            byte[] key, RedisTargetInfo targetInfo, string pattern)
        {
            return _clientAccessor.With(targetInfo, client => client.ZScan(ustring.Make(key).ToString(), 0, pattern)) // TODO: async
                .Select(scan =>
                {
                    return scan.Items.Select(tuple => 
                        new ZSetStructItemModel
                        {
                            Value = tuple.Item1,
                            Score = tuple.Item2.ToString(CultureInfo.InvariantCulture),
                            IsChecked = false,
                            IsEnabled = true,
                            CheckAction = CheckItem,
                            EditAction = EditItem
                        }).ToList();
                });
        }

        private IObservable<bool> ExecuteAdd(byte[] key, RedisTargetInfo targetInfo)
        {
            return _dialogManager.Open(EditorDialogModel.AddZSetItem(target =>
            {
                return _clientAccessor.With(targetInfo, client =>
                    {
                        var isValueEmpty = string.IsNullOrEmpty(target.Value);
                        var isValueDuplicate = !isValueEmpty && client.ZScore(ustring.Make(key).ToString(), target.Value) != null;

                        if (!isValueEmpty && !isValueDuplicate)
                        {
                            client.ZAdd(ustring.Make(key).ToString(), Tuple.Create(target.Score, target.Value));
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

        private IObservable<(ZSetStructItemModel, ZSetStructItemModel)> ExecuteEdit(
            byte[] key, RedisTargetInfo targetInfo, ZSetStructItemModel zsetItem)
        {
            var editedSetItem = new ZSetStructItemModel
            {
                Score = zsetItem.Score,
                Value = zsetItem.Value,
                IsChecked = zsetItem.IsChecked,
                IsEnabled = zsetItem.IsEnabled,
                CheckAction = zsetItem.CheckAction,
                EditAction = zsetItem.EditAction
            };
            
            return _dialogManager.Open(EditorDialogModel.ReplaceZSetItem(
                    double.Parse(zsetItem.Score), zsetItem.Value, target =>
                    {
                        return _clientAccessor.With(targetInfo, client =>
                            {
                                var isValueEmpty = string.IsNullOrEmpty(target.Value);
                                var alreadyExists = !isValueEmpty
                                    && zsetItem.Score != target.Score.ToString(CultureInfo.InvariantCulture)
                                    && zsetItem.Value != target.Value
                                    && client.ZScore(ustring.Make(key).ToString(), target.Value) != null;

                                if (!isValueEmpty && !alreadyExists)
                                {
                                    client.Multi();
                                    client.ZRem(ustring.Make(key).ToString(), zsetItem.Value);
                                    client.ZAdd(ustring.Make(key).ToString(), Tuple.Create(target.Score, target.Value));
                                    client.Exec();

                                    editedSetItem.Score = target.Score.ToString(CultureInfo.InvariantCulture);
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
                .Select(wasEdited => wasEdited ? (zsetItem, editedSetItem) : (null, null));
        }

        private IObservable<List<ZSetStructItemModel>> ExecuteDelete(
            byte[] key, RedisTargetInfo targetInfo, List<ZSetStructItemModel> items)
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
                        return Observable.Return(new List<ZSetStructItemModel>());
                    }

                    return _clientAccessor.With(targetInfo, client =>
                        {
                            var deletedItems = new List<ZSetStructItemModel>();
                            
                            client.Multi();
                            try
                            {
                                foreach (var item in items)
                                {
                                    if (item.IsChecked)
                                    {
                                        client.ZRem(ustring.Make(key).ToString(), item.Value);
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

        private void CheckItem(ZSetStructItemModel item)
        {
            ChangeActionsMode();
        }
        
        private void EditItem(ZSetStructItemModel item)
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
            var count = _model.ZSetItems.Count(item => item.IsChecked);
            
            _model.AreActionsVisible = count > 0;
            _model.IsEditAvailable = count == 1;
            _model.IsDeleteAvailable = count > 0;
        }
    }
}