using System;
using System.Collections.Generic;

namespace Colosoft.Reflection
{
    internal class FrameworkRedirectionsScanner : MarshalByRefObject
    {
        public Dictionary<string, List<Redirection>> GetFrameworkRedirections(
#pragma warning disable CA1801 // Review unused parameters
#pragma warning disable SA1114 // Parameter list should follow declaration
            List<AsmData> assembliesInGac,
#pragma warning restore SA1114 // Parameter list should follow declaration
            IAssemblyAnalyzerObserver progressDialog)
#pragma warning restore CA1801 // Review unused parameters
        {
            Dictionary<string, List<Redirection>> redirections = new Dictionary<string, List<Redirection>>();

#if !NETSTANDARD2_0 && !NETCOREAPP2_0
            try
            {
                progressDialog.ReportProgress(0, "Checking .NET Framework libraries...".GetFormatter());
                int assembliesCount = 0;
                var upgrades = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\.NETFramework\Policy\Upgrades");
                
                if (upgrades == null)
                    return redirections;
                
                var bindingRedirects = new List<BindingRedirect>();
                foreach (string targetVersion in upgrades.GetValueNames())
                {
                    string sourceVersion = upgrades.GetValue(targetVersion) as string;
                    BindingRedirect redirect = new BindingRedirect();
                    redirect.NewVersion = new Version(targetVersion);
                    if (sourceVersion.Contains("-"))
                    {
                        string[] versions = sourceVersion.Split(new char[] { '-' });
                        redirect.OldVersionMin = new Version(versions[0]);
                        redirect.OldVersionMax = new Version(versions[1]);
                    }
                    else
                    {
                        redirect.OldVersionMax = new Version(sourceVersion);
                        redirect.OldVersionMin = new Version(sourceVersion);
                    }
                    bindingRedirects.Add(redirect);
                }
                upgrades.Close();
                foreach (AsmData assemblyDescription in assembliesInGac)
                {
                    System.Reflection.Assembly asm = null;
                    try
                    {
                        asm = System.Reflection.Assembly.Load(assemblyDescription.Name);
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    var assemblyName = asm.GetName(false);
                    if (!redirections.ContainsKey(assemblyName.Name))
                    {
                        object[] attributes = null;
                        try
                        {
                            attributes = asm.GetCustomAttributes(typeof(System.Reflection.AssemblyProductAttribute), false);
                        }
                        catch (Exception)
                        {
                            // Failed to read custom attributes:
                        }
                        if ((attributes != null) && (attributes.Length > 0))
                        {
                            var productAttribute = attributes[0] as System.Reflection.AssemblyProductAttribute;
                            if ((productAttribute != null) && (productAttribute.Product == "Microsoft\x00ae .NET Framework"))
                            {
                                foreach (BindingRedirect bindingRedirect in bindingRedirects)
                                {
                                    Redirection redirection = new Redirection();
                                    redirection.AssemblyIdentity = assemblyName;
                                    redirection.BindingRedirection = bindingRedirect;
                                    if (assemblyName.Version <= redirection.BindingRedirection.NewVersion)
                                        redirection.BindingRedirection.NewVersion = assemblyName.Version;
                                    
                                    if (redirections.ContainsKey(redirection.AssemblyIdentity.Name))
                                        redirections[redirection.AssemblyIdentity.Name].Add(redirection);
                                    
                                    else
                                    {
                                        var aux = new List<Redirection>();
                                        aux.Add(redirection);
                                        redirections.Add(redirection.AssemblyIdentity.Name, aux);
                                    }
                                }
                            }
                        }
                        assembliesCount++;
                        progressDialog.ReportProgress((int) ((100.0 * assembliesCount) / ((double) assembliesInGac.Count)), "Checking .NET Framework libraries...".GetFormatter());
                        if (progressDialog.CancellationPending)
                        {
                            redirections.Clear();
                            return redirections;
                        }
                    }
                }

            }
            catch (Exception)
            {
                //Trace.WriteLine(ex);
            }
#endif
            return redirections;
        }
    }
}
