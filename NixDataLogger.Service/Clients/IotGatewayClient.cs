using NixDataLogger.Service.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NixDataLogger.Service.Clients
{
    internal class IotGatewayClient : IApiClient
    {

        public IotGatewayClient(HttpClient httpClient)
        {
            
        }

        public IEnumerable<TagData> GetTagData(IEnumerable<Tag> tags)
        {
            
        }
    }
}
