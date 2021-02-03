using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UserDefinedExpressions.SafetyValidators
{

    /// <summary>
    /// Validates if a User-Defined-Expression is safe to be executed
    /// </summary>
    public abstract class BaseValidator : ISafetyValidator
    {
        #region Members
        protected Microsoft.CodeAnalysis.Scripting.Script _script;
        protected SemanticModel _model;
        protected SyntaxNode _root;
        protected string _expression;
        #endregion

        /// <inheritdoc/>
        public BaseValidator(IUserDefinedExpression userDefinedExpression)
        {
            _script = userDefinedExpression.Script;
            var actualTree = _script.GetCompilation().SyntaxTrees;
            _model = _script.GetCompilation().GetSemanticModel(actualTree.Single()); // formulas expect a single root element!
            _root = (CompilationUnitSyntax)_model.SyntaxTree.GetRoot();
            _expression = userDefinedExpression.Expression;
        }

        /// <inheritdoc/>
        public abstract void Validate();

        /// <inheritdoc/>
        public abstract void SetInputType(Type inputType);
    }
}
