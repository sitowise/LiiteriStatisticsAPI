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
            obj.Id = (int) rdr["AreaId"];
            obj.Name = rdr["AreaName"].ToString();
            obj.AlternativeId = rdr["AlternativeId"].ToString();
            obj.Year = (int?) this.GetValueOrNull(rdr, "Year");
            return obj;
        }
    }
}