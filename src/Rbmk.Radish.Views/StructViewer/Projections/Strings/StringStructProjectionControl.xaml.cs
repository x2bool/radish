using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaEdit;
using Rbmk.Radish.Model.StructViewer.Projections.Strings;
using Rbmk.Utils.Reactive;
using ReactiveUI;

namespace Rbmk.Radish.Views.StructViewer.Projections.Strings
{
    public class StringStructProjectionControl : BaseControl<StringStructProjectionModel>
    {
        private readonly TextEditor _editor;

        public StringStructProjectionControl()
            : base(false)
        {
            AvaloniaXamlLoader.Load(this);
            
            _editor = this.FindControl<TextEditor>("ValueEditor");
            
            this.WhenActivated(disposables =>
            {
                ViewModel.WhenAnyValue(m => m.ValueText)
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
                        ViewModel.ValueText = text;
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