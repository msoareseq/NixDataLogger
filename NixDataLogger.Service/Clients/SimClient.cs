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
        public IEnumerable<TagData> GetTagData(IEnumerable<Tag> tags)
        {
            foreach (var tag in tags)
            {
                yield return new TagData()
                {
                    TagName = tag.TagName,
                    Value = new Random().NextDouble(),
                    Timestamp = DateTime.Now,
                    QualityCode = 1,                     
                };
            }
        }
    }
}
