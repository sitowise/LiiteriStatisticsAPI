using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using LiiteriStatisticsCore.Controllers;
using LiiteriStatisticsCore.Models;

namespace LiiteriStatisticsTests
{
    [TestClass]
    public class TestCommute
    {
        private CommuteStatisticsController controller =
            new CommuteStatisticsController();

        public void CheckCommuteStatistics(
            int statisticsId,
            int[] years,
            string statisticsType,
            string group,
            string workFilter = null,
            string homeFilter = null,
            int? areaYear = null)
        {
            var results = this.controller.GetCommuteStatistics(
                statisticsId,
                years,
                statisticsType,
                0,
                group,
                workFilter,
                homeFilter,
                null);
            Assert.IsTrue(results.Count() > 0);
        }

        [TestMethod]
        public void TestPrivacyLimit()
        {
            var results = this.controller.GetCommuteStatistics(
                -2,
                new int[] { 2010 },
                "r_taide",
                0,
                "home:municipality",
                "municipality=6262187",
                null,
                null);

            bool failure = false;
            int count = 0;
            foreach (var row in results) {
                if (row.PrivacyLimitTriggered == true && row.Value != null) {
                    failure = true;
                } else {
                    count++;
                }
            }
            Assert.IsFalse(failure);
            Assert.IsTrue(count > 0);
        }

        [TestMethod]
        public void TestNoPrivacyLimit()
        {
            var results = this.controller.GetCommuteStatistics(
                -2,
                new int[] { 2010 },
                "yht",
                0,
                "home:municipality",
                "municipality=6262187",
                null,
                null);

            bool failure = false;
            int count = 0;
            foreach (var row in results) {
                if (row.PrivacyLimitTriggered == true && row.Value == null) {
                    failure = true;
                } else {
                    count++;
                }
            }
            Assert.IsFalse(failure);
            Assert.IsTrue(count > 0);
        }

        [TestMethod]
        public void Test_group_municipality() {
            this.CheckCommuteStatistics(-2, new int[] { 2010 }, "yht", "home:municipality");
        }

        [TestMethod]
        public void Test_filter_municipality() {
            this.CheckCommuteStatistics(-2, new int[] { 2010 }, "yht", "home:municipality",
                workFilter: "municipality=6262187");
        }

        [TestMethod]
        public void Test_group_sub_region() {
            this.CheckCommuteStatistics(-2, new int[] { 2010 }, "yht", "home:sub_region");
        }

        [TestMethod]
        public void Test_filter_sub_region() {
            this.CheckCommuteStatistics(-2, new int[] { 2010 }, "yht", "home:municipality",
                workFilter: "sub_region=15679581");
        }

        [TestMethod]
        public void Test_group_region() {
            this.CheckCommuteStatistics(-2, new int[] { 2010 }, "yht", "home:region");
        }

        [TestMethod]
        public void Test_filter_region() {
            this.CheckCommuteStatistics(-2, new int[] { 2010 }, "yht", "home:municipality",
                workFilter: "region=15679667");
        }

        [TestMethod]
        public void Test_group_ely_e() {
            this.CheckCommuteStatistics(-2, new int[] { 2010 }, "yht", "home:ely_e");
        }

        [TestMethod]
        public void Test_filter_ely_e() {
            this.CheckCommuteStatistics(-2, new int[] { 2010 }, "yht", "home:municipality",
                workFilter: "ely_e=15679651");
        }

        [TestMethod]
        public void Test_group_ely_l() {
            this.CheckCommuteStatistics(-2, new int[] { 2010 }, "yht", "home:ely_l");
        }

        [TestMethod]
        public void Test_filter_ely_l() {
            this.CheckCommuteStatistics(-2, new int[] { 2010 }, "yht", "home:municipality",
                workFilter: "ely_l=15679651");
        }

        [TestMethod]
        public void Test_group_ely_y() {
            this.CheckCommuteStatistics(-2, new int[] { 2010 }, "yht", "home:ely_y");
        }

        [TestMethod]
        public void Test_filter_ely_y() {
            this.CheckCommuteStatistics(-2, new int[] { 2010 }, "yht", "home:municipality",
                workFilter: "ely_y=15679651");
        }

        [TestMethod]
        public void Test_group_administrative_law_area() {
            this.CheckCommuteStatistics(-2, new int[] { 2010 }, "yht", "home:administrative_law_area");
        }

        [TestMethod]
        public void Test_filter_administrative_law_area() {
            this.CheckCommuteStatistics(-2, new int[] { 2010 }, "yht", "home:municipality",
                workFilter: "administrative_law_area=15679686");
        }

        [TestMethod]
        public void Test_group_finland() {
            this.CheckCommuteStatistics(-2, new int[] { 2010 }, "yht", "home:finland");
        }
    }
}