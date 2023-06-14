using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NixDataLogger.Service
{
    public class SystemData
    {
        public DateTime? LastDataRead { get; set; }
        public DateTime? LastSync { get; set; }
        public DateTime? LastBackup { get; set; }
        public int VariableCount { get; set; }
        public float IdleTime { get; set; }
    }
}
