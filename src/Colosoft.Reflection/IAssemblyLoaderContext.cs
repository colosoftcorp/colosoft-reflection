using System.Collections.Generic;

namespace Colosoft.Reflection
{
    public interface IAssemblyLoaderContext
    {
#pragma warning disable SA1305 // Field names should not use Hungarian notation
        AssemblyLoaderContextInitializeResult InitializeContext(string[] contextNames, string uiContext);

        IEnumerable<string> GetContextAssemblies(string[] contextNames, string uiContext);
#pragma warning restore SA1305 // Field names should not use Hungarian notation
    }
}
