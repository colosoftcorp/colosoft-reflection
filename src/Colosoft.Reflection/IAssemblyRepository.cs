using System;
using System.Collections.Generic;

namespace Colosoft.Reflection
{
    public interface IAssemblyRepository : IDisposable
    {
        event AssemblyRepositoryStartedHandler Started;

        bool IsStarted { get; }

        AssemblyResolverManager AssemblyResolverManager { get; }

        void Start();

        void Add(Guid uid, System.IO.Stream inputStream);

        AssemblyPackageContainer GetAssemblyPackages(IEnumerable<AssemblyPart> assemblyParts);

        IAsyncResult BeginGetAssemblyPackages(IEnumerable<AssemblyPart> assemblyParts, AsyncCallback callback, object state);

        AssemblyPackageContainer EndGetAssemblyPackages(IAsyncResult ar);

        System.IO.Stream GetAssemblyPackageStream(IAssemblyPackage package);

        IAssemblyPackage GetAssemblyPackage(Guid assemblyPackageUid);

        AssemblyRepositoryValidateResult Validate();
    }
}
