using System;
using System.Reactive.Linq;
using System.Text;
using Rbmk.Radish.Services.Redis.Connections;

namespace Rbmk.Radish.Services.Redis.Projections
{
    public class StructProjector : IStructProjector
    {
        private readonly IClientAccessor _clientAccessor;

        public StructProjector(
            IClientAccessor clientAccessor)
        {
            _clientAccessor = clientAccessor;
        }
        
        public IObservable<StructProjectionInfo> Project(
            ResultProjectionInfo resultProjectionInfo)
        {
            if (resultProjectionInfo == null)
            {
                return Observable.Return(
                    new StructProjectionInfo(null, StructProjectionKind.None));
            }

            var key = Encoding.UTF8.GetString(resultProjectionInfo.Value);
            
            return _clientAccessor.With(
                    resultProjectionInfo.Result.TargetInfo,
                    client => client.Type(key))
                .Select(type =>
                {
                    switch (type)
                    {
                        case "string":
                            return new StructProjectionInfo(
                                resultProjectionInfo, StructProjectionKind.String);
                        case "list":
                            return new StructProjectionInfo(
                                resultProjectionInfo, StructProjectionKind.List);
                        case "set":
                            return new StructProjectionInfo(
                                resultProjectionInfo, StructProjectionKind.Set);
                        case "zset":
                            return new StructProjectionInfo(
                                resultProjectionInfo, StructProjectionKind.ZSet);
                        case "hash":
                            return new StructProjectionInfo(
                                resultProjectionInfo, StructProjectionKind.Hash);
                    }

                    return new StructProjectionInfo(
                        resultProjectionInfo, StructProjectionKind.None);
                });
        }
    }
}