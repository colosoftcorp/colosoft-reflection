using System;
using System.Collections.Generic;

namespace Colosoft.Reflection.Composition
{
    public class ExportComparer : IEqualityComparer<IExport>, IComparer<IExport>
    {
        public static readonly ExportComparer Instance = new ExportComparer();

        public string ToString(IExport export)
        {
            if (export is null)
            {
                throw new ArgumentNullException(nameof(export));
            }

            return $"[{(export.ContractType != null ? export.ContractType.FullName : null)}, {export.ContractName}]";
        }

        public bool Equals(IExport x, IExport y)
        {
            return (x == null && y == null) ||
                    (x != null && y != null &&
                    x.ContractName == y.ContractName &&
                    TypeNameEqualityComparer.Instance.Equals(x.ContractType, y.ContractType));
        }

        public int GetHashCode(IExport obj)
        {
            if (obj == null)
            {
                return 0;
            }

            return $"[{obj.ContractType} : {obj.ContractName}]".GetHashCode();
        }

        public int Compare(IExport x, IExport y)
        {
            if (this.Equals(x, y))
            {
                return 0;
            }

            return StringComparer.Ordinal.Compare(this.ToString(x), this.ToString(y));
        }
    }
}
