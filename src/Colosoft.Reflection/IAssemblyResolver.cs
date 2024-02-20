using System;

namespace Colosoft.Reflection
{
    public interface IAssemblyResolver
    {
        bool IsValid { get; }

        bool Resolve(ResolveEventArgs args, out System.Reflection.Assembly assembly, out Exception error);
    }
}
