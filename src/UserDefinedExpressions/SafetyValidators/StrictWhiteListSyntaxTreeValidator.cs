using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UserDefinedExpressions.SafetyValidators
{
    /// <summary>
    /// Walks through the whole Syntax Tree (a single tree, so it's a single Expression/Formula) 
    /// and checks if it's "safe" according to a white-list of acceptable types
    /// </summary>
    public class StrictWhiteListTypeValidator : BaseSyntaxTreeValidator
    {
        #region Members
        TypesValidator _typesValidator;
        #endregion

        #region ctors
        /// <inheritdoc/>
        public StrictWhiteListTypeValidator(IUserDefinedExpression userDefinedExpression, TypesValidator typesValidator) : base(userDefinedExpression)
        {
            _typesValidator = typesValidator;
        }

        /// <inheritdoc/>
        public StrictWhiteListTypeValidator(IUserDefinedExpression userDefinedExpression) : base(userDefinedExpression)
        {
            _typesValidator = new TypesValidator();
        }
        #endregion

        #region overrides
        /// <inheritdoc/>
        public override bool IsSafeSymbol(ISymbol symbol, string identifierType, string identifierName, string containerType)
        {
            //Type t = Type.GetType("System.Collections.Generic.List<System.DateTime>");
            //return _allowedClasses.Contains(symbol.ToString()) || _allowedClasses.Contains(containerType);
            if (symbol.IsImplicitlyDeclared && _typesValidator.IsSafe(identifierType)) // if it's a lambda variable and we're acessing an allowed type
                return true;

            if (_typesValidator.IsSafe(containerType) && !string.IsNullOrEmpty(identifierType) && !_typesValidator.IsSafe(identifierType)) // just checking if we should test 
                if (System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debugger.Break();
            return _typesValidator.IsSafe(containerType); // if we're acessing some property from an allowed type
        }

        /// <inheritdoc/>
        public override bool IsSafeType(string typeFullName)
        {
            return _typesValidator.IsSafe(typeFullName);
        }

        /// <inheritdoc/>
        public override void SetInputType(Type inputType)
        {
            _typesValidator.AddAllowedType(inputType);
        }
        #endregion
    }

}
