using System;
using XR.Core;

namespace XR.App
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start Test");

            var classAString =
                @"using System;
                    public class A 
                    {
                        public static string Print() 
                        { 
                            return ""Hello "";
                        }
                    }";

            var classBString =
                @"public class B : A
                    {
                        public static string Print()
                        { 
                            return ""World!"";
                        }
                    }";

            var programStr =
                @"Console.Write(A.Print()); 
                  Console.WriteLine(B.Print());";
            var testFullStr = $"{classAString}\n{classBString}\n{Templates.MainProgramStr.Replace("{code}",programStr)}";

            _ = new Compiler()
                //.AddSource("A", classAString)
         //       .AddSource("B", classBString, "A")
             //   .AddSource("program", Templates.MainProgramStr.Replace("{code}", programStr), "A", "B")
                .AddSource("program", testFullStr)
                .Build()
                .Run();

            Console.ReadKey();

        }
    }
}
