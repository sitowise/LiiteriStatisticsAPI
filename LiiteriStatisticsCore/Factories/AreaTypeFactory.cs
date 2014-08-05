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
            return obj;
        }
    }
}