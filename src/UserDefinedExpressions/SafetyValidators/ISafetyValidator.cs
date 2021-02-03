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
        void Validate();

        /// <summary>
        /// Sets the input type (input model available to be used in the expression), so that the validator will mark this type as safe
        /// </summary>
        /// <param name="inputType"></param>
        void SetInputType(Type inputType);
    }
}
