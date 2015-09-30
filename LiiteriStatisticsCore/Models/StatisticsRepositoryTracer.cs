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

        private IReadRepository<StatisticsResult> _Repository;
        public IReadRepository<StatisticsResult> Repository
        {
            get
            {
                return this._Repository;
            }
            set
            {
                this._Repository = value;

                if (value == null) {
                    this.RepositoryType = null;
                    this.QueryString = null;
                    return;
                }

                Type repositoryType = value.GetType();
                this.RepositoryType = repositoryType.Name.ToString();

                if (repositoryType.IsSubclassOf(
                        typeof(SqlReadRepository<StatisticsResult>))) {
                    var queries = ((SqlReadRepository<StatisticsResult>)
                        this.Repository).queries;
                    this.QueryString = new Util.DebugOutput(queries).ToString();
                }
            }
        }

        [DataMember]
        public string QueryString { get; set; }

        [DataMember]
        public string RepositoryType { get; set; }

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