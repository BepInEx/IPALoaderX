using BepInEx.Configuration;
using BepInEx.Logging;
using IllusionInjector;
using System;
using System.IO;
using System.Reflection;

namespace BepInEx.IPALoader
{
    [BepInPlugin(PluginName, PluginName, Metadata.PluginsVersion)]
    public class IPALoader : BaseUnityPlugin
    {
        public const string PluginName = "BepInEx.IPALoader";
        internal static new ManualLogSource Logger;
        private readonly bool run = true;

        public static ConfigWrapper<string> IPAPluginsPath { get; private set; }
        public static ConfigFile cfgFile { get; } = new ConfigFile(Path.Combine(Paths.ConfigPath, Metadata.ConfigFileName), false);

        private IPALoader()
        {
            IPAPluginsPath = cfgFile.Wrap(Metadata.ConfigSection, Metadata.ConfigKey, Metadata.ConfigDescription, Metadata.ConfigDefaultValue);
            Logger = base.Logger;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var name = assembly.GetName().Name;
                if (name == "IllusionInjector" || name == "IllusionPlugin")
                {
                    Logger.LogWarning($"Detected that IPA is already installed! Please either uninstall IPA or disable this loader.");
                    run = false;
                    return;
                }
            }

            // Redirect missing assembly requests to this assembly, since we have all the emulated code
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                var name = new AssemblyName(args.Name).Name;
                if (name == "IllusionPlugin" ||
                    name == "IllusionInjector")
                    return Assembly.GetExecutingAssembly();

                return null;
            };
        }

        private void Awake()
        {
            if (run)
                Injector.Inject();
        }
    }
}