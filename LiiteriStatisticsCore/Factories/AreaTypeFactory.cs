using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;

namespace LiiteriStatisticsCore.Factories
{
    public class AreaTypeFactory : BaseFactory, IFactory
    {
        public override Models.ILiiteriEntity Create(DbDataReader rdr)
        {
            var obj = new Models.AreaType();
            obj.Id = (int) rdr["AreaTypeId"];
            obj.DataSource = rdr["DataSource"].ToString();
            return obj;
        }
    }
}