using System;
using System.Collections.Generic;
using System.Linq;

namespace Colosoft.Reflection
{
    /// <summary>
    /// Implementação padrão para o AssemblyResolver.
    /// </summary>
    public class AssemblyPartsResolver : IAssemblyResolverExt, IDisposable
    {
        private readonly object objLock = new object();
        private readonly IAssemblyRepository assemblyRepository;
        private IEnumerable<AssemblyPart> assemblyParts;
        private AssemblyLoaderGetResult loadAssembliesResult;

        private bool isValid = true;

        public event AssemblyResolverLoadHandler Loaded;

        public bool IsValid
        {
            get { return this.isValid; }
        }

        public AssemblyLoaderGetResult LoadAssembliesResult
        {
            get { return this.loadAssembliesResult; }
        }

        public AssemblyPartsResolver(
            IAssemblyRepository assemblyRepository,
            IEnumerable<AssemblyPart> assemblyParts)
        {
            this.assemblyRepository = assemblyRepository ?? throw new ArgumentNullException(nameof(assemblyRepository));
            this.assemblyParts = assemblyParts ?? throw new ArgumentNullException(nameof(assemblyParts));
        }

        ~AssemblyPartsResolver()
        {
            this.Dispose(false);
        }

        private AssemblyLoaderGetResult LoadAssemblies()
        {
            var resultEntries = new List<AssemblyLoaderGetResult.Entry>();

            AssemblyPackageContainer packagesContainer = null;
            var assemblyParts1 = this.assemblyParts == null ? Array.Empty<AssemblyPart>() : this.assemblyParts.ToArray();

            try
            {
                // Recupera o pacote dos assemblies
                packagesContainer = this.assemblyRepository.GetAssemblyPackages(assemblyParts1);
            }
            catch (Exception ex)
            {
                if (ex is Net.DownloaderException)
                {
                    ex = new AssemblyResolverException(
                        ResourceMessageFormatter.Create(
                            () => Properties.Resources.AssemblyResolver_GetAssemblyDownloaderException,
                            ex.Message).Format(),
                        ex);
                }
                else
                {
                    ex = new AssemblyResolverException(ex.Message, ex);
                }

                foreach (var assemblyPart in this.assemblyParts)
                {
                    resultEntries.Add(
                        new AssemblyLoaderGetResult.Entry(assemblyPart.Source.GetAssemblyNameWithoutExtension(), null, false, ex));
                }

                return new AssemblyLoaderGetResult(resultEntries);
            }

            if (packagesContainer != null)
            {
                foreach (var assemblyPart in assemblyParts1)
                {
                    var packageFound = false;
                    foreach (var package in packagesContainer.Where(package => package.Contains(assemblyPart)))
                    {
                        packageFound = true;
                        System.Reflection.Assembly assembly = null;
                        try
                        {
                            assembly = package.GetAssembly(assemblyPart);
                        }
                        catch (Exception ex)
                        {
                            resultEntries.Add(
                                new AssemblyLoaderGetResult.Entry(assemblyPart.Source.GetAssemblyNameWithoutExtension(), null, false, ex));

                            continue;
                        }

                        resultEntries.Add(new AssemblyLoaderGetResult.Entry(assembly.GetName().Name, assembly, true, null));

                        break;
                    }

                    if (!packageFound)
                    {
                        resultEntries.Add(new AssemblyLoaderGetResult.Entry(assemblyPart.Source.GetAssemblyNameWithoutExtension(), null, true, null));
                    }
                }
            }
            else
            {
                foreach (var assemblyPart in this.assemblyParts)
                {
                    resultEntries.Add(
                        new AssemblyLoaderGetResult.Entry(assemblyPart.Source.GetAssemblyNameWithoutExtension(), null, true, null));
                }
            }

            return new AssemblyLoaderGetResult(resultEntries);
        }

        public bool Resolve(ResolveEventArgs args, out System.Reflection.Assembly assembly, out Exception error)
        {
            if (!this.IsValid)
            {
                assembly = null;
                error = null;
                return false;
            }

            this.isValid = false;

            try
            {
                lock (this.objLock)
                {
                    if (this.loadAssembliesResult == null)
                    {
                        try
                        {
                            // Carrega os assemblies
                            this.loadAssembliesResult = this.LoadAssemblies();
                        }
                        catch (Exception ex)
                        {
                            error = ex;
                            assembly = null;
                            return false;
                        }

                        if (this.Loaded != null)
                        {
                            try
                            {
                                this.Loaded(
                                    this,
                                    new AssemblyResolverLoadEventArgs
                                    {
                                        Result = this.loadAssembliesResult,
                                    });
                            }
                            catch (Exception ex)
                            {
                                error = ex;
                                assembly = null;
                                return false;
                            }
                        }
                    }

                    var assemblyName = AssemblyNameResolver.GetAssemblyNameWithoutExtension(args?.Name);

                    var entry = this.loadAssembliesResult
                        .FirstOrDefault(f => StringComparer.InvariantCultureIgnoreCase.Equals(f.AssemblyName, assemblyName));

                    // Verifica se não foi encontrado o assembly no resultado
                    if (entry == null || entry.Error != null)
                    {
                        assembly = null;
                        error = entry != null ? entry.Error : null;
                        return false;
                    }

                    assembly = entry.Assembly;
                    error = null;

                    if (assembly != null)
                    {
                        return true;
                    }

                    return false;
                }
            }
            finally
            {
                this.isValid = true;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            this.loadAssembliesResult = null;
            this.assemblyParts = null;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
