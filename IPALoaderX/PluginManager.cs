using IllusionPlugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;

namespace IPALoaderX
{
    public static class PluginManager
    {
        static List<IPlugin> _Plugins = null;

        public static IEnumerable<IPlugin> Plugins
        {
            get
            {
                if(_Plugins == null)
                    LoadPlugins();

                return _Plugins;
            }
        }

        static void LoadPlugins()
        {
            string pluginDirectory = Path.Combine(Paths.GameRootPath, "Plugins");

            if(!Directory.Exists(pluginDirectory))
                return;

            string exeName = Path.GetFileNameWithoutExtension(Paths.ExecutablePath);
            _Plugins = new List<IPlugin>();

            foreach(var s in Directory.GetFiles(pluginDirectory, "*.dll"))
            {
                _Plugins.AddRange(LoadPluginsFromFile(Path.Combine(pluginDirectory, s), exeName));
            }
            
            Console.WriteLine(new string('-', 40));
            Console.WriteLine($"Loading plugins from {pluginDirectory} and found {_Plugins.Count}");
            foreach (var plugin in _Plugins)
            {
                Console.WriteLine($" {plugin.Name}: {plugin.Version}");
            }
            Console.WriteLine(new string('-', 40));
        }

        static IEnumerable<IPlugin> LoadPluginsFromFile(string file, string exeName)
        {
            List<IPlugin> plugins = new List<IPlugin>();

            if (!File.Exists(file) || !file.EndsWith(".dll", true, null))
                return plugins;

            try
            {
                Assembly assembly = Assembly.LoadFrom(file);

                foreach(Type t in assembly.GetTypes())
                {
                    if(t.GetInterface("IPlugin") != null)
                    {
                        try
                        {
                            IPlugin pluginInstance = Activator.CreateInstance(t) as IPlugin;
                            string[] filter = null;

                            if(pluginInstance is IEnhancedPlugin)
                            {
                                filter = ((IEnhancedPlugin)pluginInstance).Filter;
                            }
                            
                            if(filter == null || Enumerable.Contains(filter, exeName, StringComparer.OrdinalIgnoreCase))
                                plugins.Add(pluginInstance);
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine($"[WARN] Could not load plugin {t.FullName} in {Path.GetFileName(file)}! {ex}");
                        }
                    }
                }

            }
            catch(Exception ex)
            {
                Console.WriteLine($"[ERROR] Could not load {Path.GetFileName(file)}! {ex}");
            }

            return plugins;
        }
    }
}
