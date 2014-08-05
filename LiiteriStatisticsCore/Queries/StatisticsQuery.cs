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

        private List<string> whereList;

        private static LiiteriStatisticsCore.Util.AreaTypeMappings
            AreaTypeMappings = new LiiteriStatisticsCore.Util.AreaTypeMappings();

        public StatisticsQuery(int id) : base()
        {
            this.IdIs = id;
            this.whereList = new List<string>();
        }

        public int IdIs
        {
            get
            {
                return (int) this.GetParameter("@IdIs");
            }
            set
            {
                this.AddParameter("@IdIs", value);
            }
        }

        public int DatabaseAreaTypeIdIs
        {
            get
            {
                return (int) this.GetParameter("@DatabaseAreaTypeIdIs");
            }
            set
            {
                this.AddParameter("@DatabaseAreaTypeIdIs", value);
            }
        }

        public int YearIs
        {
            get
            {
                return (int) this.GetParameter("@YearIs");
            }
            set
            {
                this.AddParameter("@YearIs", value);
            }
        }

        //public int SelectionAreaType { get; set; }
        public int CalculationTypeIdIs { get; set; }

        /* Filters */
        public string FilterAreaTypeIdIs { get; set; }
        public int? FilterAreaIdIs
        {
            get
            {
                return (int?) this.GetParameter("@FilterAreaIdIs");
            }
            set
            {
                if (value == null) return;
                this.AddParameter("@FilterAreaIdIs", value);
            }
        }

        /* Grouping */
        public string GroupByAreaTypeIdIs { get; set; }

        private string GetQueryString_SummedDivided()
        {
            IDictionary<string, string> fields =
                new Dictionary<string, string>();
            List<string> groups = new List<string>();
            StringBuilder sbFrom = new StringBuilder();

            fields["T1.Jakso_ID"] = "Year";
            groups.Add("T1.Jakso_ID");

            if (this.FilterAreaIdIs != null &&
                    this.FilterAreaTypeIdIs != null) {
                string column = AreaTypeMappings.GetDatabaseIdColumn(
                    this.FilterAreaTypeIdIs);
                this.whereList.Add(string.Format(
                    "{0} = @FilterAreaIdIs", column));
            }

            fields["(SUM(T1.Arvo) / SUM(T2.Arvo))"] = "Value";

            /* add the proper table that we are grouping by */
            if (this.GroupByAreaTypeIdIs != null) {
                string idColumn = AreaTypeMappings.GetDatabaseIdColumn(
                    this.GroupByAreaTypeIdIs);
                fields[idColumn] = "AreaId";
                groups.Add(idColumn);

                string nameColumn = AreaTypeMappings.GetDatabaseNameColumn(
                    this.GroupByAreaTypeIdIs);
                fields[nameColumn] = "AreaName";
                groups.Add(nameColumn);

                sbFrom.Append(AreaTypeMappings.GetDatabaseJoinQuery(
                    this.GroupByAreaTypeIdIs));
            } else {
                fields["NULL"] = "AreaId";
                fields["NULL"] = "AreaName";
            }

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
            string fieldsString = string.Join<string>(", ", (
                    from pair in fields
                    select string.Format("{0} AS {1}", pair.Key, pair.Value)
                ).ToArray());

            string groupString =
                string.Join<string>(", ", groups);

            string whereString = "";
            if (this.whereList.Count > 0) {
                whereString = " AND " + string.Join(" AND ", this.whereList);
            }

            queryString = string.Format(queryString,
                fieldsString,
                sbFrom.ToString(),
                whereString,
                groupString);

            return queryString;
        }

        private string GetQueryString_Normal()
        {
            IDictionary<string, string> fields =
                new Dictionary<string, string>();
            List<string> groups = new List<string>();

            StringBuilder sbFrom = new StringBuilder();

            fields["T.Jakso_ID"] = "Year";
            groups.Add("T.Jakso_ID");

            if (this.FilterAreaIdIs != null &&
                    this.FilterAreaTypeIdIs != null) {
                string column = AreaTypeMappings.GetDatabaseIdColumn(
                    this.FilterAreaTypeIdIs);
                this.whereList.Add(string.Format(
                    "{0} = @FilterAreaIdIs", column));
            }

            fields["SUM(T.Arvo)"] = "Value";

            /* add the proper table that we are grouping by */
            if (this.GroupByAreaTypeIdIs != null) {
                string idColumn = AreaTypeMappings.GetDatabaseIdColumn(
                    this.GroupByAreaTypeIdIs);
                fields[idColumn] = "AreaId";
                groups.Add(idColumn);

                string nameColumn = AreaTypeMappings.GetDatabaseNameColumn(
                    this.GroupByAreaTypeIdIs);
                fields[nameColumn] = "AreaName";
                groups.Add(nameColumn);

                sbFrom.Append(AreaTypeMappings.GetDatabaseJoinQuery(
                    this.GroupByAreaTypeIdIs));
            } else {
                fields["NULL"] = "AreaId";
                fields["NULL"] = "AreaName";
            }

            
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

            string fieldsString = string.Join<string>(", ", (
                    from pair in fields
                    select string.Format("{0} AS {1}", pair.Key, pair.Value)
                ).ToArray());

            string groupString =
                string.Join<string>(", ", groups);

            string whereString = "";
            if (this.whereList.Count > 0) {
                whereString = " AND " + string.Join(" AND ", this.whereList);
            }

            queryString = string.Format(queryString,
                fieldsString,
                sbFrom.ToString(),
                whereString,
                groupString);

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
                case 3: // summed & divided
                    logger.Debug("Statistics query: summed & divided");
                    queryString = this.GetQueryString_SummedDivided();
                    break;
                default:
                    throw new Exception(string.Format(
                        "Unsupported CalculationType: {0}",
                        this.CalculationTypeIdIs));
            }

            return queryString;
        }
    }
}