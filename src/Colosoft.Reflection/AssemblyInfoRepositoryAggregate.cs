using System;
using System.Collections.Generic;
using System.Linq;

namespace Colosoft.Reflection
{
    /// <summary>
    /// Implementação de um agregador de repositorios de informações de assemblies.
    /// </summary>
    public class AssemblyInfoRepositoryAggregate : IAssemblyInfoRepository
    {
        private readonly List<IAssemblyInfoRepository> assemblyInfoRepositories;
        private bool isLoaded;

        public event EventHandler Loaded;

        public int Count
        {
            get
            {
                var count = 0;
                foreach (var r in this.assemblyInfoRepositories)
                {
                    count += r.Count;
                }

                return count;
            }
        }

        public bool IsChanged
        {
            get
            {
                foreach (var r in this.assemblyInfoRepositories)
                {
                    if (r.IsChanged)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public bool IsLoaded
        {
            get { return this.isLoaded; }
        }

        public AssemblyInfoRepositoryAggregate(IEnumerable<IAssemblyInfoRepository> assemblyInfoRepositories)
        {
            if (assemblyInfoRepositories is null)
            {
                throw new ArgumentNullException(nameof(assemblyInfoRepositories));
            }

            this.assemblyInfoRepositories = new List<IAssemblyInfoRepository>();

            foreach (var i in assemblyInfoRepositories)
            {
                i.Loaded += new EventHandler(this.EntryLoaded);
                this.assemblyInfoRepositories.Add(i);
            }
        }

        private void EntryLoaded(object sender, EventArgs e)
        {
            lock (this.assemblyInfoRepositories)
            {
                this.isLoaded = this.assemblyInfoRepositories.Count(f => f.IsLoaded) == this.assemblyInfoRepositories.Count;
            }

            if (this.isLoaded)
            {
                this.OnLoaded();
            }
        }

        protected void OnLoaded()
        {
            this.Loaded?.Invoke(this, EventArgs.Empty);
        }

        public void Refresh(bool executeAnalyzer)
        {
            foreach (var r in this.assemblyInfoRepositories)
            {
                r.Refresh(executeAnalyzer);
            }
        }

        public bool TryGet(string assemblyName, out AssemblyInfo assemblyInfo, out Exception exception)
        {
            foreach (var r in this.assemblyInfoRepositories)
            {
                if (r.TryGet(assemblyName, out assemblyInfo, out exception))
                {
                    return true;
                }
            }

            assemblyInfo = null;
            exception = null;
            return false;
        }

        public bool Contains(string assemblyName)
        {
#pragma warning disable S3267 // Loops should be simplified with "LINQ" expressions
            foreach (var r in this.assemblyInfoRepositories)
            {
                if (r.Contains(assemblyName))
                {
                    return true;
                }
            }
#pragma warning restore S3267 // Loops should be simplified with "LINQ" expressions

            return false;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach (var r in this.assemblyInfoRepositories)
            {
                foreach (var j in r)
                {
                    yield return j;
                }
            }
        }

        public IEnumerator<AssemblyInfo> GetEnumerator()
        {
            foreach (var r in this.assemblyInfoRepositories)
            {
                foreach (var j in r)
                {
                    yield return j;
                }
            }
        }
    }
}
