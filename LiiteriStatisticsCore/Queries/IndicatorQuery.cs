using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Diagnostics;

namespace LiiteriStatisticsCore.Queries
{
    public class IndicatorQuery : SqlQuery, ISqlQuery
    {
        private List<string> whereList;

        public IndicatorQuery() : base()
        {
            this.whereList = new List<string>();
        }

        public int? IdIs
        {
            get
            {
                return (int) this.Parameters["IdIs"].Value;
            }
            set
            {
                if (value == null) return;
                this.whereList.Add("T.Tilasto_Id = @IdIs");
                this.Parameters.Add("IdIs", value);
            }
        }

        public string NameIs
        {
            get
            {
                return (string) this.Parameters["NameIs"].Value;
            }
            set
            {
                if (value == null) return;
                this.whereList.Add("T.Nimi = @NameIs");
                this.Parameters.Add("NameIs", value);
            }
        }

        public string NameLike
        {
            get
            {
                return (string) this.Parameters["NameLike"].Value;
            }
            set
            {
                if (value == null) return;
                this.whereList.Add("T.Nimi LIKE @NameLike");
                this.Parameters.Add("NameLike", value);
            }
        }

        public override string GetQueryString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("SELECT ");

            var fields = new Dictionary<string, string>();

            /* These are used by IndicatorBrief */
            fields["T.Tilasto_Id"] = "Id";
            fields["T.Nimi"] = "Name";

            /* These are used by IndicatorDetails */
            fields["T.EsitysDesimaalitarkkuus"] = "DecimalCount";
            fields["T.EsitysDesimaalitarkkuus"] = "DecimalCount";
            fields["T.MittayksikkoTallennus_Mittayksikko_ID"] = "InternalUnitId";
            fields["T.MittayksikkoEsitys_Mittayksikko_ID"] = "DisplayUnitId";
            fields["T.Kuvaus"] = "Description";
            fields["T.Lisatieto"] = "AdditionalInformation";
            fields["T.TilastoLaskentatyyppi_ID"] = "CalculationType";

            /* This is used by TimePeriod */
            fields["J.Jakso_ID"] = "PeriodId";

            /* These are used by AreaType */
            fields["J.AlueTaso_ID"] = "AreaTypeId";
            fields["TL.Tietolahde"] = "DataSource";

            sb.Append(
                string.Join<string>(", ", (
                    from pair in fields
                    select string.Format(
                        "{0} AS {1}", pair.Key, pair.Value)
                    ).ToArray())
                );

            sb.Append(string.Format(" FROM [{0}]..[DimTilasto] T ",
                ConfigurationManager.AppSettings["DbDataMarts"]));

            sb.Append(string.Format(@"
INNER JOIN [{0}]..[Apu_TilastoTallennusJakso] J ON
    J.Tilasto_Id = T.Tilasto_Id
",
                ConfigurationManager.AppSettings["DbDataMarts"]));

            sb.Append(string.Format(@"
OUTER APPLY (
    SELECT
        TOP 1 TL.Tietolahde
    FROM
        [LiiteriDataMarts]..[FactTilastoTietolahde] TL
    WHERE
        J.Jakso_ID >= TL.Alkaen_Jakso_ID AND
        J.AlueTaso_ID = TL.AlueTaso_ID AND
        t.Tilasto_ID = TL.Tilasto_ID
    ORDER BY
        TL.Alkaen_Jakso_ID DESC
    ) TL
",
                ConfigurationManager.AppSettings["DbDataMarts"]));

            //this.whereList.Add("J.AlueTaso_Id = 2");
            this.whereList.Add("T.TilastoLaskentatyyppi_ID <> 2");

            if (this.whereList.Count > 0) {
                sb.Append(" WHERE ");
                sb.Append(string.Join(" AND ", whereList));
            }

            sb.Append("ORDER BY T.Tilasto_ID, J.Jakso_ID, J.AlueTaso_ID ");

            return sb.ToString();
        }
    }
}