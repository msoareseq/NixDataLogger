﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NixDataLogger.Service.Entities;

namespace NixDataLogger.Service.Repositories
{
    internal interface ITagDataRepository : IDisposable
    {
        int Insert(TagData variableData, string tagName);
        int InsertBulk(IEnumerable<TagData> variableData, string tagName);
        IEnumerable<TagData> Get(DateTime from, DateTime to, string tagName);
        IEnumerable<TagData> GetAll(string tagName);
        int GetLastId(string tagName);
        DateTime GetLastTimestamp(string tagName);
        int RemovePeriod(DateTime from, DateTime to);
        int RemovePeriod(DateTime from, DateTime to, string tagName);
        int RemoveAll();
        int RemoveByTagName(string tagName);
        int RemoveById(int id, string tagName);
        int RemoveByIds(IEnumerable<int> ids, string tagName);
        void Save();

    }
}
