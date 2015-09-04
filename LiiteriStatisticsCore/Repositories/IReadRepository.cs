using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiiteriStatisticsCore.Repositories
{
    public interface IReadRepository<T>
    {
        IEnumerable<T> FindAll();

        T Single();

        T First();
    } 
}