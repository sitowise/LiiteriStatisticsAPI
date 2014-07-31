using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using System.Data.SqlClient;
using System.Configuration;
using System.Diagnostics;

namespace LiiteriStatisticsCore.Repositories
{
    public class IndicatorDetailsRepository :
        SqlReadRepository<Models.IndicatorDetails>
    {
        public IndicatorDetailsRepository(DbConnection dbConnection) :
            base(dbConnection)
        {
        }

        public override IEnumerable<Models.IndicatorDetails>
            FindAll(Queries.ISqlQuery query)
        {
            /* This is a bit different from standard way of handling results;
             * here we aggregate different sets of data from a single query
             * into nested object tree */

            var entityList = new List<Models.IndicatorDetails>();
            int prevDetailsId = 0;
            int prevPeriodId = 0;
            var detailsFactory = new Factories.IndicatorDetailsFactory();
            var periodFactory = new Factories.TimePeriodFactory();
            var areaTypeFactory = new Factories.AreaTypeFactory();

            Models.IndicatorDetails details = null;
            Models.TimePeriod timePeriod = null;
            Models.AreaType areaType = null;

            List<Models.TimePeriod> timePeriods = null;
            List<Models.AreaType> areaTypes = null;

            using (DbDataReader rdr = this.GetDbDataReader(query)) {
                while (rdr.Read()) {
                    /* each IndicatorDetails instance may have a number
                     * of TimePeriods */
                    if (prevDetailsId != (prevDetailsId = (int) rdr["Id"])) {
                        details = (Models.IndicatorDetails)
                            detailsFactory.Create(rdr);

                        timePeriods = new List<Models.TimePeriod>();
                        details.TimePeriods = timePeriods;

                        entityList.Add(details);
                    }

                    /* each TimePeriod instance may have a number
                     * of AreaTypes */
                    if (prevPeriodId != (prevPeriodId = (int) rdr["PeriodId"])) {
                        timePeriod = (Models.TimePeriod)
                            periodFactory.Create(rdr);

                        areaTypes = new List<Models.AreaType>();
                        timePeriod.AreaTypes = areaTypes;

                        timePeriods.Add(timePeriod);
                    }

                    /* and here are the AreaTypes, which contain
                     * nothing special */
                    areaType = (Models.AreaType) areaTypeFactory.Create(rdr);
                    areaTypes.Add(areaType);
                }
            }
            return entityList;
        }

        public override Models.IndicatorDetails Single(Queries.ISqlQuery query)
        {
            return this.FindAll(query).Single();
        }

        public override Models.IndicatorDetails First(Queries.ISqlQuery query)
        {
            return this.FindAll(query).First();
        }
    }
}