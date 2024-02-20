using System;

namespace Colosoft.Reflection
{
    public static class AssemblyNameResolver
    {
        public static string GetAssemblyNameWithoutExtension(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }

#if !NETSTANDARD2_0 && !NETCOREAPP2_0
            var assemblyName = new System.Reflection.AssemblyName(name);

            // Recupera o nome do assembly
            return assemblyName.Name.GetAssemblyNameWithoutExtension();
#else
            var index = name.IndexOf(",", StringComparison.Ordinal);

            if (index >= 0)
            {
                var assemblyName = name.Substring(0, index);
                var extension = System.IO.Path.GetExtension(assemblyName);

                if (StringComparer.InvariantCultureIgnoreCase.Equals(extension, ".dll") ||
                    StringComparer.InvariantCultureIgnoreCase.Equals(extension, ".exe"))
                {
                    return System.IO.Path.GetFileNameWithoutExtension(assemblyName);
                }
                else
                {
                    return assemblyName;
                }
            }

            return name;
#endif
        }

        public static string GetAssemblyName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }

#if !NETSTANDARD2_0 && !NETCOREAPP2_0
            var assemblyName = new System.Reflection.AssemblyName(name);

            // Recupera o nome do assembly
            return assemblyName.Name;
#else
            var index = name.IndexOf(",", StringComparison.Ordinal);

            if (index >= 0)
            {
                return name.Substring(0, index);
            }

            return name;
#endif
        }
    }
}
