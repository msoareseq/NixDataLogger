﻿using NixDataLogger.Service.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using NixDataLogger.Service.Models;
using System.Security.Cryptography.Xml;

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
            var groups = tags.GroupBy(x => x.Group);

            if (groups == null)
            {
                throw new ArgumentNullException(nameof(groups));
            }

            foreach (var group in groups)
            {
                RestRequest request = new RestRequest(readEndpoint, Method.Get);

                foreach (Tag tag in group)
                {
                    request.AddParameter("ids", tag.Address);
                }

                var response = client.Get<IotGatewayResponse>(request);

                if (response == null || response.ReadResults == null) continue;

                foreach (var result in response.ReadResults!)
                {
                    yield return new TagData()
                    {
                        TagName = tags.First(x => x.Address == result.Id).TagName,
                        Timestamp = DateTime.Now,
                        QualityCode = (result.Success ? 1 : 0),
                        Value = result.Value,
                    };
                }

            }
        }
    }
}
