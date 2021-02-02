using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using UserDefinedExpressions.SafetyValidators;
using UserDefinedExpressions.Tests.AventureWorksEntities;

namespace UserDefinedExpressions.Tests
{
    public class UnsafeExpressionsTests
    {
        [SetUp]
        [TearDown]
        public void CleanUp()
        {
            System.GC.Collect(); System.GC.WaitForPendingFinalizers(); // open file handles
            if (System.IO.File.Exists("dummy")) System.IO.File.Delete("dummy"); // some tests may try to write to this file
        }

        public void UnsafeExpressionsShouldThrow(string unsafeExpression, ISafetyValidator validator)
        {
            UserDefinedExpression.DefaultSafetyValidator = validator;
            Assert.Throws<UnsafeExpressionException>(() => UserDefinedExpression<SalesOrderHeader, bool>.Create(unsafeExpression));
        }
        public void UnsafeExpressionsShouldPass(string unsafeExpression, ISafetyValidator validator)
        {
            var order1 = new SalesOrderHeader() { DueDate = DateTime.Today.AddDays(-1) };
            UserDefinedExpression.DefaultSafetyValidator = validator;
            Assert.DoesNotThrow(() =>
            {
                var unsafeUDE = UserDefinedExpression<SalesOrderHeader, bool>.Create(unsafeExpression);
                bool result = unsafeUDE.Invoke(order1);
            });
        }

        [Test(Description = "Unsafe expression should throw exceptions")]
        [TestCase("DueDate < new DateTime(System.IO.File.Create(\"dummy\").Length)")]
        [TestCase("Environment.GetEnvironmentVariable(\"USERNAME\") == \"rick\"")]
        [TestCase("Environment.CommandLine == \"rick\"")] // this one is not working with SimpleWhiteListInvocationExpressionsValidator
        public void TestUnsafeExpressions(string unsafeExpression)
        {
            // This is the default validator
            UnsafeExpressionsShouldThrow(unsafeExpression, new StrictWhiteListSyntaxTreeValidator());

            // This is an alternative validator which is more strict because even getters/setters are checked
            UnsafeExpressionsShouldThrow(unsafeExpression, new SimpleWhiteListInvocationExpressionsValidator());

            // This dummy validator should accept and evaluate whatever it receives
            UnsafeExpressionsShouldPass(unsafeExpression, new UnsafeNoChecksValidator());
        }

    }
}
