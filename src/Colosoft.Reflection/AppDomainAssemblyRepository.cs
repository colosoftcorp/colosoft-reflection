using System;
using System.Collections.Generic;
using System.Linq;

namespace Colosoft.Reflection
{
    public class AppDomainAssemblyRepository : IAssemblyRepository
    {
        private readonly Dictionary<Guid, AssemblyPackage> packages = new Dictionary<Guid, AssemblyPackage>();
        private readonly AssemblyResolverManager assemblyResolverManager;
        private bool isStarted;

        private bool canDiposeAssemblyResolverManager;

        public event AssemblyRepositoryStartedHandler Started;

        public bool IsStarted
        {
            get { return this.isStarted; }
        }

        public AssemblyResolverManager AssemblyResolverManager
        {
            get { return this.assemblyResolverManager; }
        }

        public AppDomainAssemblyRepository()
            : this(AppDomain.CurrentDomain)
        {
        }

        public AppDomainAssemblyRepository(AppDomain domain)
        {
            if (domain is null)
            {
                throw new ArgumentNullException(nameof(domain));
            }

            this.assemblyResolverManager = new AssemblyResolverManager(domain);
            this.canDiposeAssemblyResolverManager = true;
        }

        public AppDomainAssemblyRepository(AssemblyResolverManager assemblyResolverManager)
        {
            this.assemblyResolverManager = assemblyResolverManager ?? throw new ArgumentNullException(nameof(assemblyResolverManager));
        }

        protected void OnStarted(AssemblyRepositoryStartedArgs e)
        {
            this.Started?.Invoke(this, e);
        }

        private void DoGetAssemblyPackages(object callState)
        {
            var arguments = (object[])callState;
            var asyncResult = (Threading.AsyncResult<AssemblyPackageContainer>)arguments[0];
            var assemblyParts = (IEnumerable<AssemblyPart>)arguments[1];

            AssemblyPackageContainer packageContainer = null;

#pragma warning disable CA1031 // Do not catch general exception types
            try
            {
                packageContainer = this.GetAssemblyPackages(assemblyParts);
            }
            catch (Exception ex)
            {
                asyncResult.HandleException(ex, false);
                return;
            }
#pragma warning restore CA1031 // Do not catch general exception types

            asyncResult.Complete(packageContainer, false);
        }

        public void Start()
        {
            if (this.isStarted)
            {
                return;
            }

            this.isStarted = true;

            this.OnStarted(new AssemblyRepositoryStartedArgs(null));
        }

        public void Add(Guid uid, System.IO.Stream inputStream)
        {
            throw new NotSupportedException();
        }

        public IAsyncResult BeginGetAssemblyPackages(
            IEnumerable<AssemblyPart> assemblyParts,
            AsyncCallback callback,
            object state)
        {
            var asyncResult = new Threading.AsyncResult<AssemblyPackageContainer>(callback, state);

            var arguments = new object[] { asyncResult, assemblyParts };

            if (!System.Threading.ThreadPool.QueueUserWorkItem(this.DoGetAssemblyPackages, arguments))
            {
                this.DoGetAssemblyPackages(arguments);
            }

            return asyncResult;
        }

        public AssemblyPackageContainer EndGetAssemblyPackages(IAsyncResult ar)
        {
            var asyncResult = (Threading.AsyncResult<AssemblyPackageContainer>)ar;

            if (asyncResult?.Exception != null)
            {
                throw asyncResult.Exception;
            }

            return asyncResult?.Result;
        }

        public AssemblyPackageContainer GetAssemblyPackages(IEnumerable<AssemblyPart> assemblyParts)
        {
            var sourceParts = assemblyParts.ToList();
            var assemblies = new List<AssemblyPart>();

            foreach (var i in this.AssemblyResolverManager.AppDomain.GetAssemblies())
            {
                for (var j = 0; j < sourceParts.Count; j++)
                {
                    if (string.Compare(sourceParts[j].Source, string.Concat(i.GetName().Name, ".dll"), true, System.Globalization.CultureInfo.InvariantCulture) == 0)
                    {
                        sourceParts[j].Assembly = i;
                        assemblies.Add(sourceParts[j]);
                        sourceParts.RemoveAt(j--);
                    }
                }
            }

            var pkg = new AssemblyPackage(assemblies)
            {
                Uid = Guid.NewGuid(),
            };

            this.packages.Add(pkg.Uid, pkg);

            return new AssemblyPackageContainer(pkg);
        }

        public System.IO.Stream GetAssemblyPackageStream(IAssemblyPackage package)
        {
            throw new NotSupportedException();
        }

        public IAssemblyPackage GetAssemblyPackage(Guid assemblyPackageUid)
        {
            AssemblyPackage pkg = null;

            if (this.packages.TryGetValue(assemblyPackageUid, out pkg))
            {
                return pkg;
            }

#pragma warning disable S1168 // Empty arrays and collections should be returned instead of null
            return null;
#pragma warning restore S1168 // Empty arrays and collections should be returned instead of null
        }

        public AssemblyRepositoryValidateResult Validate()
        {
            return new AssemblyRepositoryValidateResult(Array.Empty<AssemblyRepositoryValidateResult.Entry>());
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            this.packages.Clear();

            if (this.canDiposeAssemblyResolverManager)
            {
                this.assemblyResolverManager.Dispose();
            }
        }
    }
}
