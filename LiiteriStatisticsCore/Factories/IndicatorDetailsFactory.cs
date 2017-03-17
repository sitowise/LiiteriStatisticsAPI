using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;

namespace LiiteriStatisticsCore.Factories
{
    public class IndicatorDetailsFactory : BaseFactory, IFactory
    {
        public override Models.ILiiteriMarker Create(DbDataReader rdr)
        {
            var obj = new Models.IndicatorDetails();
            obj.Id = (int) rdr["Id"];
            obj.Name = rdr["Name"].ToString();
            obj.OrderNumber = (int?) this.GetValueOrNull(rdr, "OrderNumber");

            obj.DecimalCount = (int?) this.GetValueOrNull(rdr, "DecimalCount");

            obj.DisplayUnitId = (int?) this.GetValueOrNull(rdr, "DisplayUnitId");
            obj.InternalUnitId = (int?) this.GetValueOrNull(rdr, "InternalUnitId");
            obj.Unit = (string) this.GetValueOrNull(rdr, "Unit");
            obj.Description = rdr["Description"].ToString(); ;
            obj.AdditionalInformation = rdr["AdditionalInformation"].ToString();
            obj.CalculationType = (int) rdr["CalculationType"];

            obj.ZeroVisibility = (bool) rdr["ZeroVisibility"];

            obj.ProcessingStage = null;

            obj.TimeSpan = (string) this.GetValueOrNull(rdr, "TimeSpan");
            obj.TimeSpanDetails = (string) this.GetValueOrNull(rdr, "TimeSpanDetails");

            int? privacyLimitStatisticsId =
                (int?) this.GetValueOrNull(rdr, "PrivacyLimitStatisticsId");
            if (privacyLimitStatisticsId != null) {
                obj.PrivacyLimit = new Models.PrivacyLimit();
                obj.PrivacyLimit.RefId = (int) privacyLimitStatisticsId;
                obj.PrivacyLimit.GreaterThan = (int) rdr["PrivacyLimitGreaterThan"];
                obj.PrivacyLimit.Description =
                    rdr["PrivacyLimitDescription"].ToString();
            } else {
                obj.PrivacyLimit = null;
            }

            return obj;
        }
    }
}