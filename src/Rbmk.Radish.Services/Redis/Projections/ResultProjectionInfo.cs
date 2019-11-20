using System;
using System.Collections.Generic;
using System.Text;

namespace Rbmk.Radish.Services.Redis.Projections
{
    public class ResultProjectionInfo
    {
        public RedisResultInfo Result { get; }
        
        public ResultProjectionKind Kind { get; }
        
        public byte[] Value { get; }
        
        public int? Index { get; set; }
        
        public List<ResultProjectionInfo> Children { get; }
            = new List<ResultProjectionInfo>();
        
        public ResultProjectionInfo(
            RedisResultInfo result,
            ResultProjectionKind kind,
            byte[] value,
            int? index = null)
        {
            Result = result;
            Kind = kind;
            Value = value;
            Index = index;
        }
        
        public ResultProjectionInfo(
            RedisResultInfo result,
            ResultProjectionKind kind,
            string value,
            int? index = null)
            : this(result, kind, Encoding.UTF8.GetBytes(value), index)
        {
        }
        
        public ResultProjectionInfo(
            RedisResultInfo result,
            ResultProjectionKind kind,
            long value,
            int? index = null)
            : this(result, kind, value.ToString(), index)
        {
        }

        public ResultProjectionInfo(
            RedisResultInfo result,
            ResultProjectionKind kind,
            int? index = null)
            : this(result, kind, default(byte[]), index)
        {
        }
    }
}