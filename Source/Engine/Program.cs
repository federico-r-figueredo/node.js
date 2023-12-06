using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NodeJS.Engine.Errors;
using NodeJS.Engine.Syntax;
using static System.Environment;

namespace NodeJS.Engine {
    internal static class Program {
        private static bool hadCompilationError;
        private static bool hadRuntimeError;
        private static readonly Interpreter interpreter;

        static Program() {
            interpreter = new Interpreter();
        }

        internal static void Main(string[] args) {
            if (args.Length > 1) {
                Console.WriteLine("Usage: node [script]");
                Exit(64);
            } else if(args.Length == 1) {
                RunFile(args[0]);
            } else {
                RunPrompt();
            }
        }

        private static void RunFile(string path) {
            try {
                string fullPath = Path.GetFullPath(path);
                byte[] bytes = File.ReadAllBytes(fullPath);
                Run(Encoding.Default.GetString(bytes));

                // Indicate an erorr in the exit code
                if (hadCompilationError) Exit(65);
                if (hadRuntimeError) Exit(70);
            } catch (IOException exception) {
                Console.Error.WriteLine($"Error reading file: {exception.Message}");
            }
        }

        private static void RunPrompt() {
            using (StreamReader streamReader = new StreamReader(Console.OpenStandardInput())) {
                while (true) {
                    Console.Write("> ");
                    string line = streamReader.ReadLine();
                    if (line == null) break;

                    Run(line);
                    // Indicate an erorr in the exit code
                    if (hadCompilationError) Exit(65);
                }
            }
        }

        private static void Run(string source) {
            Lexer lexer = new Lexer(source);
            List<Token> tokens = lexer.ScanTokens();
            Parser parser = new Parser(tokens);
            List<Statement> statements = parser.Parse();

            if (hadCompilationError) return;

            Resolver resolver = new Resolver(interpreter);
            resolver.Resolve(statements);

            // Stop if there was a resolution erorr.
            if (hadCompilationError) return;

            interpreter.Interpret(statements);
        }

        internal static void ReportRuntimeError(RuntimeError error) {
            Report(error.Token.Line, error.Token.Lexeme, error.Message);
            hadRuntimeError = true;
        }

        internal static void ReportCompilationError(Token token, string message) {
            if (token.Type == TokenType.EOF) {
                Report(token.Line, " at end", message);
            } else {
                Report(token.Line, " at '" + token.Lexeme + "'", message);
            }
        }

        private static void Report(int line, string where, string message) {
            Console.Error.WriteLine($"[line {line}] Error{where}: {message}");
            hadCompilationError = true;
        }
    }
}
