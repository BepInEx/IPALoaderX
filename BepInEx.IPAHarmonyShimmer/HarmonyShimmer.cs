using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx.Harmony;
using BepInEx.Logging;
using HarmonyLib;
using Mono.Cecil;

namespace BepInEx.IPAHarmonyShimmer
{
	public static class HarmonyShimmer
	{
		public static IEnumerable<string> TargetDLLs { get; } = new[] { "Assembly-CSharp.dll" };

		private static ManualLogSource Logger = Logging.Logger.CreateLogSource("IPAHarmonyShim");

		private static DefaultAssemblyResolver resolver;
		private static ReaderParameters readerParameters;

		private static AssemblyDefinition ResolveAssemblies(object sender, AssemblyNameReference reference)
		{
			var name = new AssemblyName(reference.FullName);

			if (Utility.TryResolveDllAssembly(name, Paths.BepInExAssemblyDirectory, readerParameters, out var assembly) || Utility.TryResolveDllAssembly(name, Path.Combine(Paths.GameRootPath, "Plugins"), readerParameters, out assembly) || Utility.TryResolveDllAssembly(name, Paths.ManagedPath, readerParameters, out assembly))
				return assembly;
			if (reference.Name == "0Harmony_Shim")
				return AssemblyDefinition.ReadAssembly(Assembly.GetExecutingAssembly().Location);
			return null;
		}

		private static bool definitionsAreReferences = false;

		[HarmonyPatch(typeof(AccessTools), "DeclaredField", typeof(Type), typeof(string))]
		[HarmonyPostfix]
		private static void DeclaredFieldShim(ref FieldInfo __result, Type type, string name)
		{
			//wew at harmony for botching compatibility
			if (__result == null)
				__result = AccessTools.Field(type, name);
		}

		[HarmonyPatch(typeof(TypeDefinition), "IsDefinition", MethodType.Getter)]
		[HarmonyPostfix]
		private static void IsDefinitionPatch(TypeDefinition __instance, ref bool __result)
		{
			if (definitionsAreReferences && __instance.Scope is AssemblyNameReference assRef && assRef.Name.StartsWith("0Harmony"))
				__result = false;
		}

		public static void Initialize()
		{
			HarmonyWrapper.PatchAll(typeof(HarmonyShimmer));

			resolver = new DefaultAssemblyResolver();
			readerParameters = new ReaderParameters { AssemblyResolver = resolver };
			resolver.ResolveFailure += ResolveAssemblies;

			Logger.LogInfo("Shimming IPA plugins");
			string pluginDirectory = Path.Combine(Paths.GameRootPath, "Plugins");

			if (!Directory.Exists(pluginDirectory))
			{
				Logger.LogInfo("No IPA Plugins folder found! Skipping shimming...");
				return;
			}

			var harmonyFullTypes = new Dictionary<string, string>();
			var harmonyTypes = new HashSet<string>();

			using (var bepinAss = AssemblyDefinition.ReadAssembly(Assembly.GetExecutingAssembly().Location))
			{
				foreach (var type in bepinAss.MainModule.Types)
					if (type.Namespace.StartsWith("Harmony"))
						harmonyTypes.Add(type.FullName);
			}

			using (var origHarmonyAss = AssemblyDefinition.ReadAssembly(Path.Combine(Paths.BepInExAssemblyDirectory, "0Harmony.dll")))
			{
				foreach (var type in origHarmonyAss.MainModule.Types)
				{
					if (type.Namespace.StartsWith("HarmonyLib"))
						harmonyFullTypes[type.Name] = type.Namespace;
				}
			}

			var bakDir = Path.Combine(Paths.GameRootPath, "Plugins_backup");
			Directory.CreateDirectory(bakDir);

			foreach (string file in Directory.GetFiles(pluginDirectory, "*.dll", SearchOption.AllDirectories))
			{
				try
				{
					var ad = AssemblyDefinition.ReadAssembly(new MemoryStream(File.ReadAllBytes(file)));

					if (ad.MainModule.AssemblyResolver is DefaultAssemblyResolver pluginResolver)
						pluginResolver.ResolveFailure += ResolveAssemblies;

					var harmonyShimRef = new AssemblyNameReference("0Harmony_Shim", new Version(1, 1, 0, 0));
					var harmony2Ref = new AssemblyNameReference("0Harmony", new Version(2, 0, 0, 0));
					var harmonyRef = ad.MainModule.AssemblyReferences.FirstOrDefault(r => r.Name == "0Harmony" && r.Version.Major <= 1);
					bool shimmed = false;

					// Need to unmerge the assembly
					if (ad.MainModule.Types.Any(t => harmonyTypes.Contains(t.FullName)))
					{
						shimmed = true;
						Logger.LogInfo($"Unmerging {Path.GetFileNameWithoutExtension(file)}");
						ad.MainModule.AssemblyReferences.Add(harmony2Ref);
						ad.MainModule.AssemblyReferences.Add(harmonyShimRef);

						foreach (var typeDefinition in ad.MainModule.Types.ToList())
						{
							string @namespace = null;
							if (harmonyTypes.Contains(typeDefinition.FullName) || (typeDefinition.Namespace.StartsWith("Harmony") && harmonyFullTypes.TryGetValue(typeDefinition.Name, out @namespace)))
							{
								ad.MainModule.Types.Remove(typeDefinition);
								typeDefinition.Scope = harmonyTypes.Contains(typeDefinition.FullName) ? harmonyShimRef : harmony2Ref;
								typeDefinition.GetType().GetField("module", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(typeDefinition, ad.MainModule);
								typeDefinition.MetadataToken = new MetadataToken(TokenType.TypeRef, 0);

								if (@namespace != null)
									typeDefinition.Namespace = @namespace;

								foreach (var typeDefinitionMethod in typeDefinition.Methods)
									typeDefinitionMethod.MetadataToken = new MetadataToken(TokenType.MemberRef, 0);

								foreach (var typeDefinitionField in typeDefinition.Fields)
									typeDefinitionField.MetadataToken = new MetadataToken(TokenType.MemberRef, 0);
							}
						}
					}
					// Otherwise just shim Harmony
					else if (harmonyRef != null)
					{
						shimmed = true;

						Logger.LogInfo($"Shimming {Path.GetFileNameWithoutExtension(file)}");

						harmonyRef.Name = "0Harmony_Shim";
						ad.MainModule.AssemblyReferences.Add(harmony2Ref);

						foreach (var tr in ad.MainModule.GetTypeReferences())
							if (tr.Namespace.StartsWith("Harmony"))
							{
								if (harmonyTypes.Contains(tr.FullName) || !harmonyFullTypes.TryGetValue(tr.Name, out var @namespace))
									continue;
								tr.Namespace = @namespace;
								tr.Scope = harmony2Ref;
							}
					}

					if (shimmed)
					{
						var pathPart = file.Substring(pluginDirectory.Length + 1);
						var bakPath = Path.Combine(bakDir, pathPart);
						Logger.LogInfo($"Path part: {pathPart}; Bak dir: {bakDir}; Backup path: {bakPath}; original path: {file}");
						Directory.CreateDirectory(Path.GetDirectoryName(bakPath));
						File.Copy(file, bakPath, true);

						definitionsAreReferences = true;
						ad.Write(file);
						definitionsAreReferences = false;
					}
				}
				catch (Exception e)
				{
					Logger.LogWarning($"Failed to shim {Path.GetFileName(file)}. Reason: {e.Message}");
				}
			}
		}


		public static void Patch(AssemblyDefinition ad) { }

		public static void Finish() { AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => new AssemblyName(args.Name).Name == "0Harmony_Shim" ? Assembly.GetExecutingAssembly() : null; }
	}
}