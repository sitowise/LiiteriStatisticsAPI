using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Common;

namespace LiiteriDataAPI.Models
{
    public class StatisticIndexDetails : StatisticIndexBrief
    {
        public int? Group { get; set; }
        public string Unit { get; set; }
        //public int? StatisticId { get; set; }
        public string ProcessingStage { get; set; }
        public string TimeSpan { get; set; }
        public int? DecimalCount { get; set; }

        public string Description;
        public string AdditionalInformation;

        public string[] Years;
        public RegionLayer[] RegionLayers;
    }

    public class StatisticIndexDetailsFactory
    {
        public StatisticIndexDetails GetStatisticIndexDetails(
            StatisticIndexBrief brief,
            DbDataReader rdr,
            string[] years,
            RegionLayer[] regionLayers) /* reader for metadata */
        {
            StatisticIndexDetails result = new StatisticIndexDetails();
            result.Id = brief.Id;
            result.Name = brief.Name;

            result.Group = rdr["statisticGroup"] as int? ?? default(int);
            result.Unit = (string) rdr["unit"].ToString();
            //result.StatisticId = rdr["statisticId"] as int? ?? default(int);
            result.ProcessingStage = (string) rdr["processingStage"].ToString();
            result.TimeSpan = (string) rdr["timeSpan"].ToString();
            result.DecimalCount = rdr["EsitysDesimaaliTarkkuus"] as int? ?? default(int);

            result.Themes = brief.Themes;

            result.Description = (string) rdr["Kuvaus"].ToString();
            result.AdditionalInformation = (string) rdr["Lisatieto"].ToString();

            result.Years = years;

            result.RegionLayers = regionLayers;

            return result;
        }
    }
}