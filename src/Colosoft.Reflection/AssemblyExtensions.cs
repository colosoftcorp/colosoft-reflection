using System;

namespace Colosoft.Reflection
{
    public static class AssemblyExtensions
    {
        public static string GetAssemblyNameWithoutExtension(this string assemblyName)
        {
            if (string.IsNullOrEmpty(assemblyName))
            {
                return assemblyName;
            }

            if (assemblyName.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase) ||
                assemblyName.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase))
            {
                assemblyName = System.IO.Path.GetFileNameWithoutExtension(assemblyName);
            }

            return assemblyName;
        }
    }
}
