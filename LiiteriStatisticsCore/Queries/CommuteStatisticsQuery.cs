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

        public string TableName { get; set; }

        public int YearIs
        {
            get
            {
                return (int) this.Parameters["YearIs"].Value;
            }
            set
            {
                this.whereList.Add("T.Jakso_ID = @YearIs");
                this.Parameters.Add("YearIs", value);
            }
        }

        public int GenderIs
        {
            get
            {
                return (int) this.Parameters["GenderIs"].Value;
            }
            set
            {
                if (value >= 0 && value <= 2) {
                    this.Parameters.Add("GenderIs", value);
                } else {
                    logger.Error("Invalid gender value: " + value);
                    throw new ArgumentException("Invalid gender value");
                }
            }
        }

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

                        /* FactTyomatkaTOL2008 */
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

                        /* FactTyomatkaTOL2002 */
                        "a_alkutuot",
                        "b_kala",
                        "c_kaivuu",
                        "d_teoll",
                        "e_teknhu",
                        "f_rakent",
                        "g_kauppa",
                        "h_majrav",
                        "i_liiken",
                        "j_raha",
                        "k_kivutu",
                        "l_julkhal",
                        "m_koul",
                        "n_tervsos",
                        "o_muuyhtk",
                        "p_tyonant",
                        "q_kvjarj",
                        "x_tuntem",
                        }.Contains(value)) {
                    throw new ArgumentException("Unrecognized type: " + value);
                }
                this._Type = value;
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
                    this.fields.Add(string.Format(
                        "{0} AS AreaName", nameColumn));
                    this.groups.Add(nameColumn);
                } else {
                    this.fields.Add("NULL AS AreaName");
                }

                string alternativeIdColumn = schema["SubAlternativeIdColumn"];
                if (alternativeIdColumn != null &&
                        alternativeIdColumn.Length > 0) {
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
                    joinQuery = dataFormat(joinQuery);

                    this.sbFrom.Append("\n    ");
                    this.sbFrom.Append(joinQuery);
                }

                if (group[0] == "home") {
                    this.ReduceUsableAreaTypes_Home(group[1]);
                } else if (group[0] == "work") {
                    this.ReduceUsableAreaTypes_Work(group[1]);
                } else {
                    throw new Exception("Invalid group type: " + group[0]);
                }
            } else {
                this.fields.Add("NULL AS AreaId");
                this.fields.Add("NULL AS AreaName");
                this.fields.Add("NULL AS AlternativeId");
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

                if (tableName.EndsWith("work")) {
                    this.ReduceUsableAreaTypes_Work(areaType);
                } else if (tableName.EndsWith("home")) {
                    this.ReduceUsableAreaTypes_Home(areaType);
                } else {
                    throw new Exception("Unexpected tableName: " + tableName);
                }

                string paramName =
                    "SpatialParam_" +
                    (++this.GeometryParameterCount).ToString();
                this.sbPreQuery.Append(string.Format(
                    "DECLARE @{0} TABLE (id INT NOT NULL)\n",
                    paramName));

                int databaseAreaTypeId =
                    AreaTypeMappings.GetPrimaryDatabaseAreaType(areaType);
                this.sbPreQuery.Append(string.Format(
                    "INSERT INTO @{0} SELECT {1} FROM {2} WHERE {3} = {4} AND {5}.{6}({7}) = 1\n",
                    paramName,
                    dataFormat(schema["SubIdColumn"]),
                    dataFormat(schema["SubFromString"]),
                    dataFormat("{0}.AlueTaso_ID"),
                    databaseAreaTypeId,
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
                    this.ReduceUsableAreaTypes_Home(name);
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
                    this.ReduceUsableAreaTypes_Work(name);
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

        /* At first, we assume both 2(municipality) and 1(grid) are usable
         * areatypes, and later we will reduce this list based on
         * filters/groups */
        //private int[] UsableAreaTypes = new int[] { 2, 1 };

        /* Instead of the commented out UsableAreaTypes above, let's
         * do individual UsableAreaTypes for both work and home */
        private int[] UsableAreaTypes_Work = new int[] { 2, 1 };
        private void ReduceUsableAreaTypes_Work(string areaType)
        {
            int[] dbAreaTypes = AreaTypeMappings.GetDatabaseAreaTypes(areaType);
            this.UsableAreaTypes_Work = (
                from a in this.UsableAreaTypes_Work
                where dbAreaTypes.Contains<int>(a)
                select a).ToArray();
        }

        private int[] UsableAreaTypes_Home = new int[] { 2, 1 };
        private void ReduceUsableAreaTypes_Home(string areaType)
        {
            int[] dbAreaTypes = AreaTypeMappings.GetDatabaseAreaTypes(areaType);
            this.UsableAreaTypes_Home = (
                from a in this.UsableAreaTypes_Home
                where dbAreaTypes.Contains<int>(a)
                select a).ToArray();
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
            var grouplist = new List<string>();
            foreach (string group in this.groups) {
                // don't try to group using strings, e.g. 'Finland'
                if (group.StartsWith("'") && group.EndsWith("'")) {
                    continue;
                }
                grouplist.Add(group);
            }
            return string.Join(",\n    ", grouplist);
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

        private int DatabaseAreaTypeId_Home;
        private int DatabaseAreaTypeId_Work;
        private void SetDatabaseAreaTypeId()
        {
            if (this.UsableAreaTypes_Work.Length == 0 ||
                    this.UsableAreaTypes_Home.Length == 0) {
                throw new Exception(
                    "No suitable DatabaseAreaType could be determined with the supplied parameters");
            }

            /* Here we pick our preferred DatabaseAreaType by simply picking
             * the largest number. However, it may be necessary to start
             * using some priority value instead */

            int dbAreaType;

            Array.Sort(this.UsableAreaTypes_Work);
            dbAreaType = this.UsableAreaTypes_Work.Last();
            this.DatabaseAreaTypeId_Work = dbAreaType;

            Array.Sort(this.UsableAreaTypes_Home);
            dbAreaType = this.UsableAreaTypes_Home.Last();
            this.DatabaseAreaTypeId_Home = dbAreaType;
        }

        public override string GetQueryString()
        {
            logger.Debug("Statistics query: commute statistics");

            //this.fields.Add(string.Format("SUM({0}) AS Value", this.Type));
            switch (this.Type) {
                /* distance_avg is actually not used, but the functionality
                   exists here for future reference */
                case "distance_avg":
                    this.fields.Add("ROUND(AVG(matka) / 1000, 0) AS Value");
                    break;
                case "yht":
                    this.fields.Add(string.Format(
                        "CAST(SUM({0}) AS FLOAT) AS Value", this.Type));
                    break;
                default:
                    this.fields.Add(string.Format(
                        "CAST(SUM({0}) AS FLOAT) AS Value",
                        this.Type));
                    this.fields.Add(string.Format(
                    @"
    CASE
        WHEN SUM(yht) < 10
        THEN 1
        ELSE 0
        END AS TriggerPrivacyLimit",
                        this.Type));
                    break;
            }

            this.SetFilters();
            this.SetGroups();
            this.SetDatabaseAreaTypeId();

            /* For different tables this may need to be a variable
             * set by the CommuteStatisticsIndicator */
            this.fields.Add("T.Jakso_ID AS Year");
            this.groups.Add("T.Jakso_ID");

            logger.Debug(string.Format(
                "Table determined to be: {0}", this.TableName));

            StringBuilder sbAreaJoin = new StringBuilder();

            /* Work */
            if (this.DatabaseAreaTypeId_Work == 1) {
                sbAreaJoin.Append(@"
    INNER JOIN DimAlue A_work ON
        (A_work.Alue_ID = T.TRuutu_Alue_ID AND
        A_work.AlueTaso_ID = 1 AND
        (@YearIs BETWEEN A_work.Alkaen_Jakso_ID AND A_work.Asti_Jakso_ID))
");
            } else if (this.DatabaseAreaTypeId_Work == 2) {
                sbAreaJoin.Append(@"
    INNER JOIN DimAlue A_work ON
        (A_work.Alue_ID = T.TKunta_Alue_ID AND
        A_work.AlueTaso_ID = 2 AND
        (@YearIs BETWEEN A_work.Alkaen_Jakso_ID AND A_work.Asti_Jakso_ID))
");
            } else {
                throw new Exception("Invalid DatabaseAreaTypeId_Work specified");
            }

            /* Home */
            if (this.DatabaseAreaTypeId_Home == 1) {
                sbAreaJoin.Append(@"
    INNER JOIN DimAlue A_home ON
        (A_home.Alue_ID = T.ARuutu_Alue_ID AND
        A_home.AlueTaso_ID = 1 AND
        (@YearIs BETWEEN A_home.Alkaen_Jakso_ID AND A_home.Asti_Jakso_ID))
");
            } else if (this.DatabaseAreaTypeId_Home == 2) {
                sbAreaJoin.Append(@"
    INNER JOIN DimAlue A_home ON
        (A_home.Alue_ID = T.AKunta_Alue_ID AND
        A_home.AlueTaso_ID = 2 AND
        (@YearIs BETWEEN A_home.Alkaen_Jakso_ID AND A_home.Asti_Jakso_ID))
");
            } else {
                throw new Exception("Invalid DatabaseAreaTypeId_Home specified");
            }

            this.whereList.Add("T.sp = @GenderIs");

            string queryString = @"
SELECT
    {0}

FROM
    {1} T
{2}
{3}

WHERE
    {4}

GROUP BY
    {5}

{6}
";
            queryString = string.Format(queryString,
                this.GetFieldsString(),
                this.TableName,
                sbAreaJoin.ToString(),
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