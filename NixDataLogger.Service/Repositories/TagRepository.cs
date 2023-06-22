using NixDataLogger.Service.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NixDataLogger.Service.Repositories
{
    internal class TagRepository : ITagRepository
    {

        private ServiceConfiguration serviceConfiguration;

        public TagRepository(ServiceConfiguration serviceConfiguration)
        {
            this.serviceConfiguration = serviceConfiguration;
        }

        public IEnumerable<Tag>? GetTagList()
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
                    Group = tagParts[2].Trim(),
                };
                tagResult.ParseTagType(tagParts[3].Trim());

                yield return tagResult;
            }
        }
    }
}
