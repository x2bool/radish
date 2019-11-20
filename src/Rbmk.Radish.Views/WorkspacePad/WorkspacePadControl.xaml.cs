using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Rbmk.Radish.Model.WorkspacePad;
using Rbmk.Utils.Reactive;
using ReactiveUI;

namespace Rbmk.Radish.Views.WorkspacePad
{
    public class WorkspacePadControl : BaseControl<WorkspacePadModel>
    {
        private readonly ScrollViewer _tabScroller;
        
        public WorkspacePadControl()
            : base(false)
        {
            AvaloniaXamlLoader.Load(this);

            _tabScroller = this.FindControl<ScrollViewer>("TabScroller");

            this.WhenActivated(disposables =>
            {
                _tabScroller.WhenAnyValue(s => s.Extent)
                    .Merge(_tabScroller.WhenAnyValue(s => s.Viewport))
                    .SubscribeWithLog(_ =>
                        {
                            if (ViewModel != null)
                            {
                                ViewModel.IsTabStripScrollable =
                                    _tabScroller.Extent.Width > _tabScroller.Viewport.Width;
                            }
                        })
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.ViewModel)
                    .Where(m => m != null)
                    .SelectMany(m => m.ScrollLeftCommand)
                    .SubscribeWithLog(_ =>
                    {
                        _tabScroller.Offset -= new Vector(50, 0);
                    })
                    .DisposeWith(disposables);
                
                this.WhenAnyValue(x => x.ViewModel)
                    .Where(m => m != null)
                    .SelectMany(m => m.ScrollRightCommand)
                    .SubscribeWithLog(_ =>
                    {
                        _tabScroller.Offset += new Vector(50, 0);
                    })
                    .DisposeWith(disposables);
            });
        }
    }
}