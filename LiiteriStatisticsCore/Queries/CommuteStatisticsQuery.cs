using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

namespace LiiteriStatisticsCore.Queries
{
    /* Heavily based on StatisticsQuery */
    public class CommuteStatisticsQuery : SqlQuery, ISqlQuery
    {
        public static readonly log4net.ILog logger =
            log4net.LogManager.GetLogger(
                System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static Util.AreaTypeMappings AreaTypeMappings =
            new Util.AreaTypeMappings();

        private List<string> whereList;

        // these will be added as fields for SELECT
        private List<string> fields;

        // these will be added to GROUP BY
        private List<string> groups;

        // these will be added to ORDER BY
        private List<string> orders;

        // this is for JOINs and such
        private StringBuilder sbFrom;

        /* Dynamic custom declarations/queries before the main query
         * Intended to be used with geometry stuff */
        private StringBuilder sbPreQuery;

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

        /* Grouping */
        #region grouping
        private class GroupingInfo
        {
            private string _Type;
            public string Type
            {
                get {
                    return this._Type;
                }
                set {
                    if (!(new string[] { "work", "home" }).Contains(value)) {
                        throw new ArgumentException("Invalid grouping type");
                    }
                    this._Type = value;
                }
            }

            public string AreaTypeId { get; set; }
        }
        private GroupingInfo _GroupByAreaTypeIdIs;
        public string GroupByAreaTypeIdIs
        {
            get
            {
                return string.Format("{0}:{1}",
                    this._GroupByAreaTypeIdIs.Type,
                    this._GroupByAreaTypeIdIs.AreaTypeId);
            }
            set
            {
                string[] pieces = value.Split(':');
                this._GroupByAreaTypeIdIs = new GroupingInfo() {
                    Type = pieces[0],
                    AreaTypeId = pieces[1]
                };
            }
        }
        #endregion

        /* Filters */
        #region filtering

        public string HomeFilterQueryString { get; set; }
        public string WorkFilterQueryString { get; set; }

        /* Keep track of special parameter names for geometry operations */
        private int GeometryParameterCount = 0;

        private Parsers.AreaFilterParserVisitor.SpatialHandlerDelegate SpatialHandler(
            string tableName, string subTableName)
        {
            Func<string, string> dataFormat =
                str => string.Format(str, subTableName, tableName);

            Parsers.AreaFilterParserVisitor.SpatialHandlerDelegate handler =
                delegate(string geom1, string geom2, string func)
            {
                string areaType;
                Dictionary<string, string> schema;

                /* guess which one is geometry, so the other one
                 * should be areaType */
                /* NOTE: areaType is user input, handle carefully! */
                if (geom1.StartsWith("geometry::") ||
                        geom1.StartsWith("@")) {
                    areaType = geom2;
                    schema = AreaTypeMappings.GetDatabaseSchema(areaType);

                    geom2 = dataFormat(schema["GeometryColumn"]);
                    // geom1 should be geometry
                } else {
                    areaType = geom1;
                    schema = AreaTypeMappings.GetDatabaseSchema(areaType);

                    geom1 = dataFormat(schema["GeometryColumn"]);
                    // geom2 should be geometry
                }

                string paramName =
                    "SpatialParam_" +
                    (++this.GeometryParameterCount).ToString();
                this.sbPreQuery.Append(string.Format(
                    "DECLARE @{0} TABLE (id INT NOT NULL)\n",
                    paramName));
                this.sbPreQuery.Append(string.Format(
                    "INSERT INTO @{0} SELECT {1} FROM {2} WHERE {3}.{4}({5}) = 1\n",
                    paramName,
                    dataFormat(schema["SubIdColumn"]),
                    dataFormat(schema["SubFromString"]),
                    geom1,
                    func,
                    geom2));

                string expr = string.Format(
                    "{0} IN (SELECT id FROM @{1})",
                    dataFormat(schema["MainIdColumn"]),
                    paramName);

                return expr;
            };
            return handler;
        }

        private void SetFilters()
        {
            Parsers.AreaFilterParserVisitor.ValueHandlerDelegate
                valueHandler = delegate(object val)
            {
                    return "@" + this.Parameters.AddValue(val);
            };

            if (this.HomeFilterQueryString != null) {
                var parser = new Parsers.AreaFilterParser();
                parser.ValueHandler = valueHandler;
                parser.IdHandler = delegate(string name)
                {
                    /* StatisticsQuery would call ReduceUsableAreaTypes here */
                    var schema = AreaTypeMappings.GetDatabaseSchema(name);
                    return string.Format(schema["MainIdColumn"],
                        "A2_home", "A_home");
                };
                parser.SpatialHandler =
                    this.SpatialHandler("A_home", "A2_home");

                string whereString = parser.Parse(this.HomeFilterQueryString);
                this.whereList.Add(whereString);
            }

            if (this.WorkFilterQueryString != null) {
                var parser = new Parsers.AreaFilterParser();
                parser.ValueHandler = valueHandler;
                parser.IdHandler = delegate(string name)
                {
                    /* StatisticsQuery would call ReduceUsableAreaTypes here */
                    var schema = AreaTypeMappings.GetDatabaseSchema(name);
                    return string.Format(schema["MainIdColumn"],
                        "A2_work", "A_work");
                };
                parser.SpatialHandler =
                    this.SpatialHandler("A_work", "A2_work");

                string whereString = parser.Parse(this.WorkFilterQueryString);
                this.whereList.Add(whereString);
            }
        }

        #endregion

        private string _Type;
        public string Type
        {
            get
            {
                return _Type;
            }

            set
            {
                if (!new string[] {
                        "yht",
                        "a_alkut",
                        "b_kaivos",
                        "c_teoll",
                        "d_infra1",
                        "e_infra2",
                        "f_rakent",
                        "g_kauppa",
                        "h_kulj",
                        "i_majrav",
                        "j_info",
                        "k_raha",
                        "l_kiint",
                        "m_tekn",
                        "n_halpa",
                        "o_julk",
                        "p_koul",
                        "q_terv",
                        "r_taide",
                        "s_muupa",
                        "t_koti",
                        "u_kvjarj",
                        "x_tuntem",
                        }.Contains(value)) {
                    throw new ArgumentException("Unrecognized type: " + value);
                }
                this._Type = value;
            }
        }

        public CommuteStatisticsQuery() : base()
        {
            this.whereList = new List<string>();
            this.fields = new List<string>();
            this.groups = new List<string>();
            this.orders = new List<string>();
            this.sbFrom = new StringBuilder();
            this.sbPreQuery = new StringBuilder();
        }

        private string GetWhereString()
        {
            string whereString = "";
            if (this.whereList.Count > 0) {
                whereString = string.Join(" AND ", this.whereList);
            }
            return whereString;
        }

        private string GetFieldsString()
        {
            return string.Join(",\n    ", this.fields);
       }

        private string GetGroupString()
        {
            return string.Join<string>(",\n    ", this.groups);
        }

        private string GetOrderString()
        {
            if (this.orders.Count == 0) {
                return "";
            }
            StringBuilder sb = new StringBuilder();
            sb.Append("\nORDER BY");
            sb.Append("\n    ");
            sb.Append(string.Join<string>(",\n    ", this.orders));
            return sb.ToString();
        }

        private void SetGroups()
        {
            if (this.GroupByAreaTypeIdIs != null) {
                string[] group = this.GroupByAreaTypeIdIs.Split(':');

                Func<string, string> dataFormat = str => string.Format(str,
                    group[0] == "home" ? "A2_home" : "A2_work",
                    group[0] == "home" ? "A_home" : "A_work");

                var schema = AreaTypeMappings.GetDatabaseSchema(group[1]);

                string idColumn = schema["MainIdColumn"];
                idColumn = dataFormat(idColumn);

                if (idColumn != null && idColumn.Length > 0) {
                    this.fields.Add(string.Format("{0} AS AreaId", idColumn));
                    this.groups.Add(idColumn);
                    /* ordering is important to assure side-by-side queries
                     * are handled properly */
                    this.orders.Add(idColumn);
                } else {
                    this.fields.Add("-1 AS AreaId");
                }

                string nameColumn = schema["SubNameColumn"];
                if (nameColumn != null && nameColumn.Length > 0) {
                    nameColumn = dataFormat(nameColumn);
                    this.fields.Add(string.Format("{0} AS AreaName", nameColumn));
                    this.groups.Add(nameColumn);
                } else {
                    this.fields.Add("NULL AS AreaName");
                }

                string alternativeIdColumn = schema["SubAlternativeIdColumn"];
                if (alternativeIdColumn != null && alternativeIdColumn.Length > 0) {
                    alternativeIdColumn = dataFormat(alternativeIdColumn);
                    this.fields.Add(string.Format(
                        "{0} AS AlternativeId", alternativeIdColumn));
                    this.groups.Add(alternativeIdColumn);
                } else {
                    this.fields.Add("NULL AS AlternativeId");
                }

                if (schema["JoinQuery"] != null) {
                    /* alias substitutions are needed because the names
                     * will be different in CommuteStatistics */
                    string joinQuery = schema["JoinQuery"];
                    joinQuery = string.Format(joinQuery,
                        group[0] == "home" ? "A2_home" : "A2_work",
                        group[0] == "home" ? "A_home" : "A_work");

                    this.sbFrom.Append("\n    ");
                    this.sbFrom.Append(joinQuery);
                }
            } else {
                this.fields.Add("NULL AS AreaId");
                this.fields.Add("NULL AS AreaName");
                this.fields.Add("NULL AS AlternativeId");
            }

            /* add SubNameColumn here */
            /* add SubAlternativeIdColumn here */
        }

        /* TODO: This only needs to exist if there's a potential choice
         * between DatabaseAreaTypeIdIs == 1 or 2 */
        private void SetDatabaseAreaTypeId()
        {
            this.Parameters.Add("DatabaseAreaTypeIdIs", 1);
        }

        public override string GetQueryString()
        {
            logger.Debug("Statistics query: commute statistics");

            //this.fields.Add(string.Format("SUM({0}) AS Value", this.Type));
            this.fields.Add(string.Format(
                "CAST(SUM(yht) AS FLOAT) AS Value", this.Type));

            /*
            this.fields.Add("NULL AS AreaPointLat");
            this.fields.Add("NULL AS AreaPointLon");
            */

            this.SetFilters();
            this.SetGroups();

            this.SetDatabaseAreaTypeId();

            string tableName;
            if (this.YearIs >= 2008) {
                tableName = "FactTyomatkaTOL2008_Tyopaikka_Asuinpaikka";
                this.fields.Add("CAST(T.vuosi AS INTEGER) AS Year");
                this.groups.Add("T.vuosi");
                this.whereList.Add("T.vuosi = @YearIs");
            } else {
                tableName = "FactTyomatkaTOL2002";
                this.fields.Add("T.Jakso_ID AS Year");
                this.groups.Add("T.Jakso_ID");
                this.whereList.Add("T.Jakso_ID = @YearIs");
            }
            logger.Debug(string.Format(
                "Table determined to be: {0}", tableName));

            string queryString = @"
SELECT
    {1}

FROM
    {0} T

    INNER JOIN DimAlue A_work ON
        (A_work.Alue_ID = T.TRuutu_Alue_ID AND
        A_work.AlueTaso_ID = @DatabaseAreaTypeIdIs AND
        (@YearIs BETWEEN A_work.Alkaen_Jakso_ID AND A_work.Asti_Jakso_ID))

    INNER JOIN DimAlue A_home ON
        (A_home.Alue_ID = T.ARuutu_Alue_ID AND
        A_home.AlueTaso_ID = @DatabaseAreaTypeIdIs AND
        (@YearIs BETWEEN A_home.Alkaen_Jakso_ID AND A_home.Asti_Jakso_ID))
{2}

WHERE
    {3}

GROUP BY
    {4}

{5}
";
            queryString = string.Format(queryString,
                tableName,
                this.GetFieldsString(),
                this.sbFrom.ToString(),
                this.GetWhereString(),
                this.GetGroupString(),
                this.GetOrderString());

            /* preQuery stuff (which are geometry declarations at the moment)
             * should be common for all query types, let's prepend it here */
            queryString = this.sbPreQuery.ToString() + "\n" + queryString;

            return queryString;
        }
    }
}