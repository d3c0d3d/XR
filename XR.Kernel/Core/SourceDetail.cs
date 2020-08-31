namespace XR.Kernel.Core
{
    public class SourceDetail
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
        public byte[] BuildCode { get; set; }
    }

}
