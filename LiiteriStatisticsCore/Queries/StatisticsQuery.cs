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
        public int? FilterAreaTypeIdIs { get; set; }
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
        public int GroupByAreaTypeIdIs { get; set; }

        public override string GetQueryString()
        {
            IDictionary<string, string> fields =
                new Dictionary<string, string>();

            StringBuilder sbFrom = new StringBuilder();

            fields["T.Arvo"] = "Value";
            fields["T.Jakso_ID"] = "Year";

            if (this.FilterAreaIdIs != null &&
                    this.FilterAreaTypeIdIs != null) {
                string column = AreaTypeMappings.GetAreaColumn(
                    (int) this.FilterAreaTypeIdIs);
                this.whereList.Add(string.Format(
                    "A.{0} = @FilterAreaIdIs", column));
                /* A.Maakunta_Alue_ID = @AreaIdIs */
            }

            switch (this.CalculationTypeIdIs) {
                case 1: // normal

                    /* grouping by these fields */
                    string column = AreaTypeMappings.GetAreaColumn(
                        (int) this.GroupByAreaTypeIdIs);
                    fields["A." + column] = "AreaId";
                    fields["A2.Nimi"] = "AreaName";

                    /* add the proper table that we are grouping by */
                    sbFrom.Append(string.Format(
                        "INNER JOIN {0} A2 ON A2.Alue_ID = A.{1} ",
                        AreaTypeMappings.GetAreaTable(this.GroupByAreaTypeIdIs),
                        AreaTypeMappings.GetAreaColumn(this.GroupByAreaTypeIdIs)));

                    /* then add filtering here */
                    break;
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
	T.Jakso_ID = @YearIs AND
	T.AlueTaso_ID = @DatabaseAreaTypeIdIs AND
	T.Arvo IS NOT NULL
    {2}
GROUP BY
    {3}
";

            string fieldsString = string.Join<string>(", ", (
                    from pair in fields
                    select string.Format("{0} AS {1}", pair.Key, pair.Value)
                ).ToArray());

            string groupString =
                string.Join<string>(", ", fields.Keys);

            string whereString = "";
            if (this.whereList.Count > 0) {
                whereString = " AND " + string.Join(" AND ", this.whereList);
            }

            string fromString = sbFrom.ToString();
            
            queryString = string.Format(queryString,
                fieldsString,
                fromString,
                whereString,
                groupString);

            foreach (var param in this.Parameters) {
                Debug.WriteLine("{0}: {1}", param.Key, param.Value);
            }
            Debug.WriteLine(queryString);

            return queryString;
        }
    }
}