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

        public enum AreaTypeCategory
        {
            FunctionalArea,
            AdministrativeArea,
        };

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
                    "SubFromString",
                    "SubWhereString",
                    "InnerJoinQuery",
                    "RightJoinQuery",
                    "FilterJoinQuery",
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

        /* Get primary virtual areatype for database areaType
         * e.g. 2 -> municipality
         * Used by the special statistics indicators */
        public Models.AreaType GetPrimaryAreaType(int databaseAreaTypeId)
        {
            /* Debug.WriteLine(string.Format(
                "We need to find primary virtualAreaType for databaseAreaType {0}",
                databaseAreaTypeId)); */

            var factory = new Factories.AreaTypeFactory();

            var areaTypes = (
                from d in this.xdoc.Root.Descendants("SelectionAreaType")
                select d);
            foreach (var areaType in areaTypes) {
                /* Debug.WriteLine(string.Format(
                    "Let's consider {0}",
                    areaType.Attribute("id").Value)); */
                var found = (
                    from d in areaType
                        .Descendants("DatabaseAreaTypes")
                        .Descendants("DatabaseAreaType")
                    where
                        d.Attribute("primary") != null &&
                        d.Attribute("primary").Value.ToLower() == "true"
                    select d);
                if (found.Count() > 0) {
                    /* Debug.WriteLine(string.Format(
                        "We consider {0} to be found",
                        found.Single().Attribute("id").Value.ToString())); */
                    return (Models.AreaType) factory.Create(areaType);
                } else {
                    /* Debug.WriteLine(string.Format(
                        "No match, keep looking")); */
                }
            }
            throw new Exception(
                "No primary virtual areaType for this databaseAreaType!");
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

        public AreaTypeMappings.AreaTypeCategory GetAreaTypeCategory(
            string areaTypeId)
        {
            string category = (
                from d in this.xdoc.Root.Descendants("SelectionAreaType")
                where d.Attribute("id").Value == areaTypeId
                select d.Attribute("category").Value.ToString()).Single();

            switch (category) {
                case "functional":
                    return AreaTypeMappings.AreaTypeCategory.FunctionalArea;
                case "administrative":
                    return AreaTypeMappings.AreaTypeCategory.AdministrativeArea;
                default:
                    throw new Exception(
                        "Unknown areatype category: " + category);
            }
        }

        public Dictionary<string, string> GetExtraAreaFields(string areaTypeId)
        {
            if (areaTypeId == null) {
                throw new ArgumentNullException("areaTypeId must not be null!");
            }
            var retval = new Dictionary<string, string>();

            var selectionAreaType = (
                from d in this.xdoc.Root.Descendants("SelectionAreaType")
                where d.Attribute("id").Value == areaTypeId
                select d.Element("DatabaseSchema")).Single();

            var extraFields = (
                from d in selectionAreaType
                    .Descendants("ExtraAreaFields")
                    .Descendants("ExtraAreaField")
                select d);

            foreach (var extraField in extraFields) {
                // parent_foo_bar_blah
                string key = extraField.Attribute("name").Value.ToString();
                // A2.Field_ID
                string value = extraField.Value.ToString();
                retval[key] = value;
            }

            return retval;
        }
    }
}