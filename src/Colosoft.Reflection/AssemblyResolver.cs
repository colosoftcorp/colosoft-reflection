using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Colosoft.Reflection
{
    public class AssemblyResolver : IAssemblyResolver
    {
        private readonly AppDomain appDomain;
        private readonly IEnumerable<string> deploymentParts;

        public bool IsValid => true;

        public Dictionary<string, Assembly> Assemblies { get; }

        public AssemblyResolver(
            AppDomain appDomain,
            IEnumerable<string> deploymentParts,
            Dictionary<string, Assembly> assemblies = null)
        {
            this.appDomain = appDomain;
            this.deploymentParts = deploymentParts;
            this.Assemblies = assemblies ?? new Dictionary<string, Assembly>(StringComparer.InvariantCultureIgnoreCase);
        }

        public bool Resolve(ResolveEventArgs args, out Assembly assembly, out Exception error)
        {
            if (args is null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            var libraryName = AssemblyNameResolver.GetAssemblyName(args.Name);

            if (!libraryName.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
            {
                libraryName = string.Concat(libraryName, ".dll");
            }

            if (this.Assemblies.TryGetValue(libraryName, out assembly))
            {
                error = null;
                return true;
            }

            // Tenta localizar a parte associado
            var part = this.deploymentParts
                .FirstOrDefault(f => string.Compare(System.IO.Path.GetFileName(f), libraryName, true, System.Globalization.CultureInfo.InstalledUICulture) == 0);

            if (part != null)
            {
                try
                {
#if !NETSTANDARD2_0 && !NETCOREAPP2_0
                        var name = System.Reflection.AssemblyName.GetAssemblyName(part);
                        assembly = this.appDomain.Load(name);
#else
                    assembly = System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromAssemblyPath(part);
#endif
                    assembly.GetTypes();
                }
                catch (Exception ex)
                {
                    error = ex;
                    return false;
                }

                this.Assemblies.Add(libraryName, assembly);
                error = null;
                return true;
            }

            error = null;
            return false;
        }
    }
}
