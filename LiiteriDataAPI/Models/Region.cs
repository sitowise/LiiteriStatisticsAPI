using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/* Only used by V0 API, to be removed */

namespace LiiteriDataAPI.Models
{
    public class Region
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Category { get; set; }
        public string Title { get; set; }
    }
}