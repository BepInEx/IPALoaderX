# IPALoaderX

IPALoaderX is a plugin for BepInEx that allows to load plugins made for [Illusion Plugin Architecture](https://github.com/Eusth/IPA) without installing it.

This plugin is an improved version of the original [IPALoader](https://github.com/bbepis/BepisPlugins#ipaloader).

## Differences from IPALoader

* Integration with BepInEx logger
* Parts of IPA rewritten for simplicity
* Allows virtualization via a preloader plugin
* Patches IPA plugins with Harmony ILMerged to use the BepInEx's Harmony

While IPALoaderX aims to emulate IPA as closely as possible, I can't guarantee absolutely every plugin will work.  
If you suspect some plugin is not handled correctly by IPALoaderX, don't hesitate to make a new issue on this github page.

## Why IPA for BepInEx

Using this you could potentially be able to move away from using IPA in any moddable Unity game and still benefit from the old plugins made for the game.  
For example, this plugin allows for BepInEx to be used in Honey Select in order to benefit from the many well established IPA plugins, useful new BepInEx plugins such as the [automatic translator](https://github.com/bbepis/XUnity.AutoTranslator#readme) and the [runtime unity editor](https://github.com/ManlyMarco/RuntimeUnityEditor#readme) and the patchless nature of BepInEx, all at the same time.

## How to use

1. Install [BepInEx](https://github.com/BepInEx/BepInEx#readme) to your game (you may need to edit the BepInEx entrypoint for it to work).
2. Download the latest IPALoaderX release from [here](https://github.com/BepInEx/IPALoaderX/releases).
3. Place BepInEx.IPALoader.dll in the `BepInEx\plugins` folder and BepInEx.IPAVirtualizer.dll and BepInEx.IPAHarmonyShimmer.dll in the `BepInEx\patchers` folder.
4. Make `Plugins` folder in your game folder (just like with original IPA).
5. Put IPA plugins into `Plugins` folder, just like with IPA.
