using System;

namespace Rbmk.Radish.Services.Redis.Projections
{
    public interface IStructProjector
    {
        IObservable<StructProjectionInfo> Project(
            ResultProjectionInfo resultProjectionInfo);
    }
}