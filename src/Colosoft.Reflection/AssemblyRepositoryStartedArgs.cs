using System;
using System.Collections.Generic;

namespace Colosoft.Reflection
{
    public delegate void AssemblyRepositoryStartedHandler(object sender, AssemblyRepositoryStartedArgs e);

    public class AssemblyRepositoryStartedArgs : EventArgs
    {
#pragma warning disable CA1819 // Properties should not return arrays
        public Exception[] Exceptions { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays

        public AssemblyRepositoryStartedArgs(Exception[] exceptions)
        {
            this.Exceptions = exceptions;
        }
    }
}
