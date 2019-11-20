using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaEdit;
using Rbmk.Radish.Model.Dialogs;
using Rbmk.Radish.Model.EditorDialog;
using Rbmk.Utils.Reactive;
using ReactiveUI;

namespace Rbmk.Radish.Views.EditorDialog
{
    [DialogWindow(typeof(EditorDialogModel))]
    public class EditorDialogWindow : BaseWindow<EditorDialogModel>
    {
        private readonly TextEditor _keyEditor;
        private readonly TextEditor _valueEditor;
        private bool _resultWasSet;
        
        public EditorDialogWindow()
            : base(false)
        {
            AvaloniaXamlLoader.Load(this);
            
            _keyEditor = this.FindControl<TextEditor>("KeyEditor");
            _valueEditor = this.FindControl<TextEditor>("ValueEditor");

            this.WhenActivated(disposables =>
            {
                ViewModel.GetResult()
                    .SubscribeWithLog(_ =>
                    {
                        _resultWasSet = true;
                        Close();
                    });
                
                ViewModel.WhenAnyValue(m => m.Key)
                    .Select(text => text ?? "")
                    .SubscribeWithLog(text =>
                    {
                        _keyEditor.Document.Text = text;
                    })
                    .DisposeWith(disposables);
                
                GetTextChanges(_keyEditor)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .SubscribeWithLog(text =>
                    {
                        ViewModel.Key = text;
                    })
                    .DisposeWith(disposables);
                
                ViewModel.WhenAnyValue(m => m.Value)
                    .Select(text => text ?? "")
                    .SubscribeWithLog(text =>
                    {
                        _valueEditor.Document.Text = text;
                    })
                    .DisposeWith(disposables);
                
                GetTextChanges(_valueEditor)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .SubscribeWithLog(text =>
                    {
                        ViewModel.Value = text;
                    })
                    .DisposeWith(disposables);
            });
        }

        protected override void HandleClosed()
        {
            if (ViewModel != null && !_resultWasSet)
            {
                ViewModel.Close(false);
            }
            
            base.HandleClosed();
        }

        private IObservable<string> GetTextChanges(TextEditor editor)
        {
            return Observable.FromEventPattern<EventHandler, EventArgs>(
                    h => editor.Document.TextChanged += h,
                    h => editor.Document.TextChanged -= h)
                .Select(_ => editor.Document.Text);
        }
    }
}
