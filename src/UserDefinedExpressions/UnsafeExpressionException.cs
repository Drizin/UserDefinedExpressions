using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace UserDefinedExpressions
{
    /// <summary>
    /// 
    /// </summary>
    public class UnsafeExpressionException : Exception
    {
        public UnsafeExpressionException(string expression, SyntaxNode node, SymbolInfo symbolInfo) 
            : base($"ForbiddenCall: Cannot use {symbolInfo.Symbol.ContainingSymbol.ToString()} (location {node.GetLocation().GetLineSpan()}): {HighlighErrorInExpression(expression, node)}")
        {
        }
        public UnsafeExpressionException(string expression, SyntaxNode node)
            : base($"ForbiddenCall: Cannot use {node.Kind()} (location {node.GetLocation().GetLineSpan()}): {HighlighErrorInExpression(expression, node)}")
        {
        }

        protected static string HighlighErrorInExpression(string expression, SyntaxNode node)
        {
            Location location = node.GetLocation();
            TextSpan span = location.SourceSpan;
            string error = expression.Substring(span.Start, span.Length);
            return error;
        }
    }
}
