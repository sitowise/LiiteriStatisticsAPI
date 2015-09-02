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
            //obj.Value = (decimal?) ((double?) this.GetValueOrNull(rdr, "Value"));
            obj.Value = (decimal) ((double) this.GetValueOrNull(rdr, "Value"));
            obj.AreaId = (int) this.GetNumber(rdr, "AreaId");
            obj.AreaName = (string) rdr["AreaName"].ToString();
            obj.AlternativeId = (string) rdr["AlternativeId"].ToString();
            obj.Year = (int) rdr["Year"];
            obj.PrivacyLimitTriggered = false;

            /* this is a special case for community statistics */
            if (this.HasColumn(rdr, "TriggerPrivacyLimit")) {
                if ((int) rdr["TriggerPrivacyLimit"] == 1) {
                    obj.PrivacyLimitTriggered = true;
                    obj.Value = null;
                }
            }

            return obj;
        }
    }
}