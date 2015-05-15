using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;

namespace LiiteriStatisticsCore.Factories
{
    public class AnnotationFactory : BaseFactory, IFactory
    {
        public override Models.ILiiteriMarker Create(DbDataReader rdr)
        {
            var obj = new Models.Annotation();
            obj.Description = (string) rdr["Annotation"];
            obj.OrganizationName = (string) rdr["AnnotationOrganizationName"];
            obj.OrganizationShort = (string) rdr["AnnotationOrganizationShort"];
            obj.OrganizationNumber = (string) rdr["AnnotationOrganizationNumber"];
            return obj;
        }
    }
}