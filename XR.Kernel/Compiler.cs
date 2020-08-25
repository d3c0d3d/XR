using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using static XR.Kernel.Std.Cli;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using XR.Kernel.Extensions;
using System.Diagnostics;

namespace XR.Kernel
{
    public class Compiler
    {
        public SourceDetail SourceDetail { get; private set; }

        public Compiler Build(string assemblyName, string sourceCode, params string[] moduleRef)
        {
            SourceDetail = new SourceDetail(assemblyName, sourceCode, moduleRef);

            var compResult = GenerateCode(SourceDetail.AssemblyName, SourceDetail.SourceCode, GetRefs(SourceDetail.ModuleRef));

            byte[] emitResult = compResult.EmitToArray();

            // update
            SourceDetail.BuildCode = emitResult;

            return this;
        }

        public Compiler Run()
        {
            var assemblyLoadContextWeakRef = LoadAndExecute(SourceDetail.BuildCode, null);

            for (var i = 0; i < 8 && assemblyLoadContextWeakRef.IsAlive; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            Debug.WriteLine(assemblyLoadContextWeakRef.IsAlive ? "Unloading failed!" : "Unloading success!",ConsoleColor.White);

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
            PrintLnC("Resolving: " + assemblyName.FullName,ConsoleColor.White);
            return assembly;
        }

        private IEnumerable<MetadataReference> GetRefs(params string[] moduleNameRef)
        {
            List<MetadataReference> metadataReferences = null;

            foreach (var nameRef in moduleNameRef)
            {
                if (metadataReferences == null)
                    metadataReferences = new List<MetadataReference>();

                metadataReferences.Add(MetadataReference.CreateFromFile(nameRef));
            }

            return metadataReferences;
        }

        public static List<string> GetClassesContent(string code)
        {
            SyntaxTree syntaxTree = SyntaxFactory.ParseSyntaxTree(code, null, string.Empty);

            var visitor = new VirtualizationVisitor();
            visitor.Visit(syntaxTree.GetRoot());

            return visitor.ClassesText;
        }

        public static List<string> GetMethodsContent(string code)
        {
            SyntaxTree syntaxTree = SyntaxFactory.ParseSyntaxTree(code, null, string.Empty);

            var visitor = new VirtualizationVisitor();
            visitor.Visit(syntaxTree.GetRoot());

            return visitor.MethodsText;
        }

        private static CSharpCompilation GenerateCode(string assemblyOrModuleName, string code, IEnumerable<MetadataReference> references = null)
        {
            SyntaxTree syntaxTree = SyntaxFactory.ParseSyntaxTree(code, null, string.Empty);

            //IEnumerable<MetadataReference> refs = new MetadataReference[]
            //{
            //    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),                
            //    MetadataReference.CreateFromFile(typeof(System.Runtime.AssemblyTargetedPatchBandAttribute).Assembly.Location),
            //    MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo).Assembly.Location),
            //    MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            //    MetadataReference.CreateFromFile(typeof(ConsoleHelpers).Assembly.Location),
            //};

            var domainAssemblys = AppDomain.CurrentDomain.GetAssemblies();
            var metadataReferenceList = new List<MetadataReference>();
            foreach (var assembl in domainAssemblys)
            {
                if (assembl.Location.IsNull())
                    continue;
                var assemblyMetadata = AssemblyMetadata.CreateFromFile(assembl.Location);
                var metadataReference = assemblyMetadata.GetReference();
                metadataReferenceList.Add(metadataReference);
            }

            //if (references != null)
            //    refs.Concat(references);

            // create and return the compilation
            CSharpCompilation compilation = CSharpCompilation.Create
            (
                assemblyOrModuleName,
                new[] { syntaxTree },
                options: new CSharpCompilationOptions(OutputKind.ConsoleApplication,
                   optimizationLevel: OptimizationLevel.Release,
                   assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default),
                references: metadataReferenceList
            );

            return compilation;
        }

    }

    internal class VirtualizationVisitor : CSharpSyntaxRewriter
    {
        public List<string> ClassesText = new List<string>();
        public List<string> MethodsText = new List<string>();

        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            node = (MethodDeclarationSyntax)base.VisitMethodDeclaration(node);

            string methodBody = node.GetText().ToString();
            MethodsText.Add(methodBody);

            return node;
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            node = (ClassDeclarationSyntax)base.VisitClassDeclaration(node);

            string classBody = node.GetText().ToString();
            ClassesText.Add(classBody);

            return node;
        }
    }

}
