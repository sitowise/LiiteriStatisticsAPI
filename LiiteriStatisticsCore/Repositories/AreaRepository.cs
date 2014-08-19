using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using System.Diagnostics;

namespace LiiteriStatisticsCore.Repositories
{
    public class AreaRepository : SqlReadRepository<Models.Area>
    {
        private static LiiteriStatisticsCore.Util.AreaTypeMappings
            AreaTypeMappings = new LiiteriStatisticsCore.Util.AreaTypeMappings();

        private Dictionary<string, List<Models.Area>> areaLists;

        public AreaRepository(DbConnection dbConnection) :
            base(dbConnection)
        {

            /* this is a bit ugly, but the full lists are also good caches */
            this.areaLists = new Dictionary<string, List<Models.Area>>();
        }

        private void AddParents(Models.Area area, DbDataReader rdr)
        {
            List<Models.Area> parentAreas = new List<Models.Area>();
            foreach (Models.AreaType areaType in
                    AreaTypeMappings.GetAreaTypes()) {
                string areaTypeName = areaType.Id;
                string columnName = "parent_" + areaTypeName;

                if (!Enumerable.Range(0, rdr.FieldCount).Any(
                        i => rdr.GetName(i) == columnName)) {
                    continue;
                }

                if (Convert.IsDBNull(rdr[columnName])) continue;
                int areaId = (int) rdr[columnName];
                if (!this.areaLists.Keys.Contains(areaTypeName)) {
                    Queries.AreaQuery query = new Queries.AreaQuery();
                    query.AreaTypeIdIs = areaTypeName;
                    this.areaLists[areaTypeName] =
                        (List<Models.Area>) this.FindAll(query, true);
                }
                IEnumerable<Models.Area> parentArea = (
                    from a in this.areaLists[areaTypeName]
                    where a.Id == areaId
                    select a);
                if (parentArea.Count() == 0) {
                    continue;
                }
                parentAreas.Add(parentArea.Single());
            }
            area.ParentAreas = parentAreas;
        }

        public IEnumerable<Models.Area>
            FindAll(Queries.ISqlQuery query, bool noParents = false)
        {
            List<Models.Area> entityList = new List<Models.Area>();
            Factories.AreaFactory areaFactory = new Factories.AreaFactory();

            using (DbDataReader rdr = this.GetDbDataReader(query)) {
                while (rdr.Read()) {
                    Models.Area area = (Models.Area) areaFactory.Create(rdr);
                    if (!noParents) {
                        this.AddParents(area, rdr);
                    }
                    entityList.Add(area);
                }
            }
            return entityList;
        }

        public override IEnumerable<Models.Area> FindAll(Queries.ISqlQuery query)
        {
            return this.FindAll(query, false);
        }

        public override Models.Area Single(Queries.ISqlQuery query)
        {
            return this.FindAll(query).Single();
        }

        public override Models.Area First(Queries.ISqlQuery query)
        {
            return this.FindAll(query).First();
        }
    }
}