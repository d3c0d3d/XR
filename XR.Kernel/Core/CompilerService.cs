using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using XR.Std;
using System.Diagnostics;
using System.Reflection.Metadata;
using XR.Std.Logging;

namespace XR.Kernel.Core
{
    public class CompilerService
    {
        public static readonly Logger _logger = LoggerFactory.CreateLogger(LogLevel.Info, Util.GetEnvLoggerFile(Settings.XR_LOGGER_ENV));

        public SourceDetail SourceDetail { get; private set; }

        public CompilerService Build(string assemblyName, string location, bool useCsharpCode = false, params string[] moduleRef)
        {
            var buildWatch = new Stopwatch();
            buildWatch.Start();

            _logger.Info("Build Start");

            string rawData = SourceParse.GetSourceFileRaw(location, useCsharpCode);

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
            assemblyLoadContext.Unloading += AssemblyLoadContext_Unloading;

            var assembly = assemblyLoadContext.LoadFromStream(asm);

            var entry = assembly.EntryPoint;

            _ = entry != null && entry.GetParameters().Length > 0
                ? entry.Invoke(null, new object[] { args })
                : entry.Invoke(null, null);

            assemblyLoadContext.Unload();

            return new WeakReference(assemblyLoadContext);
        }

        private static void AssemblyLoadContext_Unloading(AssemblyLoadContext context)
        {
            _logger.Info($"Unloading: {context.Name}");
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
            var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp9);

            _logger.Info("Code Syntax Parse...");
            SyntaxTree syntaxTree = SyntaxFactory.ParseSyntaxTree(code, options);
            _logger.Info("Code Syntax Parse = OK");

            if (assemblyRefs != null)
            {
                foreach (var assemblyName in assemblyRefs)
                {
                    var dllname = assemblyName;
                    _logger.Info($"Loading: {dllname}");

                    if (dllname.Contains("%ProgramFiles(x86)%"))
                        dllname = assemblyName.Replace("%ProgramFiles(x86)%", Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86));
                    if (dllname.Contains("%ProgramFiles%"))
                        dllname = assemblyName.Replace("%ProgramFiles%", Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));

                    Assembly.LoadFrom(dllname).GetReferencedAssemblies();
                }
            }

            var domainAssemblys = AppDomain.CurrentDomain.GetAssemblies();
            var metadataReferenceList = new List<MetadataReference>();

            foreach (var assembl in domainAssemblys)
            {
                _logger.Info($"Loading: {assembl.GetName()}");
                //if (!assembl.Location.IsNull())
                //    _logger.Info($"Dll Location: {assembl.Location}");

                //if (assembl.Location.IsNull())
                //    continue;
                unsafe
                {
                    assembl.TryGetRawMetadata(out byte* blob, out int length);                    
                    var moduleMetadata = ModuleMetadata.CreateFromMetadata((IntPtr)blob, length);
                    var assemblyMetadata = AssemblyMetadata.Create(moduleMetadata);

                    // Not work in Net 5.0 Self-Contained Single File, Cause 'Path' Exception
                    //var assemblyMetadata = AssemblyMetadata.CreateFromFile(assembl.Location);
                    var metadataReference = assemblyMetadata.GetReference();
                    metadataReferenceList.Add(metadataReference);
                }
            }
            unsafe
            {
                // Add extra refs
                typeof(Process).Assembly.TryGetRawMetadata(out byte* blob, out int length);
                typeof(FileSystemWatcher).Assembly.TryGetRawMetadata(out byte* blob2, out int length2);
                typeof(System.Drawing.Bitmap).Assembly.TryGetRawMetadata(out byte* blob3, out int length3);
                typeof(System.Reactive.Observer).Assembly.TryGetRawMetadata(out byte* blob4, out int length4);
                typeof(System.ComponentModel.Component).Assembly.TryGetRawMetadata(out byte* blob5, out int length5);
                typeof(System.Linq.Expressions.Expression).Assembly.TryGetRawMetadata(out byte* blob6, out int length6);
                typeof(System.Configuration.ConfigurationManager).Assembly.TryGetRawMetadata(out byte* blob7, out int length7);

                metadataReferenceList.Add(AssemblyMetadata.Create(ModuleMetadata.CreateFromMetadata((IntPtr)blob, length)).GetReference());
                metadataReferenceList.Add(AssemblyMetadata.Create(ModuleMetadata.CreateFromMetadata((IntPtr)blob2, length2)).GetReference());
                metadataReferenceList.Add(AssemblyMetadata.Create(ModuleMetadata.CreateFromMetadata((IntPtr)blob3, length3)).GetReference());
                metadataReferenceList.Add(AssemblyMetadata.Create(ModuleMetadata.CreateFromMetadata((IntPtr)blob4, length4)).GetReference());
                metadataReferenceList.Add(AssemblyMetadata.Create(ModuleMetadata.CreateFromMetadata((IntPtr)blob5, length5)).GetReference());
                metadataReferenceList.Add(AssemblyMetadata.Create(ModuleMetadata.CreateFromMetadata((IntPtr)blob6, length6)).GetReference());
                metadataReferenceList.Add(AssemblyMetadata.Create(ModuleMetadata.CreateFromMetadata((IntPtr)blob7, length7)).GetReference());

            }

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
