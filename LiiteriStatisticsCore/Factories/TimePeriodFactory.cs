using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;

namespace LiiteriStatisticsCore.Factories
{
    public class TimePeriodFactory : BaseFactory, IFactory
    {
        public override Models.ILiiteriEntity Create(DbDataReader rdr)
        {
            var obj = new Models.TimePeriod();
            obj.Id = (int) rdr["PeriodId"];
            obj.AreaTypes = new List<Models.AreaType>();
            return obj;
        }
    }
}