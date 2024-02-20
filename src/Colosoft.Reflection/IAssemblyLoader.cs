using System;

namespace Colosoft.Reflection
{
    public interface IAssemblyLoader
    {
        bool TryGet(string assemblyName, out System.Reflection.Assembly assembly);

        bool TryGet(string assemblyName, out System.Reflection.Assembly assembly, out Exception exception);

        AssemblyLoaderGetResult Get(string[] assemblyNames);
    }
}
