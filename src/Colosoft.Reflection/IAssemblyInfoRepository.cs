using System;
using System.Collections.Generic;

namespace Colosoft.Reflection
{
    public interface IAssemblyInfoRepository : IEnumerable<AssemblyInfo>
    {
        event EventHandler Loaded;

        bool IsLoaded { get; }

        int Count { get; }

        bool IsChanged { get; }

        void Refresh(bool executeAnalyzer);

        bool TryGet(string assemblyName, out AssemblyInfo assemblyInfo, out Exception exception);

        bool Contains(string assemblyName);
    }
}
