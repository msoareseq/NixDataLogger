using Microsoft.VisualBasic;
using NixDataLogger.Service.Clients;
using NixDataLogger.Service.Entities;
using NixDataLogger.Service.Repositories;
using System.Diagnostics;

namespace NixDataLogger.Service
{
    public class DataLoggerWorker : BackgroundService
    {
        private readonly ILogger<DataLoggerWorker> _logger;
        private readonly ServiceConfiguration serviceConfiguration;
        private readonly HttpClient httpClient;

        private List<Tag> tagList;
        private ITagDataRepository dataRepository;
        private IApiClient apiClient;


        public DataLoggerWorker(ILogger<DataLoggerWorker> logger, ServiceConfiguration serviceConfiguration)
        {
            _logger = logger;
            this.serviceConfiguration = serviceConfiguration;

            _logger.LogInformation("Reading tags from: {path}", serviceConfiguration.TagListPath);
            tagList = GetTagList()?.ToList() ?? new List<Tag>();
            _logger.LogInformation("Found {count} tags", tagList.Count);


            _logger.LogInformation("Saving data to: {path}", serviceConfiguration.LocalStorageConnectionString);
            dataRepository = new LocalVariableDataRepository(serviceConfiguration.LocalStorageConnectionString!);
            
            httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);

            
            _logger.LogInformation("Simulation mode: {mode}", serviceConfiguration.EnableSimulationMode);
            if (serviceConfiguration.EnableSimulationMode)
            {
                apiClient = new SimClient();
            }
            else
            {
                apiClient = new IotGatewayClient(httpClient, serviceConfiguration.ReadEndpoint!);
                _logger.LogInformation("Reading data from: {endpoint}", serviceConfiguration.ReadEndpoint);
            }
            
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Reading tags at: {time}", DateTimeOffset.Now);
                
                try
                {
                    var resultData = ReadTagData();
                    SaveLocalData(resultData);

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reading and writing tags");
                }

                await Task.Delay(serviceConfiguration.DataReadIntervalSeconds * 1000, stoppingToken);
            }
        }

        private IEnumerable<Tag>? GetTagList()
        {
            
            if (serviceConfiguration.TagListPath == null || !File.Exists(serviceConfiguration.TagListPath)) yield return null!;

            string[] tags = File.ReadAllLines(serviceConfiguration.TagListPath!);
            foreach (string tag in tags)
            {
                if (tag.StartsWith("#")) continue;
                string[] tagParts = tag.Split(',');
                if (tagParts.Length != 3) continue;

                Tag tagResult = new Tag()
                {
                    TagName = tagParts[0],
                    Address = tagParts[1],
                    Group = tagParts[2]
                };

                yield return tagResult;
            }
        }

        private void SaveLocalData(IEnumerable<TagData> tagData)
        {
            foreach (TagData data in tagData)
            {
                dataRepository.Insert(data, data.TagName!);
            }
        }

        private IEnumerable<TagData> ReadTagData()
        {
            return apiClient.GetTagData(tagList);
        }
    }
}