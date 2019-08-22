using System;
using System.Reflection;
using BepInEx.Logging;
using IllusionInjector;

namespace BepInEx.IPALoader
{
	[BepInPlugin("keelhauled.ipaloader", "IPALoader", "1.1.1")]
	public class IPALoader : BaseUnityPlugin
	{
		internal new static ManualLogSource Logger;
		private bool run = true;

		private IPALoader()
		{
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
				Logger.LogInfo(args.Name);
				var name = new AssemblyName(args.Name).Name;
				if (name == "IllusionPlugin" ||
					name == "IllusionInjector")
					return Assembly.GetExecutingAssembly();

				return null;
			};
		}

		private void Awake()
		{
			if(run)
				Injector.Inject();
		}
	}
}