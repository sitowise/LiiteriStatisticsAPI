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
        private static LiiteriStatisticsCore.Util.AreaTypeMappings
            AreaTypeMappings = new LiiteriStatisticsCore.Util.AreaTypeMappings();

        List<string> whereList;

        public AreaQuery() : base()
        {
            this.whereList = new List<string>();
        }

        public string AreaTypeIdIs { get; set; }

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