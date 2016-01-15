using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiiteriStatisticsCore.Models
{
    public class IndicatorBrief : ILiiteriEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int? OrderNumber { get; set; }

        // themes now completely on application side
        //public int? ThemeId { get; set; }
    }
}