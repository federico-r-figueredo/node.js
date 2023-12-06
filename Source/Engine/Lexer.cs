using System.Collections.Generic;
using NodeJS.Engine.Errors;
using NodeJS.Engine.Syntax;

namespace NodeJS.Engine {
    internal class Lexer {
        private readonly string source;
        private readonly List<Token> tokens;
        private int start;
        private int current;
        private int line;
        private static readonly Dictionary<string, TokenType> keywords = new Dictionary<string, TokenType>() {
            { "class", TokenType.CLASS },
            { "const", TokenType.CONST },
            { "else", TokenType.ELSE },
            { "false", TokenType.FALSE },
            { "for", TokenType.FOR },
            { "function", TokenType.FUNCTION },
            { "if", TokenType.IF },
            { "null", TokenType.NULL },
            { "new", TokenType.NEW },
            { "return", TokenType.RETURN },
            { "undefined", TokenType.UNDEFINED },
            { "super", TokenType.SUPER },
            { "this", TokenType.THIS },
            { "true", TokenType.TRUE },
            { "let", TokenType.LET },
            { "while", TokenType.WHILE },
            { "module", TokenType.MODULE },
            { "using", TokenType.USING },
            { "extends", TokenType.EXTENDS },
            { "implements", TokenType.IMPLEMENTS }
        };

        internal Lexer(string source) {
            this.source = source;
            this.tokens = new List<Token>();
            this.start = 0;
            this.current = 0;
            this.line = 1;
        }

        internal List<Token> ScanTokens() {
            while(!IsAtEnd()) {
                // We are at the beginging of the lexeme
                this.start = this.current;
                ScanToken();
            }

            this.tokens.Add(new Token(TokenType.EOF, "", null, line));
            return this.tokens;
        }

        private void ScanToken() {
            char character = Advance();
            switch(character) {
                case '(': AddToken(TokenType.LEFT_PARENTHESIS); break;
                case ')': AddToken(TokenType.RIGHT_PARENTHESIS); break;
                case '{': AddToken(TokenType.LEFT_BRACE); break;
                case '}': AddToken(TokenType.RIGHT_BRACE); break;
                case ',': AddToken(TokenType.COMMA); break;
                case '.': AddToken(TokenType.DOT); break;
                case '-': AddToken(TokenType.MINUS); break;
                case '+': AddToken(TokenType.PLUS); break;
                case ';': AddToken(TokenType.SEMICOLON); break;
                case '*': AddToken(TokenType.STAR); break;
                case '!': AddToken(IsMatch('=') ? TokenType.BANG_EQUAL : TokenType.BANG); break;
                case '=': AddToken(IsMatch('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL); break;
                case '<': AddToken(IsMatch('=') ? TokenType.LESS_EQUAL : TokenType.LESS); break;
                case '>': AddToken(IsMatch('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER); break;
                case '|': AddToken(IsMatch('|') ? TokenType.DOUBLE_VERTICAL_BAR : TokenType.VERTICAL_BAR); break;
                case '&': AddToken(IsMatch('&') ? TokenType.DOUBLE_AMPERSAND : TokenType.AMPERSAND); break;
                case '/': 
                    if (IsMatch('/')) {
                        // A comment goes until the end of line.
                        while (Peek() != '\n' && !IsAtEnd()) Advance();
                    } else {
                        AddToken(TokenType.SLASH);
                    }
                    break;
                case ' ':
                case '\r':
                case '\t':
                    // ignore whitespace
                    break;
                case '\n':
                    this.line++;
                    break;
                case '"': String(); break;
                default:
                    if (IsDigit(character)) {
                        Number();
                    } else if (IsAlphabetic(character)) {
                        Identifier();
                    } else {
                        Error(line, "Unexpected character."); 
                    }
                    break;
            }

            char Advance() {
                this.current++;
                return this.source[this.current - 1];
            }

            bool IsMatch(char expected) {
                if (IsAtEnd()) return false;
                if (this.source[this.current] != expected) return false;

                this.current++;
                return true;
            }

            void Identifier() {
                while (IsAlphanumeric(Peek())) Advance();

                string text = this.source.Substring(this.start, this.current - this.start);
                TokenType type;
                if (!keywords.TryGetValue(text, out type)) type = TokenType.IDENTIFIER;
                AddToken(type);
            }

            bool IsAlphanumeric(char c) {
                return IsAlphabetic(c) || IsDigit(c);
            }

            bool IsDigit(char c) {
                return c >= '0' && c <= '9';
            }

            bool IsAlphabetic(char c) {
                return (c >= 'a' && c <= 'z') ||
                        (c >= 'A' && c <= 'Z') ||
                        (c == '_');
            }

            char Peek() {
                if (IsAtEnd()) return '\0';
                return this.source[this.current];
            }

            char PeekNext() {
                if (this.current + 1 >= this.source.Length) return '\0';
                return this.source[this.current + 1];
            }

            void String() {
                while (Peek() != '"' && !IsAtEnd()) {
                    if (Peek() == '\n') this.line++;
                    Advance();
                }

                if (IsAtEnd()) {
                    Error(line, "Unterminated string.");
                    return;
                }

                // The closing ".
                Advance();

                // Trim the surrounding quotes
                string value = this.source.Substring(this.start + 1, this.current - this.start - 2);
                AddToken(TokenType.STRING, value);
            }

            void Number() {
                while (IsDigit(Peek())) Advance();

                // Look for a fractional part.
                if (Peek() == '.' && IsDigit(PeekNext())) {
                    /// Consume the "."
                    Advance();

                    while (IsDigit(Peek())) Advance();
                }

                AddToken(TokenType.NUMBER, double.Parse(this.source.Substring(this.start, this.current - this.start)));
            }
        }

        private void AddToken(TokenType type) {
            AddToken(type, null);
        }

        private void AddToken(TokenType type, object literal) {
            string text = this.source.Substring(this.start, this.current - this.start);
            this.tokens.Add(new Token(type, text, literal, line));
        }

        private bool IsAtEnd() {
            return this.current >= this.source.Length;
        }

        private static void Error(int line, string message) {
            Program.ReportRuntimeError(new RuntimeError(line, null, message));
        }
    }
}
