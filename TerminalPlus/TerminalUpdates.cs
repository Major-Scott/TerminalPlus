using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using TerminalPlus;
using UnityEngine;

namespace TerminalPlus
{
    partial class Nodes
    {
        public static bool[] priceOverride;
        //private static string[] displayWeather = new string[0];
        //private static string[] halfWeather = new string[0];
        //internal static string[] fullWeather = new string[0];

        [HarmonyPatch(typeof(TimeOfDay), "OnDayChanged")]
        [HarmonyPostfix]
        public static void TermUpdate()
        {
            mls.LogInfo("Updating Moon Catalogue...");
            foreach (MoonMaster moonTU in moonMasters.Values)
            {
                mls.LogInfo($"current moon: {moonTU.mName}");

                if (moonTU.mPriceOR == true)
                {
                    mls.LogDebug("PRICE OVERRIDE ACTIVE");
                    moonTU.mPrice = routeNouns.FirstOrDefault(n => n.result.displayPlanetInfo == moonTU.mID).result.itemCost;
                }
                UpdateMoonWeather();
            }
        }

        public static void UpdateMoonWeather()
        {
            if (PluginMain.LLLExists) LLLCompatibility.ModdedWeathers();

            foreach (MoonMaster moonUMW in moonMasters.Values)
            {
                string cWeather = PluginMain.LLLExists ? moonUMW.dispWeather : moonUMW.mLevel.currentWeather.ToString();
                moonUMW.halfWeather = string.Empty;

                if ((cWeather == "None" || cWeather == string.Empty) && ConfigManager.showClear)
                {
                    if (Chainloader.PluginInfos.ContainsKey("WeatherTweaks")) moonUMW.dispWeather = moonUMW.mWeather = "None";
                    else moonUMW.dispWeather = moonUMW.mWeather = "Clear";
                }
                else if (cWeather == "None" || cWeather == string.Empty) moonUMW.dispWeather = moonUMW.mWeather = string.Empty;
                else if (cWeather.Length > 10 && !ConfigManager.longWeather) moonUMW.dispWeather = "Complex";
                else if (cWeather.Length > 10 && cWeather.Contains("/") && cWeather.IndexOf('/') <= 10)
                {
                    mls.LogDebug("splitting weather at slash");
                    moonUMW.dispWeather = cWeather.Substring(0, cWeather.IndexOf('/') + 1);
                    moonUMW.halfWeather = cWeather.Substring(cWeather.IndexOf("/") + 1);
                    moonUMW.mWeather = cWeather;
                }
                else if (cWeather.Length > 10 && cWeather.Contains(" ") && cWeather.IndexOf(' ') <= 10)
                {
                    mls.LogDebug("splitting weather at space");
                    moonUMW.dispWeather = cWeather.Substring(0, cWeather.IndexOf(' ') + 1);
                    moonUMW.halfWeather = cWeather.Substring(cWeather.IndexOf(' ') + 1);
                    moonUMW.mWeather = cWeather;
                }
                else if (cWeather.Length > 10)
                {
                    mls.LogDebug("splitting weather at length 10");
                    moonUMW.dispWeather = cWeather.Substring(0, 9) + "-";
                    moonUMW.halfWeather = cWeather.Substring(9);
                    moonUMW.mWeather = cWeather;
                }
                else if (cWeather != null) moonUMW.mWeather = moonUMW.dispWeather = cWeather;
                else moonUMW.mWeather = moonUMW.dispWeather = string.Empty;

                if (moonUMW.halfWeather.Length > 10) moonUMW.halfWeather = moonUMW.halfWeather.Substring(0, 7) + "...";
            }



            //for (int i = 0; i < moonMasters.Count; i++)
            //{
            //    string cWeather = PluginMain.LLLExists ? CompatibilityInfo.moddedWeathers[i] : moonMasters[i].buWeather;
            //    halfWeather[i] = string.Empty;

            //    if ((cWeather == "None" || cWeather == string.Empty) && ConfigManager.showClear)
            //    {
            //        if (Chainloader.PluginInfos.ContainsKey("WeatherTweaks")) fullWeather[i] = displayWeather[i] = "None";
            //        else fullWeather[i] = displayWeather[i] = "Clear";
            //    }
            //    else if (cWeather == "None" || cWeather == string.Empty) fullWeather[i] = displayWeather[i] = string.Empty;
            //    else if (cWeather.Length > 10 && !ConfigManager.longWeather) fullWeather[i] = displayWeather[i] = "Complex";
            //    else if (cWeather.Length > 10 && cWeather.Contains("/") && cWeather.IndexOf('/') <= 10)
            //    {
            //        mls.LogDebug("splitting weather at slash");
            //        displayWeather[i] = cWeather.Substring(0, cWeather.IndexOf('/') + 1);
            //        halfWeather[i] = cWeather.Substring(cWeather.IndexOf("/") + 1);
            //        fullWeather[i] = cWeather;
            //    }
            //    else if (cWeather.Length > 10 && cWeather.Contains(" ") && cWeather.IndexOf(' ') <= 10)
            //    {
            //        mls.LogDebug("splitting weather at space");
            //        displayWeather[i] = cWeather.Substring(0, cWeather.IndexOf(' ') + 1);
            //        halfWeather[i] = cWeather.Substring(cWeather.IndexOf(' ') + 1);
            //        fullWeather[i] = cWeather;
            //    }
            //    else if (cWeather.Length > 10)
            //    {
            //        mls.LogDebug("splitting weather at length 10");
            //        displayWeather[i] = cWeather.Substring(0, 9) + "-";
            //        halfWeather[i] = cWeather.Substring(9);
            //        fullWeather[i] = cWeather;
            //    }
            //    else if (cWeather != null) fullWeather[i] = displayWeather[i] = cWeather;
            //    else fullWeather[i] = displayWeather[i] = string.Empty;

            //    if (halfWeather[i].Length > 10) halfWeather[i] = halfWeather[i].Substring(0, 7) + "...";
            //}
        }

    }
}

//public static void UpdateMoonWeather()
//{
//    halfWeather = new string[moonsMaster.Count];
//    displayWeather = new string[moonsMaster.Count];
//    fullWeather = new string[moonsMaster.Count];

//    for (int i = 0; i < moonsMaster.Count; i++)
//    {
//        string cWeather = PluginMain.LLLExists ? CompatibilityInfo.moddedWeathers[i] : moonsMaster[i].mWeather;
//        halfWeather[i] = string.Empty;

//        if ((cWeather == "None" || cWeather == string.Empty) && ConfigManager.showClear)
//        {
//            if (Chainloader.PluginInfos.ContainsKey("WeatherTweaks")) fullWeather[i] = displayWeather[i] = "None";
//            else fullWeather[i] = displayWeather[i] = "Clear";
//        }
//        else if (cWeather == "None" || cWeather == string.Empty) fullWeather[i] = displayWeather[i] = string.Empty;
//        else if (cWeather.Length > 10 && !ConfigManager.longWeather) fullWeather[i] = displayWeather[i] = "Complex";
//        else if (cWeather.Length > 10 && cWeather.Contains("/") && cWeather.IndexOf('/') <= 10)
//        {
//            mls.LogDebug("splitting weather at slash");
//            displayWeather[i] = cWeather.Substring(0, cWeather.IndexOf('/') + 1);
//            halfWeather[i] = cWeather.Substring(cWeather.IndexOf("/") + 1);
//            fullWeather[i] = cWeather;
//        }
//        else if (cWeather.Length > 10 && cWeather.Contains(" ") && cWeather.IndexOf(' ') <= 10)
//        {
//            mls.LogDebug("splitting weather at space");
//            displayWeather[i] = cWeather.Substring(0, cWeather.IndexOf(' ') + 1);
//            halfWeather[i] = cWeather.Substring(cWeather.IndexOf(' ') + 1);
//            fullWeather[i] = cWeather;
//        }
//        else if (cWeather.Length > 10)
//        {
//            mls.LogDebug("splitting weather at length 10");
//            displayWeather[i] = cWeather.Substring(0, 9) + "-";
//            halfWeather[i] = cWeather.Substring(9);
//            fullWeather[i] = cWeather;
//        }
//        else if (cWeather != null) fullWeather[i] = displayWeather[i] = cWeather;
//        else fullWeather[i] = displayWeather[i] = string.Empty;

//        if (halfWeather[i].Length > 10) halfWeather[i] = halfWeather[i].Substring(0, 7) + "...";
//    }
//}