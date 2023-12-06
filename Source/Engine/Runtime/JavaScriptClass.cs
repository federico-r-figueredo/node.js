using System.Collections.Generic;
using NodeJS.Engine.Contracts;

namespace NodeJS.Engine.Runtime {
    internal class JavaScriptClass : ICallable {
        private readonly string name;
        private readonly JavaScriptClass superclass;
        private readonly Dictionary<string, JavaScriptFunction> methods;

        public JavaScriptClass(string name, JavaScriptClass superclass, Dictionary<string, JavaScriptFunction> methods) {
            this.name = name;
            this.methods = methods;
            this.superclass = superclass;
        }

        public int Arity {
            get {
                JavaScriptFunction constructor = FindMethod("constructor");
                if (constructor == null) return 0;
                return constructor.Arity;
            }
        }

        public string Name => name;

        public object Call(Interpreter interpreter, List<object> arguments) {
            JavaScriptInstance instance = new JavaScriptInstance(this);

            JavaScriptFunction constructor = FindMethod("constructor");
            if (constructor != null) {
                constructor.Bind(instance).Call(interpreter, arguments);
            }

            return instance;
        }

        internal JavaScriptFunction FindMethod(string name) {
            if (this.methods.ContainsKey(name)) {
                return this.methods[name];
            }

            if (this.superclass != null) {
                return this.superclass.FindMethod(name);
            }

            return null;
        }

        public override string ToString() {
            return this.name;
        }
    }
}