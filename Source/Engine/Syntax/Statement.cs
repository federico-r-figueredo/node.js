using System.Collections.Generic;

namespace NodeJS.Engine.Syntax {
	internal abstract class Statement {
		internal interface IVisitor<T> {
			T VisitBlockStatement(Block statement);
			T VisitClassDeclarationStatement(ClassDeclaration statement);
			T VisitExpressionStatement(StatementExpression statement);
			T VisitFunctionDeclarationStatement(FunctionDeclaration statement);
			T VisitIfStatement(If statement);
			T VisitReturnStatement(Return statement);
			T VisitVariableDeclarationStatement(VariableDeclaration statement);
			T VisitWhileStatement(While statement);
		}

		internal class Block : Statement {
			private readonly List<Statement> statements;

			internal Block(List<Statement> statements) {
				this.statements = statements;
			}

            internal List<Statement> Statements => statements;

            internal override T Accept<T>(IVisitor<T> visitor) {
				return visitor.VisitBlockStatement(this);
			}
		}

		internal class ClassDeclaration : Statement {
			private readonly Token name;
			private readonly Expression.Variable superclass;
			private readonly Expression.Variable @interface;
			private readonly List<FunctionDeclaration> methods;

			internal ClassDeclaration(Token name, Expression.Variable superclass, Expression.Variable @interface, List<FunctionDeclaration> methods) {
				this.name = name;
				this.superclass = superclass;
				this.@interface = @interface;
				this.methods = methods;
			}

            internal Token Name => name;

            internal List<FunctionDeclaration> Methods => methods;

            internal Expression.Variable Superclass => superclass;

            internal Expression.Variable Interface => @interface;

            internal override T Accept<T>(IVisitor<T> visitor) {
				return visitor.VisitClassDeclarationStatement(this);
			}
		}

		internal class StatementExpression : Statement {
			private readonly Expression expression;

			internal StatementExpression(Expression expression) {
				this.expression = expression;
			}

            internal Expression Expression => expression;

			internal override T Accept<T>(IVisitor<T> visitor) {
				return visitor.VisitExpressionStatement(this);
			}
		}

		internal class FunctionDeclaration : Statement {
			private readonly Token name;
			private readonly List<Token> parameters;
			private readonly List<Statement> body;

			internal FunctionDeclaration(Token name, List<Token> parameters, List<Statement> body) {
				this.name = name;
				this.parameters = parameters;
				this.body = body;
			}

            internal Token Name => name;

            internal List<Token> Parameters => parameters;

            internal List<Statement> Body => body;

            internal override T Accept<T>(IVisitor<T> visitor) {
				return visitor.VisitFunctionDeclarationStatement(this);
			}
		}

		internal class If : Statement {
			private readonly Expression condition;
			private readonly Statement thenBranch;
			private readonly Statement elseBranch;

			internal If(Expression condition, Statement thenBranch, Statement elseBranch) {
				this.condition = condition;
				this.thenBranch = thenBranch;
				this.elseBranch = elseBranch;
			}

            internal Expression Condition => condition;

            internal Statement ThenBranch => thenBranch;

            internal Statement ElseBranch => elseBranch;

            internal override T Accept<T>(IVisitor<T> visitor) {
				return visitor.VisitIfStatement(this);
			}
		}

		internal class Return : Statement {
			private readonly Token keyword;
			private readonly Expression value;

			internal Return(Token keyword, Expression value) {
				this.keyword = keyword;
				this.value = value;
			}

            internal Token Keyword => keyword;

            internal Expression Value => value;

            internal override T Accept<T>(IVisitor<T> visitor) {
				return visitor.VisitReturnStatement(this);
			}
		}

		internal class VariableDeclaration : Statement {
			private readonly Token name;
			private readonly Expression initializer;

			internal VariableDeclaration(Token name, Expression initializer) {
				this.name = name;
				this.initializer = initializer;
			}

            internal Token Name => name;

            internal Expression Initializer => initializer;

            internal override T Accept<T>(IVisitor<T> visitor) {
				return visitor.VisitVariableDeclarationStatement(this);
			}
		}

		internal class While : Statement {
			private readonly Expression condition;
			private readonly Statement body;

			internal While(Expression condition, Statement body) {
				this.condition = condition;
				this.body = body;
			}

            internal Expression Condition => condition;

            internal Statement Body => body;

            internal override T Accept<T>(IVisitor<T> visitor) {
				return visitor.VisitWhileStatement(this);
			}
		}

		internal abstract T Accept<T>(IVisitor<T> visitor);
	}
}
