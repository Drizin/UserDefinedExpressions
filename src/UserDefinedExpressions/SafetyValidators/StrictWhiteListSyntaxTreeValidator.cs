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
    /// Walks through a single Syntax Tree (a single Expression/Formula) and checks if it's "safe" according to a white-list of acceptable tokens/statements/classes.
    /// </summary>
    public class StrictWhiteListSyntaxTreeValidator : ISafetyValidator
    {
        #region Members
        protected List<string> _allowedClasses;
        protected Microsoft.CodeAnalysis.Scripting.Script _script;
        protected SemanticModel _model;
        protected SyntaxNode _root;
        string _formula;
        WhiteListSyntaxTreeWalker _walker;
        #endregion

        public StrictWhiteListSyntaxTreeValidator()
        {
        }

        public void Validate(string formula, Microsoft.CodeAnalysis.Scripting.Script script, List<string> allowedClasses)
        {
            _allowedClasses = allowedClasses;
            _script = script;
            var actualTree = _script.GetCompilation().SyntaxTrees;
            _model = _script.GetCompilation().GetSemanticModel(actualTree.Single()); // formulas expect a single root element!
            _root = (CompilationUnitSyntax)_model.SyntaxTree.GetRoot();
            _formula = formula;
            _walker = new WhiteListSyntaxTreeWalker(formula, allowedClasses, _model);
            _walker.Visit(_root); // recursively visits every node of the syntax tree, and will throw exception if unsafe instruction is found
        }

        internal class WhiteListSyntaxTreeWalker : CSharpSyntaxWalker
        {
            #region Members
            protected int tabLevel = 0;
            protected List<string> _allowedClasses;
            protected SemanticModel _model;
            protected string _formula;
            #endregion
            internal WhiteListSyntaxTreeWalker(string formula, List<string> allowedClasses, SemanticModel model) : base(SyntaxWalkerDepth.StructuredTrivia)
            {
                _formula = formula;
                _allowedClasses = allowedClasses;
                _model = model;
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

                    // identifiers
                    case SyntaxKind.IdentifierName: // existing identifiers (variables) from current scope (this is safe)

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
                        break;

                    // this is where danger lives
                    case SyntaxKind.InvocationExpression:
                        {
                            var expression = ((InvocationExpressionSyntax)node).Expression as ExpressionSyntax;
                            ValidateExpression(node, expression);
                        }
                        break;


                    // is this dangerous?
                    case SyntaxKind.PointerMemberAccessExpression:
                    case SyntaxKind.SimpleMemberAccessExpression:
                        {
                            var expression = ((MemberAccessExpressionSyntax)node).Expression as ExpressionSyntax;
                            ValidateExpression(node, expression);
                        }
                        break;
                    case SyntaxKind.ElementAccessExpression:
                        {
                            var expression = ((ElementAccessExpressionSyntax)node).Expression as ExpressionSyntax;
                            ValidateExpression(node, expression);
                        }
                        break;
                    case SyntaxKind.ObjectCreationExpression:
                        {
                            var type = ((TypeInfo)_model.GetTypeInfo(node)).ConvertedType.ToString();
                            if (!_allowedClasses.Contains(type))
                                throw new UnsafeExpressionException(_formula, node);

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
                        throw new UnsafeExpressionException(_formula, node);
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
                var symbolInfo = _model.GetSymbolInfo(expressionSyntax);

                // roslyn may find the exact symbol or may be uncertain about a list of possible symbols that code refers to
                var possibleSymbols = new List<ISymbol>();
                if (symbolInfo.Symbol != null)
                    possibleSymbols.Add(symbolInfo.Symbol);
                else if (symbolInfo.CandidateSymbols != null)
                    possibleSymbols.AddRange(symbolInfo.CandidateSymbols);

                foreach (var symbol in possibleSymbols)
                {
                    if (_allowedClasses.Contains(symbol.ToString()))
                        continue;
                    if (_allowedClasses.Contains(symbol.ContainingSymbol.ToString()))
                        continue;
                    throw new UnsafeExpressionException(_formula, node, symbolInfo);
                }

            }
        }


    }

}
