using System.Collections.Generic;

namespace NodeJS.Engine.Syntax {
	internal abstract class Expression {
		internal interface IVisitor<T> {
			T VisitAssignExpression(Assign expression);
			T VisitBinaryExpression(Binary expression);			
			T VisitCallExpression(Call expression);
			T VisitGetExpression(Get expression);
			T VisitGroupingExpression(Grouping expression);
			T VisitLiteralExpression(Literal expression);
			T VisitLogicalExpression(Logical expression);
			T VisitSetExpression(Set expression);
			T VisitSuperExpression(Super expression);
			T VisitThisExpression(This expression);
			T VisitUnaryExpression(Unary expression);
			T VisitVariableExpression(Variable expression);
		}

		internal class Assign : Expression {
			private readonly Token name;
			private readonly Expression value;

			internal Assign(Token name, Expression value) {
				this.name = name;
				this.value = value;
			}

            internal Token Name => name;

            internal Expression Value => value;

            internal override T Accept<T>(IVisitor<T> visitor) {
				return visitor.VisitAssignExpression(this);
			}
		}

		internal class Binary : Expression {
			private readonly Expression left;
			private readonly Token @operator;
			private readonly Expression right;

			internal Binary(Expression left, Token @operator, Expression right) {
				this.left = left;
				this.@operator = @operator;
				this.right = right;
			}

            internal Expression Left => left;

            internal Token Operator => @operator;

            internal Expression Right => right;

            internal override T Accept<T>(IVisitor<T> visitor) {
				return visitor.VisitBinaryExpression(this);
			}
		}

		internal class Call : Expression {
			private readonly Expression callee;
			private readonly Token closingParenthesis;
			private readonly List<Expression> arguments;

			internal Call(Expression callee, Token closingParenthesis, List<Expression> arguments) {
				this.callee = callee;
				this.closingParenthesis = closingParenthesis;
				this.arguments = arguments;
			}

            internal Expression Callee => callee;

            internal Token ClosingParenthesis => closingParenthesis;

            internal List<Expression> Arguments => arguments;

            internal override T Accept<T>(IVisitor<T> visitor) {
				return visitor.VisitCallExpression(this);
			}
		}

		internal class Get : Expression {
			private readonly Expression @object;
			private readonly Token name;

			internal Get(Expression @object, Token name) {
				this.@object = @object;
				this.name = name;
			}

            internal Expression Object => @object;

            internal Token Name => name;

            internal override T Accept<T>(IVisitor<T> visitor) {
				return visitor.VisitGetExpression(this);
			}
		}

		internal class Grouping : Expression {
			private readonly Expression expression;

			internal Grouping(Expression expression) {
				this.expression = expression;
			}

            internal Expression Expression => expression;

            internal override T Accept<T>(IVisitor<T> visitor) {
				return visitor.VisitGroupingExpression(this);
			}
		}

		internal class Literal : Expression {
			private readonly object value;

			internal Literal(object value) {
				this.value = value;
			}

            public object Value => value;

            internal override T Accept<T>(IVisitor<T> visitor) {
				return visitor.VisitLiteralExpression(this);
			}
		}

		internal class Logical : Expression {
			private readonly Expression left;
			private readonly Token @operator;
			private readonly Expression right;

			internal Logical(Expression left, Token @operator, Expression right) {
				this.left = left;
				this.@operator = @operator;
				this.right = right;
			}

            internal Expression Left => left;

            internal Token Operator => @operator;

            internal Expression Right => right;

            internal override T Accept<T>(IVisitor<T> visitor) {
				return visitor.VisitLogicalExpression(this);
			}
		}

		internal class Set : Expression {
			private readonly Expression @object;
			private readonly Token name;
			private readonly Expression value;

			internal Set(Expression @object, Token name, Expression value) {
				this.@object = @object;
				this.name = name;
				this.value = value;
			}

            internal Expression Object => @object;

            internal Token Name => name;

            internal Expression Value => value;

            internal override T Accept<T>(IVisitor<T> visitor) {
				return visitor.VisitSetExpression(this);
			}
		}

		internal class Super : Expression {
			private readonly Token keyword;
			private readonly Token method;

			internal Super(Token keyword, Token method) {
				this.keyword = keyword;
				this.method = method;
			}

            internal Token Keyword => keyword;

            internal Token Method => method;

            internal override T Accept<T>(IVisitor<T> visitor) {
				return visitor.VisitSuperExpression(this);
			}
		}

		internal class This : Expression {
			private readonly Token keyword;

			internal This(Token keyword) {
				this.keyword = keyword;
			}

            internal Token Keyword => keyword;

            internal override T Accept<T>(IVisitor<T> visitor) {
				return visitor.VisitThisExpression(this);
			}
		}

		internal class Unary : Expression {
			private readonly Token @operator;
			private readonly Expression right;

			internal Unary(Token @operator, Expression right) {
				this.@operator = @operator;
				this.right = right;
			}

            internal Token Operator => @operator;

            internal Expression Right => right;

            internal override T Accept<T>(IVisitor<T> visitor) {
				return visitor.VisitUnaryExpression(this);
			}
		}

		internal class Variable : Expression {
			private readonly Token name;

			internal Variable(Token name) {
				this.name = name;
			}

            internal Token Name => name;

            internal override T Accept<T>(IVisitor<T> visitor) {
				return visitor.VisitVariableExpression(this);
			}
		}

		internal abstract T Accept<T>(IVisitor<T> visitor);
	}
}
