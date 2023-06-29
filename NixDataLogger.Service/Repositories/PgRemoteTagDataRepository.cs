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
            NpgsqlConnectionStringBuilder builder = new NpgsqlConnectionStringBuilder();

            if (Uri.TryCreate(connectionString, UriKind.Absolute, out var url))
            {
                builder.Host = url.Host;
                builder.Port = url.IsDefaultPort ? 5432 : url.Port;
                builder.Username = url.UserInfo.Split(':')[0];
                builder.Password = url.UserInfo.Split(':')[1];
                builder.Database = url.AbsolutePath.TrimStart('/');
            }
            else
            {
                builder.ConnectionString = connectionString;
            }
            
            dataSource = NpgsqlDataSource.Create(builder);
            
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

        public int GetLastId(string tagName)
        {
            string sql = @"SELECT MAX(id) FROM #tablename#";
            sql = sql.Replace("#tablename#", GetTagTableName(tagName));

            var cmd = dataSource.CreateCommand(sql);
            var result = cmd.ExecuteScalar();
            if (result == null || result == DBNull.Value) return 0;
            else return Convert.ToInt32(result);
        }

        public DateTime GetLastTimestamp(string tagName)
        {
            string sql = @"SELECT MAX(ts) FROM #tablename#";
            sql = sql.Replace("#tablename#", GetTagTableName(tagName));
            
            var cmd = dataSource.CreateCommand(sql);
            var result = cmd.ExecuteScalar();
            if (result == null || result == DBNull.Value) return DateTime.MinValue;
            else return Convert.ToDateTime(result);
        }

        public int Insert(TagData variableData, string tagName)
        {
            string sql = @"INSERT INTO #tablename# (ts, tag_value, tag_quality) VALUES ($1, $2, $3)";
            sql = sql.Replace("#tablename#", GetTagTableName(tagName));

            var cmd = dataSource.CreateCommand(sql);
            cmd.Parameters.Add(new NpgsqlParameter<DateTime>() { TypedValue = variableData.Timestamp });
            cmd.Parameters.Add(GetTypedParameter(variableData));
            cmd.Parameters.Add(new NpgsqlParameter<int>() { TypedValue = variableData.QualityCode });

            return cmd.ExecuteNonQuery();
        }

        public int InsertBulk(IEnumerable<TagData> variableData, string tagName)
        {
            var batch = dataSource.CreateBatch();
            
            foreach (var data in variableData)
            {
                
                string sql = @"INSERT INTO #tablename# (ts, tag_value, tag_quality) VALUES ($1, $2, $3)";
                sql = sql.Replace("#tablename#", GetTagTableName(tagName));
                var batchCmd = new NpgsqlBatchCommand(sql);
                batchCmd.Parameters.Add(new NpgsqlParameter<DateTime>() { TypedValue = data.Timestamp });
                batchCmd.Parameters.Add(GetTypedParameter(data));
                batchCmd.Parameters.Add(new NpgsqlParameter<int>() { TypedValue = data.QualityCode });
                                
                batch.BatchCommands.Add(batchCmd);
            }

            return batch.ExecuteNonQuery();

        }

        public int RemoveAll()
        {
            int count = 0;

            foreach (var tag in tagList)
            {
                count += RemoveByTagName(tag.TagName!);
            }

            return count;
        }

        public int RemoveById(int id, string tagName)
        {
            string sql = @"DELETE FROM #tablename# WHERE id = $1";
            sql = sql.Replace("#tablename#", GetTagTableName(tagName));
            var cmd = dataSource.CreateCommand(sql);
            cmd.Parameters.Add(new NpgsqlParameter<int> { TypedValue = id });
            return cmd.ExecuteNonQuery();
        }

        public int RemoveByIds(IEnumerable<int> ids, string tagName)
        {
            throw new NotImplementedException();
        }

        public int RemoveByTagName(string tagName)
        {
            string sql = @"TRUNCATE TABLE #tablename# RESTART IDENTITY";
            sql = sql.Replace("#tablename#", GetTagTableName(tagName));
            var cmd = dataSource.CreateCommand(sql);
            return cmd.ExecuteNonQuery();
        }

        public int RemovePeriod(DateTime from, DateTime to)
        {
            int count = 0;
            foreach (var tag in tagList)
            {
                count += RemovePeriod(from, to, tag.TagName!);
            }
            return count;
        }

        public int RemovePeriod(DateTime from, DateTime to, string tagName)
        {
            string sql = @"DELETE FROM #tablename# WHERE ts >= $1 AND ts <= $2";
            sql = sql.Replace("#tablename#", GetTagTableName(tagName));
            var cmd = dataSource.CreateCommand(sql);
            cmd.Parameters.Add(new NpgsqlParameter<DateTime> { TypedValue = from.ToUniversalTime() });
            cmd.Parameters.Add(new NpgsqlParameter<DateTime> { TypedValue = to.ToUniversalTime() });
            return cmd.ExecuteNonQuery();
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        private void CreateTables()
        {
            string createTagListSql = @"CREATE TABLE IF NOT EXISTS taglist (
                                        id SERIAL PRIMARY KEY,
                                        name VARCHAR(250) NOT NULL,
                                        description VARCHAR(250),
                                        unit VARCHAR(20),
                                        address VARCHAR(250),
                                        tag_group VARCHAR(250) NOT NULL,
                                        tag_type INT NOT NULL,
                                        tag_data_table VARCHAR(250) NOT NULL
                                        )";

            string createTagDataNumericSql = @"CREATE TABLE IF NOT EXISTS #tagdata_table# (
                                        id BIGSERIAL PRIMARY KEY,
                                        ts TIMESTAMP WITH TIME ZONE NOT NULL,
                                        tag_value NUMERIC,
                                        tag_quality INT NOT NULL
                                        )";

            string createTagDataStringSql = @"CREATE TABLE IF NOT EXISTS #tagdata_table# (
                                        id BIGSERIAL PRIMARY KEY,
                                        ts TIMESTAMP WITH TIME ZONE NOT NULL,
                                        tag_value VARCHAR(50),
                                        tag_quality INT NOT NULL
                                        )";

            string createTagDataBooleanSql = @"CREATE TABLE IF NOT EXISTS #tagdata_table# (
                                        id BIGSERIAL PRIMARY KEY,
                                        ts TIMESTAMP WITH TIME ZONE NOT NULL,
                                        tag_value BOOLEAN,
                                        tag_quality INT NOT NULL
                                        )";

            var cmd = dataSource.CreateCommand(createTagListSql);
            cmd.ExecuteNonQuery();
            PopulateTagList();

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

        private NpgsqlParameter GetTypedParameter(TagData tagData)
        {
            Tag tag = tagList.FirstOrDefault(t => t.TagName == tagData.TagName)!;

            if (tag == null) return new NpgsqlParameter() { Value = null };

            if (tag.IsNumeric)
            {
                return new NpgsqlParameter<double>() { TypedValue = Convert.ToDouble(tagData.Value) };
            }
            else if (tag.DataType == Tag.TagType.Boolean)
            {
                return new NpgsqlParameter<bool>() { TypedValue = Convert.ToBoolean(tagData.Value) };
            }
            else
            {
                return new NpgsqlParameter<string>() { TypedValue = Convert.ToString(tagData.Value) };
            }
        }

        private void PopulateTagList()
        {
            string sql = @"TRUNCATE TABLE taglist RESTART IDENTITY";
            var cmd = dataSource.CreateCommand(sql);
            cmd.ExecuteNonQuery();

            foreach (var tag in tagList)
            {
                sql = @"INSERT INTO taglist (name, address, tag_group, tag_type, tag_data_table, unit, description) VALUES ($1, $2, $3, $4, $5, $6, $7)";
                cmd = dataSource.CreateCommand(sql);
                cmd.Parameters.Add(new NpgsqlParameter<string>() { TypedValue = tag.TagName });
                cmd.Parameters.Add(new NpgsqlParameter<string>() { TypedValue = tag.Address });
                cmd.Parameters.Add(new NpgsqlParameter<string>() { TypedValue = tag.Group });
                cmd.Parameters.Add(new NpgsqlParameter<int>() { TypedValue = (int)tag.DataType });
                cmd.Parameters.Add(new NpgsqlParameter<string>() { TypedValue = GetTagTableName(tag.TagName!) });
                cmd.Parameters.Add(new NpgsqlParameter<string>() { TypedValue = tag.Unit });
                cmd.Parameters.Add(new NpgsqlParameter<string>() { TypedValue = tag.Description });
                cmd.ExecuteNonQuery();
            }
        }
                
    }
}
