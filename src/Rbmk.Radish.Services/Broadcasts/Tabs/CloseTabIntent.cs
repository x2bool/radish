using Rbmk.Utils.Broadcasts;

namespace Rbmk.Radish.Services.Broadcasts.Tabs
{
    public class CloseTabIntent : Broadcast.Intent
    {
        public int Index { get; }

        public CloseTabIntent(int index)
        {
            Index = index;
        }
    }
}