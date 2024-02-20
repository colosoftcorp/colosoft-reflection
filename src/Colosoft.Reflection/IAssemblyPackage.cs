using System;
using System.Collections.Generic;

namespace Colosoft.Reflection
{
    public interface IAssemblyPackage : IEnumerable<AssemblyPart>
    {
        Guid Uid { get; }

        int Count { get; }

        DateTime CreateTime { get; }

        AssemblyPart this[int index] { get; }

        System.Reflection.Assembly GetAssembly(AssemblyPart name);

        System.Reflection.Assembly LoadAssemblyGuarded(AssemblyPart name, out Exception exception);

        System.IO.Stream GetAssemblyStream(AssemblyPart name);

        bool ExtractPackageFiles(string outputDirectory, bool canOverride);

        bool Contains(AssemblyPart assemblyPart);
    }
}
