using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using UserDefinedExpressions.Tests.AventureWorksEntities;

namespace UserDefinedExpressions.Tests
{
    public class SimpleTests
    {
        [Test(Description = "Simple test based on a single date property")]
        public void SinglePropertyTest()
        {
            var isOrderPastDueFormula = UserDefinedExpression<SalesOrderHeader, bool>.Create("DueDate < DateTime.Today");

            var order1 = new SalesOrderHeader() { DueDate = DateTime.Today.AddDays(-1) };
            var order2 = new SalesOrderHeader() { DueDate = DateTime.Today.AddDays(1) };

            bool isOrderPastDue1 = isOrderPastDueFormula.Invoke(order1);
            bool isOrderPastDue2 = isOrderPastDueFormula.Invoke(order2);
            Assert.IsTrue(isOrderPastDue1);
            Assert.IsFalse(isOrderPastDue2);
        }

        [Test(Description = "Simple test based on a single date property")]
        public void AndOrTests()
        {
            var isHeavyOrExpensive = UserDefinedExpression<SalesOrderHeader, bool>.Create("TotalDue > 1000 || Freight > 1000");
            var isHeavyAndExpensive = UserDefinedExpression<SalesOrderHeader, bool>.Create("TotalDue > 1000 && Freight > 1000");

            var order1 = new SalesOrderHeader() { TotalDue = 1200, Freight = 700 };
            var order2 = new SalesOrderHeader() { TotalDue = 800, Freight = 1300 };
            var order3 = new SalesOrderHeader() { TotalDue = 800, Freight = 700 };
            var order4 = new SalesOrderHeader() { TotalDue = 1200, Freight = 1300 };

            Assert.IsTrue(isHeavyOrExpensive.Invoke(order1));
            Assert.IsTrue(isHeavyOrExpensive.Invoke(order2));
            Assert.IsFalse(isHeavyOrExpensive.Invoke(order3));
            Assert.IsTrue(isHeavyOrExpensive.Invoke(order4));

            Assert.IsFalse(isHeavyAndExpensive.Invoke(order1));
            Assert.IsFalse(isHeavyAndExpensive.Invoke(order2));
            Assert.IsFalse(isHeavyAndExpensive.Invoke(order3));
            Assert.IsTrue(isHeavyAndExpensive.Invoke(order4));
        }

    }
}
