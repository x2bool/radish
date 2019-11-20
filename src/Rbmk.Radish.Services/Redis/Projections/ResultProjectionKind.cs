namespace Rbmk.Radish.Services.Redis.Projections
{
    public enum ResultProjectionKind
    {
        None,
        Error,
        Status,
        Key,
        SKey,
        HKey,
        ZKey,
        Value,
        Array,
        Transaction
    }
}