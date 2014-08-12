using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;

namespace LiiteriStatisticsCore.Queries
{
    public class ThemeQuery : SqlQuery, ISqlQuery
    {
        private List<string> whereList;

        public ThemeQuery() : base()
        {
            this.whereList = new List<string>();
        }

        public string NameIs
        {
            get
            {
                return (string) this.Parameters["NameIs"].Value;
            }
            set
            {
                if (value == null) return;
                this.whereList.Add("T.name = @NameIs");
                this.Parameters.Add("NameIs", value);
            }
        }

        public string NameLike
        {
            get
            {
                return (string) this.Parameters["NameLike"].Value;
            }
            set
            {
                if (value == null) return;
                this.whereList.Add("T.name LIKE @NameLike");
                this.Parameters.Add("NameLike", value);
            }
        }

        public int IdIs
        {
            get
            {
                return (int) this.Parameters["IdIs"].Value;
            }
            set
            {
                this.whereList.Add("T.id = @IdIs");
                this.Parameters.Add("IdIs", value);
            }
        }

        public int ParentIdIs
        {
            get
            {
                return (int) this.Parameters["ParentIdIs"].Value;
            }
            set
            {
                this.whereList.Add("T.parent_id = @ParentIdIs");
                this.Parameters.Add("ParentIdIs", value);
            }
        }

        public override string GetQueryString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT ");

            sb.Append("id AS Id, ");
            sb.Append("name AS Name, ");
            sb.Append("parent_id AS ParentId ");

            sb.Append(string.Format(
                "FROM [{0}]..[Themes] T",
                ConfigurationManager.AppSettings["DbDataIndex"]));

            if (this.whereList.Count > 0) {
                sb.Append(" WHERE ");
                sb.Append(string.Join(" AND ", whereList));
            }

            return sb.ToString();
        }
    }
}