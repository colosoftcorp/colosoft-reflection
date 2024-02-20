using System;

namespace Colosoft.Reflection
{
    public interface IAssemblyResolverExt : IAssemblyResolver
    {
        event AssemblyResolverLoadHandler Loaded;
    }
}
