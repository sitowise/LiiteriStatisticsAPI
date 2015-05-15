using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiiteriStatisticsCore.Repositories
{
    public interface IReadRepository<T>
    {
        IEnumerable<T> GetAll();

        IEnumerable<T> FindAll(Queries.ISqlQuery query);

        T Single(Queries.ISqlQuery query);

        T First(Queries.ISqlQuery query);
    } 
}