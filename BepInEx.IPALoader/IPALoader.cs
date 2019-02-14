using System;
using System.Reflection;
using BepInEx.Logging;
using IllusionInjector;

namespace BepInEx.IPALoader
{
	[BepInPlugin("keelhauled.ipaloader", "IPALoader", "1.0.0")]
	public class IPALoader : BaseUnityPlugin
	{
		internal new static ManualLogSource Logger;

		private IPALoader()
		{
			Logger = base.Logger;

			// Redirect missing assembly requests to this assembly, since we have all the emulated code
			AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
			{
				if (args.Name == "IllusionPlugin, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" ||
					args.Name == "IllusionInjector, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null")
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