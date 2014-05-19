using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Common;

namespace LiiteriDataAPI.Models
{
    public class StatisticIndexDetails : StatisticIndexBrief
    {
        public string Description;
        public string AdditionalInformation;
    }

    public class StatisticIndexDetailsFactory
    {
        public StatisticIndexDetails GetStatisticIndexDetails(
            StatisticIndexBrief brief,
            DbDataReader rdr2) /* reader for metadata */
        {
            StatisticIndexDetails result = new StatisticIndexDetails();
            result.Id = brief.Id;
            result.Name = brief.Name;

            result.Group = brief.Group;
            result.Unit = brief.Unit;
            result.StatisticId = brief.StatisticId;
            result.ProcessingStage = brief.ProcessingStage;
            result.TimeSpan = brief.TimeSpan;
            result.DecimalCount = brief.DecimalCount;

            result.Themes = brief.Themes;

            result.Description = (string) rdr2["Kuvaus"].ToString();
            result.AdditionalInformation = (string) rdr2["Lisatieto"].ToString();
            return result;
        }
    }
}