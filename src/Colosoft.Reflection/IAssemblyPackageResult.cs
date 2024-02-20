using System;

namespace Colosoft.Reflection
{
    public interface IAssemblyPackageResult : IDisposable
    {
        bool ExtractPackageFiles(string outputDirectory, bool canOverride);

        System.IO.Stream GetAssemblyStream(AssemblyPart name);

        System.Reflection.Assembly GetAssembly(AssemblyPart name);

        System.Reflection.Assembly LoadAssemblyGuarded(AssemblyPart name, out Exception exception);
    }
}
