using System;
using System.Collections.Generic;
using System.Text;

namespace UserDefinedExpressions.SafetyValidators
{
    /// <summary>
    /// Validates if a User-Defined-Expression is safe to be executed
    /// </summary>
    public interface ISafetyValidator
    {
        /// <summary>
        /// Validates if the expression is safe. Will throw UnsafeFormulaException if unsafe code is detected
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="script"></param>
        /// <param name="allowedClasses"></param>
        void Validate(string expression, Microsoft.CodeAnalysis.Scripting.Script script, List<string> allowedClasses);
    }
}
