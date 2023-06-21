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

        public bool IsNumeric
        {
            get
            {
                return (int)DataType < 3;
            }
        }

        public void ParseTagType(string tagType)
        {
            switch (tagType.ToLower())
            {
                case "integer":
                    DataType = TagType.Integer;
                    break;
                case "float":
                    DataType = TagType.Float;
                    break;
                case "double":
                    DataType = TagType.Double;
                    break;
                case "string":
                    DataType = TagType.String;
                    break;
                case "boolean":
                    DataType = TagType.Boolean;
                    break;
                default:
                    throw new Exception("Invalid tag type");
            }
        }

        public Type GetDataType()
        {
            switch (DataType)
            {
                case TagType.Integer:
                    return typeof(int);
                case TagType.Float:
                    return typeof(float);
                case TagType.Double:
                    return typeof(double);
                case TagType.String:
                    return typeof(string);
                case TagType.Boolean:
                    return typeof(bool);
                default:
                    break;
            }
            throw new Exception("Invalid tag type");
        }
        
    }


}
