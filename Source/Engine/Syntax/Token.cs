namespace NodeJS.Engine.Syntax {
    internal class Token {
        readonly TokenType type;
        readonly string lexeme;
        readonly object literal;
        readonly int line;

        internal Token(TokenType type, string lexeme, object literal, int line) {
            this.type = type;
            this.lexeme = lexeme;
            this.literal = literal;
            this.line = line;
        }

        public string Lexeme => lexeme;

        public object Literal => literal;

        public int Line => line;

        internal TokenType Type => type;

        public override string ToString() {
            return this.type + " " + this.lexeme + " " + this.literal;
        }
    }
}
