using System;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using IllusionInjector;

namespace IPALoaderX
{
    [BepInPlugin("keelhauled.ipaloaderx", "IPALoaderX", "1.0.0")]
    public class IPALoaderX : BaseUnityPlugin
    {
        internal new static ManualLogSource Logger;

        IPALoaderX()
        {
            Logger = base.Logger;

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
            Injector.Inject();
        }
    }
}
