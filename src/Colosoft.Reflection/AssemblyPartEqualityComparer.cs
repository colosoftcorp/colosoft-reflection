using System.Collections.Generic;

namespace Colosoft.Reflection
{
    public class AssemblyPartEqualityComparer : IEqualityComparer<AssemblyPart>
    {
        public static readonly AssemblyPartEqualityComparer Instance = new AssemblyPartEqualityComparer();

        public bool Equals(AssemblyPart x, AssemblyPart y)
        {
            return string.Equals(x?.Source, y?.Source);
        }

        public int GetHashCode(AssemblyPart obj)
        {
            if (!object.ReferenceEquals(obj, null))
            {
                return obj.GetHashCode();
            }

            return 0;
        }
    }
}
