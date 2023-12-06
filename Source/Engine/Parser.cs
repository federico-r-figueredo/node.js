using System;
using System.Collections.Generic;
using NodeJS.Engine.Syntax;
using static NodeJS.Engine.Syntax.Expression;
using static NodeJS.Engine.Syntax.Statement;
using static NodeJS.Engine.Syntax.TokenType;

namespace NodeJS.Engine {
    internal class Parser {
        private readonly List<Token> tokens;
        private int current;

        internal Parser(List<Token> tokens) {
            this.tokens = tokens;
        }

        internal List<Statement> Parse() {
            List<Statement> statements = new List<Statement>();
            while (!IsAtEnd()) {
                statements.Add(Declaration());
            }

            return statements;
        }

        private Statement Declaration() {
            try {
                if (Match(CLASS)) return ClassDeclaration();
                if (Match(FUNCTION)) return FunctionDeclaration("function");
                if (Match(LET)) return VariableDeclaration();

                return Statement();
            } catch (ParseError) {
                Synchronize();
                return null;
            }
        }

        private ClassDeclaration ClassDeclaration() {
            Token name = Consume(IDENTIFIER, "Expected class name.");

            Variable superclass = null;
            if (Match(EXTENDS)) {
                Consume(IDENTIFIER, "Expected superclass name.");
                superclass = new Variable(Previous());
            }

            Variable @interface = null;
            if (Match(IMPLEMENTS)) {
                Consume(IDENTIFIER, "Expected interface name.");
                @interface = new Variable(Previous());
            }

            Consume(LEFT_BRACE, "Expected '{' before class body.");

            List<FunctionDeclaration> methods = new List<FunctionDeclaration>();
            while (!Check(RIGHT_BRACE) && !IsAtEnd()) {
                methods.Add(FunctionDeclaration("method"));
            }

            Consume(RIGHT_BRACE, "Expected '}' after class body.");
            return new ClassDeclaration(name, superclass, @interface, methods);
        }

        private FunctionDeclaration FunctionDeclaration(string kind) {
            // Identifier
            Token name = Consume(IDENTIFIER, $"Expected {kind} name.");
            
            // Parameters
            Consume(LEFT_PARENTHESIS, $"Expected '(' after {kind} name.");
            List<Token> parameters = new List<Token>();
            if (!Check(RIGHT_PARENTHESIS)) {
                do {
                    if (parameters.Count >= 255) {
                        Error(Peek(), "Can't have more than 255 parameters");
                    }

                    parameters.Add(
                        Consume(IDENTIFIER, "Expected parameter name.")
                    );
                } while (Match(COMMA));
            }
            Consume(RIGHT_PARENTHESIS, "Expected ')' after parameters.");

            // Body
            Consume(LEFT_BRACE, $"Expected '(' before {kind} body.");
            List<Statement> body = Block();

            // AST node
            return new FunctionDeclaration(name, parameters, body);
        }

        private VariableDeclaration VariableDeclaration() {
            Token name = Consume(IDENTIFIER, "Expected variable name.");

            Expression initializer = null;
            if (Match(EQUAL)) {
                initializer = Expression();
            }

            Consume(SEMICOLON, "Expected ';' after variable declaration");
            return new VariableDeclaration(name, initializer);
        }

        private Statement Statement() {
            if (Match(FOR)) return ForStatement();
            if (Match(IF)) return IfStatement();
            //if (Match(PRINT)) return PrintStatement();
            if (Match(RETURN)) return ReturnStatement();
            if (Match(WHILE)) return WhileStatement();
            if (Match(LEFT_BRACE)) return new Block(Block());

            return ExpressionStatement();
        }

        private Statement ReturnStatement() {
            Token keyword = Previous();
            Expression value = null;
            if (!Check(SEMICOLON)) {
                value = Expression();
                Consume(SEMICOLON, "Expected ';' after return value.");
            } else {
                Consume(SEMICOLON, "Expected ';' after return keyword.");
            }

            return new Return(keyword, value);
        }

        private Statement ForStatement() {
            Consume(LEFT_PARENTHESIS, "Expected '(' after 'for'.");

            Statement initializer;
            if (Match(SEMICOLON)) {
                initializer = null;
            } else if (Match(LET)) {
                initializer = VariableDeclaration();
            } else {
                initializer = ExpressionStatement();
            }

            Expression condition = null;
            if (!Check(SEMICOLON)) {
                condition = Expression();
            }
            Consume(SEMICOLON, "Expected ';' after the loop condition.");

            Expression increment = null;
            if (!Check(RIGHT_PARENTHESIS)) {
                increment = Expression();
            }
            Consume(RIGHT_PARENTHESIS, "Expected ')' after for clauses.");

            Statement body = Statement();

            if (increment != null) {
                body = new Block(
                    new List<Statement>() {
                        body,
                        new Statement.StatementExpression(increment)
                    }
                );
            }

            if (condition == null) condition = new Literal(true);
            body = new While(condition, body);

            if (initializer != null) {
                body = new Block(new List<Statement>() {
                    initializer,
                    body
                });
            }

            return body;
        }

        private Statement WhileStatement() {
            Consume(LEFT_PARENTHESIS, "Expected '(' after 'while'");
            Expression condition = Expression();
            Consume(RIGHT_PARENTHESIS, "Expected ')' after while condition");
            Statement body = Statement();

            return new While(condition, body);
        }

        private Statement IfStatement() {
            Consume(LEFT_PARENTHESIS, "Expected '(' after 'if'.");
            Expression condition = Expression();
            Consume(RIGHT_PARENTHESIS, "Expected ')' after if condition.");

            Statement thenBranch = Statement();
            Statement elseBranch = null;
            if (Match(ELSE)) {
                elseBranch = Statement();
            }

            return new If(condition, thenBranch, elseBranch);
        }

        private List<Statement> Block() {
            List<Statement> statements = new List<Statement>();

            while(!Check(RIGHT_BRACE) && !IsAtEnd()) {
                statements.Add(Declaration());
            }

            Consume(RIGHT_BRACE, "Expected '}' after block.");
            return statements;
        }

        private StatementExpression ExpressionStatement() {
            Expression expression = Expression();
            Consume(SEMICOLON, "Expected ';' after expression.");
            return new StatementExpression(expression);
        }

        private Expression Expression() {
            return Assignment();
        }

        private Expression Assignment() {
            Expression expression = Or();

            if (Match(EQUAL)) {
                Token equals = Previous();
                Expression value = Assignment();

                if (expression is Variable variable) {
                    Token name = variable.Name;
                    return new Assign(name, value);
                } else if (expression is Get get) {
                    return new Set(get.Object, get.Name, value);
                }

                Error(equals, "Invalid assignment target.");
            }

            return expression;
        }

        private Expression Or() {
            Expression expression = And();

            while (Match(DOUBLE_VERTICAL_BAR)) {
                Token @operator = Previous();
                Expression right = And();
                expression = new Logical(expression, @operator, right);
            }

            return expression;
        }

        private Expression And() {
            Expression expression = Equality();

            while (Match(DOUBLE_AMPERSAND)) {
                Token @operator = Previous();
                Expression right = Equality();
                expression = new Logical(expression, @operator, right);
            }

            return expression;
        }

        private Expression Equality() {
            Expression expression = Comparison();

            while (Match(BANG_EQUAL, EQUAL_EQUAL)) {
                Token @operator = Previous();
                Expression right = Comparison();
                expression = new Binary(expression, @operator, right);
            }

            return expression;
        }

        private bool Match(params TokenType[] types) {
            foreach (TokenType type in types) {
                if (Check(type)) {
                    Advance();
                    return true;
                }
            }

            return false;
        }

        private bool Check(TokenType type) {
            if (IsAtEnd()) return false;
            return Peek().Type == type;
        }
        private Token Advance() {
            if (!IsAtEnd()) this.current++;
            return Previous();
        }

        private bool IsAtEnd() {
            return Peek().Type == EOF;
        }

        private Token Peek() {
            return this.tokens[this.current];
        }

        private Token Previous() {
            return this.tokens[this.current - 1];
        }

        private Expression Comparison() {
            Expression expression = Term();

            while ( Match(GREATER, GREATER_EQUAL, LESS, LESS_EQUAL)) {
                Token @operator = Previous();
                Expression right = Term();
                expression = new Binary(expression, @operator, right);
            }

            return expression;
        }

        private Expression Term() {
            Expression expression = Factor();

            while ( Match(MINUS, PLUS)) {
                Token @operator = Previous();
                Expression right = Factor();
                expression = new Binary(expression, @operator, right);
            }

            return expression;
        }

        private Expression Factor() {
            Expression expression = Unary();

            while ( Match(SLASH, STAR)) {
                Token @operator = Previous();
                Expression right = Unary();
                expression = new Binary(expression, @operator, right);
            }

            return expression;
        }

        private Expression Unary() {
            if (Match(BANG, MINUS)) {
                Token @operator = Previous();
                Expression right = Unary();
                return new Unary(@operator, right);
            }

            return Call();
        }

        private Expression Call() {
            Expression expression = Primary();

            while (true) {
                if (Match(LEFT_PARENTHESIS)) {
                    expression = FinishCall(expression);
                } else if (Match(DOT)) {
                    Token name = Consume(IDENTIFIER, "Expected property name after '.'.");
                    if (name.Lexeme == "constructor") {
                        Error(name, "Can't call constructor directly on an instance.");
                    }
                    expression = new Get(expression, name);
                } else {
                    break;
                }
            }

            return expression;
        }

        private Expression FinishCall(Expression callee) {
            List<Expression> arguments = new List<Expression>();
            if (!Check(RIGHT_PARENTHESIS)) {
                do {
                    if (arguments.Count >= 255) {
                        Error(Peek(), "Can't have more than 255 arguments.");
                    }
                    arguments.Add(Expression());
                } while (Match(COMMA));
            }

            Token rightParenthesis = Consume(RIGHT_PARENTHESIS, "Expext ')' after arguments.");

            return new Call(callee, rightParenthesis, arguments);
        }

        private Expression Primary() {
            if (Match(FALSE)) return new Literal(false);
            if (Match(TRUE)) return new Literal(true);
            if (Match(NULL)) return new Literal(null);

            if (Match(NUMBER, STRING)) {
                return new Literal(Previous().Literal);
            }

            if (Match(SUPER)) {
                Token keyword = Previous();
                Consume(DOT, "Expected '.' after 'super'.");
                Token method = Consume(IDENTIFIER, "Expected superclass method name.");
                return new Super(keyword, method);
            }

            if (Match(THIS)) return new This(Previous());

            if (Match(IDENTIFIER)) {
                return new Variable(Previous());
            }

            if (Match(LEFT_PARENTHESIS)) {
                Expression expression = Expression();
                Consume(RIGHT_PARENTHESIS, "Expected ')' after expression.");
                return new Literal(expression);
            }

            if (Match(NEW)) {
                return Call();
            }

            throw Error(Peek(), "Expected expression.");
        }

        private Token Consume(TokenType type, string message) {
            if (Check(type)) return Advance();

            throw Error(Peek(), message);
        }

        private ParseError Error(Token token, string message) {
            Program.ReportCompilationError(token, message);
            return new ParseError();
        }

        private void Synchronize() {
            Advance();

            while (!IsAtEnd()) {
                if (Previous().Type == SEMICOLON) return;

                switch (Peek().Type) {
                    case CLASS:
                    case CONST:
                    case FUNCTION:
                    case LET:
                    case FOR:
                    case IF:
                    case WHILE:
                    case RETURN:
                        return;
                }

                Advance();
            }
        }

        private class ParseError : SystemException { }
    }
}