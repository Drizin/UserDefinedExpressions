using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Text;

namespace UserDefinedExpressions.SafetyValidators
{
    /// <summary>
    /// Doesn't check the expression at all. 
    /// This is very unsafe - end-users might write malicious code! 
    /// They could for example use System.IO, or System.Web.HttpContext, etc.
    /// </summary>
    [Obsolete("This is very unsafe end-users might write malicious code!")]
    public class UnsafeNoChecksValidator : ISafetyValidator
    {
        /// <inheritdoc/>
        public void Validate()
        {
        }
        /// <inheritdoc/>
        public void SetInputType(Type inputType)
        {
        }
    }
}
