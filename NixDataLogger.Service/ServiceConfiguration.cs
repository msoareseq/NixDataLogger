using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace NixDataLogger.Service
{
    internal class ServiceConfiguration
    {
        public int DataReadIntervalSeconds { get; set; }
        public string? TagListPath { get; set; }
        public string? LocalStorageConnectionString { get; set; }
        public string? LocalBackupStoragePath { get; set; }
        public int LocalStorageRetentionDays { get; set; }
        public string? RemoteStorageConnectionString { get; set; }
        public bool SyncRemote { get; set; }
        public int SyncIntervalHours { get; set; }
        public bool EnableSimulationMode { get; set; }


        public ServiceConfiguration()
        {
            DataReadIntervalSeconds = 60;
            TagListPath = "taglist.csv";
            LocalStorageConnectionString = "data.db";
            LocalBackupStoragePath = "data.bkp";
            LocalStorageRetentionDays = 30;
            RemoteStorageConnectionString = null;
            SyncRemote = false;
            SyncIntervalHours = 24;
            EnableSimulationMode = false;
        }

        public static ServiceConfiguration LoadServiceConfiguration(string fileName)
        {
            if (!File.Exists(fileName)) return new ServiceConfiguration();
            
            var result = JsonSerializer.Deserialize<ServiceConfiguration>(File.ReadAllText(fileName));

            return result ?? new ServiceConfiguration();

        }

        public void SaveServiceConfiguration(string fileName)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(fileName, JsonSerializer.Serialize(this, options));
            JsonNode node = JsonNode.Parse(File.ReadAllText(fileName))!;

        }
    }
        
}
