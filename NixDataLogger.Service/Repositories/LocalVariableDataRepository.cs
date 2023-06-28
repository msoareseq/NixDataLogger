using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using NixDataLogger.Service.Entities;

namespace NixDataLogger.Service.Repositories
{
    internal class LocalVariableDataRepository : ITagDataRepository, IDisposable
    {
        public LiteDatabase db;

        public LocalVariableDataRepository(string connectionString)
        {
            db = new LiteDatabase(connectionString);
        }

        public void Dispose()
        {
            db.Dispose();
        }

        
        public IEnumerable<TagData> Get(DateTime from, DateTime to, string tagName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TagData> GetAll(string tagName)
        {
            var col = db.GetCollection<TagData>(tagName);
            return col.FindAll();
        }

        public int GetLastId(string tagName)
        {
            var col = db.GetCollection<TagData>(tagName);
            var last = col.FindAll().OrderByDescending(x => x.TagDataId).FirstOrDefault();
            if (last == null) return 0;
            else return last.TagDataId;
        }

        public DateTime GetLastTimestamp(string tagName)
        {
            var col = db.GetCollection<TagData>(tagName);
            var last = col.FindAll().OrderByDescending(x => x.Timestamp).FirstOrDefault();
            if (last == null) return DateTime.MinValue;
            else return last.Timestamp;
        }

        public int Insert(TagData variableData, string tagName)
        {
            var col = db.GetCollection<TagData>(tagName);
            return col.Insert(variableData);
        }

        public int InsertBulk(IEnumerable<TagData> variableData, string tagName)
        {
            var col = db.GetCollection<TagData>(tagName);
            return col.InsertBulk(variableData);
        }

        public int RemoveAll()
        {
            return RemovePeriod(DateTime.MinValue, DateTime.MaxValue);
        }

        public int RemoveById(int id, string tagName)
        {
            var col = db.GetCollection<TagData>(tagName);
            return col.DeleteMany(x => x.TagDataId == id);
        }

        public int RemoveByIds(IEnumerable<int> ids, string tagName)
        {
            throw new NotImplementedException();
        }

        public int RemoveByTagName(string tagName)
        {
            int count = db.GetCollection<TagData>(tagName).Count();
            if (db.DropCollection(tagName))
            {
                db.Rebuild();
                return count;
            }
            else return 0;
        }

        public int RemovePeriod(DateTime from, DateTime to)
        {
            var colNames = db.GetCollectionNames();
            int totalRemoved = 0;
            foreach (var colName in colNames)
            {
                if (colName.Contains('$')) continue; // Ignores a System Collection (Read Only)

                var col = db.GetCollection<TagData>(colName);
                totalRemoved += col.DeleteMany(x => x.Timestamp >= from && x.Timestamp <= to);
                db.Rebuild();
            }
            return totalRemoved;
        }

        public int RemovePeriod(DateTime from, DateTime to, string tagName)
        {
            var col = db.GetCollection<TagData>(tagName);
            return col.DeleteMany(x => x.Timestamp >= from && x.Timestamp <= to);
        }

        public void Save()
        {
            db.Checkpoint();
        }
    }
}
