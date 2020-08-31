using System;
using Microsoft.CodeAnalysis;
using System.IO;
using System.Linq;

namespace XR.Kernel.Core
{
    internal static class CompilerEx
    {
        internal static byte[] EmitToArray(this Compilation compilation)
        {
            using var stream = new MemoryStream();
            var emitResult = compilation.Emit(stream);

            if (!emitResult.Success)
            {
                // if not successful, throw an exception
                Diagnostic firstError =
                    emitResult.Diagnostics.FirstOrDefault
                        (
                            diagnostic =>
                                diagnostic.Severity == DiagnosticSeverity.Error
                        );

                throw new Exception(firstError?.GetMessage());
            }

            return stream.ToArray();
        }
    }

}
