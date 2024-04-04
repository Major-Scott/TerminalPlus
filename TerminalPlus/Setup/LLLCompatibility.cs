using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using LethalLevelLoader;
using UnityEngine.InputSystem;
using static TerminalPlus.Nodes;

namespace TerminalPlus
{
    internal class LLLCompatibility
    {
        [HarmonyPatch(typeof(TerminalManager), "RefreshExtendedLevelGroups")]
        [HarmonyPostfix]
        public static void LLLVisibleMoons(MoonsCataloguePage ___currentMoonsCataloguePage)
        {
            foreach (MoonMaster moon in moonMasters.Values) moon.mVis = ___currentMoonsCataloguePage.ExtendedLevels.Any(x => x.selectableLevel == moon.mLevel);
        }

        public static void ModdedWeathers()
        {
            PluginMain.mls.LogDebug("LLL exists, grabbing it's weather info...");

            foreach (MoonMaster moon in moonMasters.Values)
            {
                moon.dispWeather = typeof(TerminalManager).GetMethod("GetWeatherConditions", BindingFlags.Static | BindingFlags.NonPublic).Invoke
                    (null, new object[1] { moon.mLevel }).ToString().Replace("(", string.Empty).Replace(")", string.Empty);
            }

        }
    }
}
