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

		private IPALoader()
		{
			Logger = base.Logger;

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
			Injector.Inject();
		}
	}
}