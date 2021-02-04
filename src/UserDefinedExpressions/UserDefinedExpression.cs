using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UserDefinedExpressions.SafetyValidators;

namespace UserDefinedExpressions
{
    public interface IUserDefinedExpression
    {
        string Expression { get; }
        Script Script { get; }
    }
    public abstract class UserDefinedExpression
    {
        // This non-generic base type is basically to share static fields (defaults)
        #region Defaults (Globals)
        /// <summary>
        /// How expressions are checked for safety. Default is WhiteListSyntaxTree
        /// </summary>
        public static Func<IUserDefinedExpression, ISafetyValidator> DefaultSafetyValidatorFactory { get; set; }
        #endregion
    }

    /// <summary>
    /// Given an INPUT model (variables which are available to my expression) and an OUTPUT (which is probably boolean for most cases), <br />
    /// this allows to have "safe" expressions/formulas (cannot access external variables)
    /// </summary>
    /// <typeparam name="TInput">Input type - properties which are available to be used in the expression/formula</typeparam>
    /// <typeparam name="TOutput">Output type - which output is returned by the expression/formula</typeparam>
    [System.Diagnostics.DebuggerDisplay("{_expression}")]
    public class UserDefinedExpression<TInput, TOutput> : UserDefinedExpression, IDisposable, IUserDefinedExpression
    {
        #region Members
        private Script<TOutput> _script;
        private ScriptRunner<TOutput> _runner;
        private Func<TInput, TOutput> _invoker;
        private string _expression;
        private static ConcurrentDictionary<string, UserDefinedExpression<TInput, TOutput>> cachedExpressions = new ConcurrentDictionary<string, UserDefinedExpression<TInput, TOutput>>();
        private ISafetyValidator _validator;

        public string Expression { get => _expression; }
        public Script Script { get => _script; }
        #endregion

        #region ctors
        /// <summary>
        /// Creates a new evaluatable (reusable) User Defined Expression which receives an input I and returns an output type O.
        /// If the same expression (with same input/output type) is used more than once it will load a cached instances.
        /// Will use the globally defined <see cref="UserDefinedExpression.DefaultSafetyValidatorFactory"/> or (if not defined) will fallback to the default StrictWhiteListSyntaxTreeValidator
        /// </summary>
        public static UserDefinedExpression<TInput, TOutput> Create(string expression)
        {
            return Create(expression, null);
        }
        /// <summary>
        /// Creates a new evaluatable (reusable) User Defined Expression which receives an input I and returns an output type O.
        /// If the same expression (with same input/output type) is used more than once it will load a cached instances.
        /// </summary>
        /// <param name="validatorFactory">Factory to create a new ISafetyValidator that will be used to check if the expression is safe</param>
        public static UserDefinedExpression<TInput, TOutput> Create(string expression, Func<IUserDefinedExpression, ISafetyValidator> validatorFactory)
        {
            if (!cachedExpressions.ContainsKey(expression))
            {
                var userExpression = new UserDefinedExpression<TInput, TOutput>(expression, validatorFactory);
                cachedExpressions[expression] = userExpression;
            }
            return cachedExpressions[expression];
        }
        private UserDefinedExpression(string expression, Func<IUserDefinedExpression, ISafetyValidator> validatorFactory)
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

            _expression = expression;
            _script = CSharpScript.Create<TOutput>(expression, scriptOptions, typeof(TInput), interactiveLoader);
            _validator = 
                validatorFactory?.Invoke(this) // if validator was explicitly passed
                ?? DefaultSafetyValidatorFactory?.Invoke(this) // else, if there's a default validator factory
                ?? new StrictWhiteListTypeValidator(this); // else, use this as default validator (to be on the safe side)
            _validator.SetInputType(typeof(TInput)); // allow to use members from the Input model

            // Validates if the expression is safe. Will throw UnsafeExpressionException if unsafe code is detected
            _validator.Validate();

            _runner = _script.CreateDelegate();

            _invoker = (input) => _runner(input).Result;
        }
        #endregion

        #region Public Methods
        public TOutput Invoke(TInput input)
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
