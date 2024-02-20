using System;
using System.Collections.Generic;

namespace Colosoft.Reflection
{
    public partial class AssemblyLoaderContextInitializeResult
    {
#pragma warning disable CA1819 // Properties should not return arrays
        public string[] AssembliesLoaded { get; set; }

        public AssemblyLoadError[] AssemblyLoadErrors { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays

        public bool Success { get; set; }

        public Exception Error { get; set; }

        public override string ToString()
        {
            if (this.Error != null)
            {
                return $"Success={this.Success}, Error: {this.Error.Message}";
            }

            return $"Success={this.Success}";
        }
    }
}
