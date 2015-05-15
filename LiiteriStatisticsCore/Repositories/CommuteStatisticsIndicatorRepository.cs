using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.Common;

namespace LiiteriStatisticsCore.Repositories
{
    public class CommuteStatisticsIndicatorRepository
    {
        private DbConnection dbConnection;

        public CommuteStatisticsIndicatorRepository(DbConnection dbConnection)
        {
            this.dbConnection = dbConnection;
        }

        private int[] GetYears(string tableName)
        {
            Queries.ISqlQuery query = new Queries.CommuteStatisticsYearQuery(tableName);
            return (new CommuteStatisticsYearRepository(this.dbConnection))
                .FindAll(query)
                .ToArray();
        }

        public IEnumerable<Models.CommuteStatisticsIndicator> GetAll()
        {
            return new List<Models.CommuteStatisticsIndicator>() {
                new Models.CommuteStatisticsIndicator() {
                    Id = -1,
                    Name = "FactTyomatkaTOL2002",
                    Description = "Toimialaluokitus 2002: vuodet 1990-2005",
                    TableName = "FactTyomatkaTOL2002",
                    Years = this.GetYears("FactTyomatkaTOL2002"),
                    CommuteStatisticsTypes = (new List<Models.CommuteStatisticsType>() {
                        new Models.CommuteStatisticsType() {
                            Id = "yht",
                            Description = "Työmatkojen lukumäärä, Yhteensä"
                        },
                        new Models.CommuteStatisticsType() {
                            Id = "a_alkutuot",
                            Description = "Työmatkojen lukumäärä, Maatalous, riistatalous ja metsätalous"
                        },
                        new Models.CommuteStatisticsType() {
                            Id = "b_kala",
                            Description = "Työmatkojen lukumäärä, Kalatalous"
                        },
                        new Models.CommuteStatisticsType() {
                            Id = "c_kaivuu",
                            Description = "Työmatkojen lukumäärä, Kaivostoiminta ja louhinta"
                        },
                        new Models.CommuteStatisticsType() {
                            Id = "d_teoll",
                            Description = "Työmatkojen lukumäärä, Teollisuus"
                        },
                        new Models.CommuteStatisticsType() {
                            Id = "e_teknhu",
                            Description = "Työmatkojen lukumäärä, Sähkö-, kaasu ja vesihuolto"
                        },
                        new Models.CommuteStatisticsType() {
                            Id = "f_rakent",
                            Description = "Työmatkojen lukumäärä, Rakentaminen"
                        },
                        new Models.CommuteStatisticsType() {
                            Id = "g_kauppa",
                            Description = "Työmatkojen lukumäärä, Tukku- ja vähittäiskauppa"
                        },
                        new Models.CommuteStatisticsType() {
                            Id = "h_majrav",
                            Description = "Työmatkojen lukumäärä, Majoitus- ja ravitsemistoiminta"
                        },
                        new Models.CommuteStatisticsType() {
                            Id = "i_liiken",
                            Description = "Työmatkojen lukumäärä, Kuljetus, varastointi ja tietoliikenne"
                        },
                        new Models.CommuteStatisticsType() {
                            Id = "j_raha",
                            Description = "Työmatkojen lukumäärä, Rahoitustoiminta"
                        },
                        new Models.CommuteStatisticsType() {
                            Id = "k_kivutu",
                            Description = "Työmatkojen lukumäärä, Kiinteistö-, vuokraus- ja tutkimuspalvelut"
                        },
                        new Models.CommuteStatisticsType() {
                            Id = "l_julkhal",
                            Description = "Työmatkojen lukumäärä, Julkinen hallinto ja maanpuolustus"
                        },
                        new Models.CommuteStatisticsType() {
                            Id = "m_koul",
                            Description = "Työmatkojen lukumäärä, Koulutus"
                        },
                        new Models.CommuteStatisticsType() {
                            Id = "n_tervsos",
                            Description = "Työmatkojen lukumäärä, Terveydenhuolto- ja sosiaalipalvelut"
                        },
                        new Models.CommuteStatisticsType() {
                            Id = "o_muuyhtk",
                            Description = "Työmatkojen lukumäärä, Muut yhteiskunnalliset ja henkilökohtaiset palvelut"
                        },
                        new Models.CommuteStatisticsType() {
                            Id = "p_tyonant",
                            Description = "Työmatkojen lukumäärä, Työnantajakotitaloudet sekä kotitalouksien itse tuottamat tavarat ja palvelut omaan käyttöön"
                        },
                        new Models.CommuteStatisticsType() {
                            Id = "q_kvjarj",
                            Description = "Työmatkojen lukumäärä, Kansainväliset järjestöt ja ulkomaiset edustustot"
                        },
                        new Models.CommuteStatisticsType() {
                            Id = "x_tuntem",
                            Description = "Työmatkojen lukumäärä, Toimiala tuntematon"
                        },
                    }).ToArray()
                },
                new Models.CommuteStatisticsIndicator() {
                    Id = -2,
                    Name = "FactTyomatkaTOL2008",
                    Description = "Toimialaluokitus 2008: vuodet 2007-",
                    TableName = "FactTyomatkaTOL2008",
                    Years = this.GetYears("FactTyomatkaTOL2008"),
                    CommuteStatisticsTypes = (new List<Models.CommuteStatisticsType>() {
                        new Models.CommuteStatisticsType() {
                            Id = "yht",
                            Description = "Työmatkojen lukumäärä, Yhteensä"
                        },
                        new Models.CommuteStatisticsType() {
                            Id = "a_alkut",
                            Description = "Työmatkojen lukumäärä, Maatalous, metsätalous ja kalatalous"
                        },
                        new Models.CommuteStatisticsType() {
                            Id = "b_kaivos",
                            Description = "Työmatkojen lukumäärä, Kaivostoiminta ja louhinta"
                        },
                        new Models.CommuteStatisticsType() {
                            Id = "c_teoll",
                            Description = "Työmatkojen lukumäärä, Teollisuus"
                        },
                        new Models.CommuteStatisticsType() {
                            Id = "d_infra1",
                            Description = "Työmatkojen lukumäärä, Sähkö-, kaasu- ja lämpöhuolto, jäähdytysliiketoiminta"
                        },
                        new Models.CommuteStatisticsType() {
                            Id = "e_infra2",
                            Description = "Työmatkojen lukumäärä, Vesihuolto, viemäri- ja jätevesihuolto, jätehuolto ja muu ympäristön puhtaanapito"
                        },
                        new Models.CommuteStatisticsType() {
                            Id = "f_rakent",
                            Description = "Työmatkojen lukumäärä, Rakentaminen"
                        },
                        new Models.CommuteStatisticsType() {
                            Id = "g_kauppa",
                            Description = "Työmatkojen lukumäärä, Tukku- ja vähittäiskauppa; moottoriajoneuvojen ja moottoripyörien korjaus"
                        },
                        new Models.CommuteStatisticsType() {
                            Id = "h_kulj",
                            Description = "Työmatkojen lukumäärä, Kuljetus ja varastointi"
                        },
                        new Models.CommuteStatisticsType() {
                            Id = "i_majrav",
                            Description = "Työmatkojen lukumäärä, Majoitus- ja ravitsemistoiminta"
                        },
                        new Models.CommuteStatisticsType() {
                            Id = "j_info",
                            Description = "Työmatkojen lukumäärä, Informaatio ja viestintä"
                        },
                        new Models.CommuteStatisticsType() {
                            Id = "k_raha",
                            Description = "Työmatkojen lukumäärä, Rahoitus- ja vakuutustoiminta"
                        },
                        new Models.CommuteStatisticsType() {
                            Id = "l_kiint",
                            Description = "Työmatkojen lukumäärä, Kiinteistöalan toiminta"
                        },
                        new Models.CommuteStatisticsType() {
                            Id = "m_tekn",
                            Description = "Työmatkojen lukumäärä, Ammatillinen, tieteellinen ja tekninen toiminta"
                        },
                        new Models.CommuteStatisticsType() {
                            Id = "n_halpa",
                            Description = "Työmatkojen lukumäärä, Hallinto- ja tukipalvelutoiminta"
                        },
                        new Models.CommuteStatisticsType() {
                            Id = "o_julk",
                            Description = "Työmatkojen lukumäärä, Julkinen hallinto ja maanpuolustus; pakollinen sosiaalivakuutus"
                        },
                        new Models.CommuteStatisticsType() {
                            Id = "p_koul",
                            Description = "Työmatkojen lukumäärä, Koulutus"
                        },
                        new Models.CommuteStatisticsType() {
                            Id = "q_terv",
                            Description = "Työmatkojen lukumäärä, Terveys- ja sosiaalipalvelut"
                        },
                        new Models.CommuteStatisticsType() {
                            Id = "r_taide",
                            Description = "Työmatkojen lukumäärä, Taiteet, viihde ja virkistys"
                        },
                        new Models.CommuteStatisticsType() {
                            Id = "s_muupa",
                            Description = "Työmatkojen lukumäärä, Muu palvelutoiminta"
                        },
                        new Models.CommuteStatisticsType() {
                            Id = "t_koti",
                            Description = "Työmatkojen lukumäärä, Kotitalouksien toiminta työnantajina; kotitalouksien eriyttymätön toiminta tavaroiden ja palvelujen tuottamiseksi omaan käyttöön"
                        },
                        new Models.CommuteStatisticsType() {
                            Id = "u_kvjarj",
                            Description = "Työmatkojen lukumäärä, Kansainvälisten organisaatioiden ja toimielinten toiminta"
                        },
                        new Models.CommuteStatisticsType() {
                            Id = "x_tuntem",
                            Description = "Työmatkojen lukumäärä, Toimiala tuntematon"
                        }
                    }).ToArray()
                }
            };
        }

        public Models.CommuteStatisticsIndicator Get(int statisticsId)
        {
            return (from n in this.GetAll()
                    where n.Id == statisticsId
                    select n).Single();
        }
    }
}