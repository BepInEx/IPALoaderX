using BepInEx;
using BepInEx.IPALoader;
using IllusionPlugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace IllusionInjector
{
    public static class PluginManager
    {
        private static List<IPlugin> _Plugins;

        /// <summary>
        ///     Gets the list of loaded plugins and loads them if necessary.
        /// </summary>
        public static IEnumerable<IPlugin> Plugins
        {
            get
            {
                if (_Plugins == null)
                    LoadPlugins();
                return _Plugins;
            }
        }

        private static void LoadPlugins()
        {
            string pluginDirectory = Path.Combine(Paths.GameRootPath, IPALoader.IPAPluginsPath.Value);

            _Plugins = new List<IPlugin>();

            if (!Directory.Exists(pluginDirectory))
                return;

            var files = Directory.GetFiles(pluginDirectory, "*.dll");
            foreach (string s in files)
                _Plugins.AddRange(LoadPluginsFromFile(Path.Combine(pluginDirectory, s), Paths.ProcessName));

            IPALoader.Logger.LogInfo(new string('-', 40));
            IPALoader.Logger.LogInfo($"IPALoader found {_Plugins.Count} plugins in \"{pluginDirectory}\"");
            foreach (var plugin in _Plugins)
                IPALoader.Logger.LogInfo($"{plugin?.Name}: {plugin?.Version}");
            IPALoader.Logger.LogInfo(new string('-', 40));
        }

        private static IEnumerable<IPlugin> LoadPluginsFromFile(string file, string exeName)
        {
            var plugins = new List<IPlugin>();

            if (!File.Exists(file) || !file.EndsWith(".dll", true, null))
                return plugins;

            try
            {
                var assembly = Assembly.LoadFrom(file);

                foreach (var t in GetTypesSafe(assembly))
                    if (typeof(IPlugin).IsAssignableFrom(t))
                        try
                        {
                            var pluginInstance = Activator.CreateInstance(t) as IPlugin;

                            if (pluginInstance == null)
                            {
                                IPALoader.Logger.LogWarning($"[WRN] Could not load {t.FullName} because types mismatch. Please check if you have multiple instances of IPA installed.");
                                continue;
                            }

                            string[] filter = null;

                            if (pluginInstance is IEnhancedPlugin plugin)
                                filter = plugin.Filter;

                            var exeNameTrimmed = exeName.ToLower().Replace(".exe", "").Trim();
                            if (filter == null || filter.Any(f => f.ToLower().Replace(".exe", "").Trim() == exeNameTrimmed))
                                plugins.Add(pluginInstance);
                        }
                        catch (Exception e)
                        {
                            IPALoader.Logger.LogWarning($"[WARN] Could not load plugin {t.FullName} in {Path.GetFileName(file)}! {e}");
                        }
            }
            catch (Exception e)
            {
                IPALoader.Logger.LogError($"[ERROR] Could not load {Path.GetFileName(file)}! {e}");
            }

            return plugins;
        }

        private static IEnumerable<Type> GetTypesSafe(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                IPALoader.Logger.LogWarning("Could not load some types from assembly " + assembly.FullName + " - check debug log for details");
                foreach (var eLoaderException in e.LoaderExceptions)
                    IPALoader.Logger.LogDebug(eLoaderException);
                return e.Types.Where(x => x != null);
            }
        }

        public class AppInfo
        {
            private static readonly HandleRef NullHandleRef = new HandleRef(null, IntPtr.Zero);

            public static string StartupPath
            {
                get
                {
                    var stringBuilder = new StringBuilder(260);
                    GetModuleFileName(NullHandleRef, stringBuilder, stringBuilder.Capacity);
                    return stringBuilder.ToString();
                }
            }

            [DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = false)]
            private static extern int GetModuleFileName(HandleRef hModule, StringBuilder buffer, int length);
        }
    }
}