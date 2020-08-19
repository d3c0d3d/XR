using System;

namespace XR
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start Test");

            var classAString =
                @"public class A 
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
                @"System.Console.Write(A.Print()); 
                  System.Console.WriteLine(B.Print());";

            _ = new Compiler()
                .AddSource("A", classAString)
                .AddSource("B", classBString, "A")
                .AddSource("program", Templates.MainProgramStr.Replace("{code}",programStr), "A","B")
                .Build()
                .Run();

            Console.ReadKey();

        }
    }
}
