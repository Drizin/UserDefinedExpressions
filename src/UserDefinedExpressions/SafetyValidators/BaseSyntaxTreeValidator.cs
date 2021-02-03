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
    /// Walks through the whole Syntax Tree (a single tree, so it's a single Expression/Formula) 
    /// and checks if it's "safe" according to abstract methods implementation <see cref="IsSafeSymbol(ISymbol, string, string, string)"/> and <see cref="IsSafeType(string)"/>.
    /// </summary>
    public abstract class BaseSyntaxTreeValidator : BaseValidator
    {

        #region Members
        SyntaxTreeWalker _walker;
        #endregion

        #region ctor
        /// <inheritdoc/>
        public BaseSyntaxTreeValidator(IUserDefinedExpression userDefinedExpression) : base(userDefinedExpression) 
        {
            _walker = new SyntaxTreeWalker(this, this._expression, _model);
        }
        #endregion

        #region Validate
        /// <inheritdoc/>
        public override void Validate()
        {
            _walker.Visit(_root); // recursively visits every node of the syntax tree, and will throw exception if unsafe instruction is found
        }
        #endregion

        #region Abstract Methods
        /// <summary>
        /// Validates if symbol is safe.
        /// </summary>
        public abstract bool IsSafeSymbol(ISymbol symbol, string identifierType, string identifierName, string containerType);

        /// <summary>
        /// Validates if type is safe (e.g. to instantiate)
        /// </summary>
        public abstract bool IsSafeType(string typeFullName);
        #endregion

        #region internal class WhiteListSyntaxTreeWalker
        internal class SyntaxTreeWalker : CSharpSyntaxWalker
        {
            #region Members
            protected int tabLevel = 0;
            protected SemanticModel _model;
            protected string _expression;
            protected BaseSyntaxTreeValidator _validator;
            #endregion
            internal SyntaxTreeWalker(BaseSyntaxTreeValidator validator, string expression, SemanticModel model) : base(SyntaxWalkerDepth.StructuredTrivia)
            {
                _expression = expression;
                _model = model;
                _validator = validator;
            }
            public override void Visit(SyntaxNode node)
            {
                SyntaxKind kind = node.Kind();
                switch (kind)
                {
                    // root expression
                    case SyntaxKind.GlobalStatement:
                    case SyntaxKind.CompilationUnit:

                    // expressions
                    case SyntaxKind.ExpressionStatement: // expressions like &&, ||, etc, and derived expressions like BinaryExpressions (==, >=, etc)
                    case SyntaxKind.ParenthesizedExpression: // parentheses around expressions
                        break;


                    // Binary Comparison Operators
                    case SyntaxKind.LogicalOrExpression:
                    case SyntaxKind.LogicalAndExpression:
                    case SyntaxKind.EqualsExpression:
                    case SyntaxKind.NotEqualsExpression:
                    case SyntaxKind.LessThanExpression:
                    case SyntaxKind.LessThanOrEqualExpression:
                    case SyntaxKind.GreaterThanExpression:
                    case SyntaxKind.GreaterThanOrEqualExpression:
                    case SyntaxKind.BitwiseAndExpression:
                    case SyntaxKind.BitwiseOrExpression:
                    case SyntaxKind.BitwiseNotExpression:
                    case SyntaxKind.ExclusiveOrExpression:

                    // math operators
                    case SyntaxKind.SubtractExpression:
                    case SyntaxKind.AddExpression:
                    case SyntaxKind.MultiplyExpression:
                    case SyntaxKind.DivideExpression:

                    // literals
                    case SyntaxKind.NumericLiteralExpression:
                    case SyntaxKind.StringLiteralExpression:
                    case SyntaxKind.TrueLiteralExpression:
                    case SyntaxKind.FalseLiteralExpression:
                    case SyntaxKind.CharacterLiteralExpression:
                    case SyntaxKind.NullLiteralExpression:

                    // lambdas
                    case SyntaxKind.SimpleLambdaExpression:

                    // arguments
                    case SyntaxKind.Parameter:
                    case SyntaxKind.ParameterList:
                    case SyntaxKind.TypeParameterList:
                    case SyntaxKind.Argument:
                    case SyntaxKind.ArgumentList:
                    case SyntaxKind.BracketedArgumentList:
                    case SyntaxKind.BracketedParameterList:
                        {
                            var typeInfo = _model.GetTypeInfo(node);
                            var symbolInfo = _model.GetSymbolInfo(node);
                            System.Diagnostics.Debug.WriteLine($"\"{node}\": {kind} - GetTypeInfo(node).Type:{typeInfo.Type}, symbolInfo:{symbolInfo.Symbol}");
                        }
                        break;

                    // identifiers
                    case SyntaxKind.IdentifierName: // existing identifiers (variables) from current scope (this is safe)
                        ValidateIdentifier(node);
                        break;

                    // this is where danger lives
                    case SyntaxKind.InvocationExpression:
                        {
                            var expression = ((InvocationExpressionSyntax)node).Expression as ExpressionSyntax;
                            //ValidateExpression(node, expression);
                            ValidateIdentifier(node);
                        }
                        break;


                    // is this dangerous?
                    case SyntaxKind.PointerMemberAccessExpression:
                    case SyntaxKind.SimpleMemberAccessExpression:
                        {
                            //var expression = ((MemberAccessExpressionSyntax)node).Expression as ExpressionSyntax;
                            //ValidateExpression(node, expression);
                            ValidateIdentifier(node);
                        }
                        break;
                    case SyntaxKind.ElementAccessExpression:
                        {
                            var expression = ((ElementAccessExpressionSyntax)node).Expression as ExpressionSyntax;
                            //ValidateExpression(node, expression);
                            ValidateIdentifier(node);
                        }
                        break;
                    case SyntaxKind.ObjectCreationExpression:
                        {
                            var typeFullName = ((TypeInfo)_model.GetTypeInfo(node)).ConvertedType.ToString();
                            if (!_validator.IsSafeType(typeFullName))
                                throw new UnsafeExpressionException(_expression, node);
                            //var ins = ((ObjectCreationExpressionSyntax)node).Type as IdentifierNameSyntax;

                            //if (ins != null)
                            //{
                            //    System.Diagnostics.Debug.WriteLine(ins.Identifier.ToString());
                            //    break;
                            //}
                            //var ns = ((ObjectCreationExpressionSyntax)node).Type as NameSyntax;
                            //if (ns != null)
                            //{
                            //    //return ns.Identifier.ToString();
                            //}

                            //var pts = ((ObjectCreationExpressionSyntax)node).Type as PredefinedTypeSyntax;
                            //if (pts != null)
                            //{
                            //    //return pts.Keyword.ToString();
                            //}
                            //throw new UnsafeFormulaException(node);
                        }
                        break;

                    default:
                        System.Diagnostics.Debug.WriteLine(kind.ToString());
                        System.Diagnostics.Debug.WriteLine(node);
                        if (System.Diagnostics.Debugger.IsAttached)
                            System.Diagnostics.Debugger.Break();
                        // let's stay on the safe side.. do not allow any unknown instructions
                        throw new UnsafeExpressionException(_expression, node);
                }

                tabLevel++;
                var indents = new String('\t', tabLevel);
                System.Diagnostics.Debug.WriteLine(indents + node.Kind());
                base.Visit(node);
                tabLevel--;
            }
            public override void VisitToken(SyntaxToken token)
            {
                var indents = new String('\t', tabLevel);
                System.Diagnostics.Debug.WriteLine(indents + token);
                base.VisitToken(token);
            }
            void ValidateExpression(SyntaxNode node, ExpressionSyntax expressionSyntax)
            {
                var typeInfo = _model.GetTypeInfo(node);
                var symbolInfo = _model.GetSymbolInfo(expressionSyntax);

                string identifierType = typeInfo.Type.ToString(); // e.g. UserDefinedExpressions.Tests.AventureWorksEntities.Customer

                // roslyn may find the exact symbol or may be uncertain about a list of possible symbols that code refers to
                var possibleSymbols = GetPossibleSymbols(symbolInfo); 

                foreach (var symbol in possibleSymbols)
                {
                    string identifierName = symbol.Name;
                    string containerType = symbol.ContainingType?.ToString() // e.g. UserDefinedExpressions.Tests.AventureWorksEntities.SalesOrderHeader
                        ?? symbol.ToString(); // if symbol is a type, there's no ContainingType
                    if (symbol.ContainingType == null && System.Diagnostics.Debugger.IsAttached)
                    {
                        if ((symbol as INamespaceOrTypeSymbol) != null && ((INamespaceOrTypeSymbol)symbol).IsType) { } else System.Diagnostics.Debugger.Break();
                    }

                    if (_validator.IsSafeSymbol(symbol, identifierType, identifierName, containerType))
                        continue;
                    throw new UnsafeExpressionException(_expression, node, symbolInfo);
                }
            }
            void ValidateIdentifier(SyntaxNode node)
            {
                var typeInfo = _model.GetTypeInfo(node);
                var symbolInfo = _model.GetSymbolInfo(node);

                string identifierType = typeInfo.Type?.ToString(); // e.g. UserDefinedExpressions.Tests.AventureWorksEntities.Customer
                //var identifier = ((IdentifierNameSyntax)node).Identifier;

                // roslyn may find the exact symbol or may be uncertain about a list of possible symbols that code refers to
                var possibleSymbols = GetPossibleSymbols(symbolInfo);
                
                foreach (var symbol in possibleSymbols)
                {
                    string identifierName = symbol.Name; // identifier.ValueText; 
                    string containerType = symbol.ContainingType?.ToString() // e.g. UserDefinedExpressions.Tests.AventureWorksEntities.SalesOrderHeader
                        ?? symbol.ToString(); // if symbol is a type, there's no ContainingType
                    if (symbol.ContainingType == null && System.Diagnostics.Debugger.IsAttached)
                    {
                        if ((symbol as INamespaceOrTypeSymbol) != null && ((INamespaceOrTypeSymbol)symbol).IsType) { } else System.Diagnostics.Debugger.Break();
                    }
                        

                    if (_validator.IsSafeSymbol(symbol, identifierType, identifierName, containerType))
                        continue;
                    throw new UnsafeExpressionException(_expression, node, symbolInfo);
                }

                System.Diagnostics.Debug.WriteLine($"\"{node}\": GetTypeInfo(node).Type:{typeInfo.Type}, symbolInfo:{symbolInfo.Symbol}");
            }
            List<ISymbol> GetPossibleSymbols(SymbolInfo symbolInfo)
            {
                var possibleSymbols = new List<ISymbol>();
                if (symbolInfo.Symbol != null)
                    possibleSymbols.Add(symbolInfo.Symbol);
                else if (symbolInfo.CandidateSymbols != null)
                    possibleSymbols.AddRange(symbolInfo.CandidateSymbols);
                return possibleSymbols;
            }
        }
        #endregion


    }
}
