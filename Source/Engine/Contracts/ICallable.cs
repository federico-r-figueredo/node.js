using System.Collections.Generic;

namespace NodeJS.Engine.Contracts {
    internal interface ICallable {
        int Arity { get; }
        object Call(Interpreter interpreter, List<object> arguments);
    }
}