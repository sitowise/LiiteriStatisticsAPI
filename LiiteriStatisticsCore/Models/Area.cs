using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiiteriStatisticsCore.Models
{
    public class Area : ILiiteriEntity
    {
        public int Id { get; set; }
        public string AlternativeId { get; set; }
        public string Name { get; set; }
        public string AreaType { get; set; }

        // some areas only exist at some point in time
        public int? Year { get; set; }

        public IEnumerable<Area> ParentAreas;
    }
}