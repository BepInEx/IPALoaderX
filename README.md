# IPALoaderX

An improved version of the IPALoader from [BepisPlugins](https://github.com/bbepis/BepisPlugins#ipaloader). Most of the code was copied from the [IPA github page](https://github.com/Eusth/IPA).

This plugin lets you load IPA plugins while using BepInEx and attempts to do that in the most accurate way possible to allow for every plugin to work without modification. I can't guarantee absolutely every plugin will work because testing that would be impossible for one person. So if you suspect some plugin is not handled correctly by IPALoaderX, don't hesitate to make a new issue on this github page.

Using this you could potentially be able to move away from using IPA in any moddable Unity game and still benefit from the old plugins made for the game.
For example, this plugin allows for BepInEx to be used in Honey Select in order to benefit from the many well established IPA plugins, useful new BepInEx plugins such as the [automatic translator](https://github.com/bbepis/XUnity.AutoTranslator#readme) and the [runtime unity editor](https://github.com/ManlyMarco/RuntimeUnityEditor#readme) and the patchless nature of BepInEx, all at the same time.

## How to use

1. Install [BepInEx](https://github.com/BepInEx/BepInEx#readme) to your game (you may need to edit the BepInEx entrypoint for it to work).
2. Download the latest IPALoaderX release from [here](https://github.com/Keelhauled/IPALoaderX/releases).
3. Place IPALoaderX.dll in the BepInEx folder and IPAVirtualizer.dll in the patchers folder.
