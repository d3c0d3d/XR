using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace XR
{
    public class Compiler
    {
        private List<SourceDetail> SourceList = new List<SourceDetail>();

        public Compiler AddSource(string assemblyName, string sourceCode, params string[] moduleRef)
        {
            SourceList.Add(new SourceDetail(assemblyName, sourceCode, moduleRef));
            return this;
        }

        public Compiler Build()
        {
            foreach (var source in SourceList)
            {
                var compilation =
                CreateCompilationWithMscorlib(
                    source.AssemblyName,
                    source.SourceCode,
                    new CSharpCompilationOptions(OutputKind.NetModule),
                    GetRefs(source.ModuleRef)
                );

                byte[] emitResult = compilation.EmitToArray();

                // update
                source.EmitResult = emitResult;

                MetadataReference metaRef = ModuleMetadata.CreateFromImage(emitResult)
                    .GetReference(display: $"{source.AssemblyName}.netmodule");

                // update
                source.MetaRef = metaRef;
            }

            return this;
        }

        private IEnumerable<MetadataReference> GetRefs(params string[] moduleNameRef)
        {
            List<MetadataReference> metadataReferences = null;

            foreach (var nameRef in moduleNameRef)
            {
                if (metadataReferences == null)
                    metadataReferences = new List<MetadataReference>();

                metadataReferences.Add(SourceList.FirstOrDefault(x => x.AssemblyName == nameRef).MetaRef);
            }

            return metadataReferences;
        }


        private static CSharpCompilation CreateCompilationWithMscorlib(
            string assemblyOrModuleName,
            string code,
            CSharpCompilationOptions compilerOptions = null,
            IEnumerable<MetadataReference> references = null)
        {   
            // create the syntax tree
            SyntaxTree syntaxTree = SyntaxFactory.ParseSyntaxTree(code, null, string.Empty);

            // get the reference to mscore library
            MetadataReference mscoreLibRef =
                AssemblyMetadata.CreateFromFile(typeof(object).Assembly.Location).GetReference();

            MetadataReference consoleRef =
                AssemblyMetadata.CreateFromFile(typeof(Console).Assembly.Location).GetReference();

            //MetadataReference runtimeRef =
            //    MetadataReference.CreateFromFile(Path.Combine(dotNetCoreDir, "System.Runtime.dll"));

            // create the allReferences collection consisting of 
            // mscore reference and all the references passed to the method
            IEnumerable<MetadataReference> allReferences =
                new MetadataReference[] { mscoreLibRef, consoleRef/*, runtimeRef*/ };
            if (references != null)
            {
                allReferences = allReferences.Concat(references);
            }

            // create and return the compilation
            CSharpCompilation compilation = CSharpCompilation.Create
            (
                assemblyOrModuleName,
                new[] { syntaxTree },
                options: compilerOptions,
                references: allReferences
            );

            return compilation;
        }

        public Compiler Run()
        {
            var programDetail = SourceList.LastOrDefault();

            var mainCompilation =
                CreateCompilationWithMscorlib
                (
                    programDetail.AssemblyName,
                    programDetail.SourceCode,
                    // note that here we pass the OutputKind set to ConsoleApplication
                    new CSharpCompilationOptions(OutputKind.ConsoleApplication),
                    GetRefs(programDetail.ModuleRef)
                );

            // Emit the byte result of the compilation
            byte[] result = mainCompilation.EmitToArray();

            // Load the resulting assembly into the domain. 
            Assembly assembly = Assembly.Load(result);

            // load the A.netmodule and B.netmodule into the assembly.

            foreach (var src in SourceList)
            {
                if (src.AssemblyName.ToLower() != "program")
                    assembly.LoadModule($"{src.AssemblyName}.netmodule",src.EmitResult);
            }
            
            // get the type Program from the assembly
            Type programType = assembly.GetType("Program");

            // Get the static Main() method info from the type
            MethodInfo method = programType.GetMethod("Main");

            // invoke Program.Main() static method
            method.Invoke(null, null);

            return this;
        }
    }
    internal static class CompilerEx
    {
        // emit the compilation result into a byte array.
        // throw an exception with corresponding message
        // if there are errors
        internal static byte[] EmitToArray(this Compilation compilation)
        {
            using (var stream = new MemoryStream())
            {
                // emit result into a stream
                var emitResult = compilation.Emit(stream);

                if (!emitResult.Success)
                {
                    // if not successful, throw an exception
                    Diagnostic firstError =
                        emitResult
                            .Diagnostics
                            .FirstOrDefault
                            (
                                diagnostic =>
                                    diagnostic.Severity == DiagnosticSeverity.Error
                            );

                    throw new Exception(firstError?.GetMessage());
                }

                // get the byte array from a stream
                return stream.ToArray();
            }
        }
    }
    internal class SourceDetail
    {
        public SourceDetail(string assemblyName, string sourceCode, params string[] moduleRef)
        {
            AssemblyName = assemblyName;
            SourceCode = sourceCode;
            ModuleRef = moduleRef;
        }

        public string AssemblyName { get; set; }
        public string[] ModuleRef { get; set; }
        public string SourceCode { get; set; }
        public MetadataReference MetaRef { get; set; }
        public byte[] EmitResult { get; set; }
    }

}
