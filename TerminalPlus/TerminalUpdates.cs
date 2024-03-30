using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace TerminalPlus
{
    partial class Nodes
    {
        public static bool[] priceOverride;

        [HarmonyPatch(typeof(TimeOfDay), "OnDayChanged")]
        [HarmonyPostfix]
        public static void TermUpdate()
        {
            halfWeather = new string[moonsList.Count];

            mls.LogInfo("Updating Moon Catalogue...");
            foreach (SelectableLevel slUpdate in moonsList)
            {
                string cWeather = slUpdate.currentWeather.ToString();
                mls.LogMessage($"current moon: {slUpdate.PlanetName}");
                halfWeather[slUpdate.levelID] = string.Empty;

                if (priceOverride[slUpdate.levelID] == true)
                {
                    mls.LogDebug("PRICE OVERRIDE ACTIVE");
                    //moonsPrice[slUpdate.levelID] = Resources.FindObjectsOfTypeAll<TerminalNode>().Where((TerminalNode k) => k.buyRerouteToMoon == slUpdate.levelID).FirstOrDefault().itemCost;
                    moonsPrice[slUpdate.levelID] = Resources.FindObjectsOfTypeAll<TerminalNode>().FirstOrDefault((TerminalNode k) => k.buyRerouteToMoon == slUpdate.levelID).itemCost;
                }

                if (slUpdate.currentWeather == LevelWeatherType.None && ConfigManager.showClear) displayWeather[slUpdate.levelID] = "Clear";
                else if (slUpdate.currentWeather == LevelWeatherType.None) displayWeather[slUpdate.levelID] = string.Empty;
                else if (cWeather.Length > 10 && !ConfigManager.longWeather) displayWeather[slUpdate.levelID] = "Complex";
                else if (cWeather.Length > 10 && cWeather.Contains("/") && cWeather.IndexOf('/') <= 10)
                {
                    mls.LogDebug("splitting weather at slash");
                    displayWeather[slUpdate.levelID] = cWeather.Substring(0, cWeather.IndexOf('/') + 1);
                    halfWeather[slUpdate.levelID] = cWeather.Substring(cWeather.IndexOf("/") + 1);
                }
                else if (cWeather.Length > 10)
                {
                    mls.LogMessage("splitting weather at length 10");
                    displayWeather[slUpdate.levelID] = cWeather.Substring(0, 9) + "-";
                    halfWeather[slUpdate.levelID] = cWeather.Substring(9);
                }
                else displayWeather[slUpdate.levelID] = cWeather;
                if (halfWeather[slUpdate.levelID].Length > 10) halfWeather[slUpdate.levelID] = halfWeather[slUpdate.levelID].Substring(0, 7) + "...";
            }
        }

    }
}
