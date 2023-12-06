using System;
using System.Collections.Generic;
using NodeJS.Engine.Contracts;

namespace NodeJS.Engine.Native.Modules {
    internal class JavaScriptConsole : IModule {
        public Environment GenerateBody() {
            Environment moduleBody = new Environment();
            moduleBody.Define("log", new Log());
            return moduleBody;
        }

        public override string ToString() {
            return "<native module console>";
        }
    }

    internal class Log : ICallable {
        public int Arity => 1;

        public object Call(Interpreter interpreter, List<object> arguments) {
            Console.WriteLine(arguments[0]);
            return null;
        }
    }
}