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
        private static LiiteriStatisticsCore.Util.AreaTypeMappings
            AreaTypeMappings = new LiiteriStatisticsCore.Util.AreaTypeMappings();

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
            var dataAreaTypeFactory = new Factories.DataAreaTypeFactory();
            var areaTypeFactory = new Factories.AreaTypeFactory();
            var annotationFactory = new Factories.AnnotationFactory();

            Models.IndicatorDetails details = null;
            Models.TimePeriod timePeriod = null;

            List<Models.TimePeriod> timePeriods = null;

            /* DataAreaType (or "database areatype") used internally
             * AreaType is provided via API for the client to use */
            Models.DataAreaType dataAreaType = null;
            List<Models.DataAreaType> dataAreaTypes = null;
            List<Models.AreaType> areaTypes = null;

            List<Models.Annotation> annotations = null;

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
                     * of AreaTypes, as well as a a number of Annotations */
                    if (prevPeriodId != (prevPeriodId = (int) rdr["PeriodId"])) {
                        timePeriod = (Models.TimePeriod)
                            periodFactory.Create(rdr);

                        dataAreaTypes = new List<Models.DataAreaType>();
                        areaTypes = new List<Models.AreaType>();
                        timePeriod.DataAreaTypes = dataAreaTypes;
                        timePeriod.AreaTypes = areaTypes;

                        annotations = new List<Models.Annotation>();
                        timePeriod.Annotations = annotations;

                        timePeriods.Add(timePeriod);
                    }

                    /* AreaTypes are exposed by the API */

                    int databaseAreaType = (int) rdr["AreaTypeId"];

                    IEnumerable<Models.AreaType> applicableAreaTypes;
                    /* special statistics should only return
                     * one available areaType */
                    if (details.CalculationType == 4) {
                        applicableAreaTypes =
                            new List<Models.AreaType>() {
                                AreaTypeMappings.GetPrimaryAreaType(databaseAreaType)
                            };
                    } else {
                        applicableAreaTypes =
                            AreaTypeMappings.GetAreaTypes(databaseAreaType);
                    }

                    foreach (Models.AreaType a in applicableAreaTypes) {
                        areaTypes.Add((Models.AreaType)
                            areaTypeFactory.Create(a, rdr));
                    }

                    /* DataAreaTypes (or "database areatypes") are not
                     * exposed by the API, but are used internally */
                    dataAreaType = (Models.DataAreaType)
                        dataAreaTypeFactory.Create(rdr);
                    dataAreaTypes.Add(dataAreaType);

                    /* Each TimePeriod can also have a number
                     * of Annotations */
                    if (!Convert.IsDBNull(rdr["Annotation"])) {
                        annotations.Add((Models.Annotation)
                            annotationFactory.Create(rdr));
                    }
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