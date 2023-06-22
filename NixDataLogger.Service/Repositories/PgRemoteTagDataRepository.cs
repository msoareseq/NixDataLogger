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
            string sql = @"SELECT * FROM #tablename# WHERE ts >= $1 AND ts <= $2";
            sql = sql.Replace("#tablename#", GetTagTableName(tagName));
            
            var cmd = dataSource.CreateCommand(sql);
            
            cmd.Parameters.Add(from);
            cmd.Parameters.Add(to);
            
            var reader = cmd.ExecuteReader();
            
            while (reader.Read())
            {
                yield return new TagData()
                {
                    TagDataId = reader.GetInt32(0),
                    TagName = tagName,
                    Timestamp = reader.GetDateTime(2),
                    Value = reader.GetValue(3),
                    QualityCode = reader.GetInt32(4)
                };
            }
        }

        public IEnumerable<TagData> GetAll(string tagName)
        {
            return Get(DateTime.MinValue, DateTime.MaxValue, tagName);
        }

        public int Insert(TagData variableData, string tagName)
        {
            string sql = @"INSERT INTO #tablename# (ts, tag_value, quality) VALUES $1, $2, $3";
            sql = sql.Replace("#tablename#", GetTagTableName(tagName));

            var cmd = dataSource.CreateCommand(sql);
            cmd.Parameters.Add(variableData.Timestamp);
            cmd.Parameters.Add(variableData.Value ?? DBNull.Value);
            cmd.Parameters.Add(variableData.QualityCode);

            return cmd.ExecuteNonQuery();
        }

        public int InsertBulk(IEnumerable<TagData> variableData, string tagName)
        {
            var batch = dataSource.CreateBatch();
            
            foreach (var data in variableData)
            {
                string sql = @"INSERT INTO #tablename# (ts, tag_value, quality) VALUES $1, $2, $3";
                sql = sql.Replace("#tablename#", GetTagTableName(tagName));
                
                var batchCmd = new NpgsqlBatchCommand(sql);
                batchCmd.Parameters.Add(data.Timestamp);
                batchCmd.Parameters.Add(data.Value ?? DBNull.Value);
                batchCmd.Parameters.Add(data.QualityCode);
                
                batch.BatchCommands.Add(batchCmd);
            }

            return batch.ExecuteNonQuery();

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

            string createTagDataNumericSql = @"CREATE TABLE IF NOT EXISTS #tagdata_table# (
                                        id BIGSERIAL PRIMARY KEY,
                                        ts TIMESTAMP WITHOUT TIME ZONE NOT NULL,
                                        tag_value NUMERIC,
                                        tag_quality INT NOT NULL
                                        )";

            string createTagDataStringSql = @"CREATE TABLE IF NOT EXISTS #tagdata_table# (
                                        id BIGSERIAL PRIMARY KEY,
                                        ts TIMESTAMP WITHOUT TIME ZONE NOT NULL,
                                        tag_value VARCHAR(50),
                                        tag_quality INT NOT NULL
                                        )";

            string createTagDataBooleanSql = @"CREATE TABLE IF NOT EXISTS #tagdata_table# (
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

                if (tag.IsNumeric)
                {
                    cmd = dataSource.CreateCommand(createTagDataNumericSql.Replace("#tagdata_table#", GetTagTableName(tag.TagName)));
                }
                else if (tag.DataType == Tag.TagType.Boolean)
                {
                    cmd = dataSource.CreateCommand(createTagDataBooleanSql.Replace("#tagdata_table#", GetTagTableName(tag.TagName)));
                }
                else 
                {
                    cmd = dataSource.CreateCommand(createTagDataStringSql.Replace("#tagdata_table#", GetTagTableName(tag.TagName)));
                }
                
                cmd.ExecuteNonQuery();
            }
        }

        private static string GetTagTableName(string tagName)
        {
            return "data_" + tagName.ToLower();
        }
    }
}
