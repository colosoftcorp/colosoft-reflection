using System.Collections.Generic;

namespace Colosoft.Reflection
{
    public class TypeNameEqualityComparer : IEqualityComparer<TypeName>
    {
        public static readonly TypeNameEqualityComparer Instance = new TypeNameEqualityComparer();

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

            var xName = string.Concat(x?.FullName, ", ", x?.AssemblyName.Name);
            var yName = string.Concat(y?.FullName, ", ", y?.AssemblyName.Name);

            return xName.Equals(yName);
        }

        public int GetHashCode(TypeName obj)
        {
            if (object.ReferenceEquals(obj, null))
            {
                return 0;
            }

            return string.Concat(obj.FullName, ", ", obj.AssemblyName.Name).GetHashCode();
        }
    }
}
