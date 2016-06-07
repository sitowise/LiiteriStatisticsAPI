using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

namespace LiiteriStatisticsCore.Queries
{
    public class StatisticsQuery : SqlQuery, ISqlQuery
    {
        public static readonly log4net.ILog logger =
            log4net.LogManager.GetLogger(
                System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected static Util.AreaTypeMappings AreaTypeMappings =
            new Util.AreaTypeMappings();

#if DEBUG
        protected Util.TemplateCollection QueryTemplates =
            new Util.TemplateCollection("StatisticsQuery");
#else
        public static Util.TemplateCollection QueryTemplates =
            new Util.TemplateCollection("StatisticsQuery");
#endif

        // this will be filled by GenerateQueryString, and returned by GetQueryString
        protected string QueryString = null;

        private List<string> whereList;

        // these will be added as fields for SELECT
        //IDictionary<string, string> fields;
        protected List<string> fields;

        // these will be added to GROUP BY
        protected List<string> groups;

        // these will be added to ORDER BY
        protected List<string> orders;

        // this is for JOINs and such
        private StringBuilder sbFrom;

        /* Dynamic custom declarations/queries before the main query
         * Intended to be used with geometry stuff */
        private StringBuilder sbPreQuery;
        private StringBuilder sbPostQuery;

        // some tables need to be dynamically added to the Area join so
        // we can run filter queries on them
        private List<string> filterJoins = new List<string>();

        public StatisticsQuery(int id) : base()
        {
            this.IdIs = id;

            this.whereList = new List<string>();
            //this.fields = new Dictionary<string, string>();
            this.fields = new List<string>();
            this.groups = new List<string>();
            this.orders = new List<string>();
            this.sbFrom = new StringBuilder();
            this.sbPreQuery = new StringBuilder();
            this.sbPostQuery = new StringBuilder();
        }

        public int IdIs
        {
            get
            {
                return (int) this.Parameters["IdIs"].Value;
            }
            set
            {
                this.Parameters.Add("IdIs", value);
            }
        }

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

        public int? AreaYearIs
        {
            get
            {
                if (!this.Parameters.Contains("AreaYearIs")) {
                    return null;
                }
                return (int) this.Parameters["AreaYearIs"].Value;
            }
            set
            {
                this.Parameters.Add("AreaYearIs", value);
            }
        }

        //public int SelectionAreaType { get; set; }
        public int CalculationTypeIdIs { get; set; }

        /* Filters */
        public string AreaFilterQueryString { get; set; }

        /* Grouping */
        public string GroupByAreaTypeIdIs { get; set; }

        /* this is used to enforce non aggregation for calculation type 4 */
        private bool RelaxedAreaTypes = true;

        public string[] BlockedAreaTypes { get; set; }

        private int[] GetDatabaseAreaTypes(string areaType)
        {
            if (!this.RelaxedAreaTypes) {
                return new int[] {
                    AreaTypeMappings.GetPrimaryDatabaseAreaType(areaType),
                };
            } else {
                return AreaTypeMappings.GetDatabaseAreaTypes(areaType);
            }
        }

        private void ReduceUsableAreaTypes(string areaType)
        {
            int[] dbAreaTypes = this.GetDatabaseAreaTypes(areaType);
            Debug.WriteLine(string.Format(
                "For [{0}], we could use one of these [{1}]",
                areaType, string.Join(", ", dbAreaTypes)));
            Debug.WriteLine(string.Format(
                "UsableAreaTypes was: [{0}]",
                string.Join(", ", this.UsableAreaTypes)));
            this.UsableAreaTypes = (
                from a in this.UsableAreaTypes
                where dbAreaTypes.Contains<int>(a)
                select a).ToArray();
            Debug.WriteLine(string.Format(
                "UsableAreaTypes has become: [{0}]",
                string.Join(", ", this.UsableAreaTypes)));
        }

        protected void SetGroups()
        {
            /* add the proper table that we are grouping by */
            if (this.GroupByAreaTypeIdIs != null) {
                if (this.BlockedAreaTypes != null &&
                        this.BlockedAreaTypes.Contains(this.GroupByAreaTypeIdIs)) {
                    /* normally the user should never reach this exception,
                     * since the available areaTypes are already listed in the
                     * indicator */
                    throw new Exception(string.Format(
                        "Specified grouping areaType \"{2}\" is disabled for statisticsId:{0} year:{1}",
                        this.IdIs, this.YearIs, this.GroupByAreaTypeIdIs));
                }

                var schema = AreaTypeMappings.GetDatabaseSchema(
                    this.GroupByAreaTypeIdIs);

                string idColumn = schema["MainIdColumn"];
                if (idColumn != null && idColumn.Length > 0) {
                    idColumn = SchemaDataFormat(idColumn);
                    // Currently not using MainIdColumn for fields!
                    //this.fields.Add(string.Format("{0} AS AreaId", idColumn));
                    //this.groups.Add(idColumn);
                    /* ordering is important to ensure side-by-side queries
                     * are handled properly */
                    //this.orders.Add(idColumn);
                } else {
                    //this.fields.Add("-1 AS AreaId");
                }

                string subIdColumn = schema["SubIdColumn"];
                if (subIdColumn != null && subIdColumn.Length > 0) {
                    subIdColumn = SchemaDataFormat(subIdColumn);
                    this.fields.Add(string.Format("{0} AS AreaId", subIdColumn));
                    this.groups.Add(subIdColumn);
                    /* ordering is important to ensure side-by-side queries
                     * are handled properly */
                    this.orders.Add(subIdColumn);
                } else {
                    this.fields.Add("-1 AS AreaId");
                }

                string nameColumn = schema["SubNameColumn"];
                if (nameColumn != null && nameColumn.Length > 0) {
                    nameColumn = SchemaDataFormat(nameColumn);
                    this.fields.Add(string.Format("{0} AS AreaName", nameColumn));
                    this.groups.Add(nameColumn);
                } else {
                    this.fields.Add("NULL AS AreaName");
                }

                string alternativeIdColumn = schema["SubAlternativeIdColumn"];
                if (alternativeIdColumn != null && alternativeIdColumn.Length > 0) {
                    alternativeIdColumn = SchemaDataFormat(alternativeIdColumn);
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
                    joinQuery = SchemaDataFormat(joinQuery);

                    this.sbFrom.Append("\n    ");
                    this.sbFrom.Append(joinQuery);
                }

                this.ReduceUsableAreaTypes(this.GroupByAreaTypeIdIs);

            } else {
                this.fields.Add("NULL AS AreaId");
                this.fields.Add("NULL AS AreaName");
                this.fields.Add("NULL AS AlternativeId");
            }
        }

        /* This is a list of DatabaseAreaTypes that this statistic has
         * data for in the database. Based on group and filter selections,
         * we will find the proper DatabaseAreaType to use from this list. */
        private int[] _AvailableAreaTypes;
        public int[] AvailableAreaTypes {
            get
            {
                return this._AvailableAreaTypes;
            }
            set
            {
                this._AvailableAreaTypes = value;

                //this.UsableAreaTypes = this._AvailableAreaTypes.ToList<int>();
                this.UsableAreaTypes = this._AvailableAreaTypes.ToArray();
            }
        }

        /* This list will start with AvailableAreaTypes, and will be modified
         * by filter and group settings */
        private int[] UsableAreaTypes { get; set; }

        /* Keep track of special parameter names for geometry operations */
        private int GeometryParameterCount = 0;

        /* The column aliases are different for statistics and
         * commuteStatistics, so AreaTypeMappings only provides
         * string templates for various settings */
        private static string SchemaDataFormat(string str)
        {
            return string.Format(str,
                "A2", // {0}: sub-table (e.g. DimKunta or DimRuutu)
                "A"); // {1}: main table (DimAlue)
        }

        protected void SetFilters()
        {
            if (this.AreaFilterQueryString != null) {
                var parser = new Parsers.AreaFilterParser();
                parser.ValueHandler = delegate(object val)
                {
                    return "@" + this.Parameters.AddValue(val);
                };
                parser.IdHandler = delegate(string name)
                {
                    // currently disabled areaTypes should only affect grouping,
                    // so this block of code is disabled
                    if (false && this.BlockedAreaTypes != null &&
                            this.BlockedAreaTypes.Contains(name)) {
                        /* normally the user should never reach this exception,
                         * since the available areaTypes are already listed in the
                         * indicator */
                        throw new Exception(string.Format(
                            "Specified filtering areaType \"{2}\" is disabled for statisticsId:{0} year:{1}",
                            this.IdIs, this.YearIs, name));
                    }

                    this.ReduceUsableAreaTypes(name);
                    var schema = AreaTypeMappings.GetDatabaseSchema(name);
                    string idColumn = schema["MainIdColumn"];
                    idColumn = SchemaDataFormat(idColumn);

                    /* "Luokka" tables need to be added with these */
                    if (!this.filterJoins.Contains(schema["FilterJoinQuery"]) &&
                            schema["FilterJoinQuery"] != null &&
                            schema["FilterJoinQuery"].Length > 0) {
                        this.filterJoins.Add(SchemaDataFormat(
                            schema["FilterJoinQuery"]));
                    }
                    return idColumn;
                };

                /* We get both spatial atoms here (geom1, geom2),
                 * one of them is the raw areaType, and another is the
                 * SQL server geometry. At this point it is not known
                 * which is which. */
                parser.SpatialHandler = delegate(
                    string geom1,
                    string geom2,
                    string func)
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

                        geom2 = SchemaDataFormat(schema["GeometryColumn"]);
                        // geom1 should be geometry
                    } else {
                        areaType = geom1;
                        schema = AreaTypeMappings.GetDatabaseSchema(areaType);

                        geom1 = SchemaDataFormat(schema["GeometryColumn"]);
                        // geom2 should be geometry
                    }

                    // currently disabled areaTypes should only affect grouping,
                    // so this block of code is disabled
                    if (false && this.BlockedAreaTypes != null &&
                            this.BlockedAreaTypes.Contains(areaType)) {
                        /* normally the user should never reach this exception,
                         * since the available areaTypes are already listed in the
                         * indicator */
                        throw new Exception(string.Format(
                            "Specified filtering areaType \"{2}\" is disabled for statisticsId:{0} year:{1}",
                            this.IdIs, this.YearIs, areaType));
                    }

                    this.ReduceUsableAreaTypes(areaType);

                    string paramName =
                        "SpatialParam_" +
                        (++this.GeometryParameterCount).ToString();
                    this.sbPreQuery.Append(string.Format(
                        "CREATE TABLE #{0} (id INT NOT NULL)\n",
                        paramName));
                    this.sbPostQuery.Append(string.Format(
                        "DROP TABLE #{0}\n", paramName));
                    /* this.sbPreQuery.Append(string.Format(
                        "DECLARE @{0} TABLE (id INT NOT NULL)\n",
                        paramName)); */

                    int databaseAreaTypeId =
                        AreaTypeMappings.GetPrimaryDatabaseAreaType(areaType);
                    this.sbPreQuery.Append(string.Format(
                        "INSERT INTO #{0} SELECT {1} FROM {2} WHERE {3} = {4} AND {5}.{6}({7}) = 1\n",
                        paramName,
                        SchemaDataFormat(schema["SubIdColumn"]),
                        SchemaDataFormat(schema["SubFromString"]),
                        SchemaDataFormat("{0}.AlueTaso_ID"),
                        databaseAreaTypeId,
                        geom1,
                        func,
                        geom2));

                    /* only the spatial operation here to make sure we are
                     * using the spatial index */
                    /*
                    this.sbPreQuery.Append(string.Format(
                        "INSERT INTO @{0} SELECT {1} FROM {2} WHERE {3}.{4}({5}) = 1\n",
                        paramName,
                        SchemaDataFormat(schema["SubIdColumn"]),
                        SchemaDataFormat(schema["SubFromString"]),
                        geom1,
                        func,
                        geom2));
                    */

                    string expr = string.Format(
                        "{0} IN (SELECT id FROM #{1})",
                        SchemaDataFormat(schema["MainIdColumn"]),
                        paramName);

                    return expr;
                };
                string whereString = parser.Parse(this.AreaFilterQueryString);
                this.whereList.Add(whereString);
            }
        }

        private string labelSQLString(string key, string sql)
        {
            return string.Format(
                "\n/* {0} BEGIN */\n{1}\n/* {0} END */\n",
                key, sql);
        }

        protected string GetWhereString()
        {
            string whereString = "";
            if (this.whereList.Count > 0) {
                whereString = " AND " + string.Join(" AND ", this.whereList);
            }
            return this.labelSQLString("whereString", whereString);
        }

        protected string GetFieldsString()
        {
            return this.labelSQLString("fieldsString",
                string.Join(",\n    ", this.fields));
       }

        protected string GetFromString()
        {
            return this.labelSQLString("fromString",
                this.sbFrom.ToString());
        }

        protected string GetGroupString()
        {
            var grouplist = new List<string>();
            foreach (string group in this.groups) {
                // don't try to group using strings, e.g. 'Finland'
                if (group.StartsWith("'") && group.EndsWith("'")) {
                    continue;
                }
                grouplist.Add(group);
            }
            StringBuilder sb = new StringBuilder();
            sb.Append("\nGROUP BY");
            sb.Append("\n    ");
            sb.Append(string.Join(",\n    ", grouplist));
            return this.labelSQLString("groupsString", sb.ToString());
        }

        protected string GetOrderString()
        {
            if (this.orders.Count == 0) {
                return "";
            }
            StringBuilder sb = new StringBuilder();
            sb.Append("\nORDER BY");
            sb.Append("\n    ");
            sb.Append(string.Join<string>(",\n    ", this.orders));
            return this.labelSQLString("orderString", sb.ToString());
        }

        protected string GetFilterJoinsString()
        {
            return this.labelSQLString("FilterJoins",
                "\n" + string.Join("\n", this.filterJoins));
        }

        protected string GetPreQueryString()
        {
            return this.labelSQLString("preQueryString",
                this.sbPreQuery.ToString());
        }

        protected string GetPostQueryString()
        {
            return this.labelSQLString("postQueryString",
                this.sbPostQuery.ToString());
        }

        private int? databaseAreaTypeId = null;

        /* This should be called after Filters & Groups have been processed */
        protected void SetDatabaseAreaTypeId()
        {
            if (this.UsableAreaTypes.Length == 0) {
                throw new Exception(
                    "No suitable DatabaseAreaType could be determined with the supplied parameters");
            }
            if (this.databaseAreaTypeId != null) {
                throw new Exception("DatabaseAreaTypeId already set!");
            }
            /* Here we pick our preferred DatabaseAreaType by simply picking
             * the largest number. However, it may be necessary to start
             * using some priority value instead */
            Array.Sort(this.UsableAreaTypes);
            this.databaseAreaTypeId = this.UsableAreaTypes.Last();
            Debug.WriteLine(string.Format(
                "From this list: [{0}], we decided to pick [{1}]",
                string.Join(", ", this.UsableAreaTypes),
                this.databaseAreaTypeId));
            this.Parameters.Add("DatabaseAreaTypeIdIs", this.databaseAreaTypeId);
        }

        public int GetDatabaseAreaTypeId()
        {
            if (this.databaseAreaTypeId == null) {
                throw new Exception("DatabaseAreaTypeId is not set!");
            }
            return (int) this.databaseAreaTypeId;
        }

        /* for performance reasons, prefer T.Jakso_ID unless a variable is
         * actually needed */
        protected string GetAreaYearField()
        {
            if (this.AreaYearIs == null || this.AreaYearIs == this.YearIs) {
                return "T.Jakso_ID";
            }
            return "@AreaYearIs";
        }

        private void GenerateQueryString()
        {
            string queryString;

            Debug.WriteLine(string.Format(
                "We have these areaTypes available: [{0}]",
                string.Join(",", this.AvailableAreaTypes)));

            if (!new int[] { 1, 2 }.Contains(this.CalculationTypeIdIs)) {
                string errMsg = string.Format(
                    "Unsupported CalculationType: {0}",
                    this.CalculationTypeIdIs);
                logger.Error(errMsg);
                throw new Exception(errMsg);
            }

            this.fields.Add("T.Jakso_ID AS Year");

            this.groups.Add("T.Jakso_ID");
            this.fields.Add("SUM(T.Arvo) AS Value");

            this.SetFilters();
            this.SetGroups();
            this.SetDatabaseAreaTypeId();

            queryString = QueryTemplates.Get("Normal");
            queryString = string.Format(queryString,
                this.GetFieldsString(),
                this.GetFromString(),
                this.GetWhereString(),
                this.GetGroupString(),
                this.GetOrderString(),
                this.GetFilterJoinsString(),
                this.GetAreaYearField());

            /* preQuery stuff (which are geometry declarations at the moment)
             * should be common for all query types, let's prepend it here */
            queryString = this.GetPreQueryString() + "\n" + queryString;

            /* postQuery stuff (drop temporary tables) */
            queryString = queryString + "\n" + this.GetPostQueryString();

            this.QueryString = queryString;
        }

        public override string GetQueryString()
        {
            if (this.QueryString == null) {
                this.GenerateQueryString();
            }
            return this.QueryString;
        }
    }
}