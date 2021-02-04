# UserDefinedExpressions

**UserDefinedExpressions is a .NET library to safely invoke (evaluate) User Defined Expressions (Formulas) using Roslyn Scripting Engine**

One of the most common tasks in business applications is to allow end-users to configure business rules on their own.
This library allows dynamic evaluation of C# expressions (which can be written by end-users as if they were writing Excel formulas) but it's safe against malicious code.


## Installation
Just install nuget package **[UserDefinedExpressions](https://www.nuget.org/packages/UserDefinedExpressions/)**, 
add `using UserDefinedExpressions` and start using (see examples below).  
See documentation below, or more examples in [unit tests](https://github.com/Drizin/UserDefinedExpressions/tree/master/src/UserDefinedExpressions.Tests).

## Documentation

**Sample usage:**

```cs

public class SalesOrderHeader
{
    public decimal TotalDue { get; set; }
    public decimal Freight { get; set; }
}

var isHeavyOrExpensiveFormula = UserDefinedExpression<SalesOrderHeader, bool>
    .Create("TotalDue > 1000 || Freight > 1000");

var order = new SalesOrderHeader() { TotalDue = 1200, Freight = 700 };

bool isHeavyOrExpensive = isHeavyOrExpensiveFormula.Invoke(order);
```

**Unsafe expressions are blocked:**

If you use an unsafe expression, you'll get an **`UnsafeExpressionException` exception**:

```cs
var unsafeFormula = UserDefinedExpression<SalesOrderHeader, bool>
    .Create("System.IO.File.Create(\"dummy\")");

var order = new SalesOrderHeader() { TotalDue = 1200, Freight = 700 };

try 
{
    bool isHeavyOrExpensive = unsafeFormula.Invoke(order);
}
catch(UnsafeExpressionException ex)
{
    //ex.Message: 'ForbiddenCall: Cannot use System.IO.File (location : (0,0)-(0,30)): System.IO.File.Create("dummy")'
}
```

**Passing Complex Models as Expressions Input:**

```cs
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

// The input model (CreditInfo) is automatically added to white-list, 
// but other non-primitive types should be explicitly added
SafetyValidators.TypesValidator.Defaults.AddAllowedType(typeof(CreditAccount));

// Past 2 years
var recentHardInquiriesFormula = UserDefinedExpression<CreditInfo, int>
  .Create("HardInquiries.Count(h => h.Date > DateTime.Today.AddYears(-2))");

var creditUsageFormula = UserDefinedExpression<CreditInfo, decimal>
  .Create("Accounts.Sum(a => a.UsedCredit) / Accounts.Sum(a => a.TotalCredit)");


var recentHardInquiries = recentHardInquiriesFormula.Invoke(creditInfo); // 2 
var creditUsage = creditUsageFormula.Invoke(creditInfo); // 0.22
```