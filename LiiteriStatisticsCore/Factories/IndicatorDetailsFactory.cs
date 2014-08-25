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

            obj.DecimalCount = (int?) this.GetValueOrNull(rdr, "DecimalCount");

            obj.DisplayUnitId = (int) rdr["DisplayUnitId"];
            obj.InternalUnitId = (int) rdr["InternalUnitId"];
            obj.Unit = (string) rdr["Unit"];
            obj.Description = rdr["Description"].ToString(); ;
            obj.AdditionalInformation = rdr["AdditionalInformation"].ToString(); ;
            obj.CalculationType = (int) rdr["CalculationType"];

            obj.ProcessingStage = null;
            obj.TimeSpan = (string) rdr["TimeSpan"];
            obj.TimeSpanDetails = (string) rdr["TimeSpanDetails"];

            return obj;
        }
    }
}