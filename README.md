# UserDefinedExpressions

**UserDefinedExpressions is a .NET library to safely invoke (evaluate) User Defined Expressions (Formulas) using Roslyn Scripting Engine**

One of the most common tasks in business applications is to allow end-users to configure business rules on their own.
This library allows developers to evaluate C# expressions written by end-users (as if they were writing Excel formulas) but it's safe against malicious code.


## Installation
Just install nuget package **[UserDefinedExpressions](https://www.nuget.org/packages/UserDefinedExpressions/)**, 
add `using UserDefinedExpressions` and start using (see examples below).  
See documentation below, or more examples in [unit tests](https://github.com/Drizin/UserDefinedExpressions/tree/master/src/UserDefinedExpressions.Tests).

## Documentation

**Sample usage:**

```cs
var isHeavyOrExpensiveFormula = UserDefinedExpression<SalesOrderHeader, bool>.Create("TotalDue > 1000 || Freight > 1000");
var order = new SalesOrderHeader() { TotalDue = 1200, Freight = 700 };
bool isHeavyOrExpensive = isHeavyOrExpensiveFormula.Invoke(order);
```