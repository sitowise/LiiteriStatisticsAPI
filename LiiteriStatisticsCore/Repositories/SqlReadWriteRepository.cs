using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data.Common;

namespace LiiteriStatisticsCore.Repositories
{
    public abstract class SqlReadWriteRepository<T> :
        SqlReadRepository<T>,
        IReadWriteRepository<T> where T : class
    {
        public SqlReadWriteRepository(
            DbConnection dbConnection,
            IEnumerable<Queries.ISqlQuery> queries,
            Factories.IFactory factory = null) :
            base(dbConnection, queries, factory)
        {
        }

        public virtual void Insert(T entity)
        {
            throw new NotImplementedException();
        }

        public virtual void Update(T entity)
        {
            throw new NotImplementedException();
        }

        public virtual void Delete(T entity)
        {
            throw new NotImplementedException();
        }
    }
}