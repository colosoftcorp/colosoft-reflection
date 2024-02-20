using System;
using System.Collections.Generic;
using System.Linq;

namespace Colosoft.Reflection
{
    public class AssemblyResolverManager : IEnumerable<IAssemblyResolver>, IDisposable
    {
        private readonly object objLock = new object();
        private readonly List<IAssemblyResolver> resolvers = new List<IAssemblyResolver>();
        private readonly Dictionary<string, System.Reflection.Assembly> assemblies =
            new Dictionary<string, System.Reflection.Assembly>(StringComparer.InvariantCultureIgnoreCase);
        private AppDomain appDomain;
        private List<string> loadedAssemblies;

        public int Count => this.resolvers.Count;

        public IAssemblyResolver this[int index]
        {
            get { return this.resolvers[index]; }
        }

        public AppDomain AppDomain
        {
            get { return this.appDomain; }
        }

        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.LinkDemand, ControlAppDomain = true)]
        public AssemblyResolverManager(AppDomain appDomain)
        {
            this.appDomain = appDomain ?? throw new ArgumentNullException(nameof(appDomain));
            this.appDomain.AssemblyResolve += this.AppDomainAssemblyResolve;
            this.appDomain.AssemblyLoad += this.AppDomainAssemblyLoad;

            this.InitializeAssemblies();
        }

        private void InitializeAssemblies()
        {
            this.loadedAssemblies = this.appDomain.GetAssemblies().Select(f => f.GetName().Name).OrderBy(f => f).ToList();
        }

        private System.Reflection.Assembly AppDomainAssemblyResolve(object sender, ResolveEventArgs args)
        {
            System.Reflection.Assembly assembly = null;

            var assemblyName = AssemblyNameResolver.GetAssemblyNameWithoutExtension(args.Name);

            lock (this.assemblies)
            {
                if (this.assemblies.TryGetValue(assemblyName, out assembly))
                {
                    return assembly;
                }
            }

            Exception lastException = null;

            foreach (var i in this.resolvers.ToArray())
            {
                Exception error = null;

                try
                {
                    // Tenta resolver o assembly solicitado
                    if (i.IsValid && i.Resolve(args, out assembly, out error))
                    {
                        lock (this.assemblies)
                        {
                            if (!this.assemblies.ContainsKey(assemblyName))
                            {
                                this.assemblies.Add(assemblyName, assembly);
                            }
                            else
                            {
                                return this.assemblies[assemblyName];
                            }
                        }

                        return assembly;
                    }
                }
                catch (Exception ex)
                {
                    error = ex;
                }

                if (error != null)
                {
                    lastException = error;
                }
            }

            if (lastException != null)
            {
                throw lastException;
            }

            return null;
        }

        private void AppDomainAssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            var assemblyName = args.LoadedAssembly.GetName().Name;

            lock (this.loadedAssemblies)
            {
                var index = this.loadedAssemblies.BinarySearch(assemblyName);

                if (index < 0)
                {
                    this.loadedAssemblies.Insert(~index, assemblyName);
                }
            }

            lock (this.assemblies)
            {
                if (!this.assemblies.ContainsKey(assemblyName))
                {
                    this.assemblies.Add(assemblyName, args.LoadedAssembly);
                }
            }
        }

        private void AssemblyResolverExtLoaded(object sender, AssemblyResolverLoadEventArgs e)
        {
            lock (this.assemblies)
            {
                foreach (var i in e.Result)
                {
                    if (i.Assembly != null)
                    {
                        var name = i.Assembly.GetName().Name;
                        if (!this.assemblies.ContainsKey(name))
                        {
                            this.assemblies.Add(name, i.Assembly);
                        }
                    }
                }
            }
        }

        public bool CheckAssembly(string assemblyName)
        {
            lock (this.loadedAssemblies)
            {
                return this.loadedAssemblies.BinarySearch(assemblyName, StringComparer.InvariantCultureIgnoreCase) >= 0;
            }
        }

        public void Insert(int index, IAssemblyResolver resolver)
        {
            if (resolver is null)
            {
                throw new ArgumentNullException(nameof(resolver));
            }

            lock (this.objLock)
            {
                if (resolver is IAssemblyResolverExt assemblyResolverExt)
                {
                    assemblyResolverExt.Loaded += this.AssemblyResolverExtLoaded;
                }

                this.resolvers.Insert(index, resolver);
            }
        }

        public void Add(IAssemblyResolver resolver)
        {
            if (resolver is null)
            {
                throw new ArgumentNullException(nameof(resolver));
            }

            lock (this.objLock)
            {
                if (resolver is IAssemblyResolverExt assemblyResolverExt)
                {
                    assemblyResolverExt.Loaded += this.AssemblyResolverExtLoaded;
                }

                this.resolvers.Add(resolver);
            }
        }

        public void Remove(IAssemblyResolver resolver)
        {
            if (resolver is null)
            {
                throw new ArgumentNullException(nameof(resolver));
            }

            lock (this.objLock)
            {
                if (resolver is IAssemblyResolverExt assemblyResolverExt)
                {
                    assemblyResolverExt.Loaded -= this.AssemblyResolverExtLoaded;
                }

                this.resolvers.Remove(resolver);
            }
        }

        public void Clear()
        {
            lock (this.objLock)
            {
                foreach (var i in this.resolvers)
                {
                    if (i is IAssemblyResolverExt assemblyResolverExt)
                    {
                        assemblyResolverExt.Loaded -= this.AssemblyResolverExtLoaded;
                    }
                }

                this.resolvers.Clear();
            }
        }

        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand, ControlAppDomain = true)]
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand, ControlAppDomain = true)]
        protected virtual void Dispose(bool disposing)
        {
            if (this.appDomain != null)
            {
                this.appDomain.AssemblyResolve -= this.AppDomainAssemblyResolve;
                this.appDomain.AssemblyLoad -= this.AppDomainAssemblyLoad;
                this.appDomain = null;

                this.assemblies.Clear();
            }

            this.Clear();
        }

        public IEnumerator<IAssemblyResolver> GetEnumerator()
        {
            return this.resolvers.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.resolvers.GetEnumerator();
        }
    }
}
