using System;
using System.Collections.Generic;

namespace Colosoft.Reflection
{
    [Serializable]
    public class Redirection
    {
        public System.Reflection.AssemblyName AssemblyIdentity { get; set; }

        public BindingRedirect BindingRedirection { get; set; }

        public static string FindConfigFile(string appFilePath)
        {
            System.IO.Path.GetFileName(appFilePath);
            System.IO.Path.GetDirectoryName(appFilePath);
            var configName = $@"{System.IO.Path.GetDirectoryName(appFilePath)}\{System.IO.Path.GetFileName(appFilePath)}.config".Trim(new char[] { '\\' });
            if (System.IO.File.Exists(configName))
            {
                return configName;
            }

            return null;
        }

        public static System.Reflection.AssemblyName GetCorrectAssemblyName(System.Reflection.AssemblyName original, Dictionary<string, List<Redirection>> dic)
        {
            if (original is null)
            {
                throw new ArgumentNullException(nameof(original));
            }

            if (dic is null)
            {
                throw new ArgumentNullException(nameof(dic));
            }

            if (dic.ContainsKey(original.Name))
            {
                foreach (Redirection redirection in dic[original.Name])
                {
                    if (redirection.BindingRedirection == null)
                    {
                        System.Diagnostics.Trace.WriteLine("Redirection data is invalid: " + redirection.AssemblyIdentity);
                    }
                    else
                    {
                        Version redirectVersionMin = redirection.BindingRedirection.OldVersionMin;
                        Version redirectVersionMax = redirection.BindingRedirection.OldVersionMax;
                        if ((original.Version >= redirectVersionMin) && (original.Version <= redirectVersionMax))
                        {
                            var name = new System.Reflection.AssemblyName(original.FullName);
                            name.Version = redirection.BindingRedirection.NewVersion;
                            name.ProcessorArchitecture = original.ProcessorArchitecture;
                            name.SetPublicKeyToken(original.GetPublicKeyToken());
                            return name;
                        }
                    }
                }
            }

            return original;
        }

#pragma warning disable CA1801 // Review unused parameters
        public static Dictionary<string, List<Redirection>> GetFrameworkRedirections(List<AsmData> assembliesInGac, IAssemblyAnalyzerObserver observer)
#pragma warning restore CA1801 // Review unused parameters
        {
#if !NETSTANDARD2_0 && !NETCOREAPP2_0
            string friendlyName = System.Threading.Thread.GetDomain().FriendlyName;
            string exeAssembly = System.Reflection.Assembly.GetEntryAssembly().FullName;
            AppDomainSetup setup = new AppDomainSetup();
            setup.ApplicationBase = Environment.CurrentDirectory;
            setup.DisallowBindingRedirects = false;
            setup.DisallowCodeDownload = true;
            setup.ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            AppDomain redirectionsScanDomain = AppDomain.CreateDomain("Framework Redirections");
            redirectionsScanDomain.UnhandledException += new UnhandledExceptionEventHandler((sender, e) =>
                ("An unhandled exception occured while parsing .NET Framework assemblies. We are sorry for any inconvenience caused.\r\nError message: " + ((Exception)e.ExceptionObject).Message).GetFormatter());

            FrameworkRedirectionsScanner scanner1 = (FrameworkRedirectionsScanner)redirectionsScanDomain.CreateInstanceAndUnwrap(exeAssembly, typeof(FrameworkRedirectionsScanner).FullName);
            AppDomain.Unload(redirectionsScanDomain);
#endif
            return new Dictionary<string, List<Redirection>>();
        }

        public static PathRedirection GetPathRedirections(string assemblyName, string configFile)
        {
            if (!System.IO.File.Exists(configFile))
            {
                return new PathRedirection
                {
                    AssemblyName = assemblyName,
                };
            }

            var redirect = new PathRedirection
            {
                 AssemblyName = assemblyName,
            };

            var config = new System.Xml.XmlDocument();
            config.Load(configFile);
            var nsmgr = new System.Xml.XmlNamespaceManager(config.NameTable);
            nsmgr.AddNamespace("x", "urn:schemas-microsoft-com:asm.v1");
            var probingNode = config.CreateNavigator().SelectSingleNode("/configuration/runtime/x:assemblyBinding/x:probing", nsmgr);
            if (probingNode != null)
            {
                string privatePath = probingNode.GetAttribute("privatePath", string.Empty);
                if (string.IsNullOrEmpty(privatePath))
                {
                    return redirect;
                }

                foreach (string p in privatePath.Split(new char[] { ';' }))
                {
                    redirect.Directories.Add(System.IO.Path.GetFullPath(p));
                }
            }

            return redirect;
        }

        public static Dictionary<string, List<Redirection>> GetVersionRedirections(string fileName)
        {
            var ret = new Dictionary<string, List<Redirection>>();

            var config = new System.Xml.XmlDocument();
            config.Load(fileName);
            foreach (System.Xml.XmlNode dependentAssemblyTag in config.GetElementsByTagName("dependentAssembly"))
            {
                if (((dependentAssemblyTag.ParentNode.Name == "assemblyBinding") && (dependentAssemblyTag.ParentNode.ParentNode != null)) && (dependentAssemblyTag.ParentNode.ParentNode.Name == "runtime"))
                {
                    Redirection red = new Redirection();
                    foreach (System.Xml.XmlNode node in dependentAssemblyTag.ChildNodes)
                    {
                        if (node.Name == "assemblyIdentity")
                        {
                            var name = new System.Reflection.AssemblyName(node.Attributes["name"].Value);
                            if (node.Attributes["processorArchitecture"] != null)
                            {
                                name.ProcessorArchitecture = (System.Reflection.ProcessorArchitecture)Enum.Parse(typeof(System.Reflection.ProcessorArchitecture), node.Attributes["processorArchitecture"].Value, true);
                            }

                            red.AssemblyIdentity = name;
                            continue;
                        }

                        if (node.Name == "bindingRedirect")
                        {
                            BindingRedirect redirect = new BindingRedirect();
                            if (node.Attributes["oldVersion"] != null)
                            {
                                System.Xml.XmlAttribute attr = node.Attributes["oldVersion"];
                                if (attr.Value.Contains("-"))
                                {
                                    string[] versions = attr.Value.Split(new char[] { '-' });
                                    redirect.OldVersionMin = new Version(versions[0]);
                                    redirect.OldVersionMax = new Version(versions[1]);
                                }
                                else
                                {
                                    redirect.OldVersionMax = new Version(attr.Value);
                                    redirect.OldVersionMin = new Version(attr.Value);
                                }
                            }

                            if (node.Attributes["newVersion"] != null)
                            {
                                redirect.NewVersion = new Version(node.Attributes["newVersion"].Value);
                            }

                            red.BindingRedirection = redirect;
                        }
                    }

                    if (ret.ContainsKey(red.AssemblyIdentity.Name))
                    {
                        ret[red.AssemblyIdentity.Name].Add(red);
                    }
                    else
                    {
                        var aux = new List<Redirection>();
                        aux.Add(red);
                        ret.Add(red.AssemblyIdentity.Name, aux);
                    }
                }
            }

            if (ret.Count > 0)
            {
                return ret;
            }

#pragma warning disable S1168 // Empty arrays and collections should be returned instead of null
            return null;
#pragma warning restore S1168 // Empty arrays and collections should be returned instead of null
        }
    }
}
