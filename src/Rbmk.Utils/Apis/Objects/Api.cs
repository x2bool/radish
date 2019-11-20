using System;

namespace Rbmk.Utils.Apis.Objects
{
    public partial class Api
    {
        public class Release
        {
            public DateTime Date { get; set; }
            
            public string Version { get; set; }
            
            public string DownloadUrl { get; set; }
            
            public string[] Changes { get; set; }
        }
        
        public class ReleaseList
        {
            public Release[] Releases { get; set; }
        }
    }
}