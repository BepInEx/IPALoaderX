using System;
using System.Reflection;
using BepInEx;
using UnityEngine;

namespace IPALoaderX
{
    [BepInPlugin("keelhauled.ipaloaderx", "IPALoaderX", "1.0.0")]
    public class IPALoaderX : BaseUnityPlugin
    {
        IPALoaderX()
        {
            //only required for ILMerge
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                if(args.Name == "IllusionPlugin, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" ||
                   args.Name == "IllusionInjector, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null")
                        return Assembly.GetExecutingAssembly();

                return null;
            };
        }

        void Awake()
        {
            if(PluginManager.Plugins.Count > 0)
            {
                var bootstrapper = new GameObject("Bootstrapper").AddComponent<Bootstrapper>();
                bootstrapper.Destroyed += Bootstrapper_Destroyed;
            }
            else
            {
                Console.WriteLine("No IPA plugins");
            }
        }

        static void Bootstrapper_Destroyed()
        {
            PluginComponent.Create();
        }
    }
}
