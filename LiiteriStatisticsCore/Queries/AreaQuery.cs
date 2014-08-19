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
            string queryString =
                AreaTypeMappings.GetDatabaseListQuery(this.AreaTypeIdIs);

            var fields = new List<string>();
            fields.Add(string.Format("'{0}' AS AreaType", this.AreaTypeIdIs));

            var fromList = new List<string>();

            string fieldString = ""; // SELECT xxx, yyy
            string fromString = ""; // FROM xxx yyy
            string whereString = ""; // WHERE xxx, yyy

            bool addAreaTable =
                AreaTypeMappings.GetDatabaseListAddAreaTable(this.AreaTypeIdIs);
            if (addAreaTable) {
                fromString += "LEFT OUTER JOIN DimAlue A ON A.Alue_ID = A2.Alue_ID";
                foreach (Models.AreaType areaType in
                        AreaTypeMappings.GetAreaTypes()) {
                    string areaTypeName = areaType.Id;
                    string columnName1 = AreaTypeMappings.GetDatabaseIdColumn(
                        areaTypeName);
                    string columnName2 = "parent_" + areaTypeName;
                    if (columnName1 == "NULL") continue;
                    fields.Add(string.Format("{0} AS {1}",
                        columnName1, columnName2));
                }
            }

            if (fields.Count > 0) {
                fieldString = string.Join(", ", fields) + ", ";
            }
            if (fromList.Count > 0) {
                fromString = string.Join(" ", fromList);
            }
            if (this.whereList.Count > 0) {
                whereString = " AND " + string.Join(", ", fromList);
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