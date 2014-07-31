using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;

namespace LiiteriStatisticsCore.Factories
{
    public class ThemeFactory : BaseFactory, IFactory
    {
        public override Models.ILiiteriMarker Create(DbDataReader rdr)
        {
            var obj = new Models.Theme();
            obj.Id = (int) rdr["Id"];
            obj.Name = rdr["Name"].ToString();
            obj.ParentId = (int?) this.GetValueOrNull(rdr, "ParentId");
            return obj;
        }
    }
}