using System;
using System.Collections.Generic;

namespace Colosoft.Reflection.Composition
{
    public class ExportManagerStartErrorArgs : EventArgs
    {
        public Exception Error { get; set; }

#pragma warning disable CA1819 // Properties should not return arrays
        public string[] UIContexts { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays

        public ExportManagerStartErrorArgs(Exception error, string[] uiContexts)
        {
            this.Error = error;
            this.UIContexts = uiContexts;
        }
    }
}
