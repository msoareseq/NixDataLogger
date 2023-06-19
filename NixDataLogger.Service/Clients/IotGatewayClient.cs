using NixDataLogger.Service.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using NixDataLogger.Service.Models;

namespace NixDataLogger.Service.Clients
{
    internal class IotGatewayClient : IApiClient
    {
        RestClient client;
        string readEndpoint;

        public IotGatewayClient(string readEndpoint)
        {
            client = new RestClient();
            this.readEndpoint = readEndpoint;
        }

        public IotGatewayClient(HttpClient httpClient, string readEndpoint)
        {
            client = new RestClient(httpClient);
            this.readEndpoint = readEndpoint;
        }

        public IEnumerable<TagData> GetTagData(IEnumerable<Tag> tags)
        {
            foreach (Tag tag in tags)
            {
                RestRequest request = new RestRequest(readEndpoint, Method.Get);
                request.AddParameter("ids", tag.Address);
                var response = client.Get<IotGatewayResponse>(request);

                if (response == null) continue;

                yield return new TagData()
                {
                    TagName = tag.TagName,
                    TimeStamp = DateTime.Now,
                    QualityCode = (response!.ReadResults![0].Success ? 1 : 0),
                    Value = response.ReadResults![0].Value,
                };
            }
        }
    }
}
