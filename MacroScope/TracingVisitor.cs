using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MacroScope
{
    /// <summary>
    /// Maintains backlinks to <see cref="INode"/> parent during
    /// traversal.
    /// </summary>
    public class TracingVisitor : PassiveVisitor
    {
        #region Fields

        private readonly Stack<INode> m_ancestors;

        #endregion

        #region Constructor

        public TracingVisitor()
        {
            m_ancestors = new Stack<INode>();
        }

        #endregion

        #region Parent maintenance

        /// <summary>
        /// Parent of the currently traversed node if it has one, null otherwise.
        /// </summary>
        public INode Parent
        {
            get
            {
                INode parent = null;
                if (m_ancestors.Count > 0)
                {
                    parent = m_ancestors.Peek();
                    Debug.Assert(parent != null);
                }

                return parent;
            }
        }

        public void PushParent(INode parent)
        {
            if (parent == null)
            {
                throw new ArgumentNullException("parent");
            }

            m_ancestors.Push(parent);
        }

        public INode PopParent()
        {
            if (m_ancestors.Count == 0)
            {
                throw new InvalidOperationException("No parent to pop.");
            }

            INode parent = m_ancestors.Pop();
            Debug.Assert(parent != null);
            return parent;
        }

        #endregion

        #region IVisitor Members

        public override void PerformBefore(AliasedItem node)
        {
            PushParent(node);
        }

        public override void PerformAfter(AliasedItem node)
        {
            PopKnownParent(node);
        }

        public override void PerformBefore(Assignment node)
        {
            PushParent(node);
        }

        public override void PerformAfter(Assignment node)
        {
            PopKnownParent(node);
        }

        public override void PerformBefore(BracketedExpression node)
        {
            PushParent(node);
        }

        public override void PerformAfter(BracketedExpression node)
        {
            PopKnownParent(node);
        }

        public override void PerformBefore(CaseAlternative node)
        {
            PushParent(node);
        }

        public override void PerformAfter(CaseAlternative node)
        {
            PopKnownParent(node);
        }

        public override void PerformBefore(CaseExpression node)
        {
            PushParent(node);
        }

        public override void PerformAfter(CaseExpression node)
        {
            PopKnownParent(node);
        }

        public override void PerformBefore(DbObject node)
        {
            PushParent(node);
        }

        public override void PerformAfter(DbObject node)
        {
            PopKnownParent(node);
        }

        public override void PerformBefore(DeleteStatement node)
        {
            PushParent(node);
        }

        public override void PerformAfter(DeleteStatement node)
        {
            PopKnownParent(node);
            Debug.Assert(m_ancestors.Count == 0);
        }

        public override void PerformBefore(Expression node)
        {
            PushParent(node);
        }

        public override void PerformAfter(Expression node)
        {
            PopKnownParent(node);
        }

        public override void PerformBefore(ExpressionItem node)
        {
            PushParent(node);
        }

        public override void PerformAfter(ExpressionItem node)
        {
            PopKnownParent(node);
        }

        public override void PerformBefore(ExtractFunction node)
        {
            PushParent(node);
        }

        public override void PerformAfter(ExtractFunction node)
        {
            PopKnownParent(node);
        }

        public override void PerformBefore(FunctionCall node)
        {
            PushParent(node);
        }

        public override void PerformAfter(FunctionCall node)
        {
            PopKnownParent(node);
        }

        public override void PerformBefore(GroupByClause node)
        {
            PushParent(node);
        }

        public override void PerformAfter(GroupByClause node)
        {
            PopKnownParent(node);
        }

        public override void PerformBefore(InsertStatement node)
        {
            PushParent(node);
        }

        public override void PerformAfter(InsertStatement node)
        {
            PopKnownParent(node);
            Debug.Assert(m_ancestors.Count == 0);
        }

        public override void PerformBefore(Interval node)
        {
            PushParent(node);
        }

        public override void PerformAfter(Interval node)
        {
            PopKnownParent(node);
        }

        public override void PerformBefore(OrderExpression node)
        {
            PushParent(node);
        }

        public override void PerformAfter(OrderExpression node)
        {
            PopKnownParent(node);
        }

        public override void PerformBefore(PatternExpression node)
        {
            PushParent(node);
        }

        public override void PerformAfter(PatternExpression node)
        {
            PopKnownParent(node);
        }

        public override void PerformBefore(PredicateExpression node)
        {
            PushParent(node);
        }

        public override void PerformAfter(PredicateExpression node)
        {
            PopKnownParent(node);
        }

        public override void PerformBefore(QueryExpression node)
        {
            PushParent(node);
        }

        public override void PerformAfter(QueryExpression node)
        {
            PopKnownParent(node);
        }

        public override void PerformBefore(Range node)
        {
            PushParent(node);
        }

        public override void PerformAfter(Range node)
        {
            PopKnownParent(node);
        }

        public override void PerformBefore(SelectStatement node)
        {
            PushParent(node);
        }

        public override void PerformAfter(SelectStatement node)
        {
            PopKnownParent(node);
        }

        public override void PerformBefore(SwitchFunction node)
        {
            PushParent(node);
        }

        public override void PerformAfter(SwitchFunction node)
        {
            PopKnownParent(node);
        }

        public override void PerformBefore(Table node)
        {
            PushParent(node);
        }

        public override void PerformAfter(Table node)
        {
            PopKnownParent(node);
        }

        public override void PerformBefore(TableWildcard node)
        {
            PushParent(node);
        }

        public override void PerformAfter(TableWildcard node)
        {
            PopKnownParent(node);
        }

        public override void PerformBefore(TypeCast node)
        {
            PushParent(node);
        }

        public override void PerformAfter(TypeCast node)
        {
            PopKnownParent(node);
        }

        public override void PerformBefore(UpdateStatement node)
        {
            PushParent(node);
        }

        public override void PerformAfter(UpdateStatement node)
        {
            PopKnownParent(node);
            Debug.Assert(m_ancestors.Count == 0);
        }

        #endregion

        #region Utilities

        void PopKnownParent(INode node)
        {
            INode parent = PopParent();
            Debug.Assert(node == parent);
        }

        #endregion
    }
}
