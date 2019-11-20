using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading.Tasks;

namespace Rbmk.Radish.Services.Redis.Parser
{
    public class CommandParser : ICommandParser
    {
        public IObservable<RedisBatchInfo> Parse(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return Observable.Return(new RedisBatchInfo());
            }
            
            return ParseCommands(text)
                .ToArray()
                .Select(array => new RedisBatchInfo(array));
        }

        private IObservable<RedisCommandInfo> ParseCommands(string text)
        {
            return Observable.Create<RedisCommandInfo>(observer =>
            {
                return Task.Run(() =>
                {
                    var isEscaped = false;
                    var isQuoted = false;
                    
                    var currentRun = new StringBuilder();
                    var currentLine = new List<string>();

                    foreach (var ch in text)
                    {
                        switch (ch)
                        {
                            case '\r':
                                break;

                            case '\n':
                            {
                                if (isEscaped)
                                {
                                    currentRun.Append('\\');
                                    isEscaped = false;
                                }
                                var run = currentRun.ToString();
                                currentRun.Clear();
                                currentLine.Add(run);
                                
                                var command = CreateCommand(currentLine);
                                currentLine.Clear();
                                if (command != null)
                                {
                                    observer.OnNext(command);
                                }
                                
                                break;
                            }
                            
                            case ' ':
                            {
                                if (isEscaped)
                                {
                                    currentRun.Append('\\');
                                    isEscaped = false;
                                }
                                
                                if (isQuoted)
                                {
                                    currentRun.Append(ch);
                                }
                                else
                                {
                                    var run = currentRun.ToString();
                                    currentRun.Clear();
                                    currentLine.Add(run);
                                }
                                
                                break;
                            }

                            case '\\':
                            {
                                if (isEscaped)
                                {
                                    currentRun.Append(ch);
                                    isEscaped = false;
                                }
                                else
                                {
                                    isEscaped = true;
                                }
                                
                                break;
                            }

                            case '"':
                            {
                                if (isEscaped)
                                {
                                    currentRun.Append(ch);
                                    isEscaped = false;
                                }
                                else
                                {
                                    isQuoted = !isQuoted;
                                }

                                break;
                            }

                            default:
                            {
                                if (isEscaped)
                                {
                                    currentRun.Append('\\');
                                    isEscaped = false;
                                }
                                
                                currentRun.Append(ch);
                                
                                break;
                            }
                        }
                    }
                    
                    var lastRun = currentRun.ToString();
                    currentRun.Clear();
                    currentLine.Add(lastRun);
                    
                    var lastCommand = CreateCommand(currentLine);
                    currentLine.Clear();
                    if (lastCommand != null)
                    {
                        observer.OnNext(lastCommand);
                    }
                    
                    observer.OnCompleted();
                });
            });
        }

        private RedisCommandInfo CreateCommand(List<string> currentLine)
        {
            var parts = currentLine
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .ToList();

            if (parts.Count > 0)
            {
                return new RedisCommandInfo(
                    parts.FirstOrDefault(),
                    parts.Skip(1).ToArray());
            }

            return null;
        }
    }
}