using System;
using System.Collections.Generic;
using System.Text;

namespace Colosoft.Reflection
{
    [Serializable]
    public class AsmData
    {
        public enum AsmValidity
        {
            Valid,
            ReferencesOnly,
            Invalid,
            CircularDependency,
            Redirected,
        }

        private List<AsmData> references;

        public string AdditionalInfo { get; set; }

        public System.Reflection.ProcessorArchitecture Architecture { get; set; }

        public string AssemblyFullName { get; set; }

        public string AssemblyProductName { get; set; }

#pragma warning disable CA1819 // Properties should not return arrays
        public LibraryImport[] Imports { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays

        public string InvalidAssemblyDetails { get; set; }

        public string Name { get; set; }

        public string OriginalVersion { get; set; }

        public string Path { get; set; }

        public List<AsmData> References
        {
            get { return this.references; }
        }

        public AsmValidity Validity { get; set; }

        /// <param name="path">Caminho do assembly.</param>
        public AsmData(string name, string path)
        {
            this.Name = name;
            this.Path = path;
            this.Imports = Array.Empty<LibraryImport>();
            this.references = new List<AsmData>();
            this.Validity = AsmValidity.Invalid;
            this.Architecture = System.Reflection.ProcessorArchitecture.None;
        }

        public override string ToString()
        {
            var stringValue = new StringBuilder();
            stringValue.AppendLine(this.AssemblyFullName);
            stringValue.AppendLine(this.Path);
            if (!string.IsNullOrEmpty(this.OriginalVersion))
            {
                stringValue.Append("Original referenced assembly version: ");
                stringValue.AppendLine(this.OriginalVersion);
            }

            if (this.Imports.Length > 0)
            {
                stringValue.AppendLine();
                stringValue.AppendLine("Imports: ");
                foreach (var imp in this.Imports)
                {
                    stringValue.AppendLine(imp.GetLongDescription());
                }
            }

            if (!string.IsNullOrEmpty(this.InvalidAssemblyDetails) && (this.Validity != AsmValidity.Valid))
            {
                stringValue.AppendLine("\r\n" + this.InvalidAssemblyDetails);
            }

            if (!string.IsNullOrEmpty(this.AdditionalInfo))
            {
                stringValue.AppendLine("\r\n" + this.AdditionalInfo);
            }

            stringValue.AppendLine("Architecture: " + this.Architecture);
            return stringValue.ToString();
        }
    }
}
