using NixDataLogger.Service.Entities;
using NixDataLogger.Service.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace NixDataLogger.Service
{
    internal class SyncWorker : BackgroundService
    {
        private readonly ITagDataRepository? remoteDataRepository;
        private readonly ITagDataRepository? localDataRepository;
        private readonly ILogger<SyncWorker> logger;
        private readonly ITagRepository tagRepository;
        private readonly List<Tag>? tagList;
        
        private readonly bool isEnabled;
        private readonly int syncInterval;

        public SyncWorker(ServiceConfiguration serviceConfiguration, ILogger<SyncWorker> logger)
        {
            this.logger = logger;
            
            tagRepository = new TagRepository(serviceConfiguration);
            tagList = tagRepository.GetTagList()!.ToList();
            
            isEnabled = serviceConfiguration.SyncRemote;

            syncInterval = serviceConfiguration.SyncIntervalHours < 1 ? 1 : serviceConfiguration.SyncIntervalHours;

            if (serviceConfiguration.RemoteStorageConnectionString == null && isEnabled)
            {
                isEnabled = false;
                logger.LogError("Remote storage connection string not found. Remote Sync disabled.");
            }
            else if ((tagList == null || tagList.Count == 0) && isEnabled)
            {
                isEnabled = false;
                logger.LogWarning("No tags found. Remote Sync disabled.");
            }
            else if (isEnabled && serviceConfiguration.RemoteStorageConnectionString != null && tagList != null && tagList.Count > 0)
            {
                remoteDataRepository = new PgRemoteTagDataRepository(serviceConfiguration.RemoteStorageConnectionString, tagList);
                localDataRepository = new LocalVariableDataRepository(serviceConfiguration.LocalStorageConnectionString!);
            }
            else
            {
                remoteDataRepository = null;
            }
            
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested || !isEnabled)
            {
                logger.LogInformation("Start syncing data...");
                try
                {
                    SyncData();
                    logger.LogInformation("Data sync completed!");
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error syncing data");
                }
                finally
                {
                    await Task.Delay(TimeSpan.FromHours(syncInterval), stoppingToken);
                }
            }
            
        }
        private void SyncData()
        {
            if (remoteDataRepository == null || localDataRepository == null || tagList == null)
            {
                logger.LogError("Repository configuration or tag list is null!");
                return;
            }

            foreach (var tag in tagList)
            {
                if (tag.TagName == null) continue;

                int lastId = remoteDataRepository.GetLastId(tag.TagName);
                var data = localDataRepository.GetAll(tag.TagName!).Where(d => d.TagDataId > lastId).ToList();
                if (data.Count > 0)
                {
                    remoteDataRepository!.InsertBulk(data, tag.TagName);
                }
            }
        }

    }
}
