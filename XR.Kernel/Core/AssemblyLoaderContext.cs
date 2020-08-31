using System.Runtime.Loader;

namespace XR.Kernel.Core
{
    internal class AssemblyLoaderContext : AssemblyLoadContext
    {
        public AssemblyLoaderContext() : base(true)
        {

        }
    }

}
