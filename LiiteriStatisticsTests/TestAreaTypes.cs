using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using LiiteriStatisticsCore.Controllers;
using LiiteriStatisticsCore.Models;

namespace LiiteriStatisticsTests
{
    [TestClass]
    public class TestAreaTypes
    {
        private StatisticsController controller = new StatisticsController();

        [TestMethod]
        public void TestAreaType_locality()
        {
            var areas = this.controller.GetAreas("locality");
            Assert.IsTrue(areas.Count() > 0);
        }

        [TestMethod]
        public void TestAreaType_urban_area()
        {
            var areas = this.controller.GetAreas("urban_area");
            Assert.IsTrue(areas.Count() > 0);
        }

        [TestMethod]
        public void TestAreaType_planned_area_type()
        {
            var areas = this.controller.GetAreas("planned_area_type");
            Assert.IsTrue(areas.Count() > 0);
        }

        [TestMethod]
        public void TestAreaType_planned_area_class()
        {
            var areas = this.controller.GetAreas("planned_area_class");
            Assert.IsTrue(areas.Count() > 0);
        }

        [TestMethod]
        public void TestAreaType_municipality()
        {
            var areas = this.controller.GetAreas("municipality");
            Assert.IsTrue(areas.Count() > 0);
        }

        [TestMethod]
        public void TestAreaType_sub_region()
        {
            var areas = this.controller.GetAreas("sub_region");
            Assert.IsTrue(areas.Count() > 0);
        }

        [TestMethod]
        public void TestAreaType_region()
        {
            var areas = this.controller.GetAreas("region");
            Assert.IsTrue(areas.Count() > 0);
        }

        [TestMethod]
        public void TestAreaType_ely_e()
        {
            var areas = this.controller.GetAreas("ely_e");
            Assert.IsTrue(areas.Count() > 0);
        }

        [TestMethod]
        public void TestAreaType_ely_l()
        {
            var areas = this.controller.GetAreas("ely_l");
            Assert.IsTrue(areas.Count() > 0);
        }

        [TestMethod]
        public void TestAreaType_ely_y()
        {
            var areas = this.controller.GetAreas("ely_y");
            Assert.IsTrue(areas.Count() > 0);
        }

        [TestMethod]
        public void TestAreaType_finland()
        {
            var areas = this.controller.GetAreas("finland");
            Assert.IsTrue(areas.Count() > 0);
        }

        [TestMethod]
        public void TestAreaType_planned_area()
        {
            var areas = this.controller.GetAreas("planned_area");
            Assert.IsTrue(areas.Count() > 0);
        }

        [TestMethod]
        public void TestAreaType_neighborhood_type()
        {
            var areas = this.controller.GetAreas("neighborhood_type");
            Assert.IsTrue(areas.Count() > 0);
        }

        [TestMethod]
        public void TestAreaType_neighborhood_class()
        {
            var areas = this.controller.GetAreas("neighborhood_class");
            Assert.IsTrue(areas.Count() > 0);
        }

        [TestMethod]
        public void TestAreaType_administrative_law_area()
        {
            var areas = this.controller.GetAreas("administrative_law_area");
            Assert.IsTrue(areas.Count() > 0);
        }

        [TestMethod]
        public void TestAreaType_shop_area()
        {
            var areas = this.controller.GetAreas("shop_area");
            Assert.IsTrue(areas.Count() > 0);
        }

        [TestMethod]
        public void TestAreaType_city_rural_area_type()
        {
            var areas = this.controller.GetAreas("city_rural_area_type");
            Assert.IsTrue(areas.Count() > 0);
        }

        [TestMethod]
        public void TestAreaType_city_rural_area_class()
        {
            var areas = this.controller.GetAreas("city_rural_area_class");
            Assert.IsTrue(areas.Count() > 0);
        }

        [TestMethod]
        public void TestAreaType_urban_area_type()
        {
            var areas = this.controller.GetAreas("urban_area_type");
            Assert.IsTrue(areas.Count() > 0);
        }

        [TestMethod]
        public void TestAreaType_urban_area_class()
        {
            var areas = this.controller.GetAreas("urban_area_class");
            Assert.IsTrue(areas.Count() > 0);
        }

        [TestMethod]
        public void TestAreaType_city_central_area()
        {
            var areas = this.controller.GetAreas("city_central_area");
            Assert.IsTrue(areas.Count() > 0);
        }

        [TestMethod]
        public void TestAreaType_city_central_type()
        {
            var areas = this.controller.GetAreas("city_central_type");
            Assert.IsTrue(areas.Count() > 0);
        }

        [TestMethod]
        public void TestAreaType_city_central_class()
        {
            var areas = this.controller.GetAreas("city_central_class");
            Assert.IsTrue(areas.Count() > 0);
        }

        [TestMethod]
        public void TestAreaType_locality_density_type()
        {
            var areas = this.controller.GetAreas("locality_density_type");
            Assert.IsTrue(areas.Count() > 0);
        }

        [TestMethod]
        public void TestAreaType_locality_density_class()
        {
            var areas = this.controller.GetAreas("locality_density_class");
            Assert.IsTrue(areas.Count() > 0);
        }

        [TestMethod]
        public void TestAreaType_locality_rural_type()
        {
            var areas = this.controller.GetAreas("locality_rural_type");
            Assert.IsTrue(areas.Count() > 0);
        }

        [TestMethod]
        public void TestAreaType_locality_rural_class()
        {
            var areas = this.controller.GetAreas("locality_rural_class");
            Assert.IsTrue(areas.Count() > 0);
        }

        [TestMethod]
        public void TestAreaType_locality_size_type()
        {
            var areas = this.controller.GetAreas("locality_size_type");
            Assert.IsTrue(areas.Count() > 0);
        }

        [TestMethod]
        public void TestAreaType_locality_size_class()
        {
            var areas = this.controller.GetAreas("locality_size_class");
            Assert.IsTrue(areas.Count() > 0);
        }

        [TestMethod]
        public void TestAreaType_urban_zone_type()
        {
            var areas = this.controller.GetAreas("urban_zone_type");
            Assert.IsTrue(areas.Count() > 0);
        }

        [TestMethod]
        public void TestAreaType_urban_zone_class()
        {
            var areas = this.controller.GetAreas("urban_zone_class");
            Assert.IsTrue(areas.Count() > 0);
        }

        [Ignore] // grids cannot be queried
        [TestMethod]
        public void TestAreaType_grid250m()
        {
            var areas = this.controller.GetAreas("grid250m");
            Assert.IsTrue(areas.Count() > 0);
        }

        [Ignore] // grids cannot be queried
        [TestMethod]
        public void TestAreaType_grid500m()
        {
            var areas = this.controller.GetAreas("grid500m");
            Assert.IsTrue(areas.Count() > 0);
        }

        [Ignore] // grids cannot be queried
        [TestMethod]
        public void TestAreaType_grid1km()
        {
            var areas = this.controller.GetAreas("grid1km");
            Assert.IsTrue(areas.Count() > 0);
        }

        [Ignore] // grids cannot be queried
        [TestMethod]
        public void TestAreaType_grid2km()
        {
            var areas = this.controller.GetAreas("grid2km");
            Assert.IsTrue(areas.Count() > 0);
        }

        [Ignore] // grids cannot be queried
        [TestMethod]
        public void TestAreaType_grid5km()
        {
            var areas = this.controller.GetAreas("grid5km");
            Assert.IsTrue(areas.Count() > 0);
        }

        [Ignore] // grids cannot be queried
        [TestMethod]
        public void TestAreaType_grid10km()
        {
            var areas = this.controller.GetAreas("grid10km");
            Assert.IsTrue(areas.Count() > 0);
        }

        [Ignore] // grids cannot be queried
        [TestMethod]
        public void TestAreaType_grid20km()
        {
            var areas = this.controller.GetAreas("grid20km");
            Assert.IsTrue(areas.Count() > 0);
        }
    }
}