namespace Rbmk.Radish.Services.Redis.Projections
{
    public class StructProjectionInfo
    {
        public ResultProjectionInfo ResultProjectionInfo { get; }
        
        public StructProjectionKind Kind { get; }

        public StructProjectionInfo(
            ResultProjectionInfo resultProjectionInfo,
            StructProjectionKind kind)
        {
            ResultProjectionInfo = resultProjectionInfo;
            Kind = kind;
        }
    }
}