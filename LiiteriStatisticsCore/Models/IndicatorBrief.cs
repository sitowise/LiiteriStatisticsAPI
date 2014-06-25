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
        public int? ThemeId { get; set; }
    }
}