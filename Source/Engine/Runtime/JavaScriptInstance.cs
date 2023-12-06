using System.Collections.Generic;
using NodeJS.Engine.Errors;
using NodeJS.Engine.Syntax;

namespace NodeJS.Engine.Runtime {
    internal class JavaScriptInstance {
        private readonly JavaScriptClass @class;
        private readonly Dictionary<string, object> fields;

        public JavaScriptInstance(JavaScriptClass @class) {
            this.@class = @class;
            this.fields = new Dictionary<string, object>();
        }

        public override string ToString() {
            return $"{this.@class.Name} instance";
        }

        internal object Get(Token name) {
            if (this.fields.ContainsKey(name.Lexeme)) {
                return this.fields[name.Lexeme];
            }

            JavaScriptFunction method = this.@class.FindMethod(name.Lexeme);
            if (method != null) return method.Bind(this);

            // TODO: Make this return null for JS
            throw new RuntimeError(name.Line, name, $"Undefined property {name.Lexeme}.");
        }

        internal void Set(Token name, object value) {
            if (this.fields.ContainsKey(name.Lexeme)) {
                this.fields[name.Lexeme] = value;
            } else {
                this.fields.Add(name.Lexeme, value);
            }
        }
    }
}