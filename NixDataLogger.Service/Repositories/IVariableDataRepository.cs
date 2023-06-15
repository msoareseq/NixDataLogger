using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NixDataLogger.Service.Repositories
{
    internal interface IVariableDataRepository
    {
        void Insert(VariableData variableData);
        void Insert(IEnumerable<VariableData> variableData);
        IEnumerable<VariableData> Get(DateTime from, DateTime to);
        IEnumerable<VariableData> Get(DateTime from, DateTime to, string tagName);
        IEnumerable<VariableData> GetAll();

        void Remove(DateTime from, DateTime to);
        void Remove(DateTime from, DateTime to, string tagName);

    }
}
