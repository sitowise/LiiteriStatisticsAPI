using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;

namespace LiiteriStatisticsCore.Factories
{
    public abstract class BaseFactory : Factories.IFactory
    {
        public abstract Models.ILiiteriMarker Create(DbDataReader rdr);

        public object GetValueOrNull(DbDataReader rdr, string key)
        {
            if (Convert.IsDBNull(rdr[key])) return null;
            return rdr.GetValue(rdr.GetOrdinal(key));
        }

        /* tinyint in db will return object {byte}, which cannot cast
         * to int. instead of using Convert.Int32, let's be more
         * specific and attempt casting both int and byte, and fail
         * with everything else */
        public int GetNumber(DbDataReader rdr, string key)
        {
            try {
                return (int) rdr.GetInt32(rdr.GetOrdinal("AreaId"));
            } catch (InvalidCastException) {
                return (int) rdr.GetByte(rdr.GetOrdinal("AreaId"));
            }
        }

        public bool HasColumn(DbDataReader rdr, string key)
        {
            foreach (System.Data.DataRow row in rdr.GetSchemaTable().Rows) {
                if (row["ColumnName"].ToString() == key) {
                    return true;
                }
            }
            return false;
        }
    }
}