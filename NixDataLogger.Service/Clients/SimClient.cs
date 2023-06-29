using NixDataLogger.Service.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NixDataLogger.Service.Clients
{
    internal class SimClient : IApiClient
    {
        private readonly string[] stringValues = new string[] { "Approved", "Rejected", "Undefined", "Offline", "Online", "Empty", "Full", "Ok", "Fail" };
        public IEnumerable<TagData> GetTagData(IEnumerable<Tag> tags)
        {
            foreach (var tag in tags)
            {
                yield return new TagData()
                {
                    TagName = tag.TagName,
                    Value = GenerateRandomValue(tag),
                    Timestamp = DateTime.Now,
                    QualityCode = 1,                     
                };
            }
        }

        private object GenerateRandomValue(Tag tag)
        {
            Random random = new Random();
            if (!tag.IsNumeric)
            {
                if (tag.DataType == Tag.TagType.Boolean)
                {
                    if(random.Next(0, 2) == 0)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return stringValues[random.Next(0, stringValues.Length)];
                }
            }
            else
            {
                if (tag.DataType == Tag.TagType.Integer)
                {
                    return random.Next(0, 100);
                }
                else
                {
                    return random.NextDouble() * 100;
                }
            }
        }
    }
}
