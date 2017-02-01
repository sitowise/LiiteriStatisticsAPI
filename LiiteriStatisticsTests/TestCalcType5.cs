using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using LiiteriStatisticsCore.Controllers;
using LiiteriStatisticsCore.Models;

namespace LiiteriStatisticsTests
{
    [TestClass]
    public class TestCalcType5
    {
        private StatisticsController controller = new StatisticsController();

        public void CheckStatistics(
            int statisticsId,
            int[] years,
            string group,
            string filter = null,
            int? areaYear = null)
        {
            var results = this.controller.GetStatistics(
                years, statisticsId, group, filter, areaYear);
            Assert.IsTrue(results.Count() > 0);
        }

        [TestMethod]
        public void Test_group_ct5_municipality()
        {
            this.CheckStatistics(100286, new int[] { 2010 }, "municipality");
        }

        [TestMethod]
        public void Test_filter_ct5_municipality()
        {
            this.CheckStatistics(100286, new int[] { 2010 }, "municipality",
                "municipality=6262187");
        }

        [TestMethod]
        public void Test_group_ct5_sub_region()
        {
            this.CheckStatistics(100286, new int[] { 2010 }, "sub_region");
        }

        [TestMethod]
        public void Test_filter_ct5_sub_region()
        {
            this.CheckStatistics(100286, new int[] { 2010 }, "municipality",
                "sub_region=15679581");
        }

        [TestMethod]
        public void Test_group_ct5_region()
        {
            this.CheckStatistics(100286, new int[] { 2010 }, "region");
        }

        [TestMethod]
        public void Test_filter_ct5_region()
        {
            this.CheckStatistics(100286, new int[] { 2010 }, "municipality",
                "region=15679667");
        }

        [TestMethod]
        public void Test_group_ct5_ely_e()
        {
            this.CheckStatistics(100286, new int[] { 2010 }, "ely_e");
        }

        [TestMethod]
        public void Test_filter_ct5_ely_e()
        {
            this.CheckStatistics(100286, new int[] { 2010 }, "municipality",
                "ely_e=15679651");
        }

        [TestMethod]
        public void Test_group_ct5_ely_l()
        {
            this.CheckStatistics(100286, new int[] { 2010 }, "ely_l");
        }

        [TestMethod]
        public void Test_filter_ct5_ely_l()
        {
            this.CheckStatistics(100286, new int[] { 2010 }, "municipality",
                "ely_l=15679651");
        }

        [TestMethod]
        public void Test_group_ct5_ely_y()
        {
            this.CheckStatistics(100286, new int[] { 2010 }, "ely_y");
        }

        [TestMethod]
        public void Test_filter_ct5_ely_y()
        {
            this.CheckStatistics(100286, new int[] { 2010 }, "municipality",
                "ely_y=15679651");
        }

        [TestMethod]
        public void Test_group_ct5_administrative_law_area()
        {
            this.CheckStatistics(100286, new int[] { 2010 }, "administrative_law_area");
        }

        [TestMethod]
        public void Test_filter_ct5_administrative_law_area()
        {
            this.CheckStatistics(100286, new int[] { 2010 }, "municipality",
                "administrative_law_area=15679686");
        }

        [TestMethod]
        public void Test_group_ct5_finland()
        {
            this.CheckStatistics(100286, new int[] { 2010 }, "finland");
        }
    }
}