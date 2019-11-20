namespace Rbmk.Radish.Model.EditorDialog
{
    public class EditorTarget
    {
        public int Index { get; set; }
        
        public double Score { get; set; }
        
        public string Key { get; set; }
        
        public string Value { get; set; }

        public EditorAction Action { get; set; }
    }
}