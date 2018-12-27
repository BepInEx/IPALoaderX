using System;
using System.Reflection;
using BepInEx;
using IllusionInjector;

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
            Injector.Inject();
        }
    }
}
