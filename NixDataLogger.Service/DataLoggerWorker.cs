using Microsoft.Extensions.Options;
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


        public DataLoggerWorker(ILogger<DataLoggerWorker> logger, IOptions<ServiceConfiguration> serviceConfiguration)
        {
            _logger = logger;
            this.serviceConfiguration = serviceConfiguration.Value;

            _logger.LogInformation("Reading tags from: {path}", this.serviceConfiguration.TagListPath);
            tagList = GetTagList()?.ToList() ?? new List<Tag>();
            _logger.LogInformation("Found {count} tags", tagList.Count);


            _logger.LogInformation("Saving data to: {path}", this.serviceConfiguration.LocalStorageConnectionString);
            dataRepository = new LocalVariableDataRepository(this.serviceConfiguration.LocalStorageConnectionString!);
            
            httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);

            
            _logger.LogInformation("Simulation mode: {mode}", this.serviceConfiguration.EnableSimulationMode);
            if (this.serviceConfiguration.EnableSimulationMode)
            {
                apiClient = new SimClient();
            }
            else
            {
                apiClient = new IotGatewayClient(httpClient, this.serviceConfiguration.ReadEndpoint!, logger);
                _logger.LogInformation("Reading data from: {endpoint}", this.serviceConfiguration.ReadEndpoint);
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

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Data logger service is stopping");
            dataRepository.Dispose();
            await base.StopAsync(stoppingToken);
        }

        private IEnumerable<Tag>? GetTagList()
        {

            if (serviceConfiguration.TagListPath == null || !File.Exists(serviceConfiguration.TagListPath))
            {
                throw new FileNotFoundException("Tag list file not found");
            }

            string[] tags = File.ReadAllLines(serviceConfiguration.TagListPath!);
            foreach (string tag in tags)
            {
                if (tag.StartsWith("#")) continue;
                string[] tagParts = tag.Split(',');
                if (tagParts.Length != 4) continue;

                Tag tagResult = new Tag()
                {
                    TagName = tagParts[0].Trim(),
                    Address = tagParts[1].Trim(),
                    Group = tagParts[2].Trim()
                };

                yield return tagResult;
            }
        }

        private void SaveLocalData(IEnumerable<TagData> tagData)
        {
            foreach (TagData data in tagData)
            {
                dataRepository.Insert(data, data.TagName!);
                dataRepository.Save();
            }
        }

        private IEnumerable<TagData> ReadTagData()
        {
            return apiClient.GetTagData(tagList);
        }
    }
}