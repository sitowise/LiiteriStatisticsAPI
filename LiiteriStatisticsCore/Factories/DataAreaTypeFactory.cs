using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;

namespace LiiteriStatisticsCore.Factories
{
    class DataAreaTypeFactory : BaseFactory, IFactory
    {
        public override Models.ILiiteriMarker Create(DbDataReader rdr)
        {
            var obj = new Models.DataAreaType();
            obj.Id = (int) rdr["AreaTypeId"];
            obj.DataSource = rdr["DataSource"].ToString();
            return obj;
        }
    }
}