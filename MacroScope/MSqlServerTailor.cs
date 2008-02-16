using System;

namespace MacroScope
{
    /// <summary>
    /// SQL for MS SQL Server.
    /// </summary>
    public class MSqlServerTailor : MTailor
    {
        #region Constructor

        public MSqlServerTailor():
            base(ExpressionOperator.Mod)
        {
        }

        #endregion

        #region IVisitor Members

        public override void PerformBefore(DbObject node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            if (!node.HasNext && TailorUtil.IsSysdate(node.Identifier))
            {
                Expression parent = Parent as Expression;
                if (parent != null)
                {
                    if (parent.Left == node)
                    {
                        parent.Left = new FunctionCall(TailorUtil.GETDATE.ToUpperInvariant());
                    }
                    else if (parent.Right == node)
                    {
                        parent.Right = new FunctionCall(TailorUtil.GETDATE.ToUpperInvariant());
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            "No object child in expression parent.");
                    }
                }
                else
                {
                    throw new InvalidOperationException(
                        "Date function not in expression.");
                }
            }

            base.PerformBefore(node);
        }

        public override void Perform(ExpressionOperator node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            base.Perform(node);

            if (node == ExpressionOperator.MAccessMod)
            {
                Expression parent = Parent as Expression;
                if (parent != null)
                {
                    parent.Operator = ExpressionOperator.Mod;
                }
                else
                {
                    throw new InvalidOperationException("Modulo operator not in expression.");
                }
            }
            else if (node == ExpressionOperator.StrConcat)
            {
                Expression parent = Parent as Expression;
                if (parent != null)
                {
                    parent.Operator = ExpressionOperator.Plus;
                }
                else
                {
                    throw new InvalidOperationException(
                        "String concatenation operator not in expression.");
                }
            }
        }

        public override void PerformBefore(FunctionCall node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            string name = node.Name.ToLowerInvariant();
            if (name.Equals(TailorUtil.NOW))
            {
                node.Name = TailorUtil.GETDATE.ToUpperInvariant();
            }

            base.PerformBefore(node);
        }

        public override void PerformAfter(FunctionCall node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            base.PerformAfter(node);

            if (TailorUtil.SUBSTRING.Equals(node.Name.ToLowerInvariant()))
            {
                Expression parent = Parent as Expression;
                if (parent != null)
                {
                    if (parent.Left == node)
                    {
                        parent.Left = MakeSubstring(node);
                    }
                    else if (parent.Right == node)
                    {
                        parent.Right = MakeSubstring(node);
                    }
                    else
                    {
                        throw new InvalidOperationException("No function call child in expression parent.");
                    }
                }
                else
                {
                    throw new InvalidOperationException("SUBSTRING not in expression.");
                }
            }
        }

        public override void Perform(Identifier node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            base.Perform(node);

            node.NormalizeQuotes('[');
        }

        public override void Perform(LiteralDateTime node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            base.Perform(node);

            Expression parent = Parent as Expression;
            if (parent != null)
            {
                if (parent.Left == node)
                {
                    parent.Left = MakeConvert(node);
                }
                else if (parent.Right == node)
                {
                    parent.Right = MakeConvert(node);
                }
                else
                {
                    throw new InvalidOperationException(
                        "No datetime literal child in expression parent.");
                }
            }
            else
            {
                // SQL Server 2005 does, but we want to be more general than that
                throw new InvalidOperationException(
                    "MS SQL Server does not necessarily have datetime literals.");
            }
        }

        public override void PerformBefore(SwitchFunction node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            base.PerformBefore(node);

            throw new InvalidOperationException(
                "Switch not in expression.");
        }

        #endregion

        #region Transformations

        protected override FunctionCall GetDateaddCall(DateTimeUnit unit,
            INode number, INode date)
        {
            if (unit == null)
            {
                throw new ArgumentNullException("unit");
            }

            if (number == null)
            {
                throw new ArgumentNullException("number");
            }

            if (date == null)
            {
                throw new ArgumentNullException("date");
            }

            if (unit == DateTimeUnit.Month)
            {
                throw new ArgumentNullException("Standard month arithmetic not supported.");
            }

            FunctionCall dateadd = new FunctionCall(TailorUtil.DATEADD.ToUpperInvariant());
            dateadd.ExpressionArguments = new ExpressionItem(unit);
            dateadd.ExpressionArguments.Add(new ExpressionItem(number));
            dateadd.ExpressionArguments.Add(new ExpressionItem(date));
            return dateadd;
        }

        INode MakeSubstring(FunctionCall substringCall)
        {
            if (substringCall == null)
            {
                throw new ArgumentNullException("substringCall");
            }

            if (TailorUtil.HasNullArgument(substringCall))
            {
                return NullValue.Value;
            }

            ExpressionItem val = substringCall.ExpressionArguments;
            if (val == null)
            {
                throw new InvalidOperationException("No parameters for SUBSTRING.");
            }

            ExpressionItem start = val.Next;
            if (start == null)
            {
                throw new InvalidOperationException("Too few parameters for SUBSTRING.");
            }

            ExpressionItem len = start.Next;

            // before adding len to the list
            Expression when = MakeNotNullCheck(substringCall.ExpressionArguments);

            if (len == null)
            {
                IExpression argument = TailorUtil.MakeLenArg(start.Expression,
                    val.Expression, "LEN");
                start.Add(new ExpressionItem(argument));
            }
            else
            {
                if (len.Next != null)
                {
                    throw new InvalidOperationException("Too many parameters for SUBSTRING.");
                }
            }

            if (when != null)
            {
                CaseExpression caseExpression = new CaseExpression();
                caseExpression.Alternatives = new CaseAlternative(when, substringCall);

                Expression elseExpr = new Expression();
                elseExpr.Left = NullValue.Value;
                caseExpression.Else = elseExpr;

                return caseExpression;
            }
            else
            {
                return substringCall;
            }
        }

        protected override FunctionCall ReplaceExtractFunction(
            ExtractFunction extractFunction)
        {
            if (extractFunction == null)
            {
                throw new ArgumentNullException("extractFunction");
            }

            FunctionCall functionCall = new FunctionCall("DATEPART");
            functionCall.ExpressionArguments = new ExpressionItem(extractFunction.FieldSpec);
            functionCall.ExpressionArguments.Add(new ExpressionItem(extractFunction.Source));
            return functionCall;
        }

        static Expression MakeNotNullCheck(ExpressionItem list)
        {
            Expression top = null;

            while (list != null)
            {
                INode term = TailorUtil.GetTerm(list.Expression);
                if (!(term is IntegerValue) && !(term is StringValue))
                {
                    Expression leaf = new Expression();
                    leaf.Left = term.Clone();
                    leaf.Operator = ExpressionOperator.IsNotNull;

                    if (top == null)
                    {
                        top = leaf;
                    }
                    else
                    {
                        top = new Expression(top, ExpressionOperator.Or, leaf);
                    }
                }

                list = list.Next;
            }

            return top;
        }

        static FunctionCall MakeConvert(LiteralDateTime literalDateTime)
        {
            if (literalDateTime == null)
            {
                throw new ArgumentNullException("literalDateTime");
            }

            DateTime dateTime = literalDateTime.DateTime;
            string literal = dateTime.ToString("yyyy-MM-dd HH:mm:ss");

            FunctionCall functionCall = new FunctionCall(
                TailorUtil.CONVERT.ToUpperInvariant());

            // Well, technically it's a type name, not a database ID, but
            // this is what the parser builds from "CONVERT(datetime)"
            // and it should serve just as well...
            functionCall.ExpressionArguments = new ExpressionItem(
                new DbObject(new Identifier("datetime")));

            functionCall.ExpressionArguments.Add(
                TailorUtil.MakeLiteralString(literal));
            functionCall.ExpressionArguments.Add(
                TailorUtil.MakeLiteralInteger(120));

            return functionCall;
        }

        #endregion
    }
}
