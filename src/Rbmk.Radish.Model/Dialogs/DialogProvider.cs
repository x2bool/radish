using System;
using System.Collections.Generic;
using System.Reflection;

namespace Rbmk.Radish.Model.Dialogs
{
    public class DialogProvider : IDialogProvider
    {
        private readonly Dictionary<Type, Type> _types;
        
        public DialogProvider(params Assembly[] assemblies)
        {
            _types = new Dictionary<Type, Type>();

            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    var attr = type.GetCustomAttribute<DialogWindowAttribute>();
                    if (attr != null)
                    {
                        _types.Add(attr.ModelType, type);
                    }
                }
            }
        }
        
        public Type GetWindowType(Type contextType)
        {
            _types.TryGetValue(contextType, out var type);
            return type;
        }
    }
}