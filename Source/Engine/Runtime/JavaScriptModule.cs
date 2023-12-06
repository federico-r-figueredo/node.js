using NodeJS.Engine.Errors;
using NodeJS.Engine.Syntax;

namespace NodeJS.Engine.Runtime {
    internal class JavaScriptModule {
        private readonly Environment body;
        private readonly string name;

        public JavaScriptModule(Environment body, string name) {
            this.body = body;
            this.name = name;
        }

        internal object Get(Token name) {
            object statement = this.body.Get(name);
            if (statement == null) {
                throw new RuntimeError(name.Line, name, $"Undefined property '{name.Lexeme}'.");
            }

            return statement;
        }

        public override string ToString() {
            return this.name;
        }
    }
}