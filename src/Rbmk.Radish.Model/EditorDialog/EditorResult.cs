namespace Rbmk.Radish.Model.EditorDialog
{
    public class EditorResult
    {
        public string IndexError { get; set; }
        
        public string ScoreError { get; set; }
        
        public string KeyError { get; set; }
        
        public string ValueError { get; set; }

        public EditorAction Action { get; set; }
    }
}