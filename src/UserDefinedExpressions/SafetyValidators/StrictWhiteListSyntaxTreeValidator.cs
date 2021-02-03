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
        private HashSet<string> _allowedClasses;
        #endregion

        #region ctors
        /// <inheritdoc/>
        public StrictWhiteListTypeValidator(IUserDefinedExpression userDefinedExpression, List<string> allowedClasses) : base(userDefinedExpression)
        {
            _allowedClasses = new HashSet<string>(allowedClasses.ToList());
        }

        /// <inheritdoc/>
        public StrictWhiteListTypeValidator(IUserDefinedExpression userDefinedExpression) : this(userDefinedExpression, UserDefinedExpression.DefaultAllowedClasses)
        {
        }
        #endregion

        #region overrides
        /// <inheritdoc/>
        public override bool IsSafeSymbol(ISymbol symbol, string identifierType, string identifierName, string containerType)
        {
            //return _allowedClasses.Contains(symbol.ToString()) || _allowedClasses.Contains(containerType);
            return _allowedClasses.Contains(containerType);
        }

        /// <inheritdoc/>
        public override bool IsSafeType(string typeFullName)
        {
            return _allowedClasses.Contains(typeFullName);
        }

        /// <inheritdoc/>
        public override void SetInputType(Type inputType)
        {
            _allowedClasses.Add(inputType.FullName);
        }
        #endregion
    }

}
