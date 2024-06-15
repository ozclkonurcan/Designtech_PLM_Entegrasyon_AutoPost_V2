using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Designtech_PLM_Entegrasyon_AutoPost.ViewModel
{
    public class WTChangeOrder2MasterViewModel
    {
        public string? idA2A2 { get; set; }
        public string? idA3masterReference { get; set; }
        public string? statestate { get; set; }
        public string? name { get; set; }
        public string? WTPartNumber { get; set; }
        public string? Version { get; set; }
        public DateTime ProcessTimestamp { get; set; }
        public DateTime updateStampA2 { get; set; }
        public DateTime ReviseDate { get; set; }
    }




    public class AlternateStateConntrolClass
    {
        public string? idA2A2 { get; set; }
        public string? idA3masterReference { get; set; }
        public string? statestate { get; set; }
        public string? name { get; set; }
        public string? WTPartNumber { get; set; }
        public string? versionIdA2versionInfo { get; set; }
        public string? versionLevelA2versionInfo { get; set; }
        public DateTime updateStampA2 { get; set; }
        public DateTime ReviseDate { get; set; }
    }
}
