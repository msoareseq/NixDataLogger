using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NixDataLogger.Service.Entities
{
    public class TagData
    {
        public int TagDataId { get; set; }
        public DateTime TimeStamp { get; set; }
        public object? Value { get; set; }
        public int QualityCode { get; set; }
    }
}
