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
                return (int) this.GetParameter("@IdIs");
            }
            set
            {
                if (value == null) return;
                this.whereList.Add("T.Tilasto_Id = @IdIs");
                this.AddParameter("@IdIs", value);
            }
        }

        public string NameIs
        {
            get
            {
                return (string) this.GetParameter("@NameIs");
            }
            set
            {
                if (value == null) return;
                this.whereList.Add("T.Nimi = @NameIs");
                this.AddParameter("@NameIs", value);
            }
        }

        public string NameLike
        {
            get
            {
                return (string) this.GetParameter("@NameLike");
            }
            set
            {
                if (value == null) return;
                this.whereList.Add("T.Nimi LIKE @NameLike");
                this.AddParameter("@NameLike", value);
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

            fields["J.Jakso_ID"] = "PeriodId";
            fields["J.AlueTaso_ID"] = "AreaTypeId";

            sb.Append(
                string.Join<string>(", ", (
                    from pair in fields
                    select string.Format(
                        "{0} AS {1}", pair.Key, pair.Value)
                    ).ToArray())
                );

            sb.Append(string.Format(" FROM [{0}]..[DimTilasto] T ",
                ConfigurationManager.AppSettings["DbDataMarts"]));
            sb.Append(string.Format(
                "INNER JOIN [{0}]..[Apu_TilastoTallennusJakso] J ON ",
                ConfigurationManager.AppSettings["DbDataMarts"]));
            sb.Append("J.Tilasto_Id = T.Tilasto_Id ");

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