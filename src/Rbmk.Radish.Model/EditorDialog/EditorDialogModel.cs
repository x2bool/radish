using System;
using System.Globalization;
using System.Reactive;
using System.Reactive.Disposables;
using Rbmk.Radish.Model.Dialogs;
using ReactiveUI;

namespace Rbmk.Radish.Model.EditorDialog
{
    public class EditorDialogModel : DialogModel<bool>
    {
        public string TitleText { get; set; }
        
        public ReactiveCommand<Unit, Unit> CancelCommand { get; set; }
        
        public bool IsAdding { get; set; }
        
        public ReactiveCommand<Unit, EditorResult> AddCommand { get; set; }
        
        public bool IsSaving { get; set; }
        
        public ReactiveCommand<Unit, EditorResult> SaveCommand { get; set; }
        
        public bool IsPrepending { get; set; }
        
        public ReactiveCommand<Unit, EditorResult> PrependCommand { get; set; }
        
        public bool IsAppending { get; set; }
        
        public ReactiveCommand<Unit, EditorResult> AppendCommand { get; set; }
        
        public string Value { get; set; }
        
        public string ValueError { get; set; }
        
        public bool HasScore { get; set; }
        
        public string Score { get; set; }
        
        public string ScoreError { get; set; }
        
        public bool HasIndex { get; set; }
        
        public string Index { get; set; }
        
        public string IndexError { get; set; }
        
        public bool HasKey { get; set; }
        
        public string Key { get; set; }
        
        public string KeyError { get; set; }

        private EditorDialogModel(Func<EditorTarget, IObservable<EditorResult>> submit)
        {
            this.WhenActivated(disposables =>
            {
                var controller = new EditorDialogController(this);

                controller.BindFields()
                    .DisposeWith(disposables);
                
                controller.BindAddCommand(submit)
                    .DisposeWith(disposables);

                controller.BindSaveCommand(submit)
                    .DisposeWith(disposables);

                controller.BindPrependCommand(submit)
                    .DisposeWith(disposables);

                controller.BindCancelCommand()
                    .DisposeWith(disposables);

                controller.BindAppendCommand(submit)
                    .DisposeWith(disposables);
            });
        }

        /// <summary>
        /// For lists
        /// </summary>
        private EditorDialogModel(
            int index,
            string value,
            Func<EditorTarget, IObservable<EditorResult>> submit)
            : this(submit)
        {
            Index = index.ToString();
            Value = value;
        }

        /// <summary>
        /// For hashes
        /// </summary>
        private EditorDialogModel(
            string key,
            string value,
            Func<EditorTarget, IObservable<EditorResult>> submit)
            : this(submit)
        {
            Key = key;
            Value = value;
        }

        /// <summary>
        /// For sets
        /// </summary>
        private EditorDialogModel(
            string value,
            Func<EditorTarget, IObservable<EditorResult>> submit)
            : this(submit)
        {
            Value = value;
        }

        /// <summary>
        /// For sorted sets
        /// </summary>
        private EditorDialogModel(
            double score,
            string value,
            Func<EditorTarget, IObservable<EditorResult>> submit)
            : this(submit)
        {
            Score = score.ToString(CultureInfo.InvariantCulture);
            Value = value;
        }

        public static EditorDialogModel AddListItem(
            Func<EditorTarget, IObservable<EditorResult>> submit)
        {
            return new EditorDialogModel(submit)
            {
                TitleText = "Add item to list",
                IsAdding = false,
                IsSaving = false,
                IsPrepending = true,
                IsAppending = true,
                HasIndex = false,
                HasScore = false,
                HasKey = false
            };
        }

        public static EditorDialogModel UpdateListItem(
            int index,
            string value,
            Func<EditorTarget, IObservable<EditorResult>> submit)
        {
            return new EditorDialogModel(index, value, submit)
            {
                TitleText = "Update list item",
                IsAdding = false,
                IsSaving = true,
                IsPrepending = false,
                IsAppending = false,
                HasIndex = true,
                HasScore = false,
                HasKey = false
            };
        }

        public static EditorDialogModel AddHashItem(
            Func<EditorTarget, IObservable<EditorResult>> submit)
        {
            return new EditorDialogModel(submit)
            {
                TitleText = "Add item to hash",
                IsAdding = true,
                IsSaving = false,
                IsPrepending = false,
                IsAppending = false,
                HasIndex = false,
                HasScore = false,
                HasKey = true
            };
        }

        public static EditorDialogModel UpdateHashItem(
            string key, string value, Func<EditorTarget, IObservable<EditorResult>> submit)
        {
            return new EditorDialogModel(key, value, submit)
            {
                TitleText = "Update hash item",
                IsAdding = false,
                IsSaving = true,
                IsPrepending = false,
                IsAppending = false,
                HasIndex = false,
                HasScore = false,
                HasKey = true
            };
        }

        public static EditorDialogModel AddSetItem(
            Func<EditorTarget, IObservable<EditorResult>> submit)
        {
            return new EditorDialogModel(submit)
            {
                TitleText = "Add item to set",
                IsAdding = true,
                IsSaving = false,
                IsPrepending = false,
                IsAppending = false,
                HasIndex = false,
                HasScore = false,
                HasKey = false
            };
        }

        public static EditorDialogModel ReplaceSetItem(
            string value, Func<EditorTarget, IObservable<EditorResult>> submit)
        {
            return new EditorDialogModel(value, submit)
            {
                TitleText = "Update set item",
                IsAdding = false,
                IsSaving = true,
                IsPrepending = false,
                IsAppending = false,
                HasIndex = false,
                HasScore = false,
                HasKey = false
            };
        }

        public static EditorDialogModel AddZSetItem(
            Func<EditorTarget, IObservable<EditorResult>> submit)
        {
            return new EditorDialogModel(submit)
            {
                TitleText = "Add item to sorted set",
                IsAdding = true,
                IsSaving = false,
                IsPrepending = false,
                IsAppending = false,
                HasIndex = false,
                HasScore = true,
                HasKey = false
            };
        }

        public static EditorDialogModel ReplaceZSetItem(
            double score, string value, Func<EditorTarget, IObservable<EditorResult>> submit)
        {
            return new EditorDialogModel(score, value, submit)
            {
                TitleText = "Update sorted set item",
                IsAdding = false,
                IsSaving = true,
                IsPrepending = false,
                IsAppending = false,
                HasIndex = false,
                HasScore = true,
                HasKey = false
            };
        }
    }
}