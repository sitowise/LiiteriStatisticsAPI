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

        public override string GetQueryString()
        {
            Dictionary<string, string> schema =
                AreaTypeMappings.GetDatabaseSchema(this.AreaTypeIdIs);

            var fromList = new List<string>();
            var fields = new List<string>();

            fields.Add(string.Format("'{0}' AS AreaType", this.AreaTypeIdIs));

            string queryString = "SELECT {0} {1} {2}";

            if (schema["SubIdColumn"] != null &&
                    schema["SubIdColumn"].Length > 0) {
                fields.Add(string.Format(
                    "{0} AS AreaId", schema["SubIdColumn"]));
            } else {
                fields.Add("NULL AS AreaId");
            }

            if (schema["SubNameColumn"] != null &&
                    schema["SubNameColumn"].Length > 0) {
                fields.Add(string.Format(
                    "{0} AS AreaName", schema["SubNameColumn"]));
            } else {
                fields.Add("NULL AS AreaName");
            }

            if (schema["SubAlternativeIdColumn"] != null &&
                    schema["SubAlternativeIdColumn"].Length > 0) {
                fields.Add(string.Format(
                    "{0} AS AlternativeId", schema["SubAlternativeIdColumn"]));
            } else {
                fields.Add("NULL AS AlternativeId");
            }

            if (schema["SubYearColumn"] != null &&
                    schema["SubYearColumn"].Length > 0) {
                fields.Add(string.Format(
                    "{0} AS Year", schema["SubYearColumn"]));
            } else {
                fields.Add("NULL AS Year");
            }

            if (schema["SubFromString"] != null &&
                    schema["SubFromString"].Length > 0) {
                fromList.Add(schema["SubFromString"]);
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
                    string columnName2 = "parent_" + areaTypeName;
                    if (columnName1 == "NULL") continue;
                    fields.Add(string.Format("{0} AS {1}",
                        columnName1, columnName2));
                }
            }

            if (fields.Count > 0) {
                fieldString = string.Join(", ", fields);
            }
            if (fromList.Count > 0) {
                fromString = " FROM " + string.Join(" ", fromList);
            }
            if (this.whereList.Count > 0) {
                whereString = " WHERE " + string.Join(", ", fromList);
            }
            queryString = string.Format(queryString,
                fieldString, fromString, whereString);

            Debug.WriteLine(queryString);
            if (queryString == null || queryString.Length == 0) {
                throw new Exception("No area list available for this area type!");
            }

            return queryString;
        }
    }
}