namespace BepInEx.IPALoader
{
    public static class Metadata
    {
        public const string PluginsVersion = "1.2.2";

        internal const string ConfigFileName = "BepInEx.IPALoader.cfg";
        internal const string ConfigSection = "Config";
        internal const string ConfigKey = "Plugins Path";
        internal const string ConfigDescription = "Folder from which to load IPA plugins relative to the game root directory";
        internal const string ConfigDefaultValue = "Plugins";
    }
}
