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
using System.Runtime.CompilerServices;
using XR.Kernel.Util;
using System.Runtime.Loader;
using static XR.Kernel.Util.ConsoleHelpers;

namespace XR.Kernel
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

            //// Load the resulting assembly into the domain. 
            ////Assembly assembly = Assembly.Load(result);
            //var assemblyLoader = new AssemblyLoaderContext();

            //var assembly = assemblyLoader.LoadFromStream(new MemoryStream(result));

            //var entry = assembly.EntryPoint;
            //entry.Invoke(null, null);

            //// unload module
            //assemblyLoader.Unload();

            var assemblyLoadContextWeakRef = LoadAndExecute(result, null);

            for (var i = 0; i < 8 && assemblyLoadContextWeakRef.IsAlive; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            ConsoleHelpers.PrintLn(assemblyLoadContextWeakRef.IsAlive ? "Unloading failed!" : "Unloading success!");

            return this;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference LoadAndExecute(byte[] compiledAssembly, string[] args)
        {
            using var asm = new MemoryStream(compiledAssembly);
            var assemblyLoadContext = new AssemblyLoaderContext();
            assemblyLoadContext.Resolving += AssemblyLoadContext_Resolving;

            var assembly = assemblyLoadContext.LoadFromStream(asm);

            var entry = assembly.EntryPoint;

            _ = entry != null && entry.GetParameters().Length > 0
                ? entry.Invoke(null, new object[] { args })
                : entry.Invoke(null, null);

            assemblyLoadContext.Unload();

            return new WeakReference(assemblyLoadContext);
        }

        private static Assembly AssemblyLoadContext_Resolving(AssemblyLoadContext context, AssemblyName assemblyName)
        {
            var assembly = context.LoadFromAssemblyName(assemblyName);
            PrintLn("Resolving: " + assemblyName.FullName);
            return assembly;
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

            IEnumerable<MetadataReference> refs = new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Runtime.AssemblyTargetedPatchBandAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ConsoleHelpers).Assembly.Location),
        };
            if (references != null)
                refs.Concat(references);

            // create and return the compilation
            CSharpCompilation compilation = CSharpCompilation.Create
            (
                assemblyOrModuleName,
                new[] { syntaxTree },
                options: compilerOptions,
                references: refs
            );

            return compilation;
        }
       
    }

    internal class AssemblyLoaderContext : AssemblyLoadContext
    {
        //private AssemblyDependencyResolver _resolver;

        //public AssemblyLoaderContext(string mainAssemblyToLoadPath) : base(isCollectible: true)
        //{
        //    //_resolver = new AssemblyDependencyResolver(mainAssemblyToLoadPath);
        //}
        public AssemblyLoaderContext() : base(true)
        {

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
