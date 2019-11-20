using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSRedis;

namespace Rbmk.Radish.Services.Redis
{
    public abstract class RedisResultInfo
    {
        public RedisTargetInfo TargetInfo { get; private set; }
        
        public RedisCommandInfo CommandInfo { get; private set; }

        public class None : RedisResultInfo
        {
            public None(
                RedisTargetInfo targetInfo,
                RedisCommandInfo commandInfo)
            {
                TargetInfo = targetInfo;
                CommandInfo = commandInfo;
            }
        }
        
        public class Simple : RedisResultInfo
        {
            public string Status { get; }
            
            public Simple(
                RedisTargetInfo targetInfo,
                RedisCommandInfo commandInfo,
                string status)
            {
                TargetInfo = targetInfo;
                CommandInfo = commandInfo;
                Status = status;
            }
            
            public override string ToString()
            {
                return Status;
            }
        }
        
        public class Error : RedisResultInfo
        {
            public RedisException Exception { get; }
            
            public Error(
                RedisTargetInfo targetInfo,
                RedisCommandInfo commandInfo,
                RedisException exception)
            {
                TargetInfo = targetInfo;
                CommandInfo = commandInfo;
                Exception = exception;
            }

            public override string ToString()
            {
                return Exception.Message;
            }
        }
        
        public class Integer : RedisResultInfo
        {
            public long Value { get; }
            
            public Integer(
                RedisTargetInfo targetInfo,
                RedisCommandInfo commandInfo,
                long value)
            {
                TargetInfo = targetInfo;
                CommandInfo = commandInfo;
                Value = value;
            }

            public override string ToString()
            {
                return Value.ToString();
            }
        }
        
        public class Bulk : RedisResultInfo
        {
            public byte[] Bytes { get; }
            
            public Bulk(
                RedisTargetInfo targetInfo,
                RedisCommandInfo commandInfo,
                byte[] bytes)
            {
                TargetInfo = targetInfo;
                CommandInfo = commandInfo;
                Bytes = bytes;
            }

            public override string ToString()
            {
                return Encoding.UTF8.GetString(Bytes);
            }
        }
        
        public class MultiBulk : RedisResultInfo
        {
            public RedisResultInfo[] Array { get; }
            
            public MultiBulk(
                RedisTargetInfo targetInfo,
                RedisCommandInfo commandInfo,
                object[] array)
            {
                TargetInfo = targetInfo;
                CommandInfo = commandInfo;
                
                Array = array.Select(item => MapResult(targetInfo, commandInfo, item))
                    .ToArray();
            }

            public override string ToString()
            {
                return Array.ToString();
            }
        }
        
        public class TransactionBulk : RedisResultInfo
        {
            public RedisBatchInfo BatchInfo { get; }
            
            public RedisResultInfo[] Array { get; }
            
            public TransactionBulk(
                RedisTargetInfo targetInfo,
                RedisBatchInfo batchInfo,
                RedisCommandInfo commandInfo,
                object[] array)
            {
                TargetInfo = targetInfo;
                BatchInfo = batchInfo;
                CommandInfo = commandInfo;

                var commandInfos = batchInfo.CommandInfos.SkipWhile(c => !c.IsMulti)
                    .Skip(1)
                    .ToArray();
                
                Array = array.Select((item, i) =>
                        MapResult(targetInfo, i < commandInfos.Length ? commandInfos[i] : null, item))
                    .ToArray();
            }

            public override string ToString()
            {
                return Array.ToString();
            }
        }
        
        public static RedisResultInfo MapResult(
            RedisTargetInfo targetInfo,
            RedisCommandInfo commandInfo,
            object result)
        {
            switch (result)
            {
                case string str:
                    return new Simple(targetInfo, commandInfo, str);
                case long l:
                    return new Integer(targetInfo, commandInfo, l);
                case int i:
                    return new Integer(targetInfo, commandInfo, i);
                case byte[] bytes:
                    return new Bulk(targetInfo, commandInfo, bytes);
                case object[] array:
                    return new MultiBulk(targetInfo, commandInfo, array);
                default:
                    return new None(targetInfo, commandInfo);
            }
        }
        
        public static RedisResultInfo MapResult(
            RedisTargetInfo targetInfo,
            RedisBatchInfo batchInfo,
            RedisCommandInfo commandInfo,
            object result)
        {
            switch (result)
            {
                case string str:
                    return new Simple(targetInfo, commandInfo, str);
                case long l:
                    return new Integer(targetInfo, commandInfo, l);
                case int i:
                    return new Integer(targetInfo, commandInfo, i);
                case byte[] bytes:
                    return new Bulk(targetInfo, commandInfo, bytes);
                case object[] array:
                    return new TransactionBulk(targetInfo, batchInfo, commandInfo, array);
                default:
                    return new None(targetInfo, commandInfo);
            }
        }
    }
}