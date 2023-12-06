using System;
using NodeJS.Engine.Syntax;

namespace NodeJS.Engine.Errors {
    internal class RuntimeError : SystemException {
        private readonly int line;
        private readonly Token token;

        public RuntimeError(int line, Token token, string message) : base(message) {
            this.token = token;
            this.line = line;
        }

        public int Line => line;
        internal Token Token => this.token;
    }
}