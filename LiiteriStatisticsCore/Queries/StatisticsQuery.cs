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
                this.whereList.Add(string.Format("T.Jakso_ID = {0}", value));
            }
        }

        private int[] _YearIn;
        public int[] YearIn
        {
            get
            {
                return this._YearIn;
            }
            set
            {
                if (value == null) return;
                if (this._YearIn != null) {
                    throw new ArgumentException("Value already set!");
                }
                this._YearIn = value;
                string[] paramNames = value.Select(
                        (s, i) => "@YearIn_" + i.ToString()
                    ).ToArray();
                for (int i = 0; i < paramNames.Length; i++) {
                    this.AddParameter(paramNames[i], value[i]);
                }
                this.whereList.Add(string.Format(
                    "T.Jakso_ID IN ({0})",
                    string.Join(",", paramNames)));
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
            List<string> groups = new List<string>();

            StringBuilder sbFrom = new StringBuilder();

            fields["T.Jakso_ID"] = "Year";
            groups.Add("T.Jakso_ID");

            if (this.FilterAreaIdIs != null &&
                    this.FilterAreaTypeIdIs != null) {
                string column = AreaTypeMappings.GetAreaColumn(
                    (int) this.FilterAreaTypeIdIs);
                this.whereList.Add(string.Format(
                    "A.{0} = @FilterAreaIdIs", column));
            }

            switch (this.CalculationTypeIdIs) {
                case 1: // normal

                    fields["SUM(T.Arvo)"] = "Value";

                    fields["A2.Alue_ID"] = "AreaId";
                    groups.Add("A2.Alue_ID");

                    fields["A2.Nimi"] = "AreaName";
                    groups.Add("A2.Nimi");

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

            foreach (var param in this.Parameters) {
                Debug.WriteLine("DECLARE {0} INT = {1}", param.Key, param.Value);
            }

            Debug.WriteLine(queryString);

            return queryString;
        }
    }
}