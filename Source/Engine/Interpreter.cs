using System;
using System.Collections.Generic;
using NodeJS.Engine.Runtime;
using NodeJS.Engine.Errors;
using NodeJS.Engine.Syntax;
using static NodeJS.Engine.Syntax.TokenType;
using NodeJS.Engine.Contracts;
using NodeJS.Engine.Native.Modules;

namespace NodeJS.Engine {
    internal class Interpreter : Expression.IVisitor<object>, Statement.IVisitor<object> {
        private readonly Environment globals;
        private readonly Dictionary<Expression, int> locals;
        private Environment environment;

        internal Environment Globals => globals;

        public Interpreter() {
            this.globals = new Environment();
            this.locals = new Dictionary<Expression, int>();
            this.environment = this.globals;

            this.globals.Define("Date", new JavaScriptModule(new JavaScriptDate().GenerateBody(), "Date"));
            this.globals.Define("console", new JavaScriptModule(new JavaScriptConsole().GenerateBody(), "console"));
        }

        public void Interpret(List<Statement> statements) {
            try {
                foreach (Statement statement in statements) {
                    Execute(statement);
                }
            } catch (RuntimeError error) {
                Program.ReportRuntimeError(error);
            }
        }

        private void Execute(Statement statement) {
            statement.Accept(this);
        }

        private string Stringify(object @object) {
            if (@object == null) return "null";

            if (@object is double) {
                string text = @object.ToString();
                if (text.EndsWith(".0")) {
                    text = text.Substring(0, text.Length - 2);
                }

                return text;
            }

            return @object.ToString();
        }

        public object VisitBinaryExpression(Expression.Binary expression) {
            object left = Evaluate(expression.Left);
            object right = Evaluate(expression.Right);

            switch (expression.Operator.Type) {
                case GREATER:
                    CheckNumberOperand(expression.Operator, left, right);
                    return (double) left > (double) right;
                case GREATER_EQUAL: 
                    return (double) left >= (double) right;
                case LESS:
                    return (double) left < (double) right;
                case LESS_EQUAL: 
                    return (double) left <= (double) right;
                case BANG_EQUAL: 
                    return !IsEqual(left, right);
                case EQUAL_EQUAL: 
                    return IsEqual(left, right);
                case MINUS:
                    CheckNumberOperand(expression.Operator, right);
                    return Math.Round((double) left - (double) right, 2);
                case PLUS:
                    if (left is double && right is double) {
                        return (double) left + (double) right;
                    } else if (left is string && right is string) {
                        return (string) left + (string) right;
                    } else if (left is double && right is string) {
                        return Stringify((double) left) + (string) right;
                    } else if (left is string && right is double) {
                        return (string) left + Stringify((double) right);
                    }

                    throw new RuntimeError(
                        expression.Operator.Line,
                        expression.Operator, 
                        "Operands must be two numbers or two strings"
                    );
                case SLASH: 
                    return (double) left / (double) right;
                case STAR: 
                    return (double) left * (double) right;
            }

            // Unreachable.
            return null;
        }

        public object VisitGroupingExpression(Expression.Grouping expression) {
            return Evaluate(expression.Expression);
        }

        public object VisitLiteralExpression(Expression.Literal expression) {
            return expression.Value;
        }

        public object VisitUnaryExpression(Expression.Unary expression) {
            object right = Evaluate(expression.Right);

            switch (expression.Operator.Type) {
                case BANG:
                    return !IsTruthy(right);
                case MINUS:
                    return - (double) right;
            }

            // Unreachable.
            return null;
        }

        public object VisitVariableExpression(Expression.Variable expression) {
            return LookupVariable(expression.Name, expression);
        }

        private object LookupVariable(Token name, Expression expression) {
            if (this.locals.TryGetValue(expression, out int distance)) {
                return this.environment.GetAt(distance, name.Lexeme);
            } else {
                return this.globals.Get(name);
            }
        }

        private void CheckNumberOperand(Token @operator, object operand) {
            if (operand is double) return;

            throw new RuntimeError(@operator.Line, @operator, "Operand must be a number.");
        }

        private void CheckNumberOperand(Token @operator, object left, object right) {
            if (left is double && right is double) return;

            throw new RuntimeError(@operator.Line, @operator, "Operands must be numbers.");
        }


        private bool IsEqual(object left, object right) {
            if (left == null && right == null) return true;
            if (left == null) return false;

            return left.Equals(right);
        }

        private bool IsTruthy(object @object) {
            if (@object == null) return false;
            if (@object is bool) return (bool) @object;
            if (@object is string) return ((string) @object).Length > 0;
            if (@object is Array) return ((Array) @object).Length > 0;
            if (@object is double) return ((double) @object) > 0;

            return true;
        }

        private object Evaluate(Expression expression) {
            return expression.Accept(this);
        }

        public object VisitExpressionStatement(Statement.StatementExpression statement) {
            Evaluate(statement.Expression);
            return null;
        }
        
        public object VisitVariableDeclarationStatement(Statement.VariableDeclaration statement) {
            object value = null;
            if (statement.Initializer != null) {
                value = Evaluate(statement.Initializer);
            }

            this.environment.Define(statement.Name.Lexeme, value);
            return null;
        }

        public object VisitAssignExpression(Expression.Assign expression) {
            object value = Evaluate(expression.Value);
            
            if (this.locals.TryGetValue(expression, out int distance)) {
                this.environment.AssignAt(distance, expression.Name, value);
            } else {
                this.globals.Assign(expression.Name, value);
            }

            return value;
        }

        public object VisitBlockStatement(Statement.Block statement) {
            ExecuteBlock(statement.Statements, new Environment(this.environment));
            return null;
        }

        internal void ExecuteBlock(List<Statement> statements, Environment environment) {
            Environment previous = this.environment;
            try {
                this.environment = environment;

                foreach (Statement statement in statements) {
                    Execute(statement);
                }
            } finally {
                this.environment = previous;
            }
        }

        public object VisitIfStatement(Statement.If statement) {
            if (IsTruthy(Evaluate(statement.Condition))) {
                Execute(statement.ThenBranch);
            } else if (statement.ElseBranch != null) {
                Execute(statement.ElseBranch);
            }

            return null;
        }

        public object VisitLogicalExpression(Expression.Logical expression) {
            object left = Evaluate(expression.Left);

            if (expression.Operator.Type == DOUBLE_VERTICAL_BAR) {
                if (IsTruthy(left)) return left;
            } else {
                if (!IsTruthy(left)) return left;
            }

            return Evaluate(expression.Right);
        }

        public object VisitWhileStatement(Statement.While statement) {
            while (IsTruthy(Evaluate(statement.Condition))) {
                Execute(statement.Body);
            }

            return null;
        }

        public object VisitCallExpression(Expression.Call expression) {
            object callee = Evaluate(expression.Callee);

            List<object> arguments = new List<object>();
            foreach (Expression argument in expression.Arguments) {
                arguments.Add(Evaluate(argument));
            }

            if (!(callee is ICallable)) {
                throw new RuntimeError(
                    expression.ClosingParenthesis.Line,
                    expression.ClosingParenthesis, 
                    "Can only call functions, classes constructors / static methods and object's member methods."
                );
            }

            ICallable function = (ICallable) callee;
            if (arguments.Count != function.Arity) {
                throw new RuntimeError(                    
                    expression.ClosingParenthesis.Line,
                    expression.ClosingParenthesis,
                    $"Expected {function.Arity} arguments but got {arguments.Count}."
                );
            }

            return function.Call(this, arguments);
        }

        public object VisitFunctionDeclarationStatement(Statement.FunctionDeclaration statement) {
            JavaScriptFunction function = new JavaScriptFunction(statement, this.environment);
            this.environment.Define(statement.Name.Lexeme, function);
            return null;
        }

        public object VisitReturnStatement(Statement.Return statement) {
            object value = null;
            if (statement.Value != null) value = Evaluate(statement.Value);

            throw new Return(value);
        }

        internal void Resolve(Expression expression, int depth) {
            this.locals.Add(expression, depth);
        }

        public object VisitClassDeclarationStatement(Statement.ClassDeclaration statement) {
            object superclass = null;
            if (statement.Superclass != null) {
                superclass = Evaluate(statement.Superclass);
                if (!(superclass is JavaScriptClass)) {
                    throw new RuntimeError(statement.Superclass.Name.Line, statement.Superclass.Name, "Superclass must be a class.");
                }
            }

            this.environment.Define(statement.Name.Lexeme, null);

            if (superclass != null) {
                this.environment = new Environment(environment);
                this.environment.Define("super", superclass);
            }

            Dictionary<string, JavaScriptFunction> methods = new Dictionary<string, JavaScriptFunction>();
            foreach (Statement.FunctionDeclaration method in statement.Methods) {
                JavaScriptFunction function = new JavaScriptFunction(method, environment);
                methods.Add(method.Name.Lexeme, function);
            }

            JavaScriptClass @class = new JavaScriptClass(statement.Name.Lexeme, (JavaScriptClass) superclass, methods);

            if (superclass != null) {
                this.environment = this.environment.Enclosing;
            }

            this.environment.Assign(statement.Name, @class);
            return null;
        }

        public object VisitGetExpression(Expression.Get expression) {
            object leftValue = Evaluate(expression.Object);

            if (leftValue is JavaScriptInstance) {
                return ((JavaScriptInstance) leftValue).Get(expression.Name);
            }

            if (leftValue is JavaScriptModule) {
                return ((JavaScriptModule) leftValue).Get(expression.Name);
            }

            throw new RuntimeError(expression.Name.Line, expression.Name, "Only instances have properties");
        }

        public object VisitSetExpression(Expression.Set expression) {
            object @object = Evaluate(expression.Object);

            if (!(@object is JavaScriptInstance)) {
                throw new RuntimeError(expression.Name.Line, expression.Name, "Only instances have fields.");
            }

            object value = Evaluate(expression.Value);
            ((JavaScriptInstance) @object).Set(expression.Name, value);
            return value;
        }

        public object VisitThisExpression(Expression.This expression) {
            return LookupVariable(expression.Keyword, expression);
        }

        public object VisitSuperExpression(Expression.Super expression) {
            int distance = this.locals[expression];
            JavaScriptClass superclass = (JavaScriptClass) this.environment.GetAt(distance, "super");

            JavaScriptInstance instance = (JavaScriptInstance) this.environment.GetAt(distance - 1, "this");

            JavaScriptFunction method = superclass.FindMethod(expression.Method.Lexeme);

            if (method == null) {
                throw new RuntimeError(expression.Method.Line, expression.Method, $"Undefined property {expression.Method.Lexeme}.");
            }

            return method.Bind(instance);
        }
    }
}