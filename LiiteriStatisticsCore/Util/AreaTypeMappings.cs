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

        public bool GetDatabaseListAddAreaTable(string areaTypeId)
        {
            var queryElem = (
                from d in this.xdoc.Root.Descendants("SelectionAreaType")
                where d.Attribute("id").Value == areaTypeId
                select d.Element("DatabaseSchema").Element("SubFromString")
                ).Single();
            if (queryElem == null) return false;
            if (queryElem.Attribute("addAreaTable") != null &&
                    queryElem.Attribute("addAreaTable").Value.ToLower() == "true") {
                return true;
            }
            return false;
        }

        public bool GetDatabaseListDisabled(string areaTypeId)
        {
            var queryElem = (
                from d in this.xdoc.Root.Descendants("SelectionAreaType")
                where d.Attribute("id").Value == areaTypeId
                select d.Element("DatabaseSchema").Element("SubFromString")
                ).Single();
            if (queryElem == null) return false;
            if (queryElem.Attribute("disableList") != null &&
                    queryElem.Attribute("disableList").Value.ToLower() == "true") {
                return true;
            }
            return false;
        }

        public Dictionary<string, string> GetDatabaseSchema(string areaTypeId)
        {
            var queryElem = (
                from d in this.xdoc.Root.Descendants("SelectionAreaType")
                where d.Attribute("id").Value == areaTypeId
                select d.Element("DatabaseSchema")
                ).Single();
            Dictionary<string, string> schema =
                new Dictionary<string, string>();
            foreach (string key in new string[]{
                    "MainIdColumn",
                    "SubIdColumn",
                    "SubNameColumn",
                    "SubAlternativeIdColumn",
                    "SubYearColumn",
                    "GeometryColumn",
                    "GeometryJoin",
                    "SubFromString",
                    "JoinQuery",
                    "ListQuery",
                    }) {
                if (queryElem.Element(key) != null) {
                    schema[key] = queryElem.Element(key).Value.ToString();
                } else {
                    schema[key] = null;
                }
            }
            return schema;
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

        public int[] GetDatabaseAreaTypes(
            string areaTypeId)
        {
            if (areaTypeId == null) {
                throw new ArgumentNullException("areaTypeId must not be null!");
            }
            var databaseAreaTypes = (
                from d in this.xdoc.Root.Descendants("SelectionAreaType")
                where d.Attribute("id").Value == areaTypeId
                select d.Element("DatabaseAreaTypes")).Single();
            int[] retval = (
                from d in databaseAreaTypes.Descendants("DatabaseAreaType")
                select Convert.ToInt32(d.Attribute("id").Value)
                ).ToArray();
            return retval;
        }

        public int GetPrimaryDatabaseAreaType(string areaTypeId)
        {
            if (areaTypeId == null) {
                throw new ArgumentNullException("areaTypeId must not be null!");
            }
            var databaseAreaTypes = (
                from d in this.xdoc.Root.Descendants("SelectionAreaType")
                where d.Attribute("id").Value == areaTypeId
                select d.Element("DatabaseAreaTypes")).Single();
            var retval = (
                from d in databaseAreaTypes.Descendants("DatabaseAreaType")
                where (
                    d.Attribute("primary") != null &&
                    d.Attribute("primary").Value.ToLower() == "true")
                select Convert.ToInt32(d.Attribute("id").Value)
                );
            if (retval.Count() == 0) {
                throw new Exception(
                    "No primary DatabaseAreaType for this areaType!");
            }
            return (int) retval.Single();
        }

        public string GetAvailabilityExpression(
            string areaTypeId,
            int databaseAreaTypeId)
        {
            if (areaTypeId == null) {
                throw new ArgumentNullException("areaTypeId must not be null!");
            }
            var databaseAreaTypes = (
                from d in this.xdoc.Root.Descendants("SelectionAreaType")
                where d.Attribute("id").Value == areaTypeId
                select d.Element("DatabaseAreaTypes")).Single();
            var retval = (
                from d in databaseAreaTypes.Descendants("DatabaseAreaType")
                where Convert.ToInt32(d.Attribute("id").Value) == databaseAreaTypeId
                select d.Attribute("availabilityExpression").Value
                );
            if (retval.Count() == 0) {
                throw new Exception(string.Format(
                    "No availabilityExpression found for this databaseAreaType on {0}!",
                    areaTypeId));
            }
            return (string) retval.Single().ToString();
        }

        public IEnumerable<Models.AreaType> GetAreaTypes()
        {
            var factory = new Factories.AreaTypeFactory();
            var areaTypes = (
                from d in this.xdoc.Root.Descendants("SelectionAreaType")
                select (Models.AreaType) factory.Create(d));
            return areaTypes;
        }

        /* This is used for when we know statistics data is stored in one
         * databaseAreaType (for example (int) 2), and we want to know
         * what virtualAreaTypes (for example (string) "municipality") can be
         * used to query that statistic */
        public IEnumerable<Models.AreaType> GetAreaTypes(int databaseAreaType)
        {
            /* Debug.WriteLine(string.Format(
                "Getting virtualAreaTypes for databaseAreaType:{0}",
                databaseAreaType)); */

            var factory = new Factories.AreaTypeFactory();
            IList<Models.AreaType> areaTypes = new List<Models.AreaType>();

            foreach (var sAreaType in this.xdoc.Root
                    .Descendants("SelectionAreaType")) {
                /* Debug.WriteLine(string.Format(
                    "Checking virtualAreaType:{0}",
                    sAreaType.Attribute("id").Value)); */
                bool found = false;
                foreach (var dAreaType in sAreaType
                        .Descendants("DatabaseAreaTypes")
                        .Descendants("DatabaseAreaType")) {
                    int id = Convert.ToInt32(dAreaType.Attribute("id").Value);
                    /* Debug.WriteLine(string.Format(
                        "  Checking databaseAreaType:{0}", id)); */
                    if (id == databaseAreaType) {
                        // Debug.WriteLine("    Match! breaking...");
                        found = true;
                        break;
                    }
                }
                if (found) {
                    // Debug.WriteLine("Some stuff was found, add to areaTypes");
                    areaTypes.Add((Models.AreaType) factory.Create(sAreaType));
                }
            }
            return areaTypes;
        }
    }
}