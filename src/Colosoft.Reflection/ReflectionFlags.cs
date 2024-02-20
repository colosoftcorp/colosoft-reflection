using System.Reflection;

namespace Colosoft.Reflection
{
    internal static class ReflectionFlags
    {
        public const BindingFlags DefaultCriteria = BindingFlags.Public | BindingFlags.NonPublic;
        public const BindingFlags InstanceCriteria = DefaultCriteria | BindingFlags.Instance;
        public const BindingFlags StaticCriteria = DefaultCriteria | BindingFlags.Static | BindingFlags.FlattenHierarchy;
        public const BindingFlags AllCriteria = InstanceCriteria | StaticCriteria;
    }
}
