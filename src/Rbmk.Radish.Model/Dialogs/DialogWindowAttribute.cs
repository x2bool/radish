using System;

namespace Rbmk.Radish.Model.Dialogs
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DialogWindowAttribute : Attribute
    {
        public Type ModelType { get; set; }
        
        public DialogWindowAttribute(Type modelType)
        {
            ModelType = modelType;
        }
    }
}