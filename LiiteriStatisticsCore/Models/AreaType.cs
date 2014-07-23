using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace LiiteriStatisticsCore.Models
{
    public class AreaType : ILiiteriEntity
    {
        public int Id { get; set; }
        public string DataSource { get; set; }
    }
}