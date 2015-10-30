using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LiiteriStatisticsCore.Util;

namespace LiiteriStatisticsCore.Queries
{
    public class SpecialStatisticsQuery : StatisticsQuery
    {
        public SpecialStatisticsQuery(int id) : base(id)
        {
        }

        public void GenerateQueryString()
        {
            string queryString;

            int primaryDbAreaType =
                AreaTypeMappings.GetPrimaryDatabaseAreaType(
                    this.GroupByAreaTypeIdIs);
            if (!this.AvailableAreaTypes.Contains(primaryDbAreaType)) {
                throw new Exception(
                    "Supplied grouping areaType not suitable for this statistics data!");
            }

            Debug.WriteLine(string.Format(
                "We have these areaTypes available: [{0}]",
                string.Join(",", this.AvailableAreaTypes)));

            if (this.CalculationTypeIdIs != 4) {
                string errMsg = string.Format(
                    "Unsupported CalculationType: {0}",
                    this.CalculationTypeIdIs);
                logger.Error(errMsg);
                throw new Exception(errMsg);
            }

            this.fields.Add("T.Jakso_ID AS Year");
            this.fields.Add("COALESCE(T.Arvo, 0) AS Value");

            this.SetFilters();
            this.SetGroups(); // nothing should be grouped in this query
            this.SetDatabaseAreaTypeId();

            queryString = QueryTemplates.Get("Normal");
            queryString = string.Format(queryString,
                this.GetFieldsString(),
                this.GetFromString(),
                this.GetWhereString(),
                "/* group string omitted */",
                this.GetOrderString(),
                this.GetFilterJoinsString(),
                this.GetAreaYearField());

            /* preQuery stuff (which are geometry declarations at the moment)
             * should be common for all query types, let's prepend it here */
            queryString = this.GetPreQueryString() + "\n" + queryString;

            /* postQuery stuff (drop temporary tables) */
            queryString = queryString + "\n" + this.GetPostQueryString();

            this.QueryString = queryString;
        }

        public override string GetQueryString()
        {
            if (this.QueryString == null) {
                this.GenerateQueryString();
            }
            return this.QueryString;
        }
    }
}