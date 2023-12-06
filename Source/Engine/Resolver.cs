using System.Collections.Generic;
using System.Linq;
using NodeJS.Engine.Syntax;
using static NodeJS.Engine.Syntax.Statement;

namespace NodeJS.Engine {
    internal class Resolver : Expression.IVisitor<object>, IVisitor<object> {
        private readonly Interpreter interpreter;
        private readonly Stack<Dictionary<string, bool>> scopes;
        private FunctionType currentFunctionType = FunctionType.NONE;
        private ClassType currentClassType = ClassType.NONE;

        private enum FunctionType {
            NONE,
            FUNCTION,
            CONSTRUCTOR,
            METHOD
        };

        private enum ClassType {
            NONE,
            CLASS,
            SUBCLASS
        };

        public Resolver(Interpreter interpreter) {
            this.interpreter = interpreter;
            this.scopes = new Stack<Dictionary<string, bool>>();
        }

        public object VisitAssignExpression(Expression.Assign expression) {
            Resolve(expression.Value);
            ResolveLocal(expression, expression.Name);
            return null;
        }

        public object VisitBinaryExpression(Expression.Binary expression) {
            Resolve(expression.Left);
            Resolve(expression.Right);
            return null;
        }

        public object VisitBlockStatement(Block statement) {
            BeginScope();
            Resolve(statement.Statements);
            EndScope();
            return null;
        }

        private void BeginScope() {
            this.scopes.Push(new Dictionary<string, bool>());
        }

        internal void Resolve(List<Statement> statements) {
            foreach (Statement statement in statements) {
                Resolve(statement);
            }
        }

        private void Resolve(Statement statement) {
            statement.Accept(this);
        }

        private void Resolve(Expression expression) {
            expression.Accept(this);
        }

        private void EndScope() {
            this.scopes.Pop();
        }

        public object VisitCallExpression(Expression.Call expression) {
            Resolve(expression.Callee);

            foreach (Expression argument in expression.Arguments) {
                Resolve(argument);
            }

            return null;
        }

        public object VisitExpressionStatement(StatementExpression statement) {
            Resolve(statement.Expression);
            return null;
        }

        public object VisitFunctionDeclarationStatement(FunctionDeclaration statement) {
            Declare(statement.Name);
            Define(statement.Name);

            ResolveFunction(statement, FunctionType.FUNCTION);
            return null;
        }

        private void ResolveFunction(FunctionDeclaration functionDeclaration, FunctionType functionType) {
            FunctionType enclosingFunctionType = this.currentFunctionType;
            this.currentFunctionType = functionType;        

            BeginScope();
            foreach (Token parameter in functionDeclaration.Parameters) {
                Declare(parameter);
                Define(parameter);
            }

            Resolve(functionDeclaration.Body);
            EndScope();
            this.currentFunctionType = enclosingFunctionType;
        }

        public object VisitGroupingExpression(Expression.Grouping expression) {
            Resolve(expression.Expression);
            return null;
        }

        public object VisitIfStatement(If statement) {
            Resolve(statement.Condition);
            Resolve(statement.ThenBranch);
            if (statement.ElseBranch != null) Resolve(statement.ElseBranch);
            return null;
        }

        public object VisitLiteralExpression(Expression.Literal expression) {
            return null;
        }

        public object VisitLogicalExpression(Expression.Logical expression) {
            Resolve(expression.Left);
            Resolve(expression.Right);
            return null;
        }

        public object VisitReturnStatement(Return statement) {
            if (this.currentFunctionType == FunctionType.NONE) {
                Program.ReportCompilationError(statement.Keyword, "Can't return from top-level code.");
            }
            
            if (statement.Value == null && this.currentFunctionType == FunctionType.CONSTRUCTOR) {
                Program.ReportCompilationError(statement.Keyword, "Can't return from a constructor.");
            }

            if (statement.Value != null) {
                if (this.currentFunctionType == FunctionType.CONSTRUCTOR) {
                    Program.ReportCompilationError(statement.Keyword, "Can't return a value from a constructor.");
                }
                Resolve(statement.Value);
            }

            return null;
        }

        public object VisitUnaryExpression(Expression.Unary expression) {
            Resolve(expression.Right);
            return null;
        }

        public object VisitVariableDeclarationStatement(VariableDeclaration statement) {
            Declare(statement.Name);
            if (statement.Initializer != null) {
                Resolve(statement.Initializer);
            }
            Define(statement.Name);
            return null;
        }

        private void Declare(Token name) {
            if (this.scopes.Count == 0) return;

            Dictionary<string, bool> scope = this.scopes.Peek();
            if (scope.ContainsKey(name.Lexeme)) {
                Program.ReportCompilationError(name, "Variable with this name already exists in this scope.");
            }

            scope.Add(name.Lexeme, false);
        }

        private void Define(Token name) {
            if (this.scopes.Count == 0) return;

            if (this.scopes.Peek().ContainsKey(name.Lexeme)) {
                this.scopes.Peek()[name.Lexeme] = true;
            } else {
                Program.ReportCompilationError(name, "Variable as not been declared.");
            }
        }

        public object VisitVariableExpression(Expression.Variable expression) {
            // TODO: Refactor to remove out usage
            if (this.scopes.Count != 0 
                && this.scopes.Peek().ContainsKey(expression.Name.Lexeme) 
                && this.scopes.Peek()[expression.Name.Lexeme] == false) {
                Program.ReportCompilationError(expression.Name, "Can't read local variable in its own initializer");
            }

            ResolveLocal(expression, expression.Name);
            return null;
        }

        private void ResolveLocal(Expression expression, Token name) {
            // TODO: Find out why original reverse iteration didn't work
            for (int i = 0; i < this.scopes.Count; i++) {
                if (this.scopes.ElementAt(i).ContainsKey(name.Lexeme)) {
                    this.interpreter.Resolve(expression, i);
                    return;
                }
            }
        }

        public object VisitWhileStatement(While statement) {
            Resolve(statement.Condition);
            Resolve(statement.Body);
            return null;
        }

        public object VisitClassDeclarationStatement(ClassDeclaration statement) {
            ClassType enclosingClass = this.currentClassType;
            this.currentClassType = ClassType.CLASS;

            Declare(statement.Name);
            Define(statement.Name);

            if (statement.Superclass != null && statement.Name.Lexeme.Equals(statement.Superclass.Name.Lexeme)) {
                Program.ReportCompilationError(statement.Superclass.Name, "A class can't inherit from itself.");
            }

            if (statement.Superclass != null) {
                this.currentClassType = ClassType.SUBCLASS;
                Resolve(statement.Superclass);
                BeginScope();
                this.scopes.Peek().Add("super", true);
            }

            BeginScope();
            this.scopes.Peek().Add("this", true);

            foreach (FunctionDeclaration method in statement.Methods) {
                FunctionType declaration = FunctionType.METHOD;
                if (method.Name.Lexeme.Equals("constructor")) {
                    declaration = FunctionType.CONSTRUCTOR;
                }
                ResolveFunction(method, declaration);
            }

            EndScope();

            if (statement.Superclass != null) EndScope();

            this.currentClassType = enclosingClass;
            return null;
        }

        public object VisitGetExpression(Expression.Get expression) {
            Resolve(expression.Object);
            return null;
        }

        public object VisitSetExpression(Expression.Set expression) {
            Resolve(expression.Value);
            Resolve(expression.Object);
            return null;
        }

        public object VisitThisExpression(Expression.This expression) {
            if (this.currentClassType == ClassType.NONE) {
                Program.ReportCompilationError(expression.Keyword, "Can't use 'this' outside fo a class");
            }

            ResolveLocal(expression, expression.Keyword);
            return null;
        }

        public object VisitSuperExpression(Expression.Super expression) {
            if (this.currentClassType == ClassType.NONE) {
                Program.ReportCompilationError(expression.Keyword, "Can't use 'super' outside fo a class.");
            } else if (this.currentClassType != ClassType.SUBCLASS) {
                Program.ReportCompilationError(expression.Keyword, "Can't use 'super' in a class that isn't extending a superclass.");
            }

            ResolveLocal(expression, expression.Keyword);
            return null;
        }
    }
}