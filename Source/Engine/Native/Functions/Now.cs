using System;
using System.Collections.Generic;
using NodeJS.Engine.Contracts;

namespace NodeJS.Engine.Native.Functions {
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