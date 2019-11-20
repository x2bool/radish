using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using NStack;
using Rbmk.Radish.Services.Redis;
using Rbmk.Radish.Services.Redis.Connections;
using Rbmk.Utils.Reactive;
using ReactiveUI;
using Splat;

namespace Rbmk.Radish.Model.StructViewer.Projections.Strings
{
    public class StringStructProjectionController
    {
        private readonly IClientAccessor _clientAccessor;
        private readonly StringStructProjectionModel _model;

        public StringStructProjectionController(
            StringStructProjectionModel model)
            : this(
                Locator.Current.GetService<IClientAccessor>())
        {
            _model = model;
        }

        private StringStructProjectionController(
            IClientAccessor clientAccessor)
        {
            _clientAccessor = clientAccessor;
        }

        public IDisposable BindMimeTypes()
        {
            _model.MimeTypes.Clear();
        
            _model.MimeTypes.Add(new MimeTypeItem
            {
                Name = "text/plain"
            });
            
            _model.MimeTypes.Add(new MimeTypeItem
            {
                Name = "text/json"
            });
            
            _model.MimeTypes.Add(new MimeTypeItem
            {
                Name = "text/xml"
            });

            _model.SelectedMimeType = _model.MimeTypes.FirstOrDefault();
            
            return Disposable.Empty;
        }

        public IDisposable BindLength(byte[] key, RedisTargetInfo targetInfo)
        {
            _model.BadgeText = "String";
            
            return _clientAccessor.With(targetInfo, client => client.StrLen(ustring.Make(key).ToString())) // TODO: async
                .SubscribeWithLog(len =>
                {
                    _model.BadgeText = $"String ({len})";
                });
        }

        public IDisposable BindValue(byte[] key, RedisTargetInfo targetInfo)
        {
            return GetValue(targetInfo, key)
                .ObserveOn(RxApp.MainThreadScheduler)
                .SubscribeWithLog(val =>
                {
                    _model.ValueText = val;
                });
        }

        public IDisposable BindSaveCommand(byte[] key, RedisTargetInfo targetInfo)
        {   
            _model.SaveCommand = ReactiveCommand.CreateFromObservable<string, Unit>(
                text => SetValue(targetInfo, key, text),
                null,
                RxApp.MainThreadScheduler);

            return _model.SaveCommand
                .SubscribeWithLog();
        }

        private IObservable<string> GetValue(
            RedisTargetInfo targetInfo, byte[] key)
        {
            return _clientAccessor.With(targetInfo, client => client.Get(ustring.Make(key).ToString())); // TODO: async
        }

        private IObservable<Unit> SetValue(
            RedisTargetInfo targetInfo, byte[] key, string text)
        {
            return _clientAccessor.With(targetInfo, client => client.Set(ustring.Make(key).ToString(), text))
                .Select(_ => Unit.Default); // TODO: async
        }
    }
}