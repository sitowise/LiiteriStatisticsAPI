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

                    var queries = ((SqlReadRepository<StatisticsResult>)
                        this.Repository).queries;
                    return string.Join("\n\n---\n\n",
                        queries.Select(x => x.GetQueryString()));
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

        [DataMember]
        public SQLQueryDetails QueryDetails { get; set; }

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