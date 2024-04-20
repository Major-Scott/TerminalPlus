using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.IO;
using System;
using UnityEditor;
using System.Reflection;
using System.Runtime.InteropServices;
using TerminalPlus.Mods;
using UnityEngine;

namespace TerminalPlus
{
    [BepInPlugin("Slam.TerminalPlus", "TerminalPlus", "1.1.0")]
    public class PluginMain : BaseUnityPlugin
    {
        internal static ManualLogSource mls;
        internal static Harmony harmony = new Harmony("Slam.TerminalPlus");
        private static PluginMain instance;
        public static ConfigFile configFile;
        internal static bool LLLExists = false;
        internal static bool LethalLibExists = false;
        internal static bool LGUExists = false;
        public static string PluginPath { get; }

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
            mls.LogInfo("TerminalPlus has loaded!");

            if (Chainloader.PluginInfos.ContainsKey("imabatby.lethallevelloader"))
            {
                LLLExists = true;
                harmony.PatchAll(typeof(LLLCompatibility));
                mls.LogInfo("LethalLevelLoader detected. Utilizing...");
            }
            if (Chainloader.PluginInfos.ContainsKey("evaisa.lethallib"))
            {
                LethalLibExists = true;
                harmony.PatchAll(typeof(LethalLibCompatibility));
                mls.LogInfo("LethalLib detected. Utilizing...");
            }
            if (Chainloader.PluginInfos.ContainsKey("com.malco.lethalcompany.moreshipupgrades"))
            {
                LGUExists = true;
                harmony.PatchAll(typeof(LGU.LGUCompatibility));
                mls.LogInfo("LateGameUpgrades detected. Utilizing...");
            }
        }
    }
}