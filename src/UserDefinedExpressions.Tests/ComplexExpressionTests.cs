using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UserDefinedExpressions.Tests.AventureWorksEntities;

namespace UserDefinedExpressions.Tests
{
    public class ComplexExpressions
    {
        [Test(Description = "Complex expressions based on complex types")]
        public void ComplexExpressionTest()
        {
            var creditInfo = new CreditInfo()
            {
                HardInquiries = new List<DateTime>() 
                { 
                    new DateTime(2020, 12, 12), 
                    new DateTime(2019, 05, 06),
                    new DateTime(2017, 03, 03)
                },
                Accounts = new List<CreditAccount>()
                {
                    new CreditAccount() { TotalCredit = 10000, UsedCredit = 3000 },
                    new CreditAccount() { TotalCredit = 5000, UsedCredit = 300 }
                }
            };

            //creditInfo.HardInquiries.Count(h => h.Date > DateTime.Today.AddDays(-300)) > 1;
            //creditInfo.Accounts.Sum(a => a.UsedCredit) / creditInfo.Accounts.Sum(a => a.TotalCredit);

            // The input model (CreditInfo) is automatically added to white-list, but other non-primitive types should be explicitly added
            SafetyValidators.TypesValidator.Defaults.AddAllowedType(typeof(CreditAccount));


            // Past 2 years
            var recentHardInquiriesFormula = UserDefinedExpression<CreditInfo, int>
                .Create("HardInquiries.Count(h => h.Date > DateTime.Today.AddYears(-2))");

            var creditUsageFormula = UserDefinedExpression<CreditInfo, decimal>
                .Create("Accounts.Sum(a => a.UsedCredit) / Accounts.Sum(a => a.TotalCredit)");


            var recentHardInquiries = recentHardInquiriesFormula.Invoke(creditInfo);
            Assert.AreEqual(2, recentHardInquiries);
            var creditUsage = creditUsageFormula.Invoke(creditInfo);
            Assert.AreEqual(0.22, creditUsage);

        }

        public class CreditInfo
        {
            public List<DateTime> HardInquiries { get; set; }
            public List<CreditAccount> Accounts { get; set; }
        }
        public class CreditAccount
        {
            public decimal TotalCredit { get; set; }
            public decimal UsedCredit { get; set; }
        }

    }
}
