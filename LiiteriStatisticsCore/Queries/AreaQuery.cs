using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace LiiteriStatisticsCore.Queries
{
    public class AreaQuery : SqlQuery, ISqlQuery
    {
        private static Util.AreaTypeMappings
            AreaTypeMappings = new Util.AreaTypeMappings();

        List<string> whereList;

        public AreaQuery() : base()
        {
            this.whereList = new List<string>();
        }

        public string AreaTypeIdIs { get; set; }

        public string AreaFilterQueryString { get; set; }

        private void SetFilters()
        {
            if (this.AreaFilterQueryString == null) return;

            if (AreaTypeMappings.GetAreaTypeCategory(this.AreaTypeIdIs) !=
                    Util.AreaTypeMappings.AreaTypeCategory.AdministrativeArea) {
                throw new NotImplementedException(
                    "Area filters are only available for administrative areas");
            }

            var parser = new Parsers.AreaFilterParser();

            parser.ValueHandler = delegate (object val)
            {
                return "@" + this.Parameters.AddValue(val);
            };

            parser.IdHandler = delegate (string name)
            {
                if (AreaTypeMappings.GetAreaTypeCategory(name) !=
                        Util.AreaTypeMappings.AreaTypeCategory.AdministrativeArea) {
                    throw new NotImplementedException(
                        "Area filtering can only be done with administrative areas");
                }
                var schema = AreaTypeMappings.GetDatabaseSchema(name);
                string idColumn = schema["MainIdColumn"];
                idColumn = SchemaDataFormat(idColumn);
                return idColumn;
            };

            parser.SpatialHandler = delegate (
                string geom1,
                string geom2,
                string func)
            {
                throw new NotImplementedException(
                    "Spatial filtering not supported by area queries");
            };

            string whereString = parser.Parse(this.AreaFilterQueryString);
            this.whereList.Add(whereString);
        }

        /* The column aliases are different for statistics and
         * commuteStatistics, so AreaTypeMappings only provides
         * string templates for various settings */
        private static string SchemaDataFormat(string str)
        {
            return string.Format(str,
                "A2", // {0}: sub-table (e.g. DimKunta or DimRuutu)
                "A"); // {1}: main table (DimAlue)
        }

        public override string GetQueryString()
        {
            if (AreaTypeMappings.GetDatabaseListDisabled(this.AreaTypeIdIs)) {
                throw new Exception("Listing disabled for this areaType!");
            }

            Dictionary<string, string> schema =
                AreaTypeMappings.GetDatabaseSchema(this.AreaTypeIdIs);

            var fromList = new List<string>();
            var fields = new List<string>();

            fields.Add(string.Format("'{0}' AS AreaType", this.AreaTypeIdIs));

            string queryString = "SELECT {0} {1} {2}";

            if (schema["SubIdColumn"] != null &&
                    schema["SubIdColumn"].Length > 0) {
                fields.Add(string.Format("{0} AS AreaId",
                    SchemaDataFormat(schema["SubIdColumn"])));
            } else {
                fields.Add("-1 AS AreaId");
            }

            if (schema["SubNameColumn"] != null &&
                    schema["SubNameColumn"].Length > 0) {
                fields.Add(string.Format("{0} AS AreaName",
                    SchemaDataFormat(schema["SubNameColumn"])));
            } else {
                fields.Add("NULL AS AreaName");
            }

            if (schema["SubAlternativeIdColumn"] != null &&
                    schema["SubAlternativeIdColumn"].Length > 0) {
                fields.Add(string.Format("{0} AS AlternativeId",
                    SchemaDataFormat(schema["SubAlternativeIdColumn"])));
            } else {
                fields.Add("NULL AS AlternativeId");
            }

            if (schema["SubYearColumn"] != null &&
                    schema["SubYearColumn"].Length > 0) {
                fields.Add(string.Format("{0} AS Year",
                    SchemaDataFormat(schema["SubYearColumn"])));
            } else {
                fields.Add("NULL AS Year");
            }

            if (schema["SubOrderColumn"] != null &&
                    schema["SubOrderColumn"].Length > 0) {
                fields.Add(string.Format("{0} AS OrderNumber",
                    SchemaDataFormat(schema["SubOrderColumn"])));
            } else {
                fields.Add("0 AS OrderNumber");
            }

            if (schema["SubFromString"] != null &&
                    schema["SubFromString"].Length > 0) {
                fromList.Add(SchemaDataFormat(schema["SubFromString"]));
            }

            if (schema["SubWhereString"] != null &&
                    schema["SubWhereString"].Length > 0) {
                this.whereList.Add(string.Format("({0})",
                    SchemaDataFormat(schema["SubWhereString"])));
            }

            string fieldString = ""; // SELECT xxx, yyy
            string fromString = ""; // FROM xxx yyy
            string whereString = ""; // WHERE xxx, yyy

            if (this.AreaFilterQueryString != null &&
                    AreaTypeMappings.GetAreaTypeCategory(this.AreaTypeIdIs) !=
                        Util.AreaTypeMappings.AreaTypeCategory.AdministrativeArea) {
                throw new NotImplementedException(
                    "Area filters are only available for administrative areas");
            }

            bool addAreaTable =
                AreaTypeMappings.GetDatabaseListAddAreaTable(this.AreaTypeIdIs);
            if (addAreaTable) {
                fromList.Add(
                    "LEFT OUTER JOIN DimAlue A ON A.Alue_ID = A2.Alue_ID");
                foreach (Models.AreaType areaType in
                        AreaTypeMappings.GetAreaTypes()) {
                    string areaTypeName = areaType.Id;
                    string columnName1 = AreaTypeMappings.GetDatabaseSchema(
                        areaTypeName)["MainIdColumn"];
                    if (columnName1.StartsWith("F_")) continue;
                    columnName1 = SchemaDataFormat(columnName1);
                    string columnName2 = "parent_" + areaTypeName;
                    if (columnName1 == null ||
                        columnName1.Length == 0 ||
                        columnName1 == "NULL") continue;
                    fields.Add(string.Format("{0} AS {1}",
                        columnName1, columnName2));
                }
                if (AreaTypeMappings.GetAreaTypeCategory(this.AreaTypeIdIs) ==
                        Util.AreaTypeMappings.AreaTypeCategory.AdministrativeArea) {
                    fields.Add(string.Format("-1 AS parent_finland"));
                }

                this.SetFilters();
            }

            /* extra fields, this is a special additional way of adding
             * parent areas. previously we only did the addAreaTable
             * stuff above */
            IDictionary<string, string> extraFields =
                AreaTypeMappings.GetExtraAreaFields(this.AreaTypeIdIs);
            foreach (var extraField in extraFields) {
                Debug.WriteLine("I am dealing with {0}", extraField.Key);
                fields.Add(string.Format("{0} AS {1}",
                    SchemaDataFormat(extraField.Value), extraField.Key));
            }

            if (fields.Count > 0) {
                fieldString = string.Join(", ", fields);
            }
            if (fromList.Count > 0) {
                fromString = " FROM " + string.Join(" ", fromList);
            }
            if (this.whereList.Count > 0) {
                whereString = " WHERE " + string.Join(", ", whereList);
            }
            queryString = string.Format(queryString,
                fieldString, fromString, whereString);

            if (queryString == null || queryString.Length == 0) {
                throw new Exception("No area list available for this area type!");
            }

            return queryString;
        }
    }
}