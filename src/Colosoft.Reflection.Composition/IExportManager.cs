using System;
using System.Collections.Generic;

namespace Colosoft.Reflection.Composition
{
    public interface IExportManager
    {
        event EventHandler Started;

        event EventHandler<ExportManagerStartErrorArgs> StartError;

        bool IsStarted { get; }

        IExport GetExport(TypeName contractTypeName, string contractName, string uiContext);

        IEnumerable<IExport> GetExports(TypeName contractTypeName, string contractName, string uiContext);

        IEnumerable<IExport> GetExports(string uiContext);

        void Start(string[] uiContexts, bool throwError);
    }
}
