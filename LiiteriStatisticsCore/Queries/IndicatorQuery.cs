using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public int? AccessRightIdIs
        {
            get
            {
                return (int) this.Parameters["AccessRightIdIs"].Value;
            }
            set
            {
                if (value == null) return;
                this.whereList.Add("OIK.KayttoOikeus_ID = @AccessRightIdIs");
                this.Parameters.Add("AccessRightIdIs", value);
            }
        }

        [System.ComponentModel.DefaultValue(false)]
        public bool IncludeHelperStatistics { get; set; }

        public override string GetQueryString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("SELECT ");

            var fields = new Dictionary<string, string>();

            /* These are used by IndicatorBrief */
            fields["T.Tilasto_Id"] = "Id";
            fields["T.Nimi"] = "Name";
            fields["T.JarjNro"] = "OrderNumber";

            /* These are used by IndicatorDetails */
            fields["T.EsitysDesimaalitarkkuus"] = "DecimalCount";
            fields["T.EsitysDesimaalitarkkuus"] = "DecimalCount";
            fields["T.MittayksikkoTallennus_Mittayksikko_ID"] = "InternalUnitId";
            fields["T.MittayksikkoEsitys_Mittayksikko_ID"] = "DisplayUnitId";
            fields["T.Kuvaus"] = "Description";
            fields["T.Lisatieto"] = "AdditionalInformation";
            fields["T.TilastoLaskentatyyppi_ID"] = "CalculationType";

            fields["MY.MittayksikkoLyhenne"] = "Unit";

            // ProcessingStage only exists in the Excel file!
            //fields["???"] = "ProcessingStage";

            fields["AJV.AjallinenVaiheKuvaus"] = "TimeSpanDetails";
            fields["AJV.AjallinenVaiheLyhenne"] = "TimeSpan";

            /* This is used by TimePeriod */
            fields["J.Jakso_ID"] = "PeriodId";

            /* These are used by AreaType */
            fields["J.AlueTaso_ID"] = "AreaTypeId";
            fields["TL.Tietolahde"] = "DataSource";

            /* Privacy limits */
            fields["TS.Ref_Tilasto_ID"] = "PrivacyLimitStatisticsId";
            fields["TS.GreaterThan"] = "PrivacyLimitGreaterThan";
            fields["TS.Selite"] = "PrivacyLimitDescription";

            /* Annotations */
            fields["LT.Lisatieto"] = "Annotation";
            fields["LT.O_Lyhenne"] = "AnnotationOrganizationShort";
            fields["LT.O_Nimi"] = "AnnotationOrganizationName";
            fields["LT.O_Nro"] = "AnnotationOrganizationNumber";

            /* Right of access */
            fields["OIK.KayttoOikeus_ID"] = "AccessRightID";
            fields["OIK.Selite"] =  "AccessRightDescription";

            /* prevent using these areatypes for anything */
            fields["ESTO.RajapintaAlueTaso"] = "BlockedAreaTypes";

            sb.Append(
                string.Join<string>(", ", (
                    from pair in fields
                    select string.Format(
                        "{0} AS {1}", pair.Key, pair.Value)
                    ).ToArray())
                );

            sb.Append("\nFROM DimTilasto T ");

            sb.Append(@"
INNER JOIN Apu_TilastoTallennusJakso J ON
    J.Tilasto_Id = T.Tilasto_Id
");

            sb.Append(@"
OUTER APPLY (
    SELECT
        TOP 1 TL.Tietolahde
    FROM
        FactTilastoTietolahde TL
    WHERE
        J.Jakso_ID >= TL.Alkaen_Jakso_ID AND
        J.AlueTaso_ID = TL.AlueTaso_ID AND
        t.Tilasto_ID = TL.Tilasto_ID
    ORDER BY
        TL.Alkaen_Jakso_ID DESC
    ) TL
");

            sb.Append(@"
    OUTER APPLY (
        SELECT
            LTX.Lisatieto AS Lisatieto,
            YHO.Lyhenne AS O_Lyhenne,
            YHO.Nimi AS O_Nimi,
            YHO.Nro AS O_Nro
        FROM
            FactTilastolaskentaLisatieto LTX

            INNER JOIN DimYmpHalOrganisaatio YHO ON
                YHO.YmpHalOrganisaatio_ID = LTX.YmpHalOrganisaatio_ID
        WHERE
            LTX.Tilasto_ID = T.Tilasto_ID AND
            LTX.Jakso_ID = J.Jakso_ID
        ) LT
");

            sb.Append(@"
LEFT OUTER JOIN DimMittayksikko MY ON
    T.MittayksikkoEsitys_Mittayksikko_ID = MY.Mittayksikko_ID
");

            sb.Append(@"
LEFT OUTER JOIN DimAjallinenVaihe AJV ON
    T.AjallinenVaihe_ID = AJV.AjallinenVaihe_ID
");

            sb.Append(@"
LEFT OUTER JOIN Tietosuojaraja TS ON
    TS.Tilasto_ID = T.Tilasto_ID
");

            sb.Append(@"
LEFT OUTER JOIN Tilasto_KayttoOikeus TKO ON
    TKO.Tilasto_ID = T.Tilasto_ID
");

            sb.Append(@"
LEFT OUTER JOIN KayttoOikeus OIK ON
    OIK.KayttoOikeus_ID = TKO.KayttoOikeus_ID
");

            sb.Append(@"
OUTER APPLY (
    SELECT
        TOP 1 ESTO.RajapintaAlueTaso
    FROM
        TilastoKoosteAluetasoEstetty ESTO
    WHERE
        ESTO.Tilasto_ID = T.Tilasto_ID AND
        ESTO.Jakso_ID = J.Jakso_ID
    ) ESTO
");

            if (!this.IncludeHelperStatistics) {
                this.whereList.Add("T.TilastoLaskentatyyppi_ID <> 2");
            }

            if (this.whereList.Count > 0) {
                sb.Append(" WHERE ");
                sb.Append(string.Join(" AND ", whereList));
            }

            sb.Append("\nORDER BY T.Tilasto_ID, J.Jakso_ID, J.AlueTaso_ID ");

            return sb.ToString();
        }
    }
}