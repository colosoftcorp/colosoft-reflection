using System;

namespace Colosoft.Reflection
{
    public class AssemblyLoadError
    {
        public string AssemblyName { get; set; }

        public Exception Error { get; set; }

        public override string ToString()
        {
            if (this.Error != null)
            {
                return $"{this.AssemblyName}, Error: {this.Error.Message}";
            }

            return this.AssemblyName;
        }
    }
}
