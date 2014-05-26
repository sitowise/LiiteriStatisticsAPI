using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Common;
using System.Web;

namespace LiiteriDataAPI
{
    public class RegionFactory : BaseFactory
    {
        public IEnumerable<Models.Region> GetRegions()
        {
            string sqlString = "SELECT * FROM DimKunta";

            var results = new List<Models.Region>();

            Models.Region result;
            using (DbConnection db = this.GetDbConnection()) {
                using (DbCommand cmd = db.CreateCommand()) {
                    cmd.CommandText = sqlString;

                    using (DbDataReader rdr = cmd.ExecuteReader()) {
                        while (rdr.Read()) {
                            result = this.GetRegion(rdr);
                            results.Add(result);
                        }
                    }
                }
            }
            return results;
        }

        public Models.Region GetRegion(DbDataReader rdr)
        {
            Models.Region result = new Models.Region();
            result.Id = (int) rdr["Alue_ID"];
            result.Code = int.Parse((string) rdr["Nro"]).ToString();
            //result.Code = (string) rdr["Nro"].ToString();
            result.Category = "Kunta";
            result.Title = (string) rdr["Nimi"].ToString();
            return result;
        }
    }
}