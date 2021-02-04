using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using UserDefinedExpressions.SafetyValidators;
using UserDefinedExpressions.Tests.AventureWorksEntities;

namespace UserDefinedExpressions.Tests
{
    public class SafeScopeTests
    {
        SalesOrderHeader order = new SalesOrderHeader() { DueDate = DateTime.Today.AddDays(1), Customer = new Customer() {  PersonId = 1 } };

        [Test(Description = "Using member properties from TInput should work")]
        public void SafeScopeTest()
        {
            Assert.DoesNotThrow(() =>
            {
                var safeExpression = UserDefinedExpression<SalesOrderHeader, bool>.Create("Customer != null && DueDate >= DateTime.Today");
                bool result = safeExpression.Invoke(order);
            });
        }

        [Test(Description = "Using member properties from other types should fail")]
        public void UnsafeScopeTest()
        {
            Assert.Throws<UnsafeExpressionException>(() =>
            {
                var unsafeExpression = UserDefinedExpression<SalesOrderHeader, bool>.Create("Customer.PersonId != null");
                bool result = unsafeExpression.Invoke(order);
            });
        }

        [Test(Description = "Using member properties from other types should work if they are explicitly added to whitelist")]
        public void UnsafeScopeTestAllowance()
        {
            Assert.DoesNotThrow(() =>
            {
                TypesValidator.Defaults.AddAllowedType(typeof(Customer));
                var unsafeExpression = UserDefinedExpression<SalesOrderHeader, bool>.Create("Customer.PersonId != null");
                bool result = unsafeExpression.Invoke(order);
            });
        }

    }
}
