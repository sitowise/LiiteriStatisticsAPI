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

        private static Util.AreaTypeMappings AreaTypeMappings =
            new Util.AreaTypeMappings();

        private List<string> whereList;

        // these will be added as fields for SELECT
        //IDictionary<string, string> fields;
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

        //public int SelectionAreaType { get; set; }
        public int CalculationTypeIdIs { get; set; }

        /* Filters */
        public string AreaFilterQueryString { get; set; }

        /* Grouping */
        public string GroupByAreaTypeIdIs { get; set; }

        /* prevent CalculationType=4 from being aggregated */
        private bool RelaxedAreaTypes = true;
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
            int[] dbAreaTypes = AreaTypeMappings.GetDatabaseAreaTypes(areaType);
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

        private void SetGroups()
        {
            /* add the proper table that we are grouping by */
            if (this.GroupByAreaTypeIdIs != null) {
                var schema = AreaTypeMappings.GetDatabaseSchema(
                    this.GroupByAreaTypeIdIs);

                string idColumn = schema["MainIdColumn"];
                if (idColumn != null && idColumn.Length > 0) {
                    idColumn = SchemaDataFormat(idColumn);
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

        private void SetFilters()
        {
            if (this.AreaFilterQueryString != null) {
                var parser = new Parsers.AreaFilterParser();
                parser.ValueHandler = delegate(object val)
                {
                    return "@" + this.Parameters.AddValue(val);
                };
                parser.IdHandler = delegate(string name)
                {
                    this.ReduceUsableAreaTypes(name);
                    var schema = AreaTypeMappings.GetDatabaseSchema(name);
                    string idColumn = schema["MainIdColumn"];
                    idColumn = SchemaDataFormat(idColumn);
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

                    this.ReduceUsableAreaTypes(areaType);

                    string paramName =
                        "SpatialParam_" +
                        (++this.GeometryParameterCount).ToString();
                    this.sbPreQuery.Append(string.Format(
                        "DECLARE @{0} TABLE (id INT NOT NULL)\n",
                        paramName));
                    this.sbPreQuery.Append(string.Format(
                        "INSERT INTO @{0} SELECT {1} FROM {2} WHERE {3}.{4}({5}) = 1\n",
                        paramName,
                        SchemaDataFormat(schema["SubIdColumn"]),
                        SchemaDataFormat(schema["SubFromString"]),
                        geom1,
                        func,
                        geom2));

                    string expr = string.Format(
                        "{0} IN (SELECT id FROM @{1})",
                        SchemaDataFormat(schema["MainIdColumn"]),
                        paramName);

                    return expr;
                };
                string whereString = parser.Parse(this.AreaFilterQueryString);
                this.whereList.Add(whereString);
            }
        }

        private string GetWhereString()
        {
            string whereString = "";
            if (this.whereList.Count > 0) {
                whereString = " AND " + string.Join(" AND ", this.whereList);
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

        /* This should be called after Filters & Groups have been processed */
        private void SetDatabaseAreaTypeId()
        {
            if (this.UsableAreaTypes.Length == 0) {
                throw new Exception(
                    "No suitable DatabaseAreaType could be determined with the supplied parameters");
            }
            /* Here we pick our preferred DatabaseAreaType by simply picking
             * the largest number. However, it may be necessary to start
             * using some priority value instead */
            Array.Sort(this.UsableAreaTypes);
            int dbAreaType = this.UsableAreaTypes.Last();
            Debug.WriteLine(string.Format(
                "From this list: [{0}], we decided to pick [{1}]",
                string.Join(", ", this.UsableAreaTypes), dbAreaType));
            this.Parameters.Add("DatabaseAreaTypeIdIs", dbAreaType);

            /* Now that we have chosen the DatabaseAreaTypeId, let's
             * select the appropriate availability expression,
             * which will be something like:
             *
             * ATJ.Ruutu_Alue_ID = 1
             *  meaning:
             * Apu_AlueTallennusJakso.Ruutu_Alue_ID = 1
             */
            string availabilityExpression =
                AreaTypeMappings.GetAvailabilityExpression(
                    this.GroupByAreaTypeIdIs, dbAreaType);
            this.whereList.Add(availabilityExpression);
        }

        /*
         * CalculationType == 3
         * this has not been tested with actual zero values yet 
         */
        private string GetQueryString_DerivedDivided()
        {
            this.fields.Add("ATJ.Jakso_ID AS Year");
            this.groups.Add("ATJ.Jakso_ID");

            this.fields.Add("(SUM(COALESCE(T1.Arvo, 0)) / SUM(COALESCE(T2.Arvo, 0))) AS Value");

            this.SetFilters();
            this.SetGroups();
            this.SetDatabaseAreaTypeId();

            string queryString = @"
SELECT
    {0}

FROM
    Apu_AlueTallennusJakso ATJ

    INNER JOIN Apu_TilastoTallennusJakso TTJ ON
        (TTJ.Jakso_ID = ATJ.Jakso_ID AND
        TTJ.Tilasto_ID = @IdIs AND
        TTJ.AlueTaso_ID = @DatabaseAreaTypeIdIs)

    INNER JOIN DimAlue A ON
        (A.AlueTaso_ID = TTJ.AlueTaso_ID AND
        @YearIs BETWEEN A.Alkaen_Jakso_ID AND A.Asti_Jakso_ID)

    {1}

    LEFT OUTER JOIN DimTilasto_JohdettuTilasto_Jako J ON
        J.Tilasto_ID = TTJ.Tilasto_ID

    LEFT OUTER JOIN FactTilastoarvo T1 ON
        (T1.Tilasto_ID = J.Osoittaja_Tilasto_ID AND
        T1.Alue_ID = A.Alue_ID AND
        T1.Jakso_ID = TTJ.Jakso_ID AND
        T1.AlueTaso_ID = TTJ.AlueTaso_ID)

    INNER JOIN FactTilastoarvo T2 ON
        (T2.Tilasto_ID = J.Nimittaja_Tilasto_ID AND
        T2.Alue_ID = A.Alue_ID AND
        T2.Jakso_ID = TTJ.Jakso_ID AND
        T2.AlueTaso_ID = TTJ.AlueTaso_ID)

WHERE
    ATJ.Jakso_ID = @YearIs
    {2}

GROUP BY
    {3}
    {4}
";
            queryString = string.Format(queryString,
                this.GetFieldsString(),
                this.sbFrom.ToString(),
                this.GetWhereString(),
                this.GetGroupString(),
                this.GetOrderString());

            return queryString;
        }

        /*
         * CalculationType == 5
         * this has been converted to new query
         */
        private string GetQueryString_DerivedSummed()
        {
            this.fields.Add("ATJ.Jakso_ID AS Year");
            this.groups.Add("ATJ.Jakso_ID");

            this.fields.Add("SUM(COALESCE(T.Arvo, 0)) AS Value");

            this.SetFilters();
            this.SetGroups();
            this.SetDatabaseAreaTypeId();

            string queryString = @"
SELECT
    {0}

FROM
    Apu_AlueTallennusJakso ATJ

    INNER JOIN Apu_TilastoTallennusJakso TTJ ON
        (TTJ.Jakso_ID = ATJ.Jakso_ID AND
        TTJ.Tilasto_ID = @IdIs AND
        TTJ.AlueTaso_ID = @DatabaseAreaTypeIdIs)

    INNER JOIN DimAlue A ON
        (A.AlueTaso_ID = TTJ.AlueTaso_ID AND
        @YearIs BETWEEN A.Alkaen_Jakso_ID AND A.Asti_Jakso_ID)

    {1}

    LEFT OUTER JOIN DimTilasto_JohdettuTilasto_Summa TS ON
        (TS.Tilasto_ID = TTJ.Tilasto_ID AND
        TS.Ryhma_SEQ = 0)

    LEFT OUTER JOIN FactTilastoarvo T ON
        (T.Tilasto_ID = TS.Yhteenlaskettava_Tilasto_ID AND
        T.Alue_ID = A.Alue_ID AND
        TTJ.AlueTaso_ID = TTJ.AlueTaso_ID AND
        T.Jakso_ID = TTJ.Jakso_ID)

WHERE
    ATJ.Jakso_ID = @YearIs AND
    ATJ.Kunta_Alue_ID = 1 /* this column is selected by DatabaseAreaType */
    {2}

GROUP BY
    {3}
    {4}
";
            queryString = string.Format(queryString,
                this.GetFieldsString(),
                this.sbFrom.ToString(),
                this.GetWhereString(),
                this.GetGroupString(),
                this.GetOrderString());

            return queryString;
        }

        /*
         * CalculationType == 4
         */
        private string GetQueryString_Special()
        {
            int primaryDbAreaType = AreaTypeMappings.GetPrimaryDatabaseAreaType(
                    this.GroupByAreaTypeIdIs);
            if (!this.AvailableAreaTypes.Contains(primaryDbAreaType)) {
                throw new Exception("Supplied grouping areaType not suitable for this statistics data!");
            }

            this.fields.Add("ATJ.Jakso_ID AS Year");
            //this.groups.Add("T1.Jakso_ID");

            this.fields.Add("COALESCE(T.Arvo, 0) AS Value");

            /* don't allow any other areaType to be selected for this */
            this.RelaxedAreaTypes = false;

            this.SetFilters();
            this.SetGroups(); // nothing should be grouped in this query
            this.SetDatabaseAreaTypeId();

            string queryString = @"
SELECT
    {0}
FROM
    Apu_AlueTallennusJakso ATJ

    INNER JOIN Apu_TilastoTallennusJakso TTJ ON
        (TTJ.Jakso_ID = ATJ.Jakso_ID AND
        TTJ.Tilasto_ID = @IdIs AND
        TTJ.AlueTaso_ID = @DatabaseAreaTypeIdIs)

    INNER JOIN DimAlue A ON
        (A.AlueTaso_ID = TTJ.AlueTaso_ID AND
        @YearIs BETWEEN A.Alkaen_Jakso_ID AND A.Asti_Jakso_ID)

    {1}

    LEFT OUTER JOIN FactTilastoarvo T ON
        (T.Jakso_ID = TTJ.Jakso_ID AND
        T.AlueTaso_ID = TTJ.AlueTaso_ID AND
        T.Alue_ID = A.Alue_ID AND
        T.Tilasto_ID = TTJ.Tilasto_ID)
WHERE
    ATJ.Jakso_ID = @YearIs
    {2}
    {3}
";
            queryString = string.Format(queryString,
                this.GetFieldsString(),
                this.sbFrom.ToString(),
                this.GetWhereString(),
                this.GetOrderString());

            return queryString;
        }

        /*
         * CalculationTypeId == 1
         * this has been converted to new query
         */
        private string GetQueryString_Normal()
        {
            this.fields.Add("ATJ.Jakso_ID AS Year");
            this.groups.Add("ATJ.Jakso_ID");

            this.fields.Add("SUM(COALESCE(T.Arvo, 0)) AS Value");

            this.SetFilters();
            this.SetGroups();
            this.SetDatabaseAreaTypeId();

            string queryString = @"
SELECT
    {0}
FROM
    /* Jakso_ID, ..._Alue_ID == 1/0 */
    Apu_AlueTallennusJakso ATJ

    /* Tilasto_ID, Jakso_ID, AlueTaso_ID */
    INNER JOIN Apu_TilastoTallennusJakso TTJ ON
        (TTJ.Jakso_ID = ATJ.Jakso_ID AND
        TTJ.Tilasto_ID = @IdIs AND
        TTJ.AlueTaso_ID = @DatabaseAreaTypeIdIs)

    INNER JOIN DimAlue A ON
        (A.AlueTaso_ID = TTJ.AlueTaso_ID AND
        @YearIs BETWEEN A.Alkaen_Jakso_ID AND A.Asti_Jakso_ID)

    {1}

    LEFT OUTER JOIN FactTilastoarvo T ON
        (T.AlueTaso_ID = TTJ.AlueTaso_ID AND
        T.Tilasto_ID = TTJ.Tilasto_ID AND
        T.Jakso_ID = TTJ.Jakso_ID AND
        T.Alue_ID = A.Alue_ID)
WHERE
    /* The ATJ column is selected by DatabaseAreaTypeId */
    ATJ.Jakso_ID = @YearIs
    {2}
GROUP BY
    {3}
    {4}
";
            queryString = string.Format(queryString,
                this.GetFieldsString(),
                this.sbFrom.ToString(),
                this.GetWhereString(),
                this.GetGroupString(),
                this.GetOrderString());

            return queryString;
        }

        public override string GetQueryString()
        {
            string queryString;

            Debug.WriteLine(string.Format(
                "We have these areaTypes available: [{0}]",
                string.Join(",", this.AvailableAreaTypes)));

            /*
            var schema = AreaTypeMappings.GetDatabaseSchema(this.GroupByAreaTypeIdIs);
            if (schema["GeometryColumn"] != null &&
                    schema["GeometryColumn"].Length > 0) {
                this.fields.Add("A2.KoordErTmPohj AS AreaPointLat");
                this.groups.Add("A2.KoordErTmPohj");
                this.fields.Add("A2.KoordErTmIta AS AreaPointLon");
                this.groups.Add("A2.KoordErTmIta");
            } else {
                this.fields.Add("NULL AS AreaPointLat");
                this.fields.Add("NULL AS AreaPointLon");
            }
            */

            switch (this.CalculationTypeIdIs) {
                case 1: // normal
                    logger.Debug("Statistics query: normal");
                    queryString = this.GetQueryString_Normal();
                    break;
                case 3: // derived & divided
                    logger.Debug("Statistics query: derived/divided");
                    queryString = this.GetQueryString_DerivedDivided();
                    break;
                case 4: // special
                    logger.Debug("Statistics query: special");
                    queryString = this.GetQueryString_Special();
                    break;
                case 5: // derived & summed
                    logger.Debug("Statistics query: derived/summed");
                    queryString = this.GetQueryString_DerivedSummed();
                    break;
                default:
                    string errMsg = string.Format(
                        "Unsupported CalculationType: {0}",
                        this.CalculationTypeIdIs);
                    logger.Error(errMsg);
                    throw new Exception(errMsg);
            }

            /* preQuery stuff (which are geometry declarations at the moment)
             * should be common for all query types, let's prepend it here */
            queryString = this.sbPreQuery.ToString() + "\n" + queryString;

            return queryString;
        }
    }
}