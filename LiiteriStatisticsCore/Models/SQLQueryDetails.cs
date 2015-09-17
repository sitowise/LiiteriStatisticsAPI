using System.ComponentModel;
using System.Runtime.Serialization;

namespace LiiteriStatisticsCore.Models
{
    [DataContract]
    public class SQLQueryDetails
    {
        [DataMember]
        public double? QueryTimeMilliseconds;

        [DataMember]
        public int? RowCount { get; set; }
    }
}