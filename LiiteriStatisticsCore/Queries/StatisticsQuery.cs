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

        private static LiiteriStatisticsCore.Util.AreaTypeMappings
            AreaTypeMappings = new LiiteriStatisticsCore.Util.AreaTypeMappings();

        private List<string> whereList;

        // these will be added as fields for SELECT
        //IDictionary<string, string> fields;
        List<string> fields;

        // these will be added to GROUP BY
        List<string> groups;

        // this is for JOINs and such
        StringBuilder sbFrom;

        public StatisticsQuery(int id) : base()
        {
            this.IdIs = id;

            this.whereList = new List<string>();
            //this.fields = new Dictionary<string, string>();
            this.fields = new List<string>();
            this.groups = new List<string>();
            this.sbFrom = new StringBuilder();
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

        private void ReduceUsableAreaTypes(string areaType)
        {
            int[] dbAreaTypes =
                AreaTypeMappings.GetDatabaseAreaTypes(areaType);
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
                    this.fields.Add(string.Format("{0} AS AreaId", idColumn));
                    this.groups.Add(idColumn);
                } else {
                    this.fields.Add("-1 AS AreaId");
                }

                string nameColumn = schema["SubNameColumn"];
                if (nameColumn != null && idColumn.Length > 0) {
                    this.fields.Add(string.Format("{0} AS AreaName", nameColumn));
                    this.groups.Add(nameColumn);
                } else {
                    this.fields.Add("NULL AS AreaName");
                }

                string alternativeIdColumn = schema["SubAlternativeIdColumn"];
                if (alternativeIdColumn != null && alternativeIdColumn.Length > 0) {
                    this.fields.Add(string.Format(
                        "{0} AS AlternativeId", alternativeIdColumn));
                    this.groups.Add(alternativeIdColumn);
                } else {
                    this.fields.Add("NULL AS AlternativeId");
                }

                string joinQuery = schema["JoinQuery"];
                sbFrom.Append(joinQuery);

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
                    return AreaTypeMappings.GetDatabaseSchema(
                        name)["MainIdColumn"];
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
            return string.Join(", ", this.fields);
        }

        private string GetGroupString()
        {
            return string.Join<string>(", ", this.groups);
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
        }

        private string GetQueryString_DerivedDivided()
        {
            this.fields.Add("T1.Jakso_ID AS Year");
            this.groups.Add("T1.Jakso_ID");

            this.fields.Add("(SUM(T1.Arvo) / SUM(T2.Arvo)) AS Value");

            this.SetFilters();
            this.SetGroups();
            this.SetDatabaseAreaTypeId();

            string queryString = @"
SELECT
    {0}
FROM
    DimTilasto_JohdettuTilasto_Jako J

    INNER JOIN FactTilastoarvo T1 ON
        T1.Tilasto_ID = J.Osoittaja_Tilasto_ID AND
        T1.Jakso_ID = @YearIs AND
        T1.AlueTaso_ID = @DatabaseAreaTypeIdIs

    INNER JOIN FactTilastoarvo T2 ON
        T2.Tilasto_ID = J.Nimittaja_Tilasto_ID AND
        T2.Jakso_ID = @YearIs AND
        T2.AlueTaso_ID = @DatabaseAreaTypeIdIs

    INNER JOIN DimAlue A ON
        A.Alue_ID = T1.Alue_ID AND
        A.Alue_ID = T2.Alue_ID AND
        @YearIs BETWEEN A.Alkaen_Jakso_ID AND A.Asti_Jakso_ID AND
        A.AlueTaso_ID = @DatabaseAreaTypeIdIs
    {1}
WHERE
    J.Tilasto_ID = @IdIs AND
	T1.Arvo IS NOT NULL AND
	T2.Arvo IS NOT NULL AND
	T1.Arvo > 0 AND
	T2.Arvo > 0
    {2}
GROUP BY
    {3}
";
            queryString = string.Format(queryString,
                this.GetFieldsString(),
                sbFrom.ToString(),
                this.GetWhereString(),
                this.GetGroupString());

            return queryString;
        }

        private string GetQueryString_DerivedSummed()
        {
            this.fields.Add("T.Jakso_ID AS Year");
            this.groups.Add("T.Jakso_ID");

            this.fields.Add("SUM(T.Arvo) AS Value");

            this.SetFilters();
            this.SetGroups();
            this.SetDatabaseAreaTypeId();

            string queryString = @"
SELECT
    {0}
FROM
	DimTilasto_JohdettuTilasto_Summa TS

    INNER JOIN FactTilastoarvo T ON
        T.Tilasto_ID = TS.Yhteenlaskettava_Tilasto_ID AND
        T.Jakso_ID = @YearIs

    INNER JOIN DimAlue A ON
        A.Alue_ID = T.Alue_ID AND
        @YearIs BETWEEN A.Alkaen_Jakso_ID AND A.Asti_Jakso_ID AND
        A.AlueTaso_ID = @DatabaseAreaTypeIdIs
    {1}
WHERE
    TS.Tilasto_ID = @IdIs AND
    TS.Ryhma_SEQ = 0
    {2}
GROUP BY
    {3}
";
            queryString = string.Format(queryString,
                this.GetFieldsString(),
                sbFrom.ToString(),
                this.GetWhereString(),
                this.GetGroupString());

            return queryString;
        }

        private string GetQueryString_Normal()
        {

            this.fields.Add("T.Jakso_ID AS Year");
            this.groups.Add("T.Jakso_ID");

            this.fields.Add("SUM(T.Arvo) AS Value");

            this.SetFilters();
            this.SetGroups();
            this.SetDatabaseAreaTypeId();

            string queryString = @"
SELECT
    {0}
FROM
	FactTilastoArvo T
    INNER JOIN DimAlue A ON
        A.Alue_ID = T.Alue_ID AND
        @YearIs BETWEEN A.Alkaen_Jakso_ID AND A.Asti_Jakso_ID AND
        A.AlueTaso_ID = @DatabaseAreaTypeIdIs
    {1}
WHERE
	T.Tilasto_ID = @IdIs AND
	T.AlueTaso_ID = @DatabaseAreaTypeIdIs AND
	T.Arvo IS NOT NULL AND 
    T.Jakso_ID = @YearIs
    {2}
GROUP BY
    {3}
";
            queryString = string.Format(queryString,
                this.GetFieldsString(),
                sbFrom.ToString(),
                this.GetWhereString(),
                this.GetGroupString());

            return queryString;
        }

        public override string GetQueryString()
        {
            string queryString;

            Debug.WriteLine(string.Format(
                "We have these areaTypes available: [{0}]",
                string.Join(",", this.AvailableAreaTypes)));

            switch (this.CalculationTypeIdIs) {
                case 1: // normal
                    logger.Debug("Statistics query: normal");
                    queryString = this.GetQueryString_Normal();
                    break;
                case 3: // derived & divided
                    logger.Debug("Statistics query: derived/divided");
                    queryString = this.GetQueryString_DerivedDivided();
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

            return queryString;
        }
    }
}