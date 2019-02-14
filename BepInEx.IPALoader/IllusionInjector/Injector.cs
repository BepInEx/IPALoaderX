﻿using UnityEngine;

namespace IllusionInjector
{
	public static class Injector
	{
		private static bool injected;

		public static void Inject()
		{
			if (!injected)
			{
				injected = true;
				var bootstrapper = new GameObject("Bootstrapper").AddComponent<Bootstrapper>();
				bootstrapper.Destroyed += Bootstrapper_Destroyed;
			}
		}

		private static void Bootstrapper_Destroyed()
		{
			PluginComponent.Create();
		}
	}
}