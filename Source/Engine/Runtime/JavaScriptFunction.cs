using System.Collections.Generic;
using NodeJS.Engine.Contracts;
using static NodeJS.Engine.Syntax.Statement;

namespace NodeJS.Engine.Runtime {
    internal class JavaScriptFunction : ICallable {
        private readonly FunctionDeclaration declaration;
        private readonly Environment closure;

        public JavaScriptFunction(FunctionDeclaration declaration, Environment closure) {
            this.declaration = declaration;
            this.closure = closure;
        }

        public int Arity => this.declaration.Parameters.Count;

        public object Call(Interpreter interpreter, List<object> arguments) {
            Environment environment = new Environment(this.closure);
            for (int i = 0; i < this.declaration.Parameters.Count; i++) {
                environment.Define(
                    declaration.Parameters[i].Lexeme,
                    arguments[i]
                );
            }

            try {
                interpreter.ExecuteBlock(declaration.Body, environment);
            } catch (Errors.Return @return) {
                return @return.Value;    
            }

            return null;
        }

        public override string ToString() {
            return $"<fn {this.declaration.Name.Lexeme}>";
        }

        internal JavaScriptFunction Bind(JavaScriptInstance instance) {
            Environment environment = new Environment(this.closure);
            environment.Define("this", instance);
            return new JavaScriptFunction(declaration, environment);
        }
    }
}