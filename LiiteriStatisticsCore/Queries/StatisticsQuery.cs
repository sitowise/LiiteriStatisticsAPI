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
        IDictionary<string, string> fields;

        // these will be added to GROUP BY
        List<string> groups;

        // this is for JOINs and such
        StringBuilder sbFrom;

        public StatisticsQuery(int id) : base()
        {
            this.IdIs = id;

            this.whereList = new List<string>();
            this.fields = new Dictionary<string, string>();
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

        public int DatabaseAreaTypeIdIs
        {
            get
            {
                return (int) this.Parameters["DatabaseAreaTypeIdIs"].Value;
            }
            set
            {
                this.Parameters.Add("DatabaseAreaTypeIdIs", value);
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

        private void SetGroups()
        {
            /* add the proper table that we are grouping by */
            if (this.GroupByAreaTypeIdIs != null) {
                string idColumn = AreaTypeMappings.GetDatabaseIdColumn(
                    this.GroupByAreaTypeIdIs);
                this.fields[idColumn] = "AreaId";
                this.groups.Add(idColumn);

                string nameColumn = AreaTypeMappings.GetDatabaseNameColumn(
                    this.GroupByAreaTypeIdIs);
                this.fields[nameColumn] = "AreaName";
                this.groups.Add(nameColumn);

                sbFrom.Append(AreaTypeMappings.GetDatabaseJoinQuery(
                    this.GroupByAreaTypeIdIs));
            } else {
                this.fields["NULL"] = "AreaId";
                this.fields["NULL"] = "AreaName";
            }
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
                    return AreaTypeMappings.GetDatabaseIdColumn(name);
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
            return string.Join<string>(", ", (
                    from pair in this.fields
                    select string.Format("{0} AS {1}", pair.Key, pair.Value)
                ).ToArray());
        }

        private string GetGroupString()
        {
            return string.Join<string>(", ", this.groups);
        }

        private string GetQueryString_DerivedDivided()
        {
            this.fields["T1.Jakso_ID"] = "Year";
            this.groups.Add("T1.Jakso_ID");

            this.fields["(SUM(T1.Arvo) / SUM(T2.Arvo))"] = "Value";

            this.SetFilters();
            this.SetGroups();

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
            this.fields["T.Jakso_ID"] = "Year";
            this.groups.Add("T.Jakso_ID");

            this.fields["SUM(T.Arvo)"] = "Value";

            this.SetFilters();
            this.SetGroups();

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

            this.fields["T.Jakso_ID"] = "Year";
            this.groups.Add("T.Jakso_ID");

            this.fields["SUM(T.Arvo)"] = "Value";

            this.SetFilters();
            this.SetGroups();

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