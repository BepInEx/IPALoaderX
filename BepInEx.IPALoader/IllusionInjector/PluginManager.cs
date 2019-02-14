using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using BepInEx;
using IllusionPlugin;
using Plugin = IPALoaderX.IPALoaderX;

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
			// Use the same Plugins folder path as in IPA for perfect emulation
			string pluginDirectory = Path.Combine(Paths.GameRootPath, "Plugins");

			_Plugins = new List<IPlugin>();

			if (!Directory.Exists(pluginDirectory))
				return;

			var files = Directory.GetFiles(pluginDirectory, "*.dll");
			foreach (string s in files)
				_Plugins.AddRange(LoadPluginsFromFile(Path.Combine(pluginDirectory, s), Paths.ProcessName));

			Plugin.Logger.LogInfo(new string('-', 40));
			Plugin.Logger.LogInfo($"IPALoader found {_Plugins.Count} plugins in \"{pluginDirectory}\"");
			foreach (var plugin in _Plugins)
				Plugin.Logger.LogInfo($"{plugin.Name}: {plugin.Version}");
			Plugin.Logger.LogInfo(new string('-', 40));
		}

		private static IEnumerable<IPlugin> LoadPluginsFromFile(string file, string exeName)
		{
			var plugins = new List<IPlugin>();

			if (!File.Exists(file) || !file.EndsWith(".dll", true, null))
				return plugins;

			try
			{
				var assembly = Assembly.LoadFrom(file);

				foreach (var t in assembly.GetTypes())
					if (t.GetInterface("IPlugin") != null)
						try
						{
							var pluginInstance = Activator.CreateInstance(t) as IPlugin;
							string[] filter = null;

							if (pluginInstance is IEnhancedPlugin)
								filter = ((IEnhancedPlugin)pluginInstance).Filter;

							if (filter == null || filter.Contains(exeName, StringComparer.OrdinalIgnoreCase))
								plugins.Add(pluginInstance);
						}
						catch (Exception e)
						{
							Plugin.Logger.LogWarning($"[WARN] Could not load plugin {t.FullName} in {Path.GetFileName(file)}! {e}");
						}
			}
			catch (Exception e)
			{
				Plugin.Logger.LogError($"[ERROR] Could not load {Path.GetFileName(file)}! {e}");
			}

			return plugins;
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