using System.Runtime.Loader;

namespace XR.Kernel
{
    internal class AssemblyLoaderContext : AssemblyLoadContext
    {   
        public AssemblyLoaderContext() : base(true)
        {

        }
    }

}
