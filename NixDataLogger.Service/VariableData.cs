using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NixDataLogger.Service
{
    public class VariableData
    {
        DateTime TimeStamp { get; set; }
        object? Value { get; set; }
        int QualityCode { get; set; }
    }
}
