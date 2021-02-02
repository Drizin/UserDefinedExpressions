using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UserDefinedExpressions.SafetyValidators;

namespace UserDefinedExpressions
{
    public abstract class UserDefinedExpression
    {
        // This non-generic base type is basically to share static fields (defaults)
        #region Defaults (Globals)
        /// <summary>
        /// How expressions are checked for safety. Default is WhiteListSyntaxTree
        /// </summary>
        public static ISafetyValidator DefaultSafetyValidator { get; set; } = new StrictWhiteListSyntaxTreeValidator();
        
        /// <summary>
        /// Classes which are safe to use in expressions
        /// </summary>
        public static List<string> DefaultAllowedClasses = new List<string>()
        {
            "System.Collections.Generic.Dictionary<string, decimal>", //TODO: regex? or get the types of Symbols and compare with generic types?
            "System.Collections.Generic.Dictionary<string, string>",
            "string",
            "System.DateTime",
            "System.Linq.Enumerable",
        };
        #endregion
    }

    /// <summary>
    /// Given an INPUT model (variables which are available to my expression) and an OUTPUT (which is probably boolean for most cases), <br />
    /// this allows to have "safe" expressions/formulas (cannot access external variables)
    /// </summary>
    /// <typeparam name="I">Input type - properties which are available to be used in the expression/formula</typeparam>
    /// <typeparam name="O">Output type - which output is returned by the expression/formula</typeparam>
    [System.Diagnostics.DebuggerDisplay("{_expression}")]
    public class UserDefinedExpression<I, O> : UserDefinedExpression, IDisposable
    {
        #region Members
        private Microsoft.CodeAnalysis.Scripting.Script<O> _script;
        private Microsoft.CodeAnalysis.Scripting.ScriptRunner<O> _runner;
        private Func<I, O> _invoker;
        private string _expression;
        private static Dictionary<string, UserDefinedExpression<I, O>> cachedExpressions = new Dictionary<string, UserDefinedExpression<I, O>>();
        private ISafetyValidator _validator;
        #endregion

        #region ctors
        /// <summary>
        /// Creates a new evaluatable (reusable) User Defined Expression which receives an input I and returns an output type O.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="validator">If not provided will use the global <see cref="UserDefinedExpression.DefaultSafetyValidator"/></param>
        /// <returns></returns>
        public static UserDefinedExpression<I, O> Create(string expression, ISafetyValidator validator = null)
        {
            if (!cachedExpressions.ContainsKey(expression))
            {
                var userExpression = new UserDefinedExpression<I, O>(expression, validator);
                cachedExpressions[expression] = userExpression;
            }
            return cachedExpressions[expression];
        }
        private UserDefinedExpression(string expression, ISafetyValidator validator)
        {
            #region Script Options
            // https://stackoverflow.com/a/41356621/3606250
            var scriptOptions = ScriptOptions.Default;

            // Add reference to mscorlib
            var mscorlib = typeof(object).GetTypeInfo().Assembly;
            var systemCore = typeof(System.Linq.Enumerable).GetTypeInfo().Assembly;

            var references = new[] { mscorlib, systemCore };
            scriptOptions = scriptOptions.AddReferences(references);
            var interactiveLoader = new InteractiveAssemblyLoader();
            foreach (var reference in references)
                interactiveLoader.RegisterDependency(reference);
            // Add namespaces
            scriptOptions = scriptOptions.AddImports("System");
            scriptOptions = scriptOptions.AddImports("System.Linq");
            scriptOptions = scriptOptions.AddImports("System.Collections.Generic");
            #endregion

            DefaultAllowedClasses.Add(typeof(I).FullName); // allow to use members from the Input model
            _expression = expression;
            _script = CSharpScript.Create<O>(expression, scriptOptions, typeof(I), interactiveLoader);
            _validator = validator ?? DefaultSafetyValidator; // if null validator is provided, let's fallback to the default validator (to be on the safe side)

            // Validates if the expression is safe. Will throw UnsafeExpressionException if unsafe code is detected
            _validator.Validate(expression, _script, DefaultAllowedClasses);

            _runner = _script.CreateDelegate();

            _invoker = (input) => _runner(input).Result;
        }
        #endregion

        #region Public Methods
        public O Invoke(I input)
        {
            return _invoker(input);
        }
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects).
                    _invoker = null;
                    _script = null;
                    _runner = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~UserDefinedExpression()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion


    }
}
