using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OfficeOpenXml;
using System.Diagnostics;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Configuration;

namespace ImportTool
{
    class Program
    {
        private static DbConnection GetDstDbConnection(string connStr)
        {
            DbConnection conn = new SqlConnection
            {
                ConnectionString = connStr,
            };
            conn.Open();

            return conn;
        }

        public static void TruncateTable(
            DbConnection db,
            string tableName,
            DbTransaction trans = null)
        {
            using (DbCommand dstCmd = db.CreateCommand()) {
                if (trans != null) {
                    dstCmd.Transaction = trans;
                }
                dstCmd.CommandText = String.Format(
                    "TRUNCATE TABLE {0}",
                    tableName);
                Console.WriteLine(String.Format(
                    "Truncating table {0}", tableName));
                dstCmd.ExecuteNonQuery();
            }
        }

        static void Main(string[] args)
        {
            string fileName =
                @"C:\Projects\Liiteri\Liiteri_Tilastot_Rakenne.xlsx";

            string insertSqlString = @"
INSERT INTO statisticIndex (
    theme1,
    theme2,
    theme3,
    theme4,
    theme5,
    statisticGroup,
    statisticName,
    unit,
    statisticId, 
    processingStage,
    timeSpan,
    decimalCount
) VALUES (
    @theme1,
    @theme2,
    @theme3,
    @theme4,
    @theme5,
    @statisticGroup,
    @statisticName,
    @unit,
    @statisticId, 
    @processingStage,
    @timeSpan,
    @decimalCount)
";

            string connStr = ConfigurationManager.ConnectionStrings["statisticDB"].ToString();

            using (FileStream file = new FileStream(
                    fileName,
                    FileMode.Open,
                    System.IO.FileAccess.Read))
            using (DbConnection db = GetDstDbConnection(connStr)) {

                ExcelPackage p = new ExcelPackage(file);
                ExcelWorksheet ws = p.Workbook.Worksheets["Tilastot"];
                var start = ws.Dimension.Start;
                var end = ws.Dimension.End;

                /* read header first */

                var headers = new List<string>();
                for (int i = start.Column; i <= end.Column; i++) {
                    string val = ws.Cells[1, i].Text.ToString();
                    headers.Add(val);
                    // Debug.WriteLine(val);
                }

                /* the contents */
                var cur_themes = new Dictionary<string, string>() {
                    {"Teemataso 1", ""},
                    {"Teemataso 2", ""},
                    {"Teemataso 3", ""},
                    {"Teemataso 4", ""},
                    {"Teemataso 5", ""},
                };

                using (DbTransaction dbTrans = db.BeginTransaction()) {

                    TruncateTable(db, "statisticIndex", dbTrans);

                    for (int i = start.Row + 1; i <= end.Row; i++) {
                        var data = new Dictionary<string, string>();
                        for (int c = start.Column; c <= end.Column; c++) {
                            string val = ws.Cells[i, c].Text.ToString();
                            data[headers[c - 1]] = val;
                        }

                        /* some header rows, ignore */
                        if (data["Tilasto"].Trim().Length == 0) {
                            continue;
                        }

                        /* figure out all the temes for this current row */
                        foreach (var key in data.Keys) {
                            if (!key.StartsWith("Teemataso ")) {
                                continue;
                            }
                            if (data[key].Trim().Length == 0) {
                                continue;
                            }
                            cur_themes[key] = data[key];

                            /* zero pad the rest since we have a new lower
                             * level subtheme */
                            bool empty = false;
                            int count = 0;
                            foreach (string tk in cur_themes.Keys.ToArray()) {
                                if (count++ > 0 && tk == key) {
                                    empty = true;
                                } else if (empty) {
                                    cur_themes[tk] = "";
                                }
                            }
                        }

                        using (DbCommand cmd = db.CreateCommand()) {
                            cmd.CommandText = insertSqlString;
                            cmd.Transaction = dbTrans;

                            DbParameter param;

                            // Teemataso 1
                            param = cmd.CreateParameter();
                            param.DbType = DbType.String;
                            param.ParameterName = "@theme1";
                            if (cur_themes["Teemataso 1"].Trim().Length > 0) {
                                param.Value = cur_themes["Teemataso 1"];
                            } else {
                                param.Value = DBNull.Value;
                            }
                            cmd.Parameters.Add(param);

                            // Teemataso 2
                            param = cmd.CreateParameter();
                            //param.DbType = DbType.String;
                            param.ParameterName = "@theme2";
                            if (cur_themes["Teemataso 2"].Trim().Length > 0) {
                                param.Value = cur_themes["Teemataso 2"];
                            } else {
                                param.Value = DBNull.Value;
                            }
                            cmd.Parameters.Add(param);

                            // Teemataso 3
                            param = cmd.CreateParameter();
                            param.DbType = DbType.String;
                            param.ParameterName = "@theme3";
                            if (cur_themes["Teemataso 3"].Trim().Length > 0) {
                                param.Value = cur_themes["Teemataso 3"];
                            } else {
                                param.Value = DBNull.Value;
                            }
                            cmd.Parameters.Add(param);

                            // Teemataso 4
                            param = cmd.CreateParameter();
                            param.DbType = DbType.String;
                            param.ParameterName = "@theme4";
                            if (cur_themes["Teemataso 4"].Trim().Length > 0) {
                                param.Value = cur_themes["Teemataso 4"];
                            } else {
                                param.Value = DBNull.Value;
                            }
                            cmd.Parameters.Add(param);

                            // Teemataso 5
                            param = cmd.CreateParameter();
                            param.DbType = DbType.String;
                            param.ParameterName = "@theme5";
                            if (cur_themes["Teemataso 5"].Trim().Length > 0) {
                                param.Value = cur_themes["Teemataso 5"];
                            } else {
                                param.Value = DBNull.Value;
                            }
                            cmd.Parameters.Add(param);

                            // Tilastoryhma
                            param = cmd.CreateParameter();
                            param.DbType = DbType.Int32;
                            param.ParameterName = "@statisticGroup";
                            if (data["Tilastoryhma_Id"].Trim().Length > 0) {
                                param.Value = int.Parse(data["Tilastoryhma_Id"]);
                            } else {
                                param.Value = DBNull.Value;
                            }
                            cmd.Parameters.Add(param);

                            // Tilasto
                            param = cmd.CreateParameter();
                            param.DbType = DbType.String;
                            param.ParameterName = "@statisticName";
                            param.Value = data["Tilasto"];
                            cmd.Parameters.Add(param);

                            // Yksikkö
                            param = cmd.CreateParameter();
                            param.DbType = DbType.String;
                            param.ParameterName = "@unit";
                            param.Value = data["Yksikkö"];
                            cmd.Parameters.Add(param);

                            // Liiteri-Tilasto_ID
                            param = cmd.CreateParameter();
                            param.DbType = DbType.Int32;
                            param.ParameterName = "@statisticId";
                            if (data["Liiteri-Tilasto_ID"].Trim().Length > 0) {
                                param.Value = int.Parse(data["Liiteri-Tilasto_ID"]);
                            } else {
                                param.Value = DBNull.Value;
                            }
                            cmd.Parameters.Add(param);

                            // Käsittelyvaihe (Kaavoitus)
                            param = cmd.CreateParameter();
                            param.DbType = DbType.String;
                            param.ParameterName = "@processingStage";
                            param.Value = data["Käsittelyvaihe (Kaavoitus)"];
                            cmd.Parameters.Add(param);

                            // Ajallinen vaihe
                            param = cmd.CreateParameter();
                            param.DbType = DbType.String;
                            param.ParameterName = "@timeSpan";
                            param.Value = data["Ajallinen vaihe"];
                            cmd.Parameters.Add(param);

                            // Desimaalien lkm (näytettävät)
                            param = cmd.CreateParameter();
                            param.DbType = DbType.Int32;
                            param.ParameterName = "@decimalCount";
                            if (data["Desimaalien lkm (näytettävät)"].Trim().Length > 0) {
                                param.Value = int.Parse(data["Desimaalien lkm (näytettävät)"]);
                            } else {
                                param.Value = DBNull.Value;
                            }
                            cmd.Parameters.Add(param);

                            int aff = cmd.ExecuteNonQuery();
                        }
                    }

                    dbTrans.Commit();
                }
            }
        }
    }
}