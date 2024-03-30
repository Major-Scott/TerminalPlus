using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.IO;

namespace TerminalPlus
{
    [BepInPlugin("Slam.TerminalPlus", "TerminalPlus", "1.0.0")]
    public class PluginMain : BaseUnityPlugin
    {
        internal static ManualLogSource mls;
        internal static Harmony harmony = new Harmony("Slam.TerminalPlus");
        private static PluginMain instance;
        public static ConfigFile configFile;

        void Awake()
        {
            if (instance == null) { instance = this; }

            mls = BepInEx.Logging.Logger.CreateLogSource("TerminalPlus");
            harmony.PatchAll(typeof(Nodes));
            harmony.PatchAll(typeof(ConfigManager));
            harmony.PatchAll(typeof(TerminalPatches));
            harmony.PatchAll(typeof(PluginMain));

            configFile = base.Config;

            if (!File.Exists(Config.ConfigFilePath)) configFile.Save();
        }
    }
}