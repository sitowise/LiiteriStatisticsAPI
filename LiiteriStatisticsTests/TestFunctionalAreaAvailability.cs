using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using LiiteriStatisticsCore.Controllers;
using LiiteriStatisticsCore.Models;

namespace LiiteriStatisticsTests
{
    [TestClass]
    public class TestFunctionalAreaAvailability
    {
        private StatisticsController controller = new StatisticsController();

        public void CheckFunctionalAreaAvailability(
            string areaTypeId, int year, string filter = null)
        {
            var results = this.controller.GetFunctionalAreaAvailability(
                areaTypeId, year, filter);
            string[] l = results.First().AvailableFunctionalAreas.ToArray();
            Assert.IsTrue(l.Count() > 0);
        }

        [TestMethod]
        public void TestFunctionalAreaAvailability_municipality()
        {
            this.CheckFunctionalAreaAvailability("municipality", 2010);
        }

        [TestMethod]
        public void TestFunctionalAreaAvailability_sub_region()
        {
            this.CheckFunctionalAreaAvailability("sub_region", 2010);
        }

        [TestMethod]
        public void TestFunctionalAreaAvailability_region()
        {
            this.CheckFunctionalAreaAvailability("region", 2010);
        }

        [TestMethod]
        public void TestFunctionalAreaAvailability_ely_e()
        {
            this.CheckFunctionalAreaAvailability("ely_e", 2010);
        }

        [TestMethod]
        public void TestFunctionalAreaAvailability_ely_l()
        {
            this.CheckFunctionalAreaAvailability("ely_l", 2010);
        }

        [TestMethod]
        public void TestFunctionalAreaAvailability_ely_y()
        {
            this.CheckFunctionalAreaAvailability("ely_y", 2010);
        }

        [TestMethod]
        public void TestFunctionalAreaAvailability_administrative_law_area()
        {
            this.CheckFunctionalAreaAvailability("administrative_law_area", 2010);
        }
    }
}