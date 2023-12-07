# <img width="300" alt="node-js-logo" src="https://github.com/federico-r-figueredo/Node.js/assets/49570839/45dbddd3-1a04-4757-b6d3-07013eec926c">
The powerful JavaScript runtime, implemented from scratch in C#.


## Implementation
The JavaScript engine consists on a pipeline composed by a [Lexer](https://github.com/federico-r-figueredo/Node.js/blob/f33754629db4b3f7d0d31ce406d89ae256cba581/Source/Engine/Lexer.cs), [Parser](https://github.com/federico-r-figueredo/Node.js/blob/f33754629db4b3f7d0d31ce406d89ae256cba581/Source/Engine/Parser.cs), [Resolver](https://github.com/federico-r-figueredo/Node.js/blob/f33754629db4b3f7d0d31ce406d89ae256cba581/Source/Engine/Resolver.cs) & [Interpreter](https://github.com/federico-r-figueredo/Node.js/blob/f33754629db4b3f7d0d31ce406d89ae256cba581/Source/Engine/Interpreter.cs).

![runtime-diagram](https://github.com/federico-r-figueredo/Node.js/assets/49570839/e83078a1-192d-464d-9a6e-8fb3a67a3b25)

## Features
- [x] Tokens & Lexing
- [x] Abstract Syntax Tree
- [x] Recursive Descent Parsing
- [x] Prefix & Infix Expressions
- [x] Runtime Representation of Objects
- [x] Interpreting Code using the Visitor Design Pattern
- [x] Lexical Scope
- [x] Environment Chains for Storing variables
- [x] Control Flow
- [x] Functions with Parameters
- [x] Closures
- [x] Static Variable Resolution & Error Detection
- [x] Classes
- [x] Constructors
- [x] Fields
- [x] Methods
- [x] Inheritance

> [!CAUTION]
> This runtime is not yet production ready.
