using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiiteriStatisticsCore.Models
{
    public class PrivacyLimit
    {
        /* statistics id used for comparing privacy limits to */
        public int RefId { get; set;}

        /* value condition */
        public int? GreaterThan { get; set; }
    }
}