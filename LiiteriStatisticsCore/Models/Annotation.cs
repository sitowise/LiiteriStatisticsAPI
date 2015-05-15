using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace LiiteriStatisticsCore.Models
{
    public class Annotation : ILiiteriMarker
    {
        public string Description { get; set; }

        // Etelä-Pohjanmaan ELY
        public string OrganizationName { get; set; }

        // EPO
        public string OrganizationShort { get; set; }

        // string, since it can be '09'
        public string OrganizationNumber { get; set; }
    }
}