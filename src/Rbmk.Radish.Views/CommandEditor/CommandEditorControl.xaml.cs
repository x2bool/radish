using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using AvaloniaEdit;
using Rbmk.Radish.Model.CommandEditor;
using Rbmk.Radish.Views.Highlighting;
using Rbmk.Utils.Reactive;
using ReactiveUI;

namespace Rbmk.Radish.Views.CommandEditor
{
    public class CommandEditorControl : BaseControl<CommandEditorModel>
    {
        private readonly TextEditor _editor;
        private readonly AutoCompleteBox _autoComplete;

        public CommandEditorControl()
            : base(false)
        {
            AvaloniaXamlLoader.Load(this);

            _editor = this.FindControl<TextEditor>("CommandEditor");
            _autoComplete = this.FindControl<AutoCompleteBox>("QuickAccess");
            
            _autoComplete.TextFilter = (search, item) => !string.IsNullOrEmpty(item);

            this.WhenActivated(disposables =>
            {
                Observable.FromEventPattern<TemplateAppliedEventArgs>(
                    h => _autoComplete.TemplateApplied += h, h => _autoComplete.TemplateApplied -= h)
                    .SelectMany(e =>
                    {
                        var textBox = e.EventArgs.NameScope.Find<TextBox>("PART_TextBox");
                        return textBox.WhenAnyValue(t => t.Text);
                    })
                    .SubscribeWithLog(text =>
                    {
                        ViewModel.QuickAccessText = text;
                    })
                    .DisposeWith(disposables);

                ViewModel.WhenAnyValue(m => m.CommandText)
                    .Select(text => text ?? "")
                    .SubscribeWithLog(text =>
                    {
                        _editor.Document.Text = text;
                    })
                    .DisposeWith(disposables);
                
                GetTextChanges()
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .SubscribeWithLog(text =>
                    {
                        ViewModel.CommandText = text;
                    })
                    .DisposeWith(disposables);

                ViewModel.WhenAnyValue(m => m.Highlighting)
                    .SelectSeq(HighlightingSyntaxLoader.Load)
                    .SubscribeWithLog(h =>
                    {
                        _editor.SyntaxHighlighting = h;
                    })
                    .DisposeWith(disposables);
            });
        }

        private IObservable<string> GetTextChanges()
        {
            return Observable.FromEventPattern<EventHandler, EventArgs>(
                h => _editor.Document.TextChanged += h,
                h => _editor.Document.TextChanged -= h)
                .Select(_ => _editor.Document.Text);
        }
    }
}