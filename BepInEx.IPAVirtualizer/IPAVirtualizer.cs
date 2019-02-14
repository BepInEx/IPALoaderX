using System.Collections.Generic;
using BepInEx.Logging;
using Mono.Cecil;

namespace BepInEx.IPAVirtualizer
{
	public static class IPAVirtualizer
	{
		public static IEnumerable<string> TargetDLLs { get; } = new[]
		{
			"Assembly-CSharp.dll"
		};

		private static ManualLogSource Logger;

		public static void Patch(AssemblyDefinition ass)
		{
			Logger = BepInEx.Logger.CreateLogSource("IPAVirtualizer");
            try
			{
				foreach (var type in ass.MainModule.Types)
					VirtualizeType(type);
			}
			finally
			{
				BepInEx.Logger.Sources.Remove(Logger);
			}
		}

		public static void VirtualizeType(TypeDefinition type)
		{
			if (type.IsSealed)
				type.IsSealed = false;

			if (type.IsInterface)
				return;
			if (type.IsAbstract)
				return;

			// These two don't seem to work.
			if (type.Name == "SceneControl" || type.Name == "ConfigUI")
				return;

			// Write to debug to not spam the console by default
			Logger.LogDebug($"Virtualizing {type.Name}");

			// Take care of sub types
			foreach (var subType in type.NestedTypes)
				VirtualizeType(subType);

			foreach (var method in type.Methods)
				if (method.IsManaged
					&& method.IsIL
					&& !method.IsStatic
					&& !method.IsVirtual
					&& !method.IsAbstract
					&& !method.IsAddOn
					&& !method.IsConstructor
					&& !method.IsSpecialName
					&& !method.IsGenericInstance
					&& !method.HasOverrides)
				{
					method.IsVirtual = true;
					method.IsPublic = true;
					method.IsPrivate = false;
					method.IsNewSlot = true;
					method.IsHideBySig = true;
				}

			foreach (var field in type.Fields)
				if (field.IsPrivate)
					field.IsFamily = true;
		}
	}
}