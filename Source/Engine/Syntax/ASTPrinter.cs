using System.Text;

namespace NodeJS.Engine.Syntax {
    internal class ASTPrinter : Expression.IVisitor<string> {
        internal string Print(Expression expression) {
            return expression.Accept(this);
        }

        public string VisitBinaryExpression(Expression.Binary expression) {
            return Parenthesize(expression.Operator.Lexeme, expression.Left, expression.Right);
        }

        public string VisitGroupingExpression(Expression.Grouping expression) {
            return Parenthesize("group", expression.Expression);
        }

        public string VisitLiteralExpression(Expression.Literal expression) {
            if (expression.Value == null) return "null";
            return expression.Value.ToString();
        }

        public string VisitUnaryExpression(Expression.Unary expression) {
            return Parenthesize(expression.Operator.Lexeme, expression.Right);
        }

        internal string Parenthesize(string name, params Expression[] expressions) {
            StringBuilder builder = new StringBuilder();

            builder.Append('(').Append(name);
            foreach (Expression expression in expressions) {
                builder.Append(' ');
                builder.Append(expression.Accept(this));
            }
            builder.Append(')');

            return builder.ToString();
        }

        public string VisitAssignExpression(Expression.Assign expression) {
            throw new System.NotImplementedException();
        }

        public string VisitVariableExpression(Expression.Variable expression) {
            throw new System.NotImplementedException();
        }

        public string VisitLogicalExpression(Expression.Logical expression) {
            throw new System.NotImplementedException();
        }

        public string VisitCallExpression(Expression.Call expression) {
            throw new System.NotImplementedException();
        }

        public string VisitGetExpression(Expression.Get expression) {
            throw new System.NotImplementedException();
        }

        public string VisitSetExpression(Expression.Set expression) {
            throw new System.NotImplementedException();
        }

        public string VisitThisExpression(Expression.This expression) {
            throw new System.NotImplementedException();
        }

        public string VisitSuperExpression(Expression.Super expression) {
            throw new System.NotImplementedException();
        }
    }
}