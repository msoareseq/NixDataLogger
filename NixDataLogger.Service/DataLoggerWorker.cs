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
        private ITagRepository tagRepository;
        private IApiClient apiClient;


        public DataLoggerWorker(ILogger<DataLoggerWorker> logger, IOptions<ServiceConfiguration> serviceConfiguration)
        {
            _logger = logger;
            this.serviceConfiguration = serviceConfiguration.Value;
            tagRepository = new TagRepository(this.serviceConfiguration);

            _logger.LogInformation("Reading tags from: {path}", this.serviceConfiguration.TagListPath);
            tagList = tagRepository.GetTagList()?.ToList() ?? new List<Tag>();
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
                _logger.LogInformation("Reading {endpoint}: {time}", serviceConfiguration.ReadEndpoint, DateTimeOffset.Now);
                
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