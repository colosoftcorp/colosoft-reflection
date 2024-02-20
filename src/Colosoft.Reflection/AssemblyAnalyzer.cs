using System;
using System.Collections.Generic;
using System.Linq;

namespace Colosoft.Reflection
{
    public class AssemblyAnalyzer : MarshalByRefObject
    {
#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional
        private static readonly bool[,] ArchitectureCompatibilityMatrix = new bool[,]
#pragma warning restore CA1814 // Prefer jagged arrays over multidimensional
        {
            { false, false, false, false, false },
            { false, true, true, true, true },
            { false, true, true, false, false },
            { false, true, false, true, false },
            { false, true, false, false, true },
        };

        private readonly Dictionary<string, Dictionary<string, List<Redirection>>> allVersionRedirections = new Dictionary<string, Dictionary<string, List<Redirection>>>();
        private readonly Dictionary<string, string> assemblyNameToPathMap = new Dictionary<string, string>();
        private readonly Dictionary<string, PathRedirection> probingPaths = new Dictionary<string, PathRedirection>();

        private int assembliesFinished;
        private Dictionary<string, AsmData> cache;

        private bool circularDependencyWarningShown;
        private System.Reflection.ProcessorArchitecture currentLoadedArchitecture;
        private Stack<string> parentsStack;
        private int totalReferencedAssemblies;
        private string workingDir;

        public System.ComponentModel.BackgroundWorker BgWorker { get; set; }

#pragma warning disable CA2227 // Collection properties should be read only
        public List<AsmData> Gac { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        private static void TryLoad(AppDomain domain, ref string additionalInfo, ref bool invalid, ref System.Reflection.Assembly asm, string tmpPath)
        {
            byte[] raw = null;

#pragma warning disable CA1031 // Do not catch general exception types
            try
            {
                using (var stream = System.IO.File.OpenRead(tmpPath))
                {
                    raw = new byte[stream.Length];
                    stream.Read(raw, 0, raw.Length);
                }
            }
            catch (System.IO.FileLoadException ex)
            {
                invalid = true;
                additionalInfo = "File " + ex.FileName + " could not be loaded. " + ex.FusionLog;
                return;
            }
            catch (Exception ex)
            {
                invalid = true;
                additionalInfo = "Unexpected error. " + ex + "\r\n";
                return;
            }
#pragma warning restore CA1031 // Do not catch general exception types

            try
            {
                asm = domain.Load(raw);
            }
            catch
            {
                invalid = true;

#pragma warning disable CA1031 // Do not catch general exception types
                try
                {
                    asm = System.Reflection.Assembly.ReflectionOnlyLoad(raw);
                }
                catch (BadImageFormatException ex)
                {
                    additionalInfo = "Bad image format. " + ex.ToString() + "\r\n" + ex.FusionLog;
                }
                catch (Exception ex)
                {
                    additionalInfo = "Unexpected error. " + ex + "\r\n";
                }
#pragma warning restore CA1031 // Do not catch general exception types
            }
        }

        public static List<string> GetAssemblies(
            string directory,
            bool recursive,
            Func<bool> cancellationPendingCallback,
            Action<int, string> progressReportCallback)
        {
            progressReportCallback?.Invoke(0, "Reading directory " + directory);

            string[] files = System.IO.Directory.GetFiles(directory);
            List<string> assemblies = new List<string>();
            foreach (string file in files)
            {
                if ((cancellationPendingCallback != null) && cancellationPendingCallback())
                {
                    return assemblies;
                }

#pragma warning disable CA1031 // Do not catch general exception types
                try
                {
                    if (IsAssembly(file))
                    {
                        assemblies.Add(file);
                    }
                }
                catch
                {
                    // ignore
                }
#pragma warning restore CA1031 // Do not catch general exception types
            }

            if (recursive)
            {
                foreach (string dir in System.IO.Directory.GetDirectories(directory))
                {
                    assemblies.AddRange(GetAssemblies(dir, recursive, cancellationPendingCallback, progressReportCallback));
                }
            }

            return assemblies;
        }

        public static bool IsAssembly(string fileName)
        {
            uint rva15value = 0;
            bool invalid = false;
            using (var fileStream = new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                var reader = new System.IO.BinaryReader(fileStream);

                try
                {
                    int dictionaryOffset;
                    if (fileStream.Length < 60)
                    {
                        return false;
                    }

                    fileStream.Position = 60;
                    uint headerOffset = reader.ReadUInt32();
                    fileStream.Position = headerOffset + 0x18;

                    if (fileStream.Position > fileStream.Length)
                    {
                        return false;
                    }

                    switch (reader.ReadUInt16())
                    {
                        case 0x10b:
                            dictionaryOffset = 0x60;
                            break;

                        case 0x20b:
                            dictionaryOffset = 0x70;
                            break;

                        default:
                            invalid = true;
                            dictionaryOffset = 0;
                            break;
                    }

                    if (!invalid)
                    {
                        fileStream.Position = ((headerOffset + 0x18) + dictionaryOffset) + 0x70;
                        rva15value = reader.ReadUInt32();
                    }
                }
                finally
                {
                    reader.Close();
                }
            }

            return (rva15value != 0) && !invalid;
        }

        private void AnalyzeAssembly(System.Reflection.AssemblyName asmName, AsmData parent, AppDomain domain, bool throwWhenMissing)
        {
            bool redirectionApplied = false;
            string additionalInfo = string.Empty;
            System.Reflection.AssemblyName analyzedAssembly = asmName;

            Dictionary<string, List<Redirection>> asmRedirections = this.GetRedirections(asmName);

            if (asmRedirections != null)
            {
                analyzedAssembly = Redirection.GetCorrectAssemblyName(asmName, asmRedirections);
                redirectionApplied = analyzedAssembly.Version != asmName.Version;
            }

            bool invalid = false;
            bool isInGac = false;
            AsmData gacAssemblyData = null;
            System.Reflection.Assembly asm = null;

            string file = analyzedAssembly.FullName;
#pragma warning disable CA1031 // Do not catch general exception types
            try
            {
                if (this.Gac != null)
                {
#pragma warning disable S3267 // Loops should be simplified with "LINQ" expressions
                    foreach (var item in this.Gac)
                    {
                        if (item.AssemblyFullName.Contains(analyzedAssembly.FullName))
                        {
                            isInGac = true;
                            gacAssemblyData = item;
                            break;
                        }
                    }
#pragma warning restore S3267 // Loops should be simplified with "LINQ" expressions
                }
            }
            catch
            {
                // ignore
            }
#pragma warning restore CA1031 // Do not catch general exception types

            if (this.cache.ContainsKey(analyzedAssembly.FullName) && !this.parentsStack.Contains(analyzedAssembly.FullName))
            {
                AsmData cachedItem = this.cache[analyzedAssembly.FullName];
                parent.References.Add(cachedItem);
                return;
            }

            string asmLocation = null;

            AsmData currentAsmData = null;
            bool gacAssemblySet = false;
            if (!isInGac)
            {
                string extAdd = string.Empty;
                if (file.LastIndexOf(", Version=", StringComparison.InvariantCultureIgnoreCase) != -1)
                {
                    file = file.Substring(0, file.LastIndexOf(", Version=", StringComparison.InvariantCultureIgnoreCase));
                }

                if ((System.IO.Path.GetExtension(file) != ".exe") && (System.IO.Path.GetExtension(file) != ".dll"))
                {
                    extAdd = ".dll";
                }

                var tmpPath = this.FindPath(parent, file, extAdd);

                // Verifica se o arquivo existe
                if (System.IO.File.Exists(tmpPath))
                {
                    TryLoad(domain, ref additionalInfo, ref invalid, ref asm, tmpPath);
                }

                asmLocation = tmpPath;
            }
            else
            {
                try
                {
                    using (var stream = System.IO.File.OpenRead(gacAssemblyData.Path))
                    {
                        var raw = new byte[stream.Length];
                        stream.Read(raw, 0, raw.Length);
                        asm = domain.Load(raw);
                    }

                    asmLocation = gacAssemblyData.Path;

                    if (!gacAssemblyData.AssemblyFullName.Contains(asm.FullName) &&
                        !asm.FullName.Contains(gacAssemblyData.AssemblyFullName))
                    {
                        currentAsmData = gacAssemblyData;
                        gacAssemblySet = true;
                        asm = null;
                    }
                }
                catch (System.IO.FileNotFoundException ex)
                {
                    additionalInfo = "File " + ex.FileName + " could not be found.";
                }
                catch (System.IO.FileLoadException ex)
                {
                    additionalInfo = "File " + ex.FileName + " could not be loaded. " + ex.FusionLog;
                }
                catch (BadImageFormatException ex)
                {
                    additionalInfo = "Bad image format. " + ex.ToString() + "\r\n" + ex.FusionLog;
                }
            }

            if (currentAsmData == null)
            {
                currentAsmData = new AsmData(analyzedAssembly.Name, (asm == null) ? string.Empty : System.IO.Path.GetFullPath(asmLocation));
                currentAsmData.AssemblyFullName = analyzedAssembly.FullName;
                currentAsmData.Validity = AsmData.AsmValidity.Invalid;
                currentAsmData.InvalidAssemblyDetails = additionalInfo;
                currentAsmData.Architecture = this.GetArchitecture(currentAsmData.Path);
            }

            if ((!gacAssemblySet && (asm != null)) && (analyzedAssembly.Version != asm.GetName().Version))
            {
                string message = string.Concat(new object[]
                {
                    "Assembly was found with version ", asm.GetName().Version,
                    " but parent references ", analyzedAssembly.Version,
                });

                currentAsmData.AdditionalInfo = message;
                asm = null;
            }

            if ((!gacAssemblySet && (asm != null)) && !invalid)
            {
#pragma warning disable CA1031 // Do not catch general exception types
                try
                {
                    // Recupera os atributos do assembly
                    object[] attributes = asm.GetCustomAttributes(typeof(System.Reflection.AssemblyProductAttribute), false);
                    if (attributes.Length > 0)
                    {
                        // Recupera o nome do produto
                        currentAsmData.AssemblyProductName = ((System.Reflection.AssemblyProductAttribute)attributes[0]).Product;
                    }
                }
                catch (InvalidOperationException)
                {
                    currentAsmData.AssemblyProductName = "Product name could not be read.";
                }
                catch (System.IO.FileNotFoundException)
                {
                    currentAsmData.AssemblyProductName = "Product name could not be read. Assembly was loaded but later could not be found.";
                }
                catch (Exception ex)
                {
                    currentAsmData.AssemblyProductName = "Product name could not be read. Error: " + ex.Message;
                }
#pragma warning restore CA1031 // Do not catch general exception types
            }

            // Adiciona o referencia para o pai
            parent.References.Add(currentAsmData);
            if (invalid)
            {
                currentAsmData.Validity = AsmData.AsmValidity.ReferencesOnly;
            }

            if (this.parentsStack.Contains(analyzedAssembly.FullName))
            {
                currentAsmData.Validity = AsmData.AsmValidity.CircularDependency;
                if (!this.circularDependencyWarningShown)
                {
                    this.circularDependencyWarningShown = true;
                }

                return;
            }

            if (asm != null)
            {
                currentAsmData.Path = asmLocation;
                currentAsmData.AssemblyFullName = asm.FullName;
                if (!invalid)
                {
                    currentAsmData.Validity = redirectionApplied ? AsmData.AsmValidity.Redirected : AsmData.AsmValidity.Valid;
                    currentAsmData.OriginalVersion = redirectionApplied ? asmName.Version.ToString() : string.Empty;
                }

                if (((asm.CodeBase != null) && System.Runtime.InteropServices.RuntimeEnvironment.FromGlobalAccessCache(asm)) && (currentAsmData.AssemblyProductName == "Microsoft\x00ae .NET Framework"))
                {
                    return;
                }

                if ((currentAsmData.Validity != AsmData.AsmValidity.Invalid) && !this.ApplyArchitecture(currentAsmData.Architecture))
                {
                    currentAsmData.Validity = AsmData.AsmValidity.ReferencesOnly;
                    currentAsmData.AdditionalInfo = currentAsmData.AdditionalInfo + "\r\nProcessorArchitecture mismatch";
                }

                this.parentsStack.Push(currentAsmData.AssemblyFullName);
                this.cache.Add(analyzedAssembly.FullName, currentAsmData);
                foreach (System.Reflection.AssemblyName n in asm.GetReferencedAssemblies())
                {
                    this.AnalyzeAssembly(n, currentAsmData, domain, throwWhenMissing);
                }

                this.parentsStack.Pop();

                if (!System.IO.File.Exists(currentAsmData.Path))
                {
                    return;
                }
            }

            if (throwWhenMissing && !gacAssemblySet)
            {
                throw new InvalidOperationException("returning from analysis");
            }
        }

        private bool ApplyArchitecture(System.Reflection.ProcessorArchitecture processorArchitecture)
        {
            if (((this.currentLoadedArchitecture == System.Reflection.ProcessorArchitecture.Amd64) ||
                  (this.currentLoadedArchitecture == System.Reflection.ProcessorArchitecture.IA64)) ||
                (this.currentLoadedArchitecture == System.Reflection.ProcessorArchitecture.X86))
            {
                return this.IsCompatible(this.currentLoadedArchitecture, processorArchitecture);
            }

            if (this.currentLoadedArchitecture != System.Reflection.ProcessorArchitecture.MSIL)
            {
                return false;
            }

            this.currentLoadedArchitecture = processorArchitecture;
            return processorArchitecture != System.Reflection.ProcessorArchitecture.None;
        }

        private string FindPath(AsmData parent, string file, string extAdd)
        {
            string result;

            PathRedirection redirects;

            string parentDir = this.workingDir;
            if ((parent != null) && !string.IsNullOrEmpty(parent.Path))
            {
                parentDir = System.IO.Path.GetDirectoryName(parent.Path);
            }

            string tmpPath = file + extAdd;
            if (System.IO.File.Exists(tmpPath))
            {
                return tmpPath;
            }

            tmpPath = System.IO.Path.Combine(parentDir, file + extAdd);
            if (System.IO.File.Exists(tmpPath))
            {
                return tmpPath;
            }

            string ret = System.IO.Path.Combine(parentDir, file + extAdd);
            if (!this.probingPaths.TryGetValue(parent?.AssemblyFullName, out redirects))
            {
                return ret;
            }

            foreach (string currentDir in redirects.Directories)
            {
                string targetDir = currentDir;
                if (!System.IO.Path.IsPathRooted(currentDir))
                {
                    targetDir = System.IO.Path.Combine(parentDir, currentDir);
                }

                if (System.IO.File.Exists(System.IO.Path.Combine(targetDir, file + extAdd)))
                {
                    string targetPath = System.IO.Path.Combine(targetDir, file + extAdd);
                    return targetPath;
                }
            }

            result = ret;

            return result;
        }

        private System.Reflection.ProcessorArchitecture GetArchitecture(string path)
        {
            if (System.IO.File.Exists(path))
            {
#pragma warning disable CA1031 // Do not catch general exception types
                try
                {
                    var descriptor = System.Reflection.AssemblyName.GetAssemblyName(path);
                    if (descriptor != null)
                    {
                        return descriptor.ProcessorArchitecture;
                    }
                }
                catch
                {
                   // ignore
                }
#pragma warning restore CA1031 // Do not catch general exception types
            }

            return System.Reflection.ProcessorArchitecture.None;
        }

        private Dictionary<string, List<Redirection>> GetRedirections(System.Reflection.AssemblyName asmName)
        {
            string key;
            if (this.assemblyNameToPathMap.TryGetValue(asmName.Name, out key) && this.allVersionRedirections.ContainsKey(key))
            {
                return this.allVersionRedirections[key];
            }

#pragma warning disable S1168 // Empty arrays and collections should be returned instead of null
            return null;
#pragma warning restore S1168 // Empty arrays and collections should be returned instead of null
        }

        private bool IsCompatible(System.Reflection.ProcessorArchitecture parent, System.Reflection.ProcessorArchitecture child)
        {
            return ArchitectureCompatibilityMatrix[(int)parent, (int)child];
        }

        public AsmData AnalyzeRootAssembly(string assemblyName)
        {
            return this.AnalyzeRootAssembly(assemblyName, false);
        }

        public AsmData AnalyzeRootAssembly(string assemblyName, bool throwWhenMissing)
        {
            this.cache = new Dictionary<string, AsmData>();
            this.circularDependencyWarningShown = false;
            this.parentsStack = new Stack<string>();
            this.workingDir = Environment.CurrentDirectory;

            string fullPath = System.IO.Path.GetFullPath(assemblyName);

            AsmData ret = new AsmData(assemblyName, fullPath);

            var domain = AppDomain.CurrentDomain;

            if (!System.IO.File.Exists(assemblyName))
            {
                ret.Path = string.Empty;
                ret.Validity = AsmData.AsmValidity.Invalid;
                ret.AssemblyFullName = string.Empty;
                return ret;
            }

            System.Reflection.Assembly asm = null;

#pragma warning disable CA1031 // Do not catch general exception types
            try
            {
                using (var stream = System.IO.File.OpenRead(fullPath))
                {
                    var raw = new byte[stream.Length];
                    stream.Read(raw, 0, raw.Length);

                    try
                    {
                        asm = domain.Load(raw);

                        ret.Validity = AsmData.AsmValidity.Valid;
                    }
                    catch
                    {
                        asm = System.Reflection.Assembly.ReflectionOnlyLoad(raw);
                        ret.Validity = AsmData.AsmValidity.ReferencesOnly;
                    }
                }
            }
            catch
            {
                asm = null;
                ret.Validity = AsmData.AsmValidity.Invalid;
            }
#pragma warning restore CA1031 // Do not catch general exception types

            // Verifica se o assembly foi carregado
            if (asm != null)
            {
                ret.AssemblyFullName = asm.FullName;
                ret.Path = fullPath;
                ret.Architecture = asm.GetName().ProcessorArchitecture;
                this.currentLoadedArchitecture = ret.Architecture;

                string tempName = asm.GetName().Name;
                if (!this.assemblyNameToPathMap.ContainsKey(tempName))
                {
                    this.assemblyNameToPathMap.Add(tempName, asm.Location);
                }

                string cfgFilePath = Redirection.FindConfigFile(ret.Path);

                if (!string.IsNullOrEmpty(cfgFilePath) && !this.allVersionRedirections.ContainsKey(fullPath))
                {
                    var versionRedirections = Redirection.GetVersionRedirections(cfgFilePath);
                    PathRedirection pathRedirections = Redirection.GetPathRedirections(ret.AssemblyFullName, cfgFilePath);
                    this.allVersionRedirections.Add(fullPath, versionRedirections);
                    this.probingPaths.Add(ret.AssemblyFullName, pathRedirections);
                }

                var references = asm.GetReferencedAssemblies().ToList();

                var fileName =
                    System.IO.Path.Combine(
                        System.IO.Path.GetDirectoryName(fullPath),
                        string.Format(
                            System.Globalization.CultureInfo.InvariantCulture,
                            "{0}.XmlSerializers{1}",
                            System.IO.Path.GetFileNameWithoutExtension(fullPath),
                            System.IO.Path.GetExtension(fullPath)));

                if (System.IO.File.Exists(fileName))
                {
                    references.Add(System.Reflection.AssemblyName.GetAssemblyName(fileName));
                }

                this.totalReferencedAssemblies = references.Count;
                this.parentsStack.Push(ret.AssemblyFullName);

                foreach (System.Reflection.AssemblyName asmName in references)
                {
                    this.AnalyzeAssembly(asmName, ret, domain, throwWhenMissing);
                    this.assembliesFinished++;
                    if (this.BgWorker != null)
                    {
                        this.BgWorker.ReportProgress((100 * this.assembliesFinished) / this.totalReferencedAssemblies);
                    }
                }

                this.parentsStack.Pop();
            }

            return ret;
        }

        public bool IsValidAssembly(string path, out string error)
        {
            error = string.Empty;
            try
            {
                System.Reflection.Assembly asm = System.Reflection.Assembly.ReflectionOnlyLoadFrom(path);
                this.totalReferencedAssemblies = asm.GetReferencedAssemblies().Length;
            }
            catch (System.IO.FileLoadException ex)
            {
                error = "This file could not be loaded. \r\nException details: " + ex.FusionLog;
            }
            catch (BadImageFormatException ex)
            {
                error = "This file is not a valid assembly or this assembly is built with later version of CLR\r\nPlease update your CLR to the latest version. \r\nException details: " + ex.FusionLog;
            }
#if !NETSTANDARD2_0 && !NETCOREAPP2_0
            catch (System.Security.SecurityException ex)
            {
                error = "A security problem has occurred while loading the assembly.\r\nFailed permission: " + ex.FirstPermissionThatFailed;
#else
            catch (System.Security.SecurityException)
            {
                error = "A security problem has occurred while loading the assembly";
#endif
            }
            catch (System.IO.PathTooLongException)
            {
                error = "Given path is too long.";
            }

            return string.IsNullOrEmpty(error);
        }
    }
}
