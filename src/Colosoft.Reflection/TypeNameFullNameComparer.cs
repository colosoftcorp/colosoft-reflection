using System;
using System.Collections.Generic;

namespace Colosoft.Reflection
{
    public class TypeNameFullNameComparer : IComparer<TypeName>, IEqualityComparer<TypeName>
    {
        public static readonly TypeNameFullNameComparer Instance = new TypeNameFullNameComparer();

        public int Compare(TypeName x, TypeName y)
        {
            var xIsNull = object.ReferenceEquals(x, null);
            var yIsNull = object.ReferenceEquals(y, null);

#pragma warning disable S2589 // Boolean expressions should not be gratuitous
            if (xIsNull && yIsNull)
            {
                return 0;
            }
            else if ((!xIsNull && yIsNull) || (xIsNull && !yIsNull))
            {
                return -1;
            }
#pragma warning restore S2589 // Boolean expressions should not be gratuitous

            return StringComparer.Ordinal.Compare(x?.FullName, y?.FullName);
        }

        public bool Equals(TypeName x, TypeName y)
        {
            var xIsNull = object.ReferenceEquals(x, null);
            var yIsNull = object.ReferenceEquals(y, null);

#pragma warning disable S2589 // Boolean expressions should not be gratuitous
            if (xIsNull && yIsNull)
            {
                return true;
            }
            else if ((!xIsNull && yIsNull) || (xIsNull && !yIsNull))
            {
                return false;
            }
#pragma warning restore S2589 // Boolean expressions should not be gratuitous

            return StringComparer.Ordinal.Equals(x?.FullName, y?.FullName);
        }

        public int GetHashCode(TypeName obj)
        {
            if (object.ReferenceEquals(obj, null) || obj.FullName == null)
            {
                return 0;
            }

            return obj.FullName.GetHashCode();
        }
    }
}
