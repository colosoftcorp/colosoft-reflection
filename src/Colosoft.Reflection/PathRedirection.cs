using System;
using System.Collections.Generic;

namespace Colosoft.Reflection
{
    [Serializable]
    public class PathRedirection
    {
        public string AssemblyName { get; set; }

        public List<string> Directories { get; } = new List<string>();
    }
}
