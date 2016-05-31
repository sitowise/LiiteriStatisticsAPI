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

        public AreaTypeMappings(string xmlFile = null)
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
            if (xmlFile == null) {
                xmlFile = System.IO.Path.Combine(
                    (string) dataDirectory,
                    "AreaTypeMappings.xml");
            }
            Debug.WriteLine(string.Format(
                "Reading XmlFile from {0}", xmlFile));
            this.xdoc = XDocument.Load(xmlFile);
        }

        /* SubFromString/addAreaTable="true/false"
         * Used by AreaQuery, determines whether DimAlue should be
         * joined in the query */
        public bool GetDatabaseListAddAreaTable(string areaTypeId)
        {
            var queryElem = (
                from d in this.xdoc.Root
                    .Elements("SelectionAreaTypes").Single()
                    .Elements("SelectionAreaType")
                where d.Attribute("id").Value == areaTypeId
                select d
                    .Element("DatabaseSchema")
                    .Element("SubFromString")
                ).Single();
            if (queryElem == null) return false;
            if (queryElem.Attribute("addAreaTable") != null &&
                    queryElem.Attribute("addAreaTable").Value.ToLower() == "true") {
                return true;
            }
            return false;
        }

        /* SubFromString/disableList="true/false"
         * This is used to disable area listings for grids, since
         * they would return too many items */
        public bool GetDatabaseListDisabled(string areaTypeId)
        {
            var queryElem = (
                from d in this.xdoc.Root
                    .Elements("SelectionAreaTypes").Single()
                    .Elements("SelectionAreaType")
                where d.Attribute("id").Value == areaTypeId
                select d
                    .Element("DatabaseSchema")
                    .Element("SubFromString")
                ).Single();
            if (queryElem == null) return false;
            if (queryElem.Attribute("disableList") != null &&
                    queryElem.Attribute("disableList").Value.ToLower() == "true") {
                return true;
            }
            return false;
        }

        /* SelectionAreaType/DatabaseSchema/*
         * Return various fields from DatabaseSchema for
         * building SQL queries */
        public Dictionary<string, string> GetDatabaseSchema(string areaTypeId)
        {
            var queryElem = (
                from d in this.xdoc.Root
                    .Elements("SelectionAreaTypes").Single()
                    .Elements("SelectionAreaType")
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
                    "FilterJoinQuery",
                    "JoinQuery",
                    "SubOrderColumn",
                    "FunctionalAreaAvailabilityField",
                    "AreaYearAvailabilityField",
                    }) {
                if (queryElem.Element(key) != null) {
                    schema[key] = queryElem.Element(key).Value.ToString();
                } else {
                    schema[key] = null;
                }
            }
            return schema;
        }

        /* SelectionAreaType/DatabaseAreaTypes/DatabaseAreaType
         * Return all DatabaseAreaTypes for a SelectionAreaType.
         * Used when building the SQL query to reduce the list of
         * available areatypes */
        public int[] GetDatabaseAreaTypes(
            string areaTypeId)
        {
            if (areaTypeId == null) {
                throw new ArgumentNullException("areaTypeId must not be null!");
            }
            var databaseAreaTypes = (
                from d in this.xdoc.Root
                    .Elements("SelectionAreaTypes").Single()
                    .Elements("SelectionAreaType")
                where d.Attribute("id").Value == areaTypeId
                select d.Element("DatabaseAreaTypes")).Single();
            int[] retval = (
                from d in databaseAreaTypes.Descendants("DatabaseAreaType")
                select Convert.ToInt32(d.Attribute("id").Value)
                ).ToArray();
            return retval;
        }

        /* SelectionAreaType/DatabaseAreaTypes/DatabaseAreaType/primary="true/false"
         * Return primary databaseAreaType of a selectionAreaType.
         * Used by special statistics, which should not be aggregated,
         * as well as geometry filters. */
        public int GetPrimaryDatabaseAreaType(string areaTypeId)
        {
            if (areaTypeId == null) {
                throw new ArgumentNullException("areaTypeId must not be null!");
            }
            var databaseAreaTypes = (
                from d in this.xdoc.Root
                    .Elements("SelectionAreaTypes").Single()
                    .Elements("SelectionAreaType")
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

        /* SelectionAreaType/DatabaseAreaTypes/DatabaseAreaType/primary="true/false"
         * Get primary virtual areatype for database areaType
         * e.g. 2 -> municipality
         * Used by the special statistics indicators */
        public Models.AreaType GetPrimaryAreaType(int databaseAreaTypeId)
        {
            /* Debug.WriteLine(string.Format(
                "We need to find primary virtualAreaType for databaseAreaType {0}",
                databaseAreaTypeId)); */

            var factory = new Factories.AreaTypeFactory();

            var areaTypes = (
                from d in this.xdoc.Root
                    .Elements("SelectionAreaTypes").Single()
                    .Elements("SelectionAreaType")
                select d);
            foreach (var areaType in areaTypes) {
                /* Debug.WriteLine(string.Format(
                    "Let's consider {0}",
                    areaType.Attribute("id").Value)); */
                var found = (
                    from d in areaType
                        .Elements("DatabaseAreaTypes")
                        .Elements("DatabaseAreaType")
                    where
                        Convert.ToInt32(d.Attribute("id").Value) == databaseAreaTypeId &&
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

        /* SelectionAreaTypes/*
         * Returns all SelectionAreaTypes */
        public IEnumerable<Models.AreaType> GetAreaTypes()
        {
            var factory = new Factories.AreaTypeFactory();
            var areaTypes = (
                from d in this.xdoc.Root
                    .Elements("SelectionAreaTypes").Single()
                    .Elements("SelectionAreaType")
                select (Models.AreaType) factory.Create(d));
            return areaTypes;
        }

        public IEnumerable<Models.AreaType> GetAreaTypes(
            AreaTypeCategory category)
        {
            var factory = new Factories.AreaTypeFactory();
            var areaTypes = (
                from d in this.xdoc.Root
                    .Elements("SelectionAreaTypes").Single()
                    .Elements("SelectionAreaType")
                where
                    this.GetAreaTypeCategory(
                        d.Attribute("id").Value) == category
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
                    .Elements("SelectionAreaTypes").Single()
                    .Elements("SelectionAreaType")) {
                /* Debug.WriteLine(string.Format(
                    "Checking virtualAreaType:{0}",
                    sAreaType.Attribute("id").Value)); */
                bool found = false;
                foreach (var dAreaType in sAreaType
                        .Elements("DatabaseAreaTypes").Single()
                        .Elements("DatabaseAreaType")) {
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

        /* SelectionAreaType/category
         * Returns the category (functional/administrative) for an areaType. */
        public AreaTypeMappings.AreaTypeCategory GetAreaTypeCategory(
            string areaTypeId)
        {
            string category = (
                from d in this.xdoc.Root
                    .Elements("SelectionAreaTypes").Single()
                    .Elements("SelectionAreaType")
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

        /* SelectionAreaType/DatabaseSchema/ExtraAreaFields/*
         * Returns extra area fields, for adding parent areas
         * for "*_type" functional areas */
        public Dictionary<string, string> GetExtraAreaFields(string areaTypeId)
        {
            if (areaTypeId == null) {
                throw new ArgumentNullException("areaTypeId must not be null!");
            }
            var retval = new Dictionary<string, string>();

            var selectionAreaType = (
                from d in this.xdoc.Root
                    .Elements("SelectionAreaTypes").Single()
                    .Elements("SelectionAreaType")
                where d.Attribute("id").Value == areaTypeId
                select d.Element("DatabaseSchema")).Single();

            var extraFields = (
                from d in selectionAreaType
                    .Elements("ExtraAreaFields")
                    .Elements("ExtraAreaField")
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