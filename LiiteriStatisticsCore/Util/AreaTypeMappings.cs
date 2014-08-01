using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Diagnostics;

namespace LiiteriStatisticsCore.Util
{
    /*
     * Make the instance of this class static, so the XML file is
     * only loaded once
     */
    public class AreaTypeMappings
    {
        XDocument xdoc;

        public AreaTypeMappings()
        {
            string XmlFile = System.IO.Path.Combine(
                AppDomain.CurrentDomain.GetData("DataDirectory").ToString(),
                "AreaTypeMappings.xml");
            Debug.WriteLine(string.Format(
                "Reading XmlFile from {0}", XmlFile));
            this.xdoc = XDocument.Load(XmlFile);
        }

        public string GetAreaTable(int areaTypeId)
        {
            return (
                from d in this.xdoc.Root.Descendants("SelectionAreaType")
                where Convert.ToInt32(d.Attribute("id").Value) == areaTypeId
                select d.Element("DatabaseTable").Value).Single();
        }

        public string GetAreaColumn(int areaTypeId)
        {
            return (
                from d in this.xdoc.Root.Descendants("SelectionAreaType")
                where Convert.ToInt32(d.Attribute("id").Value) == areaTypeId
                select d.Element("DatabaseColumn").Value).Single();
        }

        public int? GetDatabaseAreaType(
            int areaTypeId,
            int[] availableAreaTypes)
        {
            var databaseAreaTypes = (
                from d in this.xdoc.Root.Descendants("SelectionAreaType")
                where Convert.ToInt32(d.Attribute("id").Value) == areaTypeId
                select d.Element("DatabaseAreaTypes")).Single();
            foreach (var databaseAreaType in
                    databaseAreaTypes.Descendants("DatabaseAreaType")) {
                int id = Convert.ToInt32(
                    databaseAreaType.Attribute("id").Value);
                if (availableAreaTypes.Contains(id)) {
                    return id;
                }
            }
            return null;
        }
    }
}