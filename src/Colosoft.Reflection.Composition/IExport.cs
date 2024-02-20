using System.Collections.Generic;

namespace Colosoft.Reflection.Composition
{
    public interface IExport
    {
        TypeName Type { get; }

        string ContractName { get; }

        TypeName ContractType { get; }

        bool ImportingConstructor { get; }

        CreationPolicy CreationPolicy { get; }

        bool UseDispatcher { get; }

        IDictionary<string, object> Metadata { get; }
    }
}
