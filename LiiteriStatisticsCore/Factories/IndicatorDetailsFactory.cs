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
        public override Models.ILiiteriEntity Create(DbDataReader rdr)
        {
            var obj = new Models.IndicatorDetails();
            obj.Id = (int) rdr["Id"];
            obj.Name = rdr["Name"].ToString();

            obj.DecimalCount = (int?) this.GetValueOrNull(rdr, "DecimalCount");

            obj.DisplayUnitId = (int) rdr["DisplayUnitId"];
            obj.InternalUnitId = (int) rdr["InternalUnitId"];
            obj.Description = rdr["Description"].ToString(); ;
            obj.AdditionalInformation = rdr["AdditionalInformation"].ToString(); ;
            obj.CalculationType = (int) rdr["CalculationType"];

            return obj;
        }
    }
}