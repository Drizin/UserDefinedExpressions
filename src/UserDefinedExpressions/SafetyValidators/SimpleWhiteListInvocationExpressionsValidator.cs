using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UserDefinedExpressions.SafetyValidators
{
    /// <summary>
    /// Check only nodes of type InvocationExpression and test them against a white list of allowed classes.
    /// This is SOMEHOW safe, but does NOT check for other calls (like reading Properties, etc)
    /// </summary>
    public class SimpleWhiteListValidator : BaseValidator
    {
        #region Members
        TypesValidator _typesValidator;
        #endregion

        #region ctors
        /// <inheritdoc/>
        public SimpleWhiteListValidator(IUserDefinedExpression userDefinedExpression, TypesValidator typesValidator) : base(userDefinedExpression)
        {
            _typesValidator = typesValidator;
        }

        /// <inheritdoc/>
        public SimpleWhiteListValidator(IUserDefinedExpression userDefinedExpression) : base(userDefinedExpression)
        {
            _typesValidator = new TypesValidator();
        }
        #endregion


        #region overrides
        /// <inheritdoc/>
        public override void Validate()
        {
            var allInstructions = _root.DescendantNodes().ToList(); // if we wanted to explore whole tree by types (without traversing tree)
                                                                    // based on https://github.com/dotnet/roslyn/issues/10830#issuecomment-554909161
            var invocationExpressions = _root.DescendantNodes().Where(i => i.IsKind(SyntaxKind.InvocationExpression)).OfType<InvocationExpressionSyntax>();
            //bool forbiddenCall = false;
            //Type[] _allowedTypes = new Type[] { };
            foreach (var invocationExpression in invocationExpressions)
            {
                var memberAccessExpressionSyntax = invocationExpression.Expression as MemberAccessExpressionSyntax;
                var symbolInfo = _model.GetSymbolInfo(memberAccessExpressionSyntax);
                if (symbolInfo.Symbol != null)
                {
                    if (!_typesValidator.IsSafe(symbolInfo.Symbol.ContainingSymbol.ToString()))
                    {
                        //forbiddenCall = true;
                        throw new UnsafeExpressionException(_expression, invocationExpression, symbolInfo);
                    }
                }
                else if (symbolInfo.CandidateSymbols != null)
                {
                    foreach (var symbol in symbolInfo.CandidateSymbols) // if any ambiguity in the method, candidates are here. Surprisingly, for the actual execution roslyn has no pb choosing the right one
                    {
                        if (!_typesValidator.IsSafe(symbol.ContainingSymbol.ToString()))
                        {
                            //forbiddenCall = true;
                            throw new UnsafeExpressionException(_expression, invocationExpression, symbolInfo);
                        }
                    }
                }
            }

        }

        /// <inheritdoc/>
        public override void SetInputType(Type inputType)
        {
            _typesValidator.AddAllowedType(inputType);
        }

        #endregion

    }
}
