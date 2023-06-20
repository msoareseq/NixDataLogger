using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace NixDataLogger.Service
{
    public class ServiceConfiguration
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
        public string? ReadEndpoint { get; set; }


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
            EnableSimulationMode = true;
        }

        public static ServiceConfiguration? LoadServiceConfiguration(string fileName)
        {
            if (!File.Exists(fileName)) return null;
            
            var result = JsonSerializer.Deserialize<ServiceConfiguration>(File.ReadAllText(fileName));

            return result;

        }

        public void SaveToFile(string fileName)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(fileName, JsonSerializer.Serialize(this, options));
        }

        public void LoadFromFile(string fileName)
        {
            var sc = LoadServiceConfiguration(fileName) ?? throw new Exception("Service configuration file not found or malformed.");

            DataReadIntervalSeconds = sc.DataReadIntervalSeconds;
            TagListPath = sc.TagListPath;
            LocalStorageConnectionString = sc.LocalStorageConnectionString;
            LocalBackupStoragePath = sc.LocalBackupStoragePath;
            LocalStorageRetentionDays = sc.LocalStorageRetentionDays;
            RemoteStorageConnectionString = sc.RemoteStorageConnectionString;
            SyncRemote = sc.SyncRemote;
            SyncIntervalHours = sc.SyncIntervalHours;
            EnableSimulationMode = sc.EnableSimulationMode;
            ReadEndpoint = sc.ReadEndpoint;
        }
    }
        
}
