using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.IO;

namespace TerminalPlus
{
    [BepInPlugin("Slam.TerminalPlus", "TerminalPlus", "0.2.1")]
    public class PluginMain : BaseUnityPlugin
    {
        internal static ManualLogSource mls;
        internal static Harmony harmony = new Harmony("Slam.TerminalPlus");
        private static PluginMain instance;
        internal static ConfigFile configFile = new ConfigFile(Path.Combine(Paths.ConfigPath, "Slam.TerminalPlus.cfg"), saveOnInit: true);

        void Awake()
        {
            if (instance == null) { instance = this; }

            mls = BepInEx.Logging.Logger.CreateLogSource("Slam.TerminalPlus");
            harmony.PatchAll(typeof(Nodes));
            harmony.PatchAll(typeof(ConfigManager));
            harmony.PatchAll(typeof(TerminalPatches));
            harmony.PatchAll(typeof(PluginMain));

            mls.LogInfo($"TerminalPlus has loaded!");
        }
    }
}