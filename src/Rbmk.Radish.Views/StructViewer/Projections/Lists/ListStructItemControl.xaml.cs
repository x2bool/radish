using System;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Rbmk.Radish.Model.StructViewer.Projections.Lists;
using Rbmk.Utils.Reactive;
using ReactiveUI;

namespace Rbmk.Radish.Views.StructViewer.Projections.Lists
{
    public class ListStructItemControl : BaseControl<ListStructItemModel>
    {
        private readonly TextBox _indexInput;
        private readonly TextBox _valueInput;

        private readonly IDisposable _indexActivationSubscription;
        private readonly IDisposable _valueActivationSubscription;

        public ListStructItemControl()
        {
            AvaloniaXamlLoader.Load(this);

            _indexInput = this.FindControl<TextBox>("IndexInput");
            _indexActivationSubscription = SubscribeForActivation(_indexInput);
            
            _valueInput = this.FindControl<TextBox>("ValueInput");
            _valueActivationSubscription = SubscribeForActivation(_valueInput);
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            _indexActivationSubscription.Dispose();
            _valueActivationSubscription.Dispose();
            
            base.OnDetachedFromVisualTree(e);
        }

        private IDisposable SubscribeForActivation(TextBox textBox)
        {
            var focusObservable = Observable.FromEventPattern<GotFocusEventArgs>(
                h => textBox.GotFocus += h, h => textBox.GotFocus -= h);

            return focusObservable
                .Throttle(TimeSpan.FromMilliseconds(200))
                .ObserveOn(RxApp.MainThreadScheduler)
                .SubscribeWithLog(_ =>
                {
                    textBox.CaretIndex = 0;
                    textBox.SelectionStart = 0;
                    textBox.SelectionEnd = 0;
                    
                    Focus();

                    ViewModel.EditAction?.Invoke(ViewModel);
                });
        }
    }
}