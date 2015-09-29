using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;

namespace LiiteriStatisticsCore.Factories
{
    public class AccessRightFactory : BaseFactory, IFactory
    {
        public override Models.ILiiteriMarker Create(DbDataReader rdr)
        {
            var obj = new Models.AccessRight();
            obj.Id = (int) rdr["AccessRightId"];
            obj.Description = (string) rdr["AccessRightDescription"];
            return obj;
        }
    }
}