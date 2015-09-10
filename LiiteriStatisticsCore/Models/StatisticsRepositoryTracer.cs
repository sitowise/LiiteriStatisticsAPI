using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using LiiteriStatisticsCore.Repositories;
using System.ComponentModel;

namespace LiiteriStatisticsCore.Models
{

    /* we keep this as an object, so we can assign a reference to it early
     * and modify it later when the actual SQL query is executed */
    public class SQLQueryTime {
        public double? TotalMilliseconds;
    }

    [DataContract]
    public class StatisticsRepositoryTracer
    {
        public IReadRepository<StatisticsResult> Repository;

        [DataMember]
        [ReadOnly(true)]
        public string QueryString
        {
            get
            {
                if (this.Repository != null &&
                        this.Repository.GetType().IsSubclassOf(typeof(
                            SqlReadRepository<StatisticsResult>))) {
                    return ((SqlReadRepository<StatisticsResult>)
                        this.Repository).queries.Single().GetQueryString();
                } else {
                    return null;
                }
            }
            protected set
            {
            }
        }

        [DataMember]
        [ReadOnly(true)]
        public string RepositoryType
        {
            get
            {
                return this.Repository.GetType().Name.ToString();
            }
            protected set
            {
            }
        }

        public SQLQueryTime QueryTime { get; set; }

        [DataMember(Name = "QueryTime")]
        [ReadOnly(true)]
        public double? QueryTimeSerialized
        {
            get
            {
                if (this.QueryTime == null) {
                    return null;
                }
                return this.QueryTime.TotalMilliseconds;
            }

            protected set
            {
            }
        }

        [DataMember]
        public Requests.StatisticsRequest Request { get; set; }

        public StatisticsRepositoryTracer Parent = null;

        [DataMember]
        public List<StatisticsRepositoryTracer> Children =
            new List<StatisticsRepositoryTracer>();

        public StatisticsRepositoryTracer CreateChild()
        {
            var tracer = new StatisticsRepositoryTracer();
            tracer.Parent = this;
            this.Children.Add(tracer);
            return tracer;
        }
    }
}