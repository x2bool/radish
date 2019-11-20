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

namespace Rbmk.Radish.Model.StructViewer.Projections.Hashes
{
    public class HashStructProjectionController
    {
        private readonly IDialogManager _dialogManager;
        private readonly IClientAccessor _clientAccessor;
        
        private readonly HashStructProjectionModel _model;

        public HashStructProjectionController(
            HashStructProjectionModel model)
            : this (
                Locator.Current.GetService<IDialogManager>(),
                Locator.Current.GetService<IClientAccessor>())
        {
            _model = model;
        }

        private HashStructProjectionController(
            IDialogManager dialogManager,
            IClientAccessor clientAccessor)
        {
            _dialogManager = dialogManager;
            _clientAccessor = clientAccessor;
        }
        
        public IDisposable BindLength(
            byte[] key, RedisTargetInfo targetInfo)
        {
            return UpdateBadge(key, targetInfo);
        }

        public IDisposable BindScanCommand(
            byte[] key, RedisTargetInfo targetInfo)
        {   
            _model.ScanCommand = ReactiveCommand.CreateFromObservable<string, IList<HashStructItemModel>>(
                pattern => ExecuteScan(key, targetInfo, pattern),
                null,
                RxApp.MainThreadScheduler);

            return _model.ScanCommand
                .SubscribeWithLog(hashItems =>
                {
                    ChangeActionsMode();
                    
                    _model.HashItems.Clear();
                    _model.HashItems.AddRange(hashItems);
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
            _model.DeleteCommand = ReactiveCommand.CreateFromObservable<List<HashStructItemModel>, List<HashStructItemModel>>(
                hashItems => ExecuteDelete(key, targetInfo, hashItems ?? _model.HashItems.ToList()),
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
            _model.EditCommand = ReactiveCommand.CreateFromObservable<HashStructItemModel, HashStructItemModel>(
                hashItem => ExecuteEdit(key, targetInfo, hashItem ?? _model.HashItems.First(item => item.IsChecked)),
                _model.WhenAnyValue(m => m.IsEditAvailable),
                RxApp.MainThreadScheduler);

            return _model.EditCommand
                .SubscribeWithLog(editedHashItem =>
                {
                    if (editedHashItem != null)
                    {
                        var index = _model.HashItems
                            .Select((item, i) => new { Item = item, Index = i })
                            .First(tuple => tuple.Item.Key == editedHashItem.Key)
                            .Index;
                        
                        editedHashItem.IsChecked = false;
                        _model.HashItems[index] = editedHashItem;
                    }
                });
        }

        public IDisposable BindCancelCommand()
        {
            _model.CancelCommand = ReactiveCommand.Create(
                () => _model.HashItems.ToList(), null, RxApp.MainThreadScheduler);

            return _model.CancelCommand
                .SubscribeWithLog(hashItems =>
                {
                    foreach (var hashItem in hashItems)
                    {
                        hashItem.IsChecked = false;
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
            return _clientAccessor.With(targetInfo, client => client.HLen(ustring.Make(key).ToString())) // TODO: async
                .SubscribeWithLog(len =>
                {
                    _model.BadgeText = $"Hash ({len})";
                });
        }
        
        private IObservable<IList<HashStructItemModel>> ExecuteScan(
            byte[] key, RedisTargetInfo targetInfo, string pattern)
        {
            return _clientAccessor.With(targetInfo, client => client.HScan(ustring.Make(key).ToString(), 0, pattern)) // TODO: async
                .Select(scan =>
                {
                    return scan.Items.Select(tuple => 
                        new HashStructItemModel
                        {
                            Key = tuple.Item1,
                            Value = tuple.Item2,
                            IsChecked = false,
                            IsEnabled = true,
                            CheckAction = CheckItem,
                            EditAction = EditItem
                        }).ToList();
                });
        }

        private IObservable<bool> ExecuteAdd(
            byte[] key, RedisTargetInfo targetInfo)
        {
            return _dialogManager.Open(EditorDialogModel.AddHashItem(target =>
                {
                    return _clientAccessor.With(targetInfo, client =>
                        {
                            var isValueEmpty = string.IsNullOrEmpty(target.Value);
                            var isKeyEmpty = string.IsNullOrEmpty(target.Key);
                            var isKeyDuplicate = !isKeyEmpty && client.Exists(target.Key); // TODO: async

                            if (!isValueEmpty && !isKeyEmpty && !isKeyDuplicate)
                            {
                                client.HSet(ustring.Make(key).ToString(), target.Key, target.Value);
                            }

                            return new EditorResult
                            {
                                KeyError = isKeyEmpty ? "Key is empty"
                                    : (isKeyDuplicate ? "Key already exists" : null),
                                ValueError = isValueEmpty ? "Value is empty" : null,
                                Action = target.Action
                            };
                        });
                }));
        }

        private IObservable<HashStructItemModel> ExecuteEdit(
            byte[] key, RedisTargetInfo targetInfo, HashStructItemModel hashItem)
        {
            var editedHashItem = new HashStructItemModel
            {
                Key = hashItem.Key,
                Value = hashItem.Value,
                IsChecked = hashItem.IsChecked,
                IsEnabled = hashItem.IsEnabled,
                CheckAction = hashItem.CheckAction,
                EditAction = hashItem.EditAction
            };
            
            return _dialogManager.Open(EditorDialogModel.UpdateHashItem(
                    hashItem.Key, hashItem.Value, target =>
                    {
                        return _clientAccessor.With(targetInfo, client =>
                            {
                                var isValueEmpty = string.IsNullOrEmpty(target.Value);

                                if (!isValueEmpty)
                                {
                                    client.HSet(ustring.Make(key).ToString(), hashItem.Key, target.Value);
                                    editedHashItem.Value = target.Value;
                                }
                                
                                return new EditorResult
                                {
                                    ValueError = isValueEmpty ? "Value is empty" : null,
                                    Action = target.Action
                                };
                            });
                    }))
                .Select(wasEdited => wasEdited ? editedHashItem : null);
        }

        private IObservable<List<HashStructItemModel>> ExecuteDelete(
            byte[] key, RedisTargetInfo targetInfo, List<HashStructItemModel> items)
        {
            return _dialogManager.Open(new ConfirmDialogModel
                {
                    TitleText = "Delete hash entries?",
                    MessageText = "Do you want to delete hash entries with selected keys?",
                    ConfirmText = "Delete",
                    CancelText = "Cancel"
                })
                .SelectMany(confirmed =>
                {
                    if (!confirmed)
                    {
                        return Observable.Return(new List<HashStructItemModel>());
                    }

                    return _clientAccessor.With(targetInfo, client =>
                        {
                            var deletedItems = new List<HashStructItemModel>();
                            
                            client.Multi();
                            try
                            {
                                foreach (var item in items)
                                {
                                    if (item.IsChecked)
                                    {
                                        client.HDel(ustring.Make(key).ToString(), item.Key);
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

        private void CheckItem(HashStructItemModel item)
        {
            ChangeActionsMode();
        }
        
        private void EditItem(HashStructItemModel item)
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
            var count = _model.HashItems.Count(item => item.IsChecked);
            
            _model.AreActionsVisible = count > 0;
            _model.IsEditAvailable = count == 1;
            _model.IsDeleteAvailable = count > 0;
        }
    }
}