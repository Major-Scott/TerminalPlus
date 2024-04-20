using System.Linq;
using BepInEx.Bootstrap;
using HarmonyLib;
using UnityEngine;
using static TerminalPlus.TerminalPatches;

namespace TerminalPlus
{
    partial class Nodes
    {
        public static bool[] priceOverride;
        public static string displayTime;
        public static string numTime;
        public static int placeholder;

        [HarmonyPatch(typeof(TimeOfDay), "OnDayChanged")]
        [HarmonyPostfix]
        public static void TermUpdate()
        {
            mls.LogInfo("Updating Moon Catalogue...");
            foreach (MoonMaster moonTU in moonMasters.Values)
            {
                mls.LogInfo($"current moon: {moonTU.mName}");

                if (moonTU.mPriceOR)
                {
                    mls.LogDebug("PRICE OVERRIDE ACTIVE");
                    moonTU.mPrice = routeNouns.FirstOrDefault(n => n.result.displayPlanetInfo == moonTU.mID).result.itemCost;
                }
                UpdateMoonWeather();
            }
            clockBG.localScale = Vector3.zero;
            terminalClock.text = string.Empty;
        }

        [HarmonyPatch(typeof(StartOfRound), "StartGame")]
        [HarmonyPostfix]
        public static void ClockPatch()
        {
            mls.LogDebug("day start");
            switch (ConfigManager.clockSetting)
            {
                case 0:
                    clockBG.localScale = Vector3.zero;
                    break;
                case 1:
                    clockBG.localScale = new Vector3(1.9f, 1.1f, 1f);
                    clockBG.localPosition = new Vector3(175f, 208f, -1f);
                    break;
                case 2:
                    clockBG.localScale = new Vector3(1.25f, 1.1f, 1f);
                    clockBG.localPosition = new Vector3(198f, 208f, -1f);
                    break;
                case 3:
                    clockBG.localScale = new Vector3(1.28f, 1.1f, 1f);
                    clockBG.localPosition = new Vector3(197f, 208f, -1f);
                    break;
            }
        }

        [HarmonyPatch(typeof(HUDManager), "SetClock")]
        [HarmonyPostfix]
        public static void TerminalTime(float timeNormalized, float numberOfHours, bool createNewLine, HUDManager __instance)
        {
            string currentTime = __instance.clockNumber.text != null ? __instance.clockNumber.text : string.Empty;
            numTime = currentTime.Substring(0, currentTime.IndexOf(':'));

            mls.LogInfo(terminal.topRightText.transform.parent.childCount);

            switch (ConfigManager.clockSetting)
            {
                case 0:
                    displayTime = string.Empty;
                    break;
                case 1:
                    displayTime = currentTime.Replace("\n", "<space=0.4en>");
                    break;
                case 2:
                    displayTime = numTime + "<space=0.6en>" + currentTime.Substring(currentTime.Length - 2);
                    break;
                case 3:
                    if (int.TryParse(numTime, out int miltHour))
                    {
                        miltHour = currentTime.ToLower().Contains("pm") && miltHour != 12 ? miltHour + 12 : miltHour;
                        displayTime = miltHour + currentTime.Substring(currentTime.IndexOf(':'), currentTime.Length - 4);
                    }
                    else displayTime = string.Empty;
                    break;
            }
            terminalClock.text = displayTime;
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
        }

    }
}