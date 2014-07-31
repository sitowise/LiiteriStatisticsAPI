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

        /*
         * This table lists all the area types from DimAlueTaso
         * and links them to their column name in DimAlue
         * as well as the table containing detailed area data
         */
        private static IDictionary<int, string[]> AreaFields =
                new Dictionary<int, string[]>() {
            /* id, Description, ColumnName, TableName*/
            {1, new string[] {"Ruutu 250 m", "Ruutu_Alue_ID", "DimRuutu"}},
            {2, new string[] {"Kunta", "Kunta_Alue_ID", "DimKunta"}},
            {3, new string[] {"Taajama", "Taajama_Alue_ID", "DimTaajama"}},
            {4, new string[] {"Kylä", null, null}},
            {5, new string[] {"Pienkylä", null, null}},
            {7, new string[] {"Ruutu 500 m", "Ruutu_500m_Alue_ID", "DimRuutu"}},
            {8, new string[] {"Harva pientaloasutus", null, null}},
            {9, new string[] {"Kerrostaloalue", null, null}},
            {10, new string[] {"Pientaloalue", null, null}},
            {11, new string[] {"Asemakaavoitettu alue", "AsemakaavoitettuAlue_Alue_ID", "DimAsemakaavoitettuAlue"}},
            {12, new string[] {"Ruutu 250 m, asemakaavoitettu osa", null, null}},
            {13, new string[] {"Kunta, asemakaavoitettu osa", null, null}},
            {14, new string[] {"Harva maaseutuasutus", null, null}},
            {15, new string[] {"Kaupunkiseutu", "Kaupunkiseutu_Alue_ID", "DimKaupunkiseutu"}},
            {16, new string[] {"Maakunta", "Maakunta_Alue_ID", "DimMaakunta"}},
            {17, new string[] {"Seutukunta", "Seutukunta_Alue_ID", "DimSeutukunta"}},
            {18, new string[] {"ELY", null, null}},
            {19, new string[] {"Hallinto-oikeus", "HallintoOikeus_Alue_ID", "DimHallintoOikeus"}},
            {20, new string[] {"Haja-asutusalue", null, null}},
            {21, new string[] {"Suomi", null, null}},
            {22, new string[] {"Ruutu 1 km", "Ruutu_1km_Alue_ID", "DimRuutu"}},
            {23, new string[] {"Ruutu 2 km", "Ruutu_2km_Alue_ID", "DimRuutu"}},
            {24, new string[] {"Ruutu 5 km", "Ruutu_5km_Alue_ID", "DimRuutu"}},
            {25, new string[] {"Ruutu 10 km", "Ruutu_10km_Alue_ID", "DimRuutu"}},
            {26, new string[] {"Ruutu 20 km", "Ruutu_20km_Alue_ID", "DimRuutu"}},
            {27, new string[] {"Keskusta", "Keskusta_Alue_ID", "DimKeskusta"}},
            {28, new string[] {"Kaupan alue", "KaupanAlue_Alue_ID", "DimKaupanAlue"}},
        };

        private string GetAreaColumn(int areaTypeId)
        {
            return StatisticsQuery.AreaFields[areaTypeId][1];
        }

        private string GetAreaTable(int areaTypeId)
        {
            return StatisticsQuery.AreaFields[areaTypeId][2];
        }

        public override string GetQueryString()
        {
            IDictionary<string, string> fields =
                new Dictionary<string, string>();

            StringBuilder sb = new StringBuilder();

            StringBuilder sbFrom = new StringBuilder();

            fields["T.Arvo"] = "Value";

            if (this.FilterAreaIdIs != null &&
                    this.FilterAreaTypeIdIs != null) {
                this.whereList.Add(string.Format("A.{0} = @FilterAreaIdIs",
                    this.GetAreaColumn((int) this.FilterAreaTypeIdIs)));
                /* A.Maakunta_Alue_ID = @AreaIdIs */
            }
            switch (this.CalculationTypeIdIs) {
                case 1: // normal

                    /* grouping by these fields */
                    fields["A." + this.GetAreaColumn(this.GroupByAreaTypeIdIs)] =
                        "AreaId";
                    fields["A2.Nimi"] = "AreaName";

                    /* add the proper table that we are grouping by */
                    sbFrom.Append(string.Format(
                        "INNER JOIN {0} A2 ON A2.Alue_ID = A.{1} ",
                        this.GetAreaTable(this.GroupByAreaTypeIdIs),
                        this.GetAreaColumn(this.GroupByAreaTypeIdIs)));

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

            if (this.whereList.Count > 0) {
                sb.Append(string.Join(" AND ", whereList));
            }

            foreach (var param in this.Parameters) {
                Debug.WriteLine("{0}: {1}", param.Key, param.Value);
            }
            Debug.WriteLine(queryString);

            return queryString;
        }
    }
}