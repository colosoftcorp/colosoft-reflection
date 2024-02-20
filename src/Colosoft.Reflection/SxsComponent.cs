using System;

namespace Colosoft.Reflection
{
    internal class SxsComponent
    {
        public string Name { get; set; }

        public string ProcessorArchitecture { get; set; }

        public string Version { get; set; }

        public SxsComponent(string name, string version, string processorArchitecture)
        {
            this.Name = name;
            this.Version = version;
            this.ProcessorArchitecture = processorArchitecture;
        }

        public string GetFullPath()
        {
            foreach (string dir in System.IO.Directory.GetDirectories(System.IO.Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.System) + @"\..\WinSxs")))
            {
                if ((dir.Contains(this.Name) && dir.Contains(this.Version)) && dir.Contains(this.ProcessorArchitecture))
                {
                    return dir;
                }
            }

            return string.Empty;
        }

        public override string ToString()
        {
            return $"{this.Name} {this.Version}({this.ProcessorArchitecture})";
        }
    }
}
