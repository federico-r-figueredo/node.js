using System.Collections.Generic;
using NodeJS.Engine.Errors;
using NodeJS.Engine.Syntax;

namespace NodeJS.Engine {
    internal class Environment {
        private readonly Environment enclosing;
        private readonly Dictionary<string, object> values;

        internal Environment() {
            this.enclosing = null;
            this.values = new Dictionary<string, object>();
        }

        internal Environment(Environment enclosing) {
            this.enclosing = enclosing;
            this.values = new Dictionary<string, object>();
        }

        internal Environment Enclosing => enclosing;

        internal void Assign(Token name, object value) {
            if (this.values.ContainsKey(name.Lexeme)) {
                this.values[name.Lexeme] = value;
                return;
            }

            if (this.enclosing != null) {
                this.enclosing.Assign(name, value);
                return;
            }

            throw new RuntimeError(
                name.Line,
                name,
                $"Undefined variable {name.Lexeme}."
            );
        }

        internal void AssignAt(int distance, Token name, object value) {
            if (!Ancestor(distance).values.TryAdd(name.Lexeme, value)) {
                Ancestor(distance).values[name.Lexeme] = value;
            }
        }

        internal void Define(string name, object value) {
            this.values.Add(name, value);
        }

        internal object Get(Token name) {
            if (this.values.ContainsKey(name.Lexeme)) {
                return values[name.Lexeme];
            }

            if (this.enclosing != null) return this.enclosing.Get(name);

            throw new RuntimeError(
                name.Line,
                name,
                $"Undefined variable {name.Lexeme}."
            );
        }

        internal object GetAt(int distance, string name) {
            return Ancestor(distance).values[name];
        }

        private Environment Ancestor(int distance) {
            Environment environment = this;
            for (int i = 0; i < distance; i++) {
                environment = environment.enclosing;
            }

            return environment;
        }
    }
}