using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using LiiteriStatisticsCore.Controllers;
using LiiteriStatisticsCore.Models;

namespace LiiteriStatisticsTests
{
    public sealed class ExpectedExceptionWithMessage : ExpectedExceptionBaseAttribute
    {
        private Type _expectedExceptionType;
        private string _expectedExceptionMessage;

        public ExpectedExceptionWithMessage(Type expectedExceptionType)
        {
            _expectedExceptionType = expectedExceptionType;
            _expectedExceptionMessage = string.Empty;
        }

        public ExpectedExceptionWithMessage(
            Type expectedExceptionType, string expectedExceptionMessage)
        {
            _expectedExceptionType = expectedExceptionType;
            _expectedExceptionMessage = expectedExceptionMessage;
        }

        protected override void Verify(Exception exception)
        {
            Assert.IsNotNull(exception);

            Assert.IsInstanceOfType(exception, _expectedExceptionType,
                "Wrong type of exception was thrown.");

            if (!_expectedExceptionMessage.Length.Equals(0)) {
                Assert.AreEqual(_expectedExceptionMessage, exception.Message,
                    "Wrong exception message was returned.");
            }
        }
    }

    [TestClass]
    public class TestCalcType4
    {
        private StatisticsController controller = new StatisticsController();

        [TestMethod]
        public void Test_group_ct4_grid250m()
        {
            var results = this.controller.GetStatistics(
                new int[] { 2010 }, 23, "grid250m", "municipality=6262223", null);
            Assert.IsTrue(results.Count() > 2);
        }

        [TestMethod]
        [ExpectedExceptionWithMessage(typeof(Exception),
            "Supplied grouping areaType not suitable for this statistics data!")]
        public void Test_fail()
        {
            var results = this.controller.GetStatistics(
                new int[] { 2010 }, 5001, "municipality", null, null);
        }
    }
}