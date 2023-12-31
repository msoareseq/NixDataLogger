using Microsoft.Extensions.Options;
using NixDataLogger.Service.Clients;
using NixDataLogger.Service.Entities;
using NixDataLogger.Service.Repositories;
using System.Text.Json;
using System.Text.Json.Serialization;

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
                    _logger.LogInformation("Read and saved {count} tags", resultData.Count());
                    
                    var fileData = ReadInputFiles();
                    if (fileData.Any())
                    {
                        _logger.LogInformation("Read {count} tags from input files", fileData.Count());
                        SaveLocalData(fileData);
                    }

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
            }
            
            dataRepository.Save();
        }

        private IEnumerable<TagData> ReadTagData()
        {
            return apiClient.GetTagData(tagList);
        }

        private IEnumerable<TagData> ReadInputFiles()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string inputDirectory = Path.Combine(baseDirectory, @"input\");

            if (!Directory.Exists(inputDirectory))
            {
                _logger.LogInformation("Input directory does not exist. Creating...");
                Directory.CreateDirectory(inputDirectory);
            }
            
            string[] files = Directory.GetFiles(Path.Combine(baseDirectory, @"input\"), "*.nif");
            _logger.LogInformation("Found {count} input files. Importing...", files.Length);
            
            List<TagData> tagDataList = new List<TagData>();

            foreach (string file in files)
            {
                string data = File.ReadAllText(file);

                try
                {
                    var tagData = JsonSerializer.Deserialize<TagData>(data);

                    if (tagData != null)
                    {
                        tagDataList.Add(tagData);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reading file: {file}. File will be discarded.", file);
                    
                }
                finally
                {
                    File.Delete(file);                    
                }

            }
            
            return tagDataList;
                       
        }
    }
}