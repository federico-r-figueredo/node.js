using System;
using System.Collections.Generic;
using NodeJS.Engine.Contracts;

namespace NodeJS.Engine.Native.Modules {
    internal class JavaScriptDate : IModule {
        public Environment GenerateBody() {
            Environment moduleBody = new Environment();
            moduleBody.Define("now", new Now());
            return moduleBody;
        }

        public override string ToString() {
            return "<native module console>";
        }
    }

    internal class Now : ICallable {
        public int Arity => 0;

        public object Call(Interpreter interpreter, List<object> arguments) {
            return Math.Round(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0, 2);
        }

        public override string ToString() {
            return "<native fn>";
        }
    }
}