using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rbmk.Radish.Services.Redis.Projections
{
    public class ResultProjector : IResultProjector
    {
        public IEnumerable<ResultProjectionInfo> Project(RedisResultInfo[] resultInfos)
        {
            foreach (var resultInfo in resultInfos)
            {
                switch (resultInfo)
                {
                    case RedisResultInfo.None none:
                        yield return ProjectNoneInfo(none);
                        break;
                    
                    case RedisResultInfo.Simple simple:
                        yield return ProjectSimpleInfo(simple);
                        break;
                    
                    case RedisResultInfo.Error error:
                        yield return ProjectErrorInfo(error);
                        break;
                    
                    case RedisResultInfo.Integer integer:
                        yield return ProjectIntegerInfo(integer);
                        break;
                    
                    case RedisResultInfo.Bulk bulk:
                        yield return ProjectBulkInfo(bulk);
                        break;
                    
                    case RedisResultInfo.MultiBulk multiBulk:
                        yield return ProjectMultiBulkInfo(multiBulk);
                        break;
                    
                    case RedisResultInfo.TransactionBulk transactionBulk:
                        yield return ProjectTransactionBulkInfo(transactionBulk);
                        break;
                }
            }
        }

        private ResultProjectionInfo ProjectNoneInfo(
            RedisResultInfo.None none)
        {
            return new ResultProjectionInfo(
                none,
                ResultProjectionKind.Value);
        }

        private ResultProjectionInfo ProjectSimpleInfo(
            RedisResultInfo.Simple simple)
        {
            return new ResultProjectionInfo(
                simple,
                ResultProjectionKind.Status,
                simple.Status);
        }

        private ResultProjectionInfo ProjectErrorInfo(
            RedisResultInfo.Error error)
        {
            return new ResultProjectionInfo(
                error,
                ResultProjectionKind.Error,
                error.Exception.Message);
        }

        private ResultProjectionInfo ProjectIntegerInfo(
            RedisResultInfo.Integer integer)
        {
            return new ResultProjectionInfo(
                integer,
                ResultProjectionKind.Value,
                integer.Value);
        }

        private ResultProjectionInfo ProjectBulkInfo(
            RedisResultInfo.Bulk bulk,
            ResultProjectionKind kind = ResultProjectionKind.None)
        {
            if (kind == ResultProjectionKind.None)
            {
                var commandInfo = bulk.CommandInfo;
                var commandName = commandInfo?.Name?.ToLowerInvariant();

                switch (commandName)
                {
                    // Strings
                    case "set":
                    case "mset":
                        kind = ResultProjectionKind.Status;
                        break;
                
                    case "get":
                        kind = ResultProjectionKind.Value;
                        break;
                
                    // Lists
                    case "lindex":
                    case "linsert":
                    case "llen":
                    case "lpop":
                    case "lpush":
                    case "lpushx":
                    case "lrem":
                    case "rpop":
                    case "rpoplpush":
                    case "rpush":
                    case "rpushx":
                        kind = ResultProjectionKind.Value;
                        break;
                    
                    case "lset":
                    case "ltrim":
                        kind = ResultProjectionKind.Status;
                        break;
                
                    // Hashes
                    case "hget":
                    case "hset":
                        kind = ResultProjectionKind.Value;
                        break;
                
                    // Sets
                    case "sadd":
                    case "srem":
                        kind = ResultProjectionKind.Value;
                        break;
                
                    // Sorted Sets
                    case "zadd":
                    case "zrem":
                        kind = ResultProjectionKind.Value;
                        break;
                    
                    default:
                        kind = ResultProjectionKind.Value;
                        break;
                }
            }
            
            return new ResultProjectionInfo(
                bulk,
                kind,
                bulk.Bytes);
        }

        private ResultProjectionInfo ProjectMultiBulkInfo(
            RedisResultInfo.MultiBulk multiBulk)
        {
            var projectionInfo = new ResultProjectionInfo(
                multiBulk,
                ResultProjectionKind.Array);

            var commandInfo = multiBulk.CommandInfo;
            var commandName = commandInfo?.Name?.ToLowerInvariant();

            switch (commandName)
            {
                case "keys":
                    projectionInfo.Children.AddRange(
                        Project(multiBulk.Array)
                            .Select((p, i) =>
                            {
                                switch (p.Kind)
                                {
                                    case ResultProjectionKind.Value:
                                        p = ProjectBulkInfo(
                                            p.Result as RedisResultInfo.Bulk,
                                            ResultProjectionKind.Key);
                                        p.Index = i;
                                        return p;
                                    default:
                                        return p;
                                }
                            }));
                    break;
                
                case "scan":
                    projectionInfo.Children.AddRange(
                        Project(multiBulk.Array)
                            .Select(p =>
                            {
                                switch (p.Kind)
                                {
                                    case ResultProjectionKind.Array:
                                        for (int i = 0; i < p.Children.Count; i++)
                                        {
                                            p.Children[i] = ProjectBulkInfo(
                                                p.Children[i].Result as RedisResultInfo.Bulk,
                                                ResultProjectionKind.Key);
                                            p.Children[i].Index = i;
                                        }
                                        return p;
                                    default:
                                        return p;
                                }
                            }));
                    break;
                
                case "sscan":
                    projectionInfo.Children.AddRange(
                        Project(multiBulk.Array)
                            .Select(p =>
                            {
                                switch (p.Kind)
                                {
                                    case ResultProjectionKind.Array:
                                        for (int i = 0; i < p.Children.Count; i++)
                                        {
                                            p.Children[i] = ProjectBulkInfo(
                                                p.Children[i].Result as RedisResultInfo.Bulk,
                                                ResultProjectionKind.SKey);
                                            p.Children[i].Index = i;
                                        }
                                        return p;
                                    default:
                                        return p;
                                }
                            }));
                    break;
                
                case "hscan":
                    projectionInfo.Children.AddRange(
                        Project(multiBulk.Array)
                            .Select(p =>
                            {
                                switch (p.Kind)
                                {
                                    case ResultProjectionKind.Array:
                                        for (int i = 0; i < p.Children.Count; i++)
                                        {
                                            p.Children[i] = ProjectBulkInfo(
                                                p.Children[i].Result as RedisResultInfo.Bulk,
                                                i % 2 == 0
                                                    ? ResultProjectionKind.HKey
                                                    : ResultProjectionKind.Value);
                                            p.Children[i].Index = i;
                                        }
                                        return p;
                                    default:
                                        return p;
                                }
                            }));
                    break;
                
                case "zscan":
                    projectionInfo.Children.AddRange(
                        Project(multiBulk.Array)
                            .Select(p =>
                            {
                                switch (p.Kind)
                                {
                                    case ResultProjectionKind.Array:
                                        for (int i = 0; i < p.Children.Count; i++)
                                        {
                                            p.Children[i] = ProjectBulkInfo(
                                                p.Children[i].Result as RedisResultInfo.Bulk,
                                                i % 2 == 0
                                                    ? ResultProjectionKind.ZKey
                                                    : ResultProjectionKind.Value);
                                            p.Children[i].Index = i;
                                        }
                                        return p;
                                    default:
                                        return p;
                                }
                            }));
                    break;

                default:
                    projectionInfo.Children.AddRange(
                        Project(multiBulk.Array));
                    break;
            }

            for (int i = 0; i < projectionInfo.Children.Count; i++)
            {
                projectionInfo.Children[i].Index = i;
            }
            
            return projectionInfo;
        }

        private ResultProjectionInfo ProjectTransactionBulkInfo(
            RedisResultInfo.TransactionBulk transactionBulk)
        {
            var projectionInfo = new ResultProjectionInfo(
                transactionBulk,
                ResultProjectionKind.Transaction);

            projectionInfo.Children.AddRange(
                Project(transactionBulk.Array));
            
            return projectionInfo;
        }
    }
}