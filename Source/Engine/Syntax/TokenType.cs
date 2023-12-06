namespace NodeJS.Engine.Syntax {
    internal enum TokenType {
        // Single-character tokens
        LEFT_PARENTHESIS, RIGHT_PARENTHESIS, LEFT_BRACE, RIGHT_BRACE,
        COMMA, DOT, MINUS, PLUS, SEMICOLON, SLASH, STAR,

        // One or two character tokens
        BANG, BANG_EQUAL,
        EQUAL, EQUAL_EQUAL,
        GREATER, GREATER_EQUAL,
        LESS, LESS_EQUAL,
        AMPERSAND, DOUBLE_AMPERSAND,
        VERTICAL_BAR, DOUBLE_VERTICAL_BAR,

        // Literals
        IDENTIFIER, STRING, NUMBER,

        // Keywords
        CLASS, CONST, ELSE, FALSE,  FOR, FUNCTION, IF, NULL, NEW,
        PRINT, RETURN, UNDEFINED, SUPER, THIS, TRUE, LET, WHILE, MODULE, USING, EXTENDS, IMPLEMENTS,

        EOF
    }
}
