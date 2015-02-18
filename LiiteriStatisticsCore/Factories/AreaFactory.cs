using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;

namespace LiiteriStatisticsCore.Factories
{
    public class AreaFactory : BaseFactory, IFactory
    {
        public override Models.ILiiteriMarker Create(DbDataReader rdr)
        {
            var obj = new Models.Area();
            obj.Id = (int) this.GetNumber(rdr, "AreaId");
            obj.Name = rdr["AreaName"].ToString();
            obj.AlternativeId = rdr["AlternativeId"].ToString();
            obj.AreaType = rdr["AreaType"].ToString();
            obj.Year = (int?) this.GetValueOrNull(rdr, "Year");

            List<Models.Area> parentAreas = new List<Models.Area>();
            obj.ParentAreas = parentAreas;
            return obj;
        }
    }
}