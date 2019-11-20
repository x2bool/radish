using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reactive.Linq;
using System.Xml;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using Rbmk.Radish.Model.Highlighting;

namespace Rbmk.Radish.Views.Highlighting
{
    public class HighlightingSyntaxLoader
    {
        private static readonly ConcurrentDictionary<string, IHighlightingDefinition> Definitions
            = new ConcurrentDictionary<string, IHighlightingDefinition>();
        
        public static IObservable<IHighlightingDefinition> Load(HighlightingSyntax syntax)
        {
            var assembly = typeof(HighlightingSyntaxLoader).Assembly;
            var syntaxName = syntax.ToString();
            var resourceName = $"{assembly.GetName().Name}.Highlighting.{syntaxName}.xshd";

            var definition = Definitions.GetOrAdd(resourceName, _ =>
            {
                using (var stream = assembly.GetManifestResourceStream(resourceName)) {
                    using (var reader = new XmlTextReader(stream)) {
                        return HighlightingLoader.Load(reader, HighlightingManager.Instance);
                    }
                }
            });
            
            return Observable.Return(definition);
        }
    }
}