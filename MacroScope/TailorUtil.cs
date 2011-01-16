using System;

namespace MacroScope
{
    /// <summary>
    /// Tailor support.
    /// </summary>
    /// <remarks>
    /// Note that all strings defined by this class are expected to be in lowercase.
    /// </remarks>
    public static class TailorUtil
    {
        #region Well-known identifiers

        /// <summary>
        /// A function defined in SQL 92, not known by MS Access.
        /// </summary>
        public static readonly string COALESCE = "coalesce";

        /// <summary>
        /// String concatenation function used by MySQL.
        /// </summary>
        public static readonly string CONCAT = "concat";

        /// <summary>
        /// A function with different semantics in MS SQL Server and Oracle,
        /// not known by MS Access.
        /// </summary>
        public static readonly string CONVERT = "convert";

        /// <summary>
        /// The standard way to get the current time.
        /// </summary>
        public static readonly string CURRENT_TIMESTAMP = "CURRENT_TIMESTAMP";

        /// <summary>
        /// A pseudo-table used by Oracle, not known by MS engines.
        /// </summary>
        public static readonly string DUAL = "dual";

        /// <summary>
        /// A MS date function, not known by Oracle.
        /// </summary>
        public static readonly string DATEADD = "dateadd";

        /// <summary>
        /// A MS SQL Server function returning the current time, not
        /// known by other engines.
        /// </summary>
        public static readonly string GETDATE = "getdate";

        /// <summary>
        /// A MS string transformation function (also an SQL keyword,
        /// but this declaration isn't used for that meaning), not known
        /// by Oracle.
        /// </summary>
        public static readonly string LEFT = "left";

        /// <summary>
        /// A MS Access string transformation function, not
        /// known by other engines.
        /// </summary>
        public static readonly string MID = "mid";

        /// <summary>
        /// An Oracle function and MS Access operator, spelled '%' by MS SQL Server.
        /// </summary>
        public static readonly string MOD = "mod";

        /// <summary>
        /// A MS Access function returning the current time, not
        /// known by other engines.
        /// </summary>
        public static readonly string NOW = "now";

        /// <summary>
        /// A MS string transformation function (also an SQL keyword,
        /// but this declaration isn't used for that meaning), not known
        /// by Oracle.
        /// </summary>
        public static readonly string RIGHT = "right";

        /// <summary>
        /// ROWNUM is an Oracle pseudo-column used to limit
        /// the number of returned rows (analogically to Microsoft TOP).
        /// </summary>
        public static readonly string ROWNUM = "rownum";

        /// <summary>
        /// An Oracle keyword (some documentation calls it "function",
        /// but syntactically it isn't) used to get the current
        /// time, not known by MS engines.
        /// </summary>
        public static readonly string SYSDATE = "sysdate";

        /// <summary>
        /// An Oracle string transformation function, not known
        /// by MS engines.
        /// </summary>
        public static readonly string SUBSTR = "substr";


        /// <summary>Jet locate function. </summary>
        /// <remarks>
        ///    	SQL:LOCATE( stringToFind, stringToSearch  [, start] )
        ///     ACCESS: Instr ( [start], stringToSearch, stringToFind)
        /// </remarks>
        public static readonly string LOCATE = "locate";

        /// <summary>
        /// Standard string transformation function, not known
        /// by MS Access and Oracle.
        /// </summary>
        public static readonly string SUBSTRING = "substring";

        /// <summary>
        /// Standard keyword defining a data type, not known by MS engines.
        /// </summary>
        public static readonly string TIMESTAMP = "timestamp";

        #endregion

        #region Utility functions

        /// <summary>
        /// Gets the limit on query rows from a comparison of <see cref="ROWNUM"/>
        /// with a constant number.
        /// </summary>
        /// <param name="expr">The condition limiting rows returned by a query.</param>
        /// <returns>The limit on query rows, or (if <paramref name="expr"/> isn't
        /// a comparison of <see cref="ROWNUM"/> with a constant number) -1.</returns>
        public static int GetRownumExpressionLimit(Expression expr)
        {
            if (expr == null)
            {
                throw new ArgumentNullException("expr");
            }

            decimal limit = -1;
            if (expr.Operator == ExpressionOperator.LessOrEqual)
            {
                if (IsRownumTerm(expr.Left))
                {
                    limit = GetTermLimit(expr.Right);
                }
            }
            else if (expr.Operator == ExpressionOperator.Less)
            {
                if (IsRownumTerm(expr.Left))
                {
                    limit = GetSharpTermLimit(expr.Right);
                }
            }
            else if (expr.Operator == ExpressionOperator.GreaterOrEqual)
            {
                if (IsRownumTerm(expr.Right))
                {
                    limit = GetTermLimit(expr.Left);
                }
            }
            else if (expr.Operator == ExpressionOperator.Greater)
            {
                if (IsRownumTerm(expr.Right))
                {
                    limit = GetSharpTermLimit(expr.Left);
                }
            }

            int ilimit = (int)limit;
            if (limit != ilimit)
            {
                string message = string.Format("TOP argument {0} too large.",
                    limit);
                throw new Exception(message);
            }

            return ilimit;
        }

        public static bool HasNullArgument(FunctionCall functionCall)
        {
            if (functionCall == null)
            {
                throw new ArgumentNullException("functionCall");
            }

            ExpressionItem argument = functionCall.ExpressionArguments;
            while (argument != null)
            {
                INode term = GetTerm(argument.Expression);
                if (term == NullValue.Value)
                {
                    return true;
                }

                argument = argument.Next;
            }

            return false;
        }

        /// <summary>
        /// Test for unquoted <see cref="ROWNUM"/>.
        /// </summary>
        /// <remarks>
        /// Not canonicalizing - we're accepting quoted "rownum"
        /// as a regular identifier.
        /// </remarks>
        public static bool IsRownum(Identifier identifier)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException("identifier");
            }

            return ROWNUM.Equals(
                identifier.ID.ToLowerInvariant());
        }

        /// <summary>
        /// Test for unquoted <see cref="SYSDATE"/>.
        /// </summary>
        /// <remarks>
        /// Not canonicalizing - we're accepting quoted "sysdate"
        /// as a regular identifier.
        /// </remarks>
        public static bool IsSysdate(Identifier identifier)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException("identifier");
            }

            return SYSDATE.Equals(
                identifier.ID.ToLowerInvariant());
        }

        public static Interval GetInterval(INode arg)
        {
            Expression expr = arg as Expression;
            while ((expr != null) && (expr.Operator == null))
            {
                arg = (expr.Left != null) ? expr.Left : expr.Right;
                expr = arg as Expression;
            }

            return arg as Interval;
        }

        public static FunctionCall CondGetFunctionCall(INode arg)
        {
            if (arg == null)
            {
                return null;
            }

            arg = GetTerm(arg);
            return arg as FunctionCall;
        }

        static bool IsRownumTerm(INode arg)
        {
            arg = GetComparedTerm(arg);

            DbObject dbObject = arg as DbObject;
            if ((dbObject == null) || dbObject.HasNext)
            {
                return false;
            }

            return IsRownum(dbObject.Identifier);
        }

        static decimal GetSharpTermLimit(INode arg)
        {
            decimal limit = GetTermLimit(arg);
            if (limit > 0)
            {
                --limit;
            }

            return limit;
        }

        static decimal GetTermLimit(INode arg)
        {
            arg = GetComparedTerm(arg);

            IntegerValue iv = arg as IntegerValue;
            if (iv == null)
            {
                return -1;
            }

            return iv.Value;
        }

        static INode GetComparedTerm(INode arg)
        {
            if (arg == null)
            {
                throw new InvalidOperationException(
                    "Comparison operator missing argument.");
            }

            return GetTerm(arg);
        }

        public static INode GetTerm(INode arg)
        {
            if (arg == null)
            {
                throw new ArgumentNullException("arg");
            }

            Expression expr = arg as Expression;
            while ((expr != null) && (expr.Operator == null))
            {
                arg = (expr.Left != null) ? expr.Left : expr.Right;
                expr = arg as Expression;
            }

            return arg;
        }

        /// <summary>
        /// Constructs the last argument for substring functions.
        /// </summary>
        /// <param name="startExpr">
        /// First argument of the substring function
        /// (<c>ExpressionItem.Expression</c>, not the top-level <c>ExpressionItem</c>).
        /// </param>
        /// <param name="valExpr">
        /// Second argument of the substring function
        /// (<c>ExpressionItem.Expression</c>, not the top-level <c>ExpressionItem</c>).
        /// </param>
        /// <param name="lenName">
        /// Name of the length function (must not be null).
        /// </param>
        public static IExpression MakeLenArg(INode startExpr, INode valExpr,
            string lenName)
        {
            if (startExpr == null)
            {
                throw new ArgumentNullException("startExpr");
            }

            if (valExpr == null)
            {
                throw new ArgumentNullException("valExpr");
            }

            if (lenName == null)
            {
                throw new ArgumentNullException("lenName");
            }

            FunctionCall lenCall = new FunctionCall(lenName);
            lenCall.ExpressionArguments = new ExpressionItem(valExpr.Clone());

            IExpression argument;
            INode startTerm = GetTerm(startExpr);
            IntegerValue integerValue = startTerm as IntegerValue;
            if ((integerValue != null) && (integerValue.Value == 1))
            {
                argument = lenCall;
            }
            else
            {
                Expression left = new Expression(lenCall,
                    ExpressionOperator.Plus,
                    new IntegerValue(1));
                argument = new Expression(left,
                    ExpressionOperator.Minus,
                    startTerm.Clone());
            }

            return argument;
        }

        public static ExpressionItem MakeLiteralString(string literal)
        {
            if (literal == null)
            {
                throw new ArgumentNullException("literal");
            }

            StringValue stringValue = new StringValue(literal);
            return new ExpressionItem(stringValue);
        }

        public static ExpressionItem MakeLiteralInteger(int literal)
        {
            IntegerValue integerValue = new IntegerValue(literal);
            return new ExpressionItem(integerValue);
        }

        public static string GetCapitalized(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }

            if (s.Equals(""))
            {
                throw new ArgumentException("Empty input.", "s");
            }

            char head = char.ToUpperInvariant(s[0]);
            if (s.Length == 1)
            {
                return char.ToString(head);
            }

            string tail = s.Substring(1);
            tail = tail.ToLowerInvariant();

            return string.Format("{0}{1}", head, tail);
        }

        #endregion
    }
}
