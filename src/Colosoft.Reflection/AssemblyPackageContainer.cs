using System.Collections.Generic;

namespace Colosoft.Reflection
{
    public class AssemblyPackageContainer : IEnumerable<IAssemblyPackage>
    {
        private readonly List<IAssemblyPackage> packages;

        public int Count
        {
            get { return this.packages.Count; }
        }

        public IAssemblyPackage this[int index]
        {
            get { return this.packages[index]; }
        }

        public AssemblyPackageContainer(IAssemblyPackage package)
        {
            this.packages = new List<IAssemblyPackage>();
            this.packages.Add(package);
        }

        public AssemblyPackageContainer(IEnumerable<IAssemblyPackage> packages)
        {
            this.packages = new List<IAssemblyPackage>(packages);
        }

        public IEnumerator<IAssemblyPackage> GetEnumerator()
        {
            return this.packages.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.packages.GetEnumerator();
        }
    }
}
