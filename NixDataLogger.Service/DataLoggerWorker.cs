using Microsoft.VisualBasic;
using NixDataLogger.Service.Entities;
using NixDataLogger.Service.Repositories;
using System.Diagnostics;

namespace NixDataLogger.Service
{
    public class DataLoggerWorker : BackgroundService
    {
        private readonly ILogger<DataLoggerWorker> _logger;
        private readonly ServiceConfiguration serviceConfiguration;

        private List<Tag> tagList;
        private ITagDataRepository dataRepository;


        public DataLoggerWorker(ILogger<DataLoggerWorker> logger, ServiceConfiguration serviceConfiguration)
        {
            _logger = logger;
            this.serviceConfiguration = serviceConfiguration;

            tagList = GetTagList()?.ToList() ?? new List<Tag>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Reading tags at: {time}", DateTimeOffset.Now);



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
    }
}