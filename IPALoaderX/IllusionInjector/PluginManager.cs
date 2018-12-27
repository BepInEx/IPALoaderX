using IllusionPlugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using BepInEx.Logging;
using static BepInEx.Logger;
using BepInEx;

namespace IllusionInjector
{
    public static class PluginManager
    {
        private static List<IPlugin> _Plugins = null;

        /// <summary>
        /// Gets the list of loaded plugins and loads them if necessary.
        /// </summary>
        public static IEnumerable<IPlugin> Plugins
        {
            get
            {
                if(_Plugins == null)
                {
                    LoadPlugins();
                }
                return _Plugins;
            }
        }

        private static void LoadPlugins()
        {
            string pluginDirectory = Path.Combine(Environment.CurrentDirectory, "Plugins");

            // Process.GetCurrentProcess().MainModule crashes the game and Assembly.GetEntryAssembly() is NULL,
            // so we need to resort to P/Invoke
            //string exeName = Path.GetFileNameWithoutExtension(AppInfo.StartupPath);
            string exeName = Path.GetFileNameWithoutExtension(Paths.ExecutablePath);
            _Plugins = new List<IPlugin>();

            if (!Directory.Exists(pluginDirectory)) return;

            string[] files = Directory.GetFiles(pluginDirectory, "*.dll");
            foreach (var s in files)
            {
                _Plugins.AddRange(LoadPluginsFromFile(Path.Combine(pluginDirectory, s), exeName));
            }

            Log(LogLevel.Info, new string('-', 40));
            Log(LogLevel.Info, $"IPALoaderX found {_Plugins.Count} plugins in \"{pluginDirectory}\"");
            foreach (var plugin in _Plugins)
            {
                Log(LogLevel.Info, $"{plugin.Name}: {plugin.Version}");
            }
            Log(LogLevel.Info, new string('-', 40));
        }

        private static IEnumerable<IPlugin> LoadPluginsFromFile(string file, string exeName)
        {
            List<IPlugin> plugins = new List<IPlugin>();

            if (!File.Exists(file) || !file.EndsWith(".dll", true, null))
                return plugins;

            try
            {
                Assembly assembly = Assembly.LoadFrom(file);

                foreach (Type t in assembly.GetTypes())
                {
                    if (t.GetInterface("IPlugin") != null)
                    {
                        try
                        {

                            IPlugin pluginInstance = Activator.CreateInstance(t) as IPlugin;
                            string[] filter = null;

                            if (pluginInstance is IEnhancedPlugin)
                            {
                                filter = ((IEnhancedPlugin)pluginInstance).Filter;
                            }
                            
                            if(filter == null || Enumerable.Contains(filter, exeName, StringComparer.OrdinalIgnoreCase))
                                plugins.Add(pluginInstance);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("[WARN] Could not load plugin {0} in {1}! {2}", t.FullName, Path.GetFileName(file), e);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[ERROR] Could not load {0}! {1}", Path.GetFileName(file), e);
            }

            return plugins;
        }

        public class AppInfo
        {
            [DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = false)]
            private static extern int GetModuleFileName(HandleRef hModule, StringBuilder buffer, int length);
            private static HandleRef NullHandleRef = new HandleRef(null, IntPtr.Zero);
            public static string StartupPath
            {
                get
                {
                    StringBuilder stringBuilder = new StringBuilder(260);
                    GetModuleFileName(NullHandleRef, stringBuilder, stringBuilder.Capacity);
                    return stringBuilder.ToString();
                }
            }
        }

    }
}
