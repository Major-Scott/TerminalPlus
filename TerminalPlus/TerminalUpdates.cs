using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using LethalLevelLoader;
using UnityEngine;

namespace TerminalPlus
{
    partial class Nodes
    {
        public static bool[] priceOverride;
        private static string[] displayWeather = new string[0];
        private static string[] halfWeather = new string[0];
        internal static string[] fullWeather = new string[0];

        [HarmonyPatch(typeof(TimeOfDay), "OnDayChanged")]
        [HarmonyPostfix]
        public static void TermUpdate()
        {
            mls.LogInfo("Updating Moon Catalogue...");
            foreach (SelectableLevel slUpdate in moonsList)
            {
                mls.LogInfo($"current moon: {slUpdate.PlanetName}");

                if (priceOverride[slUpdate.levelID] == true)
                {
                    mls.LogDebug("PRICE OVERRIDE ACTIVE");
                    //moonsPrice[slUpdate.levelID] = Resources.FindObjectsOfTypeAll<TerminalNode>().Where((TerminalNode k) => k.buyRerouteToMoon == slUpdate.levelID).FirstOrDefault().itemCost;
                    moonsPrice[slUpdate.levelID] = Resources.FindObjectsOfTypeAll<TerminalNode>().FirstOrDefault((TerminalNode k) => k.buyRerouteToMoon == slUpdate.levelID).itemCost;
                }
                UpdateMoonWeather();
            }
        }

        public static void UpdateMoonWeather()
        {
            halfWeather = new string[moonsList.Count];
            displayWeather = new string[moonsList.Count];
            fullWeather = new string[moonsList.Count];

            foreach (SelectableLevel sl in  moonsList)
            {
                string cWeather = sl.currentWeather.ToString();
                halfWeather[sl.levelID] = string.Empty;

                if (PluginMain.LLLExists)
                {
                    mls.LogDebug("LLL exists, grabbing it's weather info...");
                    cWeather = typeof(TerminalManager).GetMethod("GetWeatherConditions", BindingFlags.Static | BindingFlags.NonPublic).Invoke
                        (null, new object[1] { sl }).ToString().Replace("(", string.Empty).Replace(")", string.Empty);
                }

                if ((cWeather == "None" || cWeather == string.Empty) && ConfigManager.showClear)
                {
                    if (Chainloader.PluginInfos.ContainsKey("WeatherTweaks")) fullWeather[sl.levelID] = displayWeather[sl.levelID] = "None";
                    else fullWeather[sl.levelID] = displayWeather[sl.levelID] = "Clear";
                }
                else if (cWeather == "None" || cWeather == string.Empty) fullWeather[sl.levelID] = displayWeather[sl.levelID] = string.Empty;
                else if (cWeather.ToLower().Contains("unknown")) fullWeather[sl.levelID] = displayWeather[sl.levelID] = "[UNKNOWN]";
                else if (cWeather.Length > 10 && !ConfigManager.longWeather) fullWeather[sl.levelID] = displayWeather[sl.levelID] = "Complex";
                else if (cWeather.Length > 10 && cWeather.Contains("/") && cWeather.IndexOf('/') <= 10)
                {
                    mls.LogDebug("splitting weather at slash");
                    displayWeather[sl.levelID] = cWeather.Substring(0, cWeather.IndexOf('/') + 1);
                    halfWeather[sl.levelID] = cWeather.Substring(cWeather.IndexOf("/") + 1);
                    fullWeather[sl.levelID] = cWeather;
                }
                else if (cWeather.Length > 10 && cWeather.Contains(" ") && cWeather.IndexOf(' ') <= 10)
                {
                    mls.LogDebug("splitting weather at space");
                    displayWeather[sl.levelID] = cWeather.Substring(0, cWeather.IndexOf(' ') + 1);
                    halfWeather[sl.levelID] = cWeather.Substring(cWeather.IndexOf(' ') + 1);
                    fullWeather[sl.levelID] = cWeather;
                }
                else if (cWeather.Length > 10)
                {
                    mls.LogDebug("splitting weather at length 10");
                    displayWeather[sl.levelID] = cWeather.Substring(0, 9) + "-";
                    halfWeather[sl.levelID] = cWeather.Substring(9);
                    fullWeather[sl.levelID] = cWeather;
                }
                else if (cWeather != null) fullWeather[sl.levelID] = displayWeather[sl.levelID] = cWeather;
                else fullWeather[sl.levelID] = displayWeather[sl.levelID] = string.Empty;

                if (halfWeather[sl.levelID].Length > 10) halfWeather[sl.levelID] = halfWeather[sl.levelID].Substring(0, 7) + "...";
            }

        }


    }
}
