using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;

namespace LiiteriStatisticsCore.Factories
{
    public class AreaTypeFactory
    {
        public Models.ILiiteriMarker Create(System.Xml.Linq.XElement element)
        {
            var obj = new Models.AreaType();
            obj.Id = element.Attribute("id").Value.ToString();
            obj.Description = element.Element("Description").Value.ToString();
            obj.Category = element.Attribute("category").Value.ToString();
            return obj;
        }

        /* Special factory method used when constructing AreaTypes in the
         * indicator */
        public Models.ILiiteriMarker Create(
            Models.AreaType areaType,
            DbDataReader rdr)
        {
            var obj = new Models.AreaType();
            obj.Id = areaType.Id;
            obj.Description = areaType.Description;
            obj.Category = areaType.Category;
            obj.DataSource = (string) rdr["DataSource"].ToString();
            return obj;
        }
    }
}