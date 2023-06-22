using NixDataLogger.Service.Entities;

namespace NixDataLogger.Service.Repositories
{
    internal interface ITagRepository
    {
        IEnumerable<Tag>? GetTagList();
    }
}