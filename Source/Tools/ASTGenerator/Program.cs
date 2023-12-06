using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GenerateAST {    
    public static class Program {
        public static void Main(string[] args) {
            if (args.Length != 1) {
                Console.Error.WriteLine("Usage: generate-ast <output directory>");
                Environment.Exit(64);
            }

            string outputDir = args[0];

            DefineAST(outputDir, "Expression", new List<string>() {
                "Assign     : Token name, Expression value",
                "Binary     : Expression left, Token @operator, Expression right",
                "Call       : Expression callee, Token closingParenthesis, List<Expression> arguments",
                "Get        : Expression @object, Token name",
                "Grouping   : Expression expression",
                "Literal    : object value",
                "Logical    : Expression left, Token @operator, Expression right",
                "Set        :  Expression @object, Token name, Expression value",
                "Super      : Token keyword, Token method",
                "This       : Token keyword",
                "Unary      : Token @operator, Expression right",
                "Variable   : Token name"
            });

            DefineAST(outputDir, "Statement", new List<string>() {
                "Block               : List<Statement> statements",
                "ClassDeclaration    : Token name, Expression.Variable superclass, Expression.Variable @interface, List<FunctionDeclaration> methods",
                "StatementExpression : Expression expression",
                "FunctionDeclaration : Token name, List<Token> parameters, List<Statement> body",
                "If                  : Expression condition, Statement thenBranch, Statement elseBranch",
                "Print               : Expression expression",
                "Return              : Token keyword, Expression value",
                "VariableDeclaration : Token name, Expression initializer",
                "While               : Expression condition, Statement body"
            });
        }

        private static void DefineAST(string outputDir, string baseName, List<string> types) {
            string path = Path.Combine(outputDir, $"{baseName}.cs");

            using (StreamWriter writer = new StreamWriter(path, false, Encoding.UTF8)) {
                writer.WriteLine("namespace Engine {");
                writer.WriteLine($"\tinternal abstract class {baseName} {{");

                DefineVisitor(writer, baseName, types);

                // The AST classes.
                foreach (string type in types) {
                    string className = type.Split(":")[0].Trim();
                    string fields = type.Split(":")[1].Trim();
                    DefineType(writer, baseName, className, fields);
                    writer.WriteLine();
                }

                // The base Accept() method.
                writer.WriteLine("\t\tinternal abstract T Accept<T>(IVisitor<T> visitor);");

                writer.WriteLine("\t}");
                writer.WriteLine("}");
            }
        }

        private static void DefineVisitor(StreamWriter writer, string baseName, List<string> types) {
            writer.WriteLine($"\t\tinternal interface IVisitor<T> {{");

            foreach (string type in types) {
                string typeName = type.Split(":")[0].Trim();
                writer.WriteLine($"\t\t\tT Visit{typeName}{baseName}({typeName} {baseName.ToLower()});");
            }

            writer.WriteLine("\t\t}");
            writer.WriteLine();
        }

        private static void DefineType(StreamWriter writer, string baseName, string className, string fieldList) {
            writer.WriteLine($"\t\tinternal class {className} : {baseName} {{");

            // Store parameters in fields.
            string[] fields = fieldList.Split(", ");
            
            // Fields.
            foreach (string field in fields) {
                writer.WriteLine($"\t\t\tprivate readonly {field};");
            }

            // Constructor.
            writer.WriteLine();
            writer.WriteLine($"\t\t\tinternal {className}({fieldList}) {{");

            foreach (string field in fields) {
                string name = field.Split(" ")[1];
                writer.WriteLine($"\t\t\t\tthis.{name} = {name};");
            }

            writer.WriteLine("\t\t\t}");

            // Visitor pattern.
            writer.WriteLine();
            writer.WriteLine("\t\t\tinternal override T Accept<T>(IVisitor<T> visitor) {");
            writer.WriteLine($"\t\t\t\treturn visitor.Visit{className}{baseName}(this);");
            writer.WriteLine("\t\t\t}");

            writer.WriteLine("\t\t}");
        }
    }
}

