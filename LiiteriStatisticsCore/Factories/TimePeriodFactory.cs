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
        public override Models.ILiiteriMarker Create(DbDataReader rdr)
        {
            var obj = new Models.TimePeriod();
            obj.Id = (int) rdr["PeriodId"];
            obj.DataAreaTypes = new List<Models.DataAreaType>();
            return obj;
        }
    }
}