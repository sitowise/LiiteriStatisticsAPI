﻿using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using LiiteriStatisticsCore.Controllers;
using LiiteriStatisticsCore.Models;

namespace LiiteriStatisticsTests
{
    [TestClass]
    public class TestCalcType3
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
        public void Test_group_ct3_locality()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "locality");
        }

        [TestMethod]
        public void Test_filter_ct3_locality()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "municipality",
                "locality=6642865");
        }

        [TestMethod]
        public void Test_group_ct3_urban_area()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "urban_area");
        }

        [TestMethod]
        public void Test_filter_ct3_urban_area()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "municipality",
                "urban_area=19407492");
        }

        [TestMethod]
        public void Test_group_ct3_municipality()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "municipality");
        }

        [TestMethod]
        public void Test_filter_ct3_municipality()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "finland",
                "municipality=6262187");
        }

        [TestMethod]
        public void Test_group_ct3_sub_region()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "sub_region");
        }

        [TestMethod]
        public void Test_filter_ct3_sub_region()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "municipality",
                "sub_region=15679581");
        }

        [TestMethod]
        public void Test_group_ct3_region()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "region");
        }

        [TestMethod]
        public void Test_filter_ct3_region()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "municipality",
                "region=15679667");
        }

        [TestMethod]
        public void Test_group_ct3_ely_e()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "ely_e");
        }

        [TestMethod]
        public void Test_filter_ct3_ely_e()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "municipality",
                "ely_e=15679651");
        }

        [TestMethod]
        public void Test_group_ct3_ely_l()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "ely_l");
        }

        [TestMethod]
        public void Test_filter_ct3_ely_l()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "municipality",
                "ely_l=15679651");
        }

        [TestMethod]
        public void Test_group_ct3_ely_y()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "ely_y");
        }

        [TestMethod]
        public void Test_filter_ct3_ely_y()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "municipality",
                "ely_y=15679651");
        }

        [TestMethod]
        public void Test_group_ct3_finland()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "finland");
        }

        [TestMethod]
        public void Test_group_ct3_planned_area()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "planned_area");
        }

        [TestMethod]
        public void Test_filter_ct3_planned_area()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "municipality",
                "planned_area=19407884");
        }

        [TestMethod]
        public void Test_filter_ct3_planned_area_type()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "municipality",
                "planned_area_type=1");
        }

        [TestMethod]
        public void Test_group_ct3_planned_area_class()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "planned_area_class");
        }

        [TestMethod]
        public void Test_filter_ct3_planned_area_class()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "planned_area_type",
                "planned_area_class=1");
        }

        [TestMethod]
        public void Test_group_ct3_neighborhood_type()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "neighborhood_type");
        }

        [TestMethod]
        public void Test_filter_ct3_neighborhood_type()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "municipality",
                "neighborhood_type=5 OR neighborhood_type=8");
        }

        [TestMethod]
        public void Test_group_ct3_neighborhood_class()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "neighborhood_class");
        }

        [TestMethod]
        public void Test_filter_ct3_neighborhood_class()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "neighborhood_type",
                "neighborhood_class=1");
        }

        [TestMethod]
        public void Test_group_ct3_administrative_law_area()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "administrative_law_area");
        }

        [TestMethod]
        public void Test_filter_ct3_administrative_law_area()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "municipality",
                "administrative_law_area=15679686");
        }

        [TestMethod]
        public void Test_group_ct3_shop_area()
        {
            this.CheckStatistics(4100, new int[] { 2012 }, "shop_area");
        }

        [TestMethod]
        public void Test_filter_ct3_shop_area()
        {
            this.CheckStatistics(4100, new int[] { 2012 }, "municipality",
                "shop_area=19598699");
        }

        [TestMethod]
        public void Test_group_ct3_city_rural_area_type()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "city_rural_area_type");
        }

        [TestMethod]
        public void Test_filter_ct3_city_rural_area_type()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "municipality",
                "city_rural_area_type=1 OR city_rural_area_type=5");
        }

        [TestMethod]
        public void Test_group_ct3_city_rural_area_class()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "city_rural_area_class");
        }

        [TestMethod]
        public void Test_filter_ct3_city_rural_area_class()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "city_rural_area_type",
                "city_rural_area_class=1");
        }

        [TestMethod]
        public void Test_group_ct3_urban_area_type()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "urban_area_type");
        }

        [TestMethod]
        public void Test_filter_ct3_urban_area_type()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "municipality",
                "urban_area_type=1 OR urban_area_type=3");
        }

        [TestMethod]
        public void Test_group_ct3_urban_area_class()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "urban_area_class");
        }

        [TestMethod]
        public void Test_filter_ct3_urban_area_class()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "urban_area_type",
                "urban_area_class=1");
        }

        [TestMethod]
        public void Test_group_ct3_city_central_area()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "city_central_area");
        }

        [TestMethod]
        public void Test_filter_ct3_city_central_area()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "municipality",
                "city_central_area=19406755");
        }

        [TestMethod]
        public void Test_group_ct3_city_central_type()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "city_central_type");
        }

        [TestMethod]
        public void Test_filter_ct3_city_central_type()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "municipality",
                "city_central_type=1 OR city_central_type=4 OR city_central_type=7");
        }

        [TestMethod]
        public void Test_group_ct3_city_central_class()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "city_central_class");
        }

        [TestMethod]
        public void Test_filter_ct3_city_central_class()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "city_central_type",
                "city_central_class=1");
        }

        [TestMethod]
        public void Test_group_ct3_locality_density_type()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "locality_density_type");
        }

        [TestMethod]
        public void Test_filter_ct3_locality_density_type()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "municipality",
                "locality_density_type=2");
        }

        [TestMethod]
        public void Test_group_ct3_locality_density_class()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "locality_density_class");
        }

        [TestMethod]
        public void Test_filter_ct3_locality_density_class()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "locality_density_type",
                "locality_density_class=1");
        }

        [TestMethod]
        public void Test_group_ct3_locality_rural_type()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "locality_rural_type");
        }

        [TestMethod]
        public void Test_group_ct3_locality_size_type()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "locality_size_type");
        }

        [TestMethod]
        public void Test_filter_ct3_locality_rural_type()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "municipality",
                "locality_rural_type=2");
        }

        [TestMethod]
        public void Test_group_ct3_locality_rural_class()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "locality_rural_class");
        }

        [TestMethod]
        public void Test_filter_ct3_locality_rural_class()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "locality_rural_type",
                "locality_rural_class=1");
        }

        [TestMethod]
        public void Test_group_ct3_locality_size_class()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "locality_size_class");
        }

        [TestMethod]
        public void Test_filter_ct3_locality_size_class()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "locality_size_type",
                "locality_size_class=1");
        }

        [TestMethod]
        public void Test_group_ct3_urban_zone_type()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "urban_zone_type");
        }

        [TestMethod]
        public void Test_filter_ct3_urban_zone_type()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "municipality",
                "urban_zone_type=2 OR urban_zone_type=5");
        }

        [TestMethod]
        public void Test_group_ct3_urban_zone_class()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "urban_zone_class");
        }

        [TestMethod]
        public void Test_filter_ct3_urban_zone_class()
        {
            this.CheckStatistics(3002, new int[] { 2010 }, "urban_zone_type",
                "urban_zone_class=1");
        }
    }
}