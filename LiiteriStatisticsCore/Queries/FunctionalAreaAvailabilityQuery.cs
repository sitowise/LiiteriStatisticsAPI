using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiiteriStatisticsCore.Queries
{
    public class FunctionalAreaAvailabilityQuery : SqlQuery, ISqlQuery
    {
        private static Util.AreaTypeMappings
            AreaTypeMappings = new Util.AreaTypeMappings();

        List<string> whereList;

        public FunctionalAreaAvailabilityQuery() : base()
        {
            this.whereList = new List<string>();
        }

        public string AreaTypeIdIs { get; set; }

        public string AreaFilterQueryString { get; set; }

        public int YearIs
        {
            get
            {
                return (int) this.Parameters["YearIs"].Value;
            }
            set
            {
                this.Parameters.Add("YearIs", value);
            }
        }

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
            if (AreaTypeMappings.GetAreaTypeCategory(this.AreaTypeIdIs) !=
                    Util.AreaTypeMappings.AreaTypeCategory.AdministrativeArea) {
                throw new Exception(
                    "This query is only available for administrative areas");
            }

            var schema = AreaTypeMappings.GetDatabaseSchema(this.AreaTypeIdIs);

            var fields = new List<string>();
            var groups = new List<string>();

            fields.Add(string.Format("'{0}' AS AreaType", this.AreaTypeIdIs));

            if (schema["SubIdColumn"] != null &&
                    schema["SubIdColumn"].Length > 0) {
                fields.Add(string.Format("{0} AS AreaId",
                    SchemaDataFormat(schema["SubIdColumn"])));
                groups.Add(SchemaDataFormat(schema["SubIdColumn"]));
            } else {
                fields.Add("-1 AS AreaId");
            }

            if (schema["SubNameColumn"] != null &&
                    schema["SubNameColumn"].Length > 0) {
                fields.Add(string.Format("{0} AS AreaName",
                    SchemaDataFormat(schema["SubNameColumn"])));
                groups.Add(SchemaDataFormat(schema["SubNameColumn"]));
            } else {
                fields.Add("NULL AS AreaName");
            }

            if (schema["SubAlternativeIdColumn"] != null &&
                    schema["SubAlternativeIdColumn"].Length > 0) {
                fields.Add(string.Format("{0} AS AlternativeId",
                    SchemaDataFormat(schema["SubAlternativeIdColumn"])));
                groups.Add(SchemaDataFormat(schema["SubAlternativeIdColumn"]));
            } else {
                fields.Add("NULL AS AlternativeId");
            }

            if (schema["SubYearColumn"] != null &&
                    schema["SubYearColumn"].Length > 0) {
                fields.Add(string.Format("{0} AS Year",
                    SchemaDataFormat(schema["SubYearColumn"])));
                groups.Add(SchemaDataFormat(schema["SubYearColumn"]));
            } else {
                fields.Add("NULL AS Year");
            }

            if (schema["SubOrderColumn"] != null &&
                    schema["SubOrderColumn"].Length > 0) {
                fields.Add(string.Format("{0} AS OrderNumber",
                    SchemaDataFormat(schema["SubOrderColumn"])));
                groups.Add(SchemaDataFormat(schema["SubOrderColumn"]));
            } else {
                fields.Add("0 AS OrderNumber");
            }

            if (this.AreaFilterQueryString != null &&
                    AreaTypeMappings.GetAreaTypeCategory(this.AreaTypeIdIs) !=
                        Util.AreaTypeMappings.AreaTypeCategory.AdministrativeArea) {
                throw new NotImplementedException(
                    "FunctionalAreaAvailability filters are only available for administrative areas");
            } else {
                this.SetFilters();
            }

            string sqlString = @"
SELECT
    {0},

    -- SUM = number of municipalities within the group with 1 as value
    {5}
FROM
    {1}
    INNER JOIN DimAlue A ON
        ({2} = {3} AND
        A.AlueTaso_ID = 2 AND
        (@YearIs BETWEEN A.Alkaen_Jakso_ID AND A.Asti_Jakso_ID))
    LEFT JOIN Apu_KuntaToiminnallinenAlueTallennusJakso KTAT ON
        (KTAT.Kunta_Alue_ID = A.Alue_ID AND
        KTAT.Jakso_ID = @YearIs)
{6}
GROUP BY
    {4}
";

            string availTmpl = "SUM(COALESCE({0}, 0)) AS {1}";

            var availabilityStrings = new List<string>();
            foreach (Models.AreaType areaType in AreaTypeMappings.GetAreaTypes(
                    Util.AreaTypeMappings.AreaTypeCategory.FunctionalArea)) {
                var subschema = AreaTypeMappings.GetDatabaseSchema(areaType.Id);
                if (subschema["FunctionalAreaAvailabilityField"] == null) {
                    continue;
                }
                string column = string.Format(
                    subschema["FunctionalAreaAvailabilityField"],
                    "KTAT");
                string newColumn = string.Format("{0}_avail", areaType.Id);
                string avail = string.Format(
                    availTmpl,
                    column,
                    newColumn);
                availabilityStrings.Add(avail);
            }

            string whereString = "";
            if (this.whereList.Count > 0) {
                whereString = " WHERE " + string.Join(", ", whereList);
            }

            sqlString = string.Format(sqlString,
                string.Join(", ", fields),
                SchemaDataFormat(schema["SubFromString"]),
                SchemaDataFormat(schema["MainIdColumn"]),
                SchemaDataFormat(schema["SubIdColumn"]),
                string.Join(", ", groups),
                string.Join(",\n", availabilityStrings),
                whereString);

            return sqlString;
        }
    }
}