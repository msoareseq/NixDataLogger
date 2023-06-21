using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace NixDataLogger.Service.Models
{
    [Serializable]
    internal class IotGatewayResponse
    {
        [JsonPropertyName("readResults")]
        public List<ReadResult>? ReadResults { get; set; }
        
        [Serializable]
        internal class ReadResult
        {
            [JsonPropertyName("id")]
            public string? Id { get; set; }
            
            [JsonPropertyName("s")]
            public bool Success { get; set; }

            [JsonPropertyName("r")]
            public string? Result { get; set; }
            
            [JsonPropertyName("v")]
            public string? Value { get; set; }

            [JsonPropertyName("t")]
            public long Timestamp { get; set; }

        }



    }

}
