using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiiteriStatisticsCore.Models
{
    public class FunctionalAreaAvailability : ILiiteriEntity
    {
        public int Id { get; set; }

        public string AlternativeId { get; set; }

        public string Name { get; set; }

        public string AreaType { get; set; }

        public int? Year { get; set; }

        public IEnumerable<string> AvailableFunctionalAreas;

        public int OrderNumber { get; set; }
    }
}