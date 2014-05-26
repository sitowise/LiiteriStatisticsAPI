using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Common;

namespace LiiteriDataAPI.Models
{
    public class StatisticIndexBrief
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string[] Themes { get; set; }

        //public Models.StatisticIndexTheme[] StatisticIndexThemes { get; set; }
    }

    public class StatisticIndexBriefFactory
    {
        public StatisticIndexBrief GetStatisticIndexResult(DbDataReader rdr)
        {
            StatisticIndexBrief result = new StatisticIndexBrief();
            result.Id = (int) rdr["Tilasto_ID"];
            result.Name = (string) rdr["statisticName"].ToString();

            //var themes = new List<StatisticIndexTheme>();
            var themes = new List<string>();
            for (int i = 0; i < rdr.FieldCount; i ++) {
                string key = rdr.GetName(i);
                if (!key.StartsWith("theme")) continue;
                string theme = "";
                if (!rdr.IsDBNull(i)) {
                    theme = rdr.GetString(i);
                }
                themes.Add(theme);
            }
            result.Themes = themes.ToArray();

            return result;
        }
    }
}