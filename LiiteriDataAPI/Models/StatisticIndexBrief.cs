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
        public int? Group { get; set; }
        public string Unit { get; set; }
        public int? StatisticId { get; set; }
        public string ProcessingStage { get; set; }
        public string TimeSpan { get; set; }
        public int? DecimalCount { get; set; }
        //public Models.StatisticIndexTheme[] StatisticIndexThemes { get; set; }
        public string[] Themes { get; set; }
    }

    public class StatisticIndexBriefFactory
    {
        public StatisticIndexBrief GetStatisticIndexResult(DbDataReader rdr)
        {
            StatisticIndexBrief result = new StatisticIndexBrief();
            result.Id = (int) rdr["id"];
            result.Name = (string) rdr["statisticName"].ToString();

            result.Group = rdr["statisticGroup"] as int? ?? default(int);
            result.Unit = (string) rdr["unit"].ToString();
            result.StatisticId = rdr["statisticId"] as int? ?? default(int);
            result.ProcessingStage = (string) rdr["processingStage"].ToString();
            result.TimeSpan = (string) rdr["timeSpan"].ToString();
            result.DecimalCount = rdr["decimalCount"] as int? ?? default(int);

            //var themes = new List<StatisticIndexTheme>();
            var themes = new List<string>();
            for (int i = 0; i < rdr.FieldCount; i ++) {
                string key = rdr.GetName(i);
                if (!key.StartsWith("theme")) continue;
                if (rdr.IsDBNull(i)) continue;
                //var theme = new StatisticIndexTheme();
                //theme.Name = rdr.GetString(i);
                string theme = rdr.GetString(i);
                themes.Add(theme);
            }
            result.Themes = themes.ToArray();

            return result;
        }
    }
}