using NStack;
using PropertyChanged;
using Rbmk.Radish.Services.Redis.Projections;
using ReactiveUI;

namespace Rbmk.Radish.Model.ResultViewer.Projections.Some
{
    [AddINotifyPropertyChangedInterface]
    public abstract class ResultNodeModel : IActivatableViewModel
    {
        public bool IsExpanded { get; set; }
        
        public int? Ordinal { get; set; }

        public string FormattedOrdinal =>
            Ordinal == null ? null : $"[{Ordinal}]";
        
        public ResultProjectionInfo ResultProjectionInfo { get; }
        
        protected ResultNodeModel(ResultProjectionInfo resultProjectionInfo)
        {
            ResultProjectionInfo = resultProjectionInfo;
        }
        
        public ViewModelActivator Activator { get; } = new ViewModelActivator();

        private const int CutLength = 32;

        protected string AsString(byte[] bytes, bool cut = false)
        {
            if (bytes == null)
            {
                return null;
            }

            var str = ustring.Make(bytes);
            
            if (cut && str.Length > CutLength)
            {
                return str[0, CutLength].ToString();
            }

            return str.ToString();
        }
    }
}