using System;
using System.Collections.Generic;
using System.Linq;

namespace Colosoft.Reflection
{
    public class AssemblyLoader : IAssemblyLoader
    {
        private readonly IAssemblyRepository assemblyRepository;
        private readonly AppDomain domain;

        public AssemblyLoader(AppDomain domain, IAssemblyRepository assemblyRepository)
        {
            this.domain = domain;
            this.assemblyRepository = assemblyRepository ?? throw new ArgumentNullException(nameof(assemblyRepository));
        }

        public bool TryGet(string assemblyName, out System.Reflection.Assembly assembly)
        {
            Exception exception = null;
            return this.TryGet(assemblyName, out assembly, out exception);
        }

        public bool TryGet(string assemblyName, out System.Reflection.Assembly assembly, out Exception exception)
        {
            var assemblyName2 = new System.Reflection.AssemblyName(assemblyName);

            System.Reflection.Assembly assembly2 = null;
            exception = null;

#pragma warning disable CA1031 // Do not catch general exception types
            try
            {
                assembly2 = this.domain.Load(assemblyName2);
            }
            catch (Exception ex)
            {
                exception = ex;
                assembly = null;
            }

            if (assembly2 != null)
            {
                assembly = assembly2;
                return true;
            }

            var assemblyPart = new AssemblyPart(assemblyName);
            IAssemblyPackage package = null;

            try
            {
                var container = this.assemblyRepository.GetAssemblyPackages(new AssemblyPart[]
                {
                    assemblyPart,
                });

                package = container.FirstOrDefault();
            }
            catch (Exception ex)
            {
                exception = ex;
                assembly = null;
                return false;
            }
#pragma warning restore CA1031 // Do not catch general exception types

            if (package != null)
            {
                assembly2 = package.GetAssembly(assemblyPart);

                assembly = assembly2;
                exception = null;

                return assembly2 != null;
            }
            else
            {
                assembly = null;
                return false;
            }
        }

        public AssemblyLoaderGetResult Get(string[] assemblyNames)
        {
            var resultEntries = new List<AssemblyLoaderGetResult.Entry>();
            var assemblyNames2 = new List<string>();

            foreach (var assemblyName in assemblyNames.Distinct())
            {
                var assemblyName2 = new System.Reflection.AssemblyName(assemblyName);
                System.Reflection.Assembly assembly2 = null;

                try
                {
                    assembly2 = this.domain.Load(assemblyName2);
                }
                catch
                {
                    // ignore
                }

                if (assembly2 == null)
                {
                    assemblyNames2.Add(assemblyName);
                }
            }

            AssemblyPackageContainer packagesContainer = null;
            var assemblyParts = assemblyNames2.Select(f => new AssemblyPart(f)).ToArray();

            try
            {
                packagesContainer = this.assemblyRepository.GetAssemblyPackages(assemblyParts);
            }
            catch (Exception ex)
            {
                // Percorre os nome dos assemblies
                foreach (var assemblyName in assemblyNames2)
                {
                    // Registra que ocorreu um erro ao carregar o assembly
                    resultEntries.Add(new AssemblyLoaderGetResult.Entry(assemblyName, null, false, ex));
                }
            }

            if (packagesContainer != null)
            {
                foreach (var assemblyPart in assemblyParts)
                {
                    var packageFound = false;

                    foreach (var package in packagesContainer)
                    {
                        if (package.Contains(assemblyPart))
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
                                    new AssemblyLoaderGetResult.Entry(assemblyPart.Source, null, false, ex));

                                continue;
                            }

                            resultEntries.Add(
                                new AssemblyLoaderGetResult.Entry(assemblyPart.Source, assembly, true, null));

                            break;
                        }
                    }

                    if (!packageFound)
                    {
                        resultEntries.Add(new AssemblyLoaderGetResult.Entry(assemblyPart.Source, null, true, null));
                    }
                }
            }
            else
            {
                foreach (var assemblyName in assemblyNames2)
                {
                    resultEntries.Add(new AssemblyLoaderGetResult.Entry(assemblyName, null, true, null));
                }
            }

            return new AssemblyLoaderGetResult(resultEntries);
        }
    }
}
