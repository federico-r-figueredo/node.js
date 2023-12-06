using System.Collections.Generic;
using NodeJS.Engine.Contracts;

namespace NodeJS.Engine.Native.Functions {
    internal class Print : ICallable {
        public int Arity => 1;

        public object Call(Interpreter interpreter, List<object> arguments) {
            System.Console.WriteLine(arguments[0]);
            return null;
        }

        public override string ToString() {
            return "<native fn>";
        }
    }
}