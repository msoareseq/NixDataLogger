using NixDataLogger.Service.Entities;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NixDataLogger.Service.Repositories
{
    internal class PgRemoteTagDataRepository : ITagDataRepository
    {
        NpgsqlDataSource dataSource;
        List<Tag> tagList;

        public PgRemoteTagDataRepository(string connectionString, IEnumerable<Tag> tagList)
        {
            dataSource = NpgsqlDataSource.Create(connectionString);
            this.tagList = tagList.ToList();

            CreateTables();

        }
        public void Dispose()
        {
            dataSource.Dispose();
        }

        public IEnumerable<TagData> Get(DateTime from, DateTime to, string tagName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TagData> GetAll(string tagName)
        {
            throw new NotImplementedException();
        }

        public int Insert(TagData variableData, string tagName)
        {
            throw new NotImplementedException();
        }

        public int InsertBulk(IEnumerable<TagData> variableData, string tagName)
        {
            throw new NotImplementedException();
        }

        public int RemoveAll()
        {
            throw new NotImplementedException();
        }

        public int RemoveById(int id, string tagName)
        {
            throw new NotImplementedException();
        }

        public int RemoveByIds(IEnumerable<int> ids, string tagName)
        {
            throw new NotImplementedException();
        }

        public int RemoveByTagName(string tagName)
        {
            throw new NotImplementedException();
        }

        public int RemovePeriod(DateTime from, DateTime to)
        {
            throw new NotImplementedException();
        }

        public int RemovePeriod(DateTime from, DateTime to, string tagName)
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        private void CreateTables()
        {
            string createTagListSql = @"CREATE TABLE IF NOT EXISTS taglist (
                                        taglist_id SERIAL PRIMARY KEY,
                                        name VARCHAR(250) NOT NULL,
                                        address VARCHAR(250),
                                        tag_group VARCHAR(250) NOT NULL,
                                        tag_type INT NOT NULL
                                        )";

            string createTagDataSql = @"CREATE TABLE IF NOT EXISTS #tagdata_table# (
                                        id BIGSERIAL PRIMARY KEY,
                                        ts TIMESTAMP WITHOUT TIME ZONE NOT NULL,
                                        tag_value NUMERIC,
                                        tag_quality INT NOT NULL
                                        )";

            var cmd = dataSource.CreateCommand(createTagListSql);
            cmd.ExecuteNonQuery();

            foreach (var tag in tagList)
            {
                if (tag.Group == null || tag.Group == string.Empty || tag.TagName == null || tag.TagName == string.Empty) continue;
                cmd = dataSource.CreateCommand(createTagDataSql.Replace("#tagdata_table#", "data_" + tag.TagName.ToLower()));
                cmd.ExecuteNonQuery();
            }
        }
    }
}
