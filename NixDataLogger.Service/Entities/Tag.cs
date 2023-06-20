using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NixDataLogger.Service.Entities
{
    public class Tag
    {
        public enum TagType { Integer, Float, Double, String, Boolean };

        public int TagId { get; set; }
        public string? TagName { get; set; }
        public string? Address { get; set; }
        public string? Group { get; set; }
        public TagType DataType { get; set; }
    }
}
