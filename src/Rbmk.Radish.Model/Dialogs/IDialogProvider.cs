using System;

namespace Rbmk.Radish.Model.Dialogs
{
    public interface IDialogProvider
    {
        Type GetWindowType(Type contextType);
    }
}