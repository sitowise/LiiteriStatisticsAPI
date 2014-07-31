using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Common;

namespace LiiteriStatisticsCore.Factories
{
    public interface IFactory
    {
        Models.ILiiteriMarker Create(DbDataReader rdr);
    }
}