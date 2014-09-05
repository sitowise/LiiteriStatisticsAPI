using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;

namespace LiiteriStatisticsCore.Factories
{
    class StatisticsResultFactory : BaseFactory, IFactory
    {
        public override Models.ILiiteriMarker Create(DbDataReader rdr)
        {
            var obj = new Models.StatisticsResult();
            obj.Value = (decimal) ((double) this.GetValueOrNull(rdr, "Value"));
            obj.AreaId = (int) rdr["AreaId"];
            obj.AreaName = (string) rdr["AreaName"].ToString();
            obj.AlternativeId = (string) rdr["AlternativeId"].ToString();
            obj.Year = (int) rdr["Year"];
            obj.PrivacyLimitTriggered = false;
            return obj;
        }
    }
}