using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiiteriStatisticsCore.Factories
{
    public class FunctionalAreaAvailabilityFactory : BaseFactory, IFactory
    {
        public override Models.ILiiteriMarker Create(DbDataReader rdr)
        {
            var obj = new Models.FunctionalAreaAvailability();

            obj.Id = (int) this.GetNumber(rdr, "AreaId");
            obj.Name = rdr["AreaName"].ToString();
            obj.AlternativeId = rdr["AlternativeId"].ToString();
            obj.AreaType = rdr["AreaType"].ToString();
            obj.Year = (int?) this.GetValueOrNull(rdr, "Year");
            obj.OrderNumber = (int) this.GetNumber(rdr, "OrderNumber");

            var available = new List<string>();

            foreach (System.Data.DataRow drow in rdr.GetSchemaTable().Rows) {
                string field = drow.ItemArray[0].ToString();
                if (!field.EndsWith("_avail")) {
                    continue;
                }
                if (this.GetNumber(rdr, field) == 0) {
                    continue;
                }
                // strip the _avail
                string areaType = field.Substring(0, field.Length - 6);
                available.Add(areaType);
            }

            obj.AvailableFunctionalAreas = available.ToArray();

            return obj;
        }
    }
}