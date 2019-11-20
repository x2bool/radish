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

namespace Rbmk.Radish.Model.StructViewer.Projections.Lists
{
    public class ListStructProjectionController
    {
        private readonly IDialogManager _dialogManager;
        private readonly IClientAccessor _clientAccessor;
        
        private readonly ListStructProjectionModel _model;
        
        public ListStructProjectionController(
            ListStructProjectionModel model)
            : this (
                Locator.Current.GetService<IDialogManager>(),
                Locator.Current.GetService<IClientAccessor>())
        {
            _model = model;
        }

        private ListStructProjectionController(
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

        public IDisposable BindStartIndex()
        {
            return _model.WhenAnyValue(m => m.StartIndex)
                .DistinctUntilChanged()
                .SubscribeWithLog(str =>
                {
                    int.TryParse(_model.StartIndex, out var startIndex);
                    int.TryParse(_model.StopIndex, out var stopIndex);

                    _model.Range = Tuple.Create(startIndex, stopIndex);
                });
        }

        public IDisposable BindStopIndex()
        {
            return _model.WhenAnyValue(m => m.StopIndex)
                .DistinctUntilChanged()
                .SubscribeWithLog(str =>
                {
                    int.TryParse(_model.StartIndex, out var startIndex);
                    int.TryParse(_model.StopIndex, out var stopIndex);

                    _model.Range = Tuple.Create(startIndex, stopIndex);
                });
        }

        public IDisposable BindRange()
        {
            return _model.WhenAnyValue(m => m.Range)
                .SubscribeWithLog(range =>
                {
                    _model.StartIndex = range.Item1.ToString();
                    _model.StopIndex = range.Item2.ToString();
                });
        }

        public IDisposable BindRangeCommand(byte[] key, RedisTargetInfo targetInfo)
        {
            _model.RangeCommand = ReactiveCommand.CreateFromObservable<Tuple<int, int>, List<ListStructItemModel>>(
                range => ExecuteRange(key, targetInfo, range.Item1, range.Item2),
                null,
                RxApp.MainThreadScheduler);

            return _model.RangeCommand
                .SubscribeWithLog(listItems =>
                {
                    ChangeFlags();
                    
                    _model.ListItems.Clear();
                    _model.ListItems.AddRange(listItems);
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
                        ExecuteRangeCommand();
                        UpdateBadge(key, targetInfo);
                    }
                });
        }

        public IDisposable BindDeleteCommand(byte[] key, RedisTargetInfo targetInfo)
        {
            _model.DeleteCommand = ReactiveCommand.CreateFromObservable<List<ListStructItemModel>, List<ListStructItemModel>>(
                listItems => ExecuteDelete(key, targetInfo, listItems ?? _model.ListItems.ToList()),
                _model.WhenAnyValue(m => m.IsDeleteAvailable),
                RxApp.MainThreadScheduler);

            return _model.DeleteCommand
                .SubscribeWithLog(deletedItems =>
                {
                    if (deletedItems.Count > 0)
                    {
                        ExecuteRangeCommand();
                        UpdateBadge(key, targetInfo);
                    }
                });
        }

        public IDisposable BindEditCommand(byte[] key, RedisTargetInfo targetInfo)
        {
            _model.EditCommand = ReactiveCommand.CreateFromObservable<ListStructItemModel, ListStructItemModel>(
                listItem => ExecuteEdit(key, targetInfo, listItem ?? _model.ListItems.First(item => item.IsChecked)),
                _model.WhenAnyValue(m => m.IsEditAvailable),
                RxApp.MainThreadScheduler);

            return _model.EditCommand
                .SubscribeWithLog(editedListItem =>
                {
                    if (editedListItem != null)
                    {
                        var index = _model.ListItems
                            .Select((item, i) => new { Item = item, Index = i })
                            .First(tuple => tuple.Item.Index == editedListItem.Index)
                            .Index;

                        editedListItem.IsChecked = false;
                        _model.ListItems[index] = editedListItem;
                    }
                });
        }

        public IDisposable BindCancelCommand()
        {
            _model.CancelCommand = ReactiveCommand.Create(
                () => _model.ListItems.ToList(), null, RxApp.MainThreadScheduler);

            return _model.CancelCommand
                .SubscribeWithLog(listItems =>
                {
                    foreach (var listItem in listItems)
                    {
                        listItem.IsChecked = false;
                    }
                });
        }

        public IDisposable ExecuteRangeCommand()
        {
            return _model.RangeCommand.Execute(_model.Range)
                .SubscribeOn(RxApp.TaskpoolScheduler)
                .ObserveOn(RxApp.MainThreadScheduler)
                .SubscribeWithLog();
        }

        private IDisposable UpdateBadge(byte[] key, RedisTargetInfo targetInfo)
        {
            return _clientAccessor.With(targetInfo, client => client.LLen(ustring.Make(key).ToString())) // TODO: async
                .SubscribeWithLog(len =>
                {
                    _model.BadgeText = $"List ({len})";
                });
        }

        private IObservable<List<ListStructItemModel>> ExecuteRange(
            byte[] key, RedisTargetInfo targetInfo, int startIndex, int stopIndex)
        {
            return _clientAccessor.With(targetInfo, client => client.LRange(ustring.Make(key).ToString(), startIndex, stopIndex)) // TODO: async
                .Select(list =>
                {
                    var items = new List<ListStructItemModel>();

                    for (
                        int i = 0, j = startIndex;
                        i < list.Length && j <= stopIndex;
                        i++, j++)
                    {
                        items.Add(new ListStructItemModel
                        {
                            Index = j,
                            Value = list[i],
                            IsChecked = false,
                            IsEnabled = true,
                            CheckAction = CheckItem,
                            EditAction = EditItem
                        });
                    }

                    return items;
                });
        }

        private IObservable<bool> ExecuteAdd(
            byte[] key, RedisTargetInfo targetInfo)
        {
            return _dialogManager.Open(EditorDialogModel.AddListItem(target =>
                {
                    return _clientAccessor.With(targetInfo, client =>
                        {
                            var isValueEmpty = string.IsNullOrEmpty(target.Value);

                            if (!isValueEmpty)
                            {
                                switch (target.Action)
                                {
                                    case EditorAction.Prepend:
                                        client.LPush(ustring.Make(key).ToString(), target.Value); // TODO: async
                                        break;
                                    
                                    case EditorAction.Append:
                                        client.RPush(ustring.Make(key).ToString(), target.Value); // TODO: async
                                        break;
                                }
                            }
                            
                            return new EditorResult
                            {
                                ValueError = isValueEmpty ? "Value is empty" : null,
                                Action = target.Action
                            };
                        });
                }));
        }

        private IObservable<ListStructItemModel> ExecuteEdit(
            byte[] key, RedisTargetInfo targetInfo, ListStructItemModel listItem)
        {
            var editedListItem = new ListStructItemModel
            {
                Index = listItem.Index,
                Value = listItem.Value,
                IsChecked = listItem.IsChecked,
                IsEnabled = listItem.IsEnabled,
                CheckAction = listItem.CheckAction,
                EditAction = listItem.EditAction
            };
            
            return _dialogManager.Open(EditorDialogModel.UpdateListItem(
                    listItem.Index, listItem.Value, target =>
                {
                    return _clientAccessor.With(targetInfo, client =>
                        {
                            var isValueEmpty = string.IsNullOrEmpty(target.Value);

                            if (!isValueEmpty)
                            {
                                client.LSet(ustring.Make(key).ToString(), listItem.Index, target.Value);
                                editedListItem.Value = target.Value;
                            }
                                
                            return new EditorResult
                            {
                                ValueError = isValueEmpty ? "Value is empty" : null,
                                Action = target.Action
                            };
                        });
                }))
                .Select(wasEdited => wasEdited ? editedListItem : null);
        }

        private IObservable<List<ListStructItemModel>> ExecuteDelete(
            byte[] key, RedisTargetInfo targetInfo, List<ListStructItemModel> items)
        {
            return _dialogManager.Open(new ConfirmDialogModel
                {
                    TitleText = "Delete list entries?",
                    MessageText = "Do you want to delete list entries at selected positions?",
                    ConfirmText = "Delete",
                    CancelText = "Cancel"
                })
                .SelectMany(confirmed =>
                {
                    if (!confirmed)
                    {
                        return Observable.Return(new List<ListStructItemModel>());
                    }

                    return _clientAccessor.With(targetInfo, client =>
                        {
                            var deletedItems = new List<ListStructItemModel>();
                            
                            client.Multi();
                            try
                            {
                                // TODO: check correctness of this approach
                                var temp = "$" + Guid.NewGuid().ToString("N");

                                // from bigger indexes to smaller
                                for (int i = items.Count - 1; i >= 0; i--)
                                {
                                    var item = items[i];

                                    if (item.IsChecked)
                                    {
                                        client.LSet(ustring.Make(key).ToString(), item.Index, temp);
                                        deletedItems.Add(item);
                                    }
                                }

                                // remove all with temp value
                                client.LRem(ustring.Make(key).ToString(), 0, temp);

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

        private void CheckItem(ListStructItemModel item)
        {
            ChangeFlags();
        }
        
        private void EditItem(ListStructItemModel item)
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
        
        private void ChangeFlags()
        {
            var count = _model.ListItems.Count(item => item.IsChecked);
            
            _model.AreActionsVisible = count > 0;
            _model.IsEditAvailable = count == 1;
            _model.IsDeleteAvailable = count > 0;
        }
    }
}