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
     * only loaded once. Keep all XML magic in this file, out of sight from
     * the rest of the project!
     */
    public class AreaTypeMappings
    {
        XDocument xdoc;

        public AreaTypeMappings()
        {
            object dataDirectory =
                AppDomain.CurrentDomain.GetData("DataDirectory");

            // Check BaseDirectory in case we are running UnitTests
            // BaseDirectory is probably bin\Debug\
            string baseDirectory =
                AppDomain.CurrentDomain.BaseDirectory;

            if (dataDirectory == null) {
                dataDirectory = baseDirectory;
            } else {
                dataDirectory = dataDirectory.ToString();
            }
            if (dataDirectory == null) {
                throw new System.IO.DirectoryNotFoundException(
                    "Unable to figure out Data Directory");
            }
            string XmlFile = System.IO.Path.Combine(
                (string) dataDirectory,
                "AreaTypeMappings.xml");
            Debug.WriteLine(string.Format(
                "Reading XmlFile from {0}", XmlFile));
            this.xdoc = XDocument.Load(XmlFile);
        }

        public string GetDatabaseIdColumn(string areaTypeId)
        {
            return this.GetValue(areaTypeId, "DatabaseIdColumn");
        }

        public string GetDatabaseNameColumn(string areaTypeId)
        {
            return this.GetValue(areaTypeId, "DatabaseNameColumn");
        }

        public string GetDatabaseJoinQuery(string areaTypeId)
        {
            return this.GetValue(areaTypeId, "DatabaseJoinQuery");
        }

        public string GetDatabaseListQuery(string areaTypeId)
        {
            return this.GetValue(areaTypeId, "DatabaseListQuery");
        }

        public bool GetDatabaseListAddAreaTable(string areaTypeId)
        {
            var queryElem = (
                from d in this.xdoc.Root.Descendants("SelectionAreaType")
                where d.Attribute("id").Value == areaTypeId
                select d.Element("DatabaseListQuery")).Single();
            if (queryElem.Attribute("addAreaTable") != null &&
                    queryElem.Attribute("addAreaTable").Value == "true") {
                return true;
            }
            return false;
        }

        private string GetValue(string areaTypeId, string key)
        {
            return (
                from d in this.xdoc.Root.Descendants("SelectionAreaType")
                where d.Attribute("id").Value == areaTypeId
                select d.Element(key).Value).Single();
        }

        public int? GetDatabaseAreaType(
            string areaTypeId,
            int[] availableAreaTypes)
        {
            if (areaTypeId == null) {
                throw new ArgumentNullException("areaTypeId must not be null!");
            }
            var databaseAreaTypes = (
                from d in this.xdoc.Root.Descendants("SelectionAreaType")
                where d.Attribute("id").Value == areaTypeId
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

        public IEnumerable<Models.AreaType> GetAreaTypes()
        {
            var factory = new Factories.AreaTypeFactory();
            var areaTypes = (
                from d in this.xdoc.Root.Descendants("SelectionAreaType")
                select (Models.AreaType) factory.Create(d));
            return areaTypes;
        }
    }
}