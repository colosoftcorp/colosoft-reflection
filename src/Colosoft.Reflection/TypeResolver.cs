using System;

namespace Colosoft.Reflection
{
    public static class TypeResolver
    {
        public static Type ResolveType(string typeNameOrAlias, IAssemblyLoader assemblyLoader)
        {
            return ResolveType(typeNameOrAlias, true, assemblyLoader);
        }

        public static Type ResolveType(string typeNameOrAlias, bool throwIfResolveFails, IAssemblyLoader assemblyLoader)
        {
            if (string.IsNullOrEmpty(typeNameOrAlias))
            {
                throw new ArgumentException($"'{nameof(typeNameOrAlias)}' cannot be null or empty.", nameof(typeNameOrAlias));
            }

            var typeName = new TypeName(typeNameOrAlias);
            return ResolveType(typeName, throwIfResolveFails, assemblyLoader);
        }

        public static Type ResolveType(TypeName typeName, bool throwIfResolveFails, IAssemblyLoader assemblyLoader)
        {
            if (typeName is null)
            {
                throw new ArgumentNullException(nameof(typeName));
            }

            if (assemblyLoader is null)
            {
                throw new ArgumentNullException(nameof(assemblyLoader));
            }

            var assemblyName = typeName.AssemblyName != null ? typeName.AssemblyName.FullName : null;
            var assemblyFile = $"{assemblyName}.dll";
            Exception error = null;

            System.Reflection.Assembly assembly;
            if (string.IsNullOrEmpty(assemblyName) ||
                !assemblyLoader.TryGet(assemblyFile, out assembly, out error))
            {
                if (throwIfResolveFails)
                {
                    var errorMessage = $"Assembly '{assemblyName}' of type '{typeName.FullName}' not found";

                    if (error != null)
                    {
                        throw new InvalidOperationException(errorMessage, error);
                    }
                    else
                    {
                        throw new InvalidOperationException(errorMessage);
                    }
                }

                return null;
            }

            return assembly.GetType(typeName.FullName, throwIfResolveFails, false);
        }
    }
}
