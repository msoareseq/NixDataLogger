using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NixDataLogger.Service.Entities;
using System.Threading.Tasks;

namespace NixDataLogger.Service.Clients
{
    internal interface IApiClient
    {
        IEnumerable<TagData> GetTagData(IEnumerable<Tag> tags);
    }
}
