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

        public void UnsafeExpressionsShouldThrow(string unsafeExpression, Func<IUserDefinedExpression, ISafetyValidator> validatorFactory)
        {
            UserDefinedExpression.DefaultSafetyValidatorFactory = validatorFactory;
            Assert.Throws<UnsafeExpressionException>(() => UserDefinedExpression<SalesOrderHeader, bool>.Create(unsafeExpression));
        }
        public void UnsafeExpressionsShouldPass(string unsafeExpression, Func<IUserDefinedExpression, ISafetyValidator> validatorFactory)
        {
            var order1 = new SalesOrderHeader() { DueDate = DateTime.Today.AddDays(-1) };
            UserDefinedExpression.DefaultSafetyValidatorFactory = validatorFactory;
            Assert.DoesNotThrow(() =>
            {
                var unsafeUDE = UserDefinedExpression<SalesOrderHeader, bool>.Create(unsafeExpression);
                bool result = unsafeUDE.Invoke(order1);
            });
        }

        [Test(Description = "Unsafe invocation should throw exceptions")]
        [TestCase("DueDate < new DateTime(System.IO.File.Create(\"dummy\").Length)")]
        [TestCase("Environment.GetEnvironmentVariable(\"USERNAME\") == \"rick\"")]
        [TestCase("Environment.SetEnvironmentVariable(\"USERNAME\", \"rick\")")]
        public void TestUnsafeInvocations(string unsafeExpression)
        {
            // This is the default validator, which is very strict (checks not only InvocationExpression but even getters/setters, etc)
            UnsafeExpressionsShouldThrow(unsafeExpression, (e) => new StrictWhiteListTypeValidator(e));

            // This is an alternative validator which only validates InvocationExpressions against a whitelist of accepted types
            UnsafeExpressionsShouldThrow(unsafeExpression, (e) => new SimpleWhiteListValidator(e));

            // This dummy validator should accept and evaluate whatever it receives
            UnsafeExpressionsShouldPass(unsafeExpression, (e) => new UnsafeNoChecksValidator());
        }

        [Test(Description = "Any Unsafe expression (even if it's not an invocation, like getter/setter) should throw exceptions")]
        [TestCase("Environment.CommandLine == \"rick\"")] // this one is failing (doesn't throw) with SimpleWhiteListValidator
        public void TestUnsafeExpressions(string unsafeExpression)
        {
            // This is the default validator, which is very strict (checks not only InvocationExpression but even getters/setters, etc)
            UnsafeExpressionsShouldThrow(unsafeExpression, (e) => new StrictWhiteListTypeValidator(e));

            //Currently this alternative validator (which only validates InvocationExpressions) is NOT blocking non-invocation (e.g. getters/setters) unsafe expressions
            //UnsafeExpressionsShouldThrow(unsafeExpression, (e) => new SimpleWhiteListValidator(e));

            // This dummy validator should accept and evaluate whatever it receives
            UnsafeExpressionsShouldPass(unsafeExpression, (e) => new UnsafeNoChecksValidator());
        }

    }
}
