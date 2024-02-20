using System;

namespace Colosoft.Reflection
{
    [Serializable]
    public class BindingRedirect
    {
        public Version NewVersion { get; set; }

        public Version OldVersionMax { get; set; }

        public Version OldVersionMin { get; set; }
    }
}
