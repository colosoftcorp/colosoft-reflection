using System;
using System.Runtime.Serialization;

namespace Colosoft.Reflection
{
    [Serializable]
    public class AssemblyResolverException : Exception
    {
        public AssemblyResolverException()
        {
        }

        public AssemblyResolverException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public AssemblyResolverException(string message)
            : base(message)
        {
        }

        protected AssemblyResolverException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
