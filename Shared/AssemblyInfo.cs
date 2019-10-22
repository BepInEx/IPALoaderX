using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyCompany("https://github.com/BepInEx/IPALoaderX")]
[assembly: AssemblyCopyright("Copyright © 2019")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

#if DEBUG
[assembly: AssemblyConfiguration("DEBUG")]
#else
[assembly: AssemblyConfiguration("RELEASE")]
#endif

[assembly: ComVisible(false)]

[assembly: AssemblyVersion(BepInEx.IPALoader.Metadata.PluginsVersion)]
[assembly: AssemblyFileVersion(BepInEx.IPALoader.Metadata.PluginsVersion)]