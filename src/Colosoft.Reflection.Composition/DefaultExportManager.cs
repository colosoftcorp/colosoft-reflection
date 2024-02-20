using System;
using System.Collections.Generic;
using System.Linq;

namespace Colosoft.Reflection.Composition
{
    public class DefaultExportManager : IExportManager
    {
        private readonly List<IExport> exports;
        private bool isStarted;
        private string[] uiContexts;

        public event EventHandler Started;

        public event EventHandler<ExportManagerStartErrorArgs> StartError;

        public bool IsStarted => this.isStarted;

        public DefaultExportManager()
        {
            this.exports = new List<IExport>();
        }

        public DefaultExportManager(IEnumerable<IExport> exports)
        {
            if (exports is null)
            {
                throw new ArgumentNullException(nameof(exports));
            }

            this.exports = new List<IExport>(exports);
        }

        protected virtual void OnStarted()
        {
            this.Started?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnStartError(Exception exception, string[] uiContexts)
        {
            this.StartError?.Invoke(this, new ExportManagerStartErrorArgs(exception, uiContexts));
        }

        public IExport GetExport(TypeName contractTypeName, string contractName, string uiContext)
        {
            return this.GetExports(contractTypeName, contractName, uiContext).FirstOrDefault();
        }

        public IEnumerable<IExport> GetExports(TypeName contractTypeName, string contractName, string uiContext)
        {
            foreach (var export in this.GetExports(uiContext))
            {
                if (export.ContractName == contractName &&
                    TypeNameEqualityComparer.Instance.Equals(export.ContractType, contractTypeName))
                {
                    yield return export;
                }
            }
        }

        public IEnumerable<IExport> GetExports(string uiContext)
        {
            if (string.IsNullOrEmpty(uiContext))
            {
                foreach (var i in this.exports)
                {
                    yield return i;
                }
            }

            foreach (var i in this.exports)
            {
                var export2 = i as IExport2;

                if (export2.UIContext == uiContext && (this.uiContexts == null || (!string.IsNullOrEmpty(uiContext) && this.uiContexts.Contains(uiContext))))
                {
                    yield return i;
                }
            }
        }

        public void Start(string[] uiContexts, bool throwError)
        {
            this.isStarted = true;
            this.uiContexts = uiContexts;

            this.OnStarted();
        }
    }
}
