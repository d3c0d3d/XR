using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using XR.Kernel.Extensions;
using XR.Kernel.Logging;
using XR.Kernel.Std;
using System.Diagnostics;

namespace XR.Kernel.Core
{
    public class CompilerService
    {
        public static readonly Logger _logger = LoggerFactory.CreateLogger(LogLevel.Info, Util.GetEnvLoggerFile(Statics.XR_LOGGER_ENV));

        public SourceDetail SourceDetail { get; private set; }

        public CompilerService Build(string assemblyName, string location, params string[] moduleRef)
        {
            var buildWatch = new Stopwatch();
            buildWatch.Start();

            _logger.Info("Build Start");

            string rawData = SourceParse.GetSourceFileRaw(location);

            string source = SourceParse.ParseFile(rawData);

            _logger.Info($"---- Source Code Generate ----:\n{source}");

            SourceDetail = new SourceDetail(assemblyName, source, moduleRef);

            var compResult = GenerateCode(SourceDetail.AssemblyName, SourceDetail.SourceCode, SourceDetail.ModuleRef);

            byte[] emitResult = compResult.EmitToArray();

            Cli.PrintLnC($"{location} Success", ConsoleColor.White);

            // update
            SourceDetail.BuildCode = emitResult;

            buildWatch.Stop();
            TimeSpan ts = buildWatch.Elapsed;

            string elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);

            _logger.Info($"Build End - {elapsedTime}");

            return this;
        }

        public CompilerService Run(params string[] args)
        {
            Cli.PrintLnC($"Start Running...", ConsoleColor.White);

            var assemblyLoadContextWeakRef = LoadAndExecute(SourceDetail.BuildCode, args);

            for (var i = 0; i < 8 && assemblyLoadContextWeakRef.IsAlive; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            _logger.Info(assemblyLoadContextWeakRef.IsAlive ? "Unloading failed!" : "Unloading success!");

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
            _logger.Info($"Resolving: {assemblyName.FullName}");
            var assembly = context.LoadFromAssemblyName(assemblyName);
            return assembly;
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

        private static CSharpCompilation GenerateCode(string assemblyOrModuleName, string code, string[] assemblyRefs = null)
        {
            SyntaxTree syntaxTree = SyntaxFactory.ParseSyntaxTree(code, null, string.Empty);

            if (assemblyRefs != null)
            {
                foreach (var assemblyName in assemblyRefs)
                {
                    var dllname = assemblyName;
                    if (dllname.Contains("%ProgramFiles(x86)%"))
                        dllname = assemblyName.Replace("%ProgramFiles(x86)%", Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86));
                    if (dllname.Contains("%ProgramFiles%"))
                        dllname = assemblyName.Replace("%ProgramFiles%", Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));

                    _logger.Info($"Loading: {dllname}");
                    Assembly.LoadFrom(dllname).GetReferencedAssemblies();
                }
            }

            var domainAssemblys = AppDomain.CurrentDomain.GetAssemblies();
            var metadataReferenceList = new List<MetadataReference>();
            foreach (var assembl in domainAssemblys)
            {
                if (assembl.Location.IsNull())
                    continue;
                var assemblyMetadata = AssemblyMetadata.CreateFromFile(assembl.Location);
                _logger.Info($"{nameof(assemblyMetadata)}: {assembl.Location}");
                var metadataReference = assemblyMetadata.GetReference();
                metadataReferenceList.Add(metadataReference);
            }

            // Add extra refs
            metadataReferenceList.Add(MetadataReference.CreateFromFile(typeof(System.Linq.Expressions.Expression).Assembly.Location));
            metadataReferenceList.Add(MetadataReference.CreateFromFile(typeof(System.ComponentModel.Component).Assembly.Location));
            metadataReferenceList.Add(MetadataReference.CreateFromFile(typeof(FileSystemWatcher).Assembly.Location));
            metadataReferenceList.Add(MetadataReference.CreateFromFile(typeof(System.Reactive.Observer).Assembly.Location));
            metadataReferenceList.Add(MetadataReference.CreateFromFile(typeof(System.Drawing.Bitmap).Assembly.Location));
            metadataReferenceList.Add(MetadataReference.CreateFromFile(typeof(Process).Assembly.Location));

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
