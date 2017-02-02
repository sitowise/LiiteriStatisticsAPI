using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using LiiteriStatisticsCore.Controllers;
using LiiteriStatisticsCore.Models;

namespace LiiteriStatisticsTests
{
    [TestClass]
    public class TestAreaYearAvailability
    {
        private StatisticsController controller = new StatisticsController();

        public void CheckAreaYearAvailability(
            string areaTypeId)
        {
            var results = this.controller.GetAreaYearAvailability(areaTypeId);
            foreach (int row in results) {
                Assert.IsTrue(row > 1900);
            }

            Assert.IsTrue(results.Count() > 0);
        }

        [TestMethod]
        public void TestAreaYearAvailability_locality()
        {
            this.CheckAreaYearAvailability("locality");
        }

        [TestMethod]
        public void TestAreaYearAvailability_urban_area()
        {
            this.CheckAreaYearAvailability("urban_area");
        }

        [TestMethod]
        public void TestAreaYearAvailability_planned_area_type()
        {
            this.CheckAreaYearAvailability("planned_area_type");
        }

        [TestMethod]
        public void TestAreaYearAvailability_planned_area_class()
        {
            this.CheckAreaYearAvailability("planned_area_class");
        }

        [TestMethod]
        public void TestAreaYearAvailability_municipality()
        {
            this.CheckAreaYearAvailability("municipality");
        }

        [TestMethod]
        public void TestAreaYearAvailability_sub_region()
        {
            this.CheckAreaYearAvailability("sub_region");
        }

        [TestMethod]
        public void TestAreaYearAvailability_region()
        {
            this.CheckAreaYearAvailability("region");
        }

        [TestMethod]
        public void TestAreaYearAvailability_ely_e()
        {
            this.CheckAreaYearAvailability("ely_e");
        }

        [TestMethod]
        public void TestAreaYearAvailability_ely_l()
        {
            this.CheckAreaYearAvailability("ely_l");
        }

        [TestMethod]
        public void TestAreaYearAvailability_ely_y()
        {
            this.CheckAreaYearAvailability("ely_y");
        }

        [TestMethod]
        public void TestAreaYearAvailability_planned_area()
        {
            this.CheckAreaYearAvailability("planned_area");
        }

        [TestMethod]
        public void TestAreaYearAvailability_neighborhood_type()
        {
            this.CheckAreaYearAvailability("neighborhood_type");
        }

        [TestMethod]
        public void TestAreaYearAvailability_neighborhood_class()
        {
            this.CheckAreaYearAvailability("neighborhood_class");
        }

        [TestMethod]
        public void TestAreaYearAvailability_administrative_law_area()
        {
            this.CheckAreaYearAvailability("administrative_law_area");
        }

        [TestMethod]
        public void TestAreaYearAvailability_shop_area()
        {
            this.CheckAreaYearAvailability("shop_area");
        }

        [TestMethod]
        public void TestAreaYearAvailability_city_rural_area_type()
        {
            this.CheckAreaYearAvailability("city_rural_area_type");
        }

        [TestMethod]
        public void TestAreaYearAvailability_city_rural_area_class()
        {
            this.CheckAreaYearAvailability("city_rural_area_class");
        }

        [TestMethod]
        public void TestAreaYearAvailability_urban_area_type()
        {
            this.CheckAreaYearAvailability("urban_area_type");
        }

        [TestMethod]
        public void TestAreaYearAvailability_urban_area_class()
        {
            this.CheckAreaYearAvailability("urban_area_class");
        }

        [TestMethod]
        public void TestAreaYearAvailability_city_central_area()
        {
            this.CheckAreaYearAvailability("city_central_area");
        }

        [TestMethod]
        public void TestAreaYearAvailability_city_central_type()
        {
            this.CheckAreaYearAvailability("city_central_type");
        }

        [TestMethod]
        public void TestAreaYearAvailability_city_central_class()
        {
            this.CheckAreaYearAvailability("city_central_class");
        }

        [TestMethod]
        public void TestAreaYearAvailability_locality_density_type()
        {
            this.CheckAreaYearAvailability("locality_density_type");
        }

        [TestMethod]
        public void TestAreaYearAvailability_locality_density_class()
        {
            this.CheckAreaYearAvailability("locality_density_class");
        }

        [TestMethod]
        public void TestAreaYearAvailability_locality_rural_type()
        {
            this.CheckAreaYearAvailability("locality_rural_type");
        }

        [TestMethod]
        public void TestAreaYearAvailability_locality_rural_class()
        {
            this.CheckAreaYearAvailability("locality_rural_class");
        }

        [TestMethod]
        public void TestAreaYearAvailability_urban_zone_type()
        {
            this.CheckAreaYearAvailability("urban_zone_type");
        }

        [TestMethod]
        public void TestAreaYearAvailability_urban_zone_class()
        {
            this.CheckAreaYearAvailability("urban_zone_class");
        }

        [TestMethod]
        public void TestAreaYearAvailability_grid250m()
        {
            this.CheckAreaYearAvailability("grid250m");
        }

        [TestMethod]
        public void TestAreaYearAvailability_grid500m()
        {
            this.CheckAreaYearAvailability("grid500m");
        }

        [TestMethod]
        public void TestAreaYearAvailability_grid1km()
        {
            this.CheckAreaYearAvailability("grid1km");
        }

        [TestMethod]
        public void TestAreaYearAvailability_grid2km()
        {
            this.CheckAreaYearAvailability("grid2km");
        }

        [TestMethod]
        public void TestAreaYearAvailability_grid5km()
        {
            this.CheckAreaYearAvailability("grid5km");
        }

        [TestMethod]
        public void TestAreaYearAvailability_grid10km()
        {
            this.CheckAreaYearAvailability("grid10km");
        }

        [TestMethod]
        public void TestAreaYearAvailability_grid20km()
        {
            this.CheckAreaYearAvailability("grid20km");
        }
    }
}