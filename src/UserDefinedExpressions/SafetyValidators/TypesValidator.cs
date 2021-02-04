using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace UserDefinedExpressions.SafetyValidators
{
    /// <summary>
    /// Validates which types are allowed to be used
    /// </summary>
    public class TypesValidator
    {
        #region Members
        private HashSet<string> _allowedTypes;

        public static TypesValidatorDefaults Defaults = new TypesValidatorDefaults();

        private static Regex genericType = new Regex(
              "^(?<GenericType>[^<]*)<\r\n  (?<GenericArgs>.*)\r\n>$",
            RegexOptions.CultureInvariant
            | RegexOptions.IgnorePatternWhitespace
            | RegexOptions.Compiled
            );
        private static Regex splitGenericArgs = new Regex(
              "^\r\n  (?:\r\n    (?<GenericArg>\r\n    (\r\n      [^<>,\\s]\r\n      " +
              "|\r\n      (?<open> < ) # match '<' and increase 'open'\r\n     " +
              " |\r\n      (?<-open> > ) # match '>' and decrease 'open'\r\n   " +
              "   |\r\n      (?(open) (  [\\s|,]*) )   # if open, also ignore" +
              "s/skips commas and spaces\r\n    )+\r\n    )\r\n      (?(open) (?!" +
              ")  )   # fails if 'open' > 0\r\n    (?: (\\s*)(,)?(\\s*) )\r\n  " +
              "  ) *\r\n$",
            RegexOptions.CultureInvariant
            | RegexOptions.IgnorePatternWhitespace
            | RegexOptions.Compiled
            );


        #endregion

        #region ctors

        /// <summary>
        /// Initializes a TypesValidator with a specific list of allowed types
        /// </summary>
        /// <param name="allowedTypes"></param>
        public TypesValidator(List<string> allowedTypes)
        {
            _allowedTypes = new HashSet<string>(allowedTypes.ToList());
        }

        /// <summary>
        /// Initializes a TypesValidator with a default list of allowed types
        /// </summary>
        public TypesValidator()
        {
            _allowedTypes = new HashSet<string>(Defaults.AllowedTypes);
        }
        #endregion

        #region Inner Classes
        public class TypesValidatorDefaults
        {
            private List<string> _allowedTypes = new List<string>()
            {
                "string",
                "decimal",
                "double",
                "float",
                "int",
                "System.DateTime",
                "System.Linq.Enumerable",
                "System.Collections.Generic.Dictionary",
                "System.Collections.Generic.List",
            };

            /// <summary>
            /// Classes which are safe to use in expressions
            /// </summary>
            public IList<string> AllowedTypes { get { return _allowedTypes; } }

            /// <summary>
            /// Registers a new type as a valid type
            /// </summary>
            public void AddAllowedType(Type type)
            {
                _allowedTypes.Add(type.FullName.Replace("+", "."));
            }


        }

        #endregion

        /// <summary>
        /// Registers a new type as a valid type
        /// </summary>
        public void AddAllowedType(Type type)
        {
            _allowedTypes.Add(type.FullName.Replace("+", "."));
        }

        /// <summary>
        /// Checks if a type is safe
        /// </summary>
        public bool IsSafe(Type type)
        {
            return IsSafe(type.FullName);
        }

        /// <summary>
        /// Checks if a type is safe
        /// </summary>
        public bool IsSafe(string type)
        {
            if (_allowedTypes.Contains(type))
                return true;

            var match = genericType.Match(type);
            if (match.Success)
            {
                string genericType = match.Groups["GenericType"].Value;
                string genericArgs = match.Groups["GenericArgs"].Value;
                if (!IsSafe(genericType))
                    return false;
                var matchedArgs = splitGenericArgs.Match(genericArgs);
                if (matchedArgs.Success)
                {
                    foreach(Capture arg in matchedArgs.Groups["GenericArg"].Captures)
                    {
                        if (string.IsNullOrEmpty(arg.Value))
                            continue;
                        if (!IsSafe(arg.Value))
                            return false;
                    }
                }
                return true;
            }

            return false;
        }
    }
}
