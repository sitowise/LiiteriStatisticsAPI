using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiiteriStatisticsCore.Models
{
    public class AccessRight : ILiiteriMarker
    {
        public int Id { get; set; }
        public string Description { get; set; }
    }
}