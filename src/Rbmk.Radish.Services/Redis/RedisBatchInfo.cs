using System;
using System.Linq;
using Rbmk.Utils.Linq;

namespace Rbmk.Radish.Services.Redis
{
    public class RedisBatchInfo
    {
        public Guid Id { get; }
        
        public RedisCommandInfo[] CommandInfos { get; }

        public RedisBatchInfo(
            Guid id,
            params RedisCommandInfo[] commandInfos)
        {
            Id = id;
            CommandInfos = commandInfos;
        }

        public RedisBatchInfo(params RedisCommandInfo[] commandInfos)
            : this(
                Guid.NewGuid(),
                commandInfos)
        {
        }

        public RedisBatchInfo()
            : this(new RedisCommandInfo[0])
        {
        }

        public bool IsTransaction
        {
            get
            {
                var multi = CommandInfos.FirstOrDefault(c => c.IsMulti);
                if (multi == null)
                {
                    var exec = CommandInfos.FirstOrDefault(c => c.IsExec);
                    if (exec == null)
                    {
                        var discard = CommandInfos.FirstOrDefault(c => c.IsDiscard);
                        if (discard == null)
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
        }

        public bool IsValid
        {
            get
            {
                if (IsTransaction)
                {
                    var multiIndex = CommandInfos.FirstIndexOf(c => c.IsMulti);
                    var execIndex = CommandInfos.FirstIndexOf(c => c.IsExec);
                    var discardIndex = CommandInfos.FirstIndexOf(c => c.IsDiscard);

                    // multi before exec or before discard
                    if (multiIndex < execIndex || multiIndex < discardIndex)
                    {
                        // all watch commands before multi
                        var transactionWatches = CommandInfos.AllIndexesOf(c => c.IsWatch);
                        if (transactionWatches.All(i => i < multiIndex))
                        {
                            // all unwatch commands after exec or discard
                            var transactionUnwatches = CommandInfos.AllIndexesOf(c => c.IsUnwatch);
                            if (transactionUnwatches.All(i => i > execIndex || i > discardIndex))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                var watches = CommandInfos.AllIndexesOf(c => c.IsWatch);
                var unwatches = CommandInfos.AllIndexesOf(c => c.IsUnwatch);

                if (watches.Length > 0)
                {
                    // if there is watches then unwatches also should be present
                    if (unwatches.Length > 0)
                    {
                        // any sequence if watches should be followed by unwatch command
                        if (unwatches.Max() > watches.Max())
                        {
                            return true;
                        }
                    }

                    return false;
                }

                return true;
            }
        }
    }
}