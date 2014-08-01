using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace LiiteriStatisticsCore.Queries
{
    public class AreaQuery : SqlQuery, ISqlQuery
    {
        private List<string> whereList;

        private static LiiteriStatisticsCore.Util.AreaTypeMappings
            AreaTypeMappings = new LiiteriStatisticsCore.Util.AreaTypeMappings();

        public AreaQuery() : base()
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
                this.whereList.Add("A.Alue_ID = @IdIs");
                this.AddParameter("@IdIs", value);
            }
        }

        public int AreaTypeIdIs { get; set; }
        /*
        public int? AreaTypeIdIs
        {
            get
            {
                return (int) this.GetParameter("@AreaTypeIdIs");
            }
            set
            {
                if (value == null) return;
                this.whereList.Add("A.AlueTaso_ID = @AreaTypeIdIs");
                this.AddParameter("@AreaTypeIdIs", value);
            }
        }
        */

        public override string GetQueryString()
        {
            string tableName = AreaTypeMappings.GetAreaTable(
                (int) this.AreaTypeIdIs);

            if (tableName == null || tableName.Length == 0) {
                throw new Exception("No table known for this datatype!");
            }

            string queryString = @"
SELECT
    A.Alue_ID AS AreaId,
    A.Nimi AS AreaName
FROM
    {0} A
{1}
";
            string whereString = "";
            if (this.whereList.Count > 0) {
                whereString = string.Join(" AND ", whereList);
            }

            queryString = string.Format(queryString,
                tableName,
                whereString);

            foreach (var param in this.Parameters) {
                Debug.WriteLine("DECLARE {0} INT = {1}", param.Key, param.Value);
            }
            Debug.WriteLine(queryString);

            return queryString;
        }
    }
}