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
                return (string) this.GetParameter("@NameIs");
            }
            set
            {
                if (value == null) return;
                this.whereList.Add("T.name = @NameIs");
                this.AddParameter("@NameIs", value);
            }
        }

        public string NameLike
        {
            get
            {
                return (string) this.GetParameter("@NameLike");
            }
            set
            {
                if (value == null) return;
                this.whereList.Add("T.name LIKE @NameLike");
                this.AddParameter("@NameLike", value);
            }
        }

        public int IdIs
        {
            get
            {
                return (int) this.GetParameter("@IdIs");
            }
            set
            {
                this.whereList.Add("T.id = @IdIs");
                this.AddParameter("@IdIs", value);
            }
        }

        public int ParentIdIs
        {
            get
            {
                return (int) this.GetParameter("@ParentIdIs");
            }
            set
            {
                this.whereList.Add("T.parent_id = @ParentIdIs");
                this.AddParameter("@ParentIdIs", value);
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

            Debug.WriteLine(sb.ToString());
            return sb.ToString();
        }
    }
}