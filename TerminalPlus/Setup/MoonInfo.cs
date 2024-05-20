using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace TerminalPlus
{
    public class MoonMaster
    {
        public readonly SelectableLevel mLevel;
        public readonly string mLevelName;
        public bool mPriceOR;

        public readonly string origName;
        public string mName = string.Empty;    
        public string mPrefix = string.Empty;
        public int mID;
        public string mGrade;
        public bool mVis = true;
        public string mWeather;
        public int mPrice = -1;
        public string mDesc;

        public string dispName = string.Empty;
        public string dispPrefix = string.Empty;
        public string dispGrade = string.Empty;
        public string dispWeather;
        public string halfWeather = string.Empty;

        public MoonMaster(SelectableLevel moon)
        {
            mLevel = moon;
            mLevelName = moon.name;
            origName = moon.PlanetName;
            mID = moon.levelID;
            mWeather = dispWeather = moon.currentWeather.ToString();
            mGrade = moon.riskLevel;
            mDesc = moon.LevelDescription;
        }
    }

    public partial class Nodes
    {
        public static List<SelectableLevel> moonsList = new List<SelectableLevel>();
        public static Dictionary<int, MoonMaster> moonMasters = new Dictionary<int, MoonMaster>();
        public static List<MoonMaster> masterList = new List<MoonMaster>();

        public static CompatibleNoun[] routeNouns = new CompatibleNoun[0];
        public static TerminalNode[] confirmNodes = Resources.FindObjectsOfTypeAll<TerminalNode>().Where((TerminalNode k) => k.buyRerouteToMoon >= 0).ToArray();

        public static string catalogueSort = "   DEFAULT ⇩";

        public static ManualLogSource mls = BepInEx.Logging.Logger.CreateLogSource("TerminalPlus");


        [HarmonyPatch(typeof(RoundManager), "Start")]
        [HarmonyPostfix]
        [HarmonyPriority(3)]
        private static void PatchMoonInfo()
        {
            List<int> IDChecker = new List<int>();
            List<SelectableLevel> errorArray = new List<SelectableLevel>();

            routeNouns = terminal.terminalNodes.allKeywords[27].compatibleNouns;

            moonMasters.Clear();
            moonsList = Resources.FindObjectsOfTypeAll<SelectableLevel>().ToList();

            foreach (SelectableLevel selectableLevel in moonsList)
            {
                if (selectableLevel.levelID < 0)//-----------------------------------------------------confirms IDs
                {
                    mls.LogInfo($"Entry \"{selectableLevel.PlanetName}\" has an ID below zero. Removing...");
                    errorArray.Add(selectableLevel);
                    continue;
                }
                else if (IDChecker.Contains(selectableLevel.levelID))//-----------------------------------------------------checks for dupes
                {
                    mls.LogInfo($"Entry \"{selectableLevel.PlanetName}\" has a repeat ID. Removing...");
                    errorArray.Add(selectableLevel);
                    continue;
                }
                else if (selectableLevel.PlanetName.ToLower().Contains("liquidation"))
                {
                    mls.LogInfo($"Moon \"{selectableLevel.PlanetName}\" is unreleased. Removing...");
                    errorArray.Add(selectableLevel);
                    continue;
                }
                IDChecker.Add(selectableLevel.levelID);
                moonMasters.Add(selectableLevel.levelID, new MoonMaster(selectableLevel));
            }

            //foreach (SelectableLevel item in errorArray) moonsList.Remove(item);//-----------------fixes arrays

            for(int i = errorArray.Count - 1; i >= 0; i--)
            {
                moonsList.Remove(errorArray[i]);
            }
            IDChecker.Clear();

            foreach (MoonMaster moonFP in moonMasters.Values)
            {
                foreach (CompatibleNoun compatibleNoun in routeNouns)
                {
                    if (compatibleNoun.result.displayPlanetInfo == moonFP.mID)
                    {
                        moonFP.mPrice = compatibleNoun.result.itemCost;
                        mls.LogInfo($"Found price of {moonFP.mLevelName}: ${moonFP.mPrice}");
                    }
                }
            }
        }

        [HarmonyPatch(typeof(RoundManager), "Start")]
        [HarmonyPostfix]
        [HarmonyPriority(3)]
        public static void NameSeparator()
        {   
            foreach (MoonMaster moonNS in moonMasters.Values)
            {
                if (!char.IsDigit(moonNS.origName.First())) moonNS.mName = moonNS.origName;
                else
                {
                    moonNS.mPrefix = moonNS.origName.Substring(0, moonNS.origName.IndexOf(' '));
                    moonNS.mName = moonNS.origName.Substring(moonNS.origName.IndexOf(' ') + 1);
                }
            }
        }

        [HarmonyPatch(typeof(RoundManager), "Start")]
        [HarmonyPostfix]
        [HarmonyPriority(1)]
        public static void MoonCatalogueSetup()
        {
            mls.LogDebug("SETUP START");
            UpdateMoonWeather();

            foreach (MoonMaster moonMCS in moonMasters.Values)
            {
                int cID = moonMCS.mID;
                bool isLongPrefix = moonMasters.Values.Any(p => p.mPrefix.Length == 4);

                moonMCS.dispName = moonMCS.mName.Length <= 15 ? moonMCS.mName.PadRight(15) : moonMCS.mName.Substring(0, 12) + "...";

                if (isLongPrefix) { moonMCS.dispPrefix = moonMCS.mPrefix.Length <= 4 ? moonMCS.mPrefix.PadLeft(4, ConfigManager.padChar) : moonMCS.mPrefix.Substring(0, 4); }
                else { moonMCS.dispPrefix = moonMCS.mPrefix.Length <= 3 ? moonMCS.mPrefix.PadLeft(3, ConfigManager.padChar) : moonMCS.mPrefix.Substring(0, 3); }

                if (moonMCS.mGrade.ToLower() == "unknown") moonMCS.dispGrade = " ?? ";
                else moonMCS.dispGrade = moonMCS.mGrade.Length <= 2 || moonMCS.mGrade == "Safe" ? moonMCS.mGrade.PadRight(3).PadLeft(4) : moonMCS.mGrade.Substring(0, 2).PadRight(3).PadLeft(4);

                if (moonMCS.mLevelName == "CompanyBuildingLevel" && moonMCS.mName == "Company Building") moonMCS.dispName = "Company<space=0.3en>Building<space=-0.3en>";
            }
            masterList = moonMasters.Values.ToList();

            switch (ConfigManager.defaultSort)
            {
                case 1:
                    masterList.Sort((x, y) => x.mName.CompareTo(y.mName));
                    catalogueSort = "      NAME ⇩";
                    break;
                case 2:
                    masterList.Sort(SortByPrefix);
                    catalogueSort = "    PREFIX ⇩";   //⇧⇩
                    break;
                case 3:
                    masterList.Sort(SortByGrade);
                    catalogueSort = "     GRADE ⇩";
                    break;
                case 4:
                    masterList.Sort((x, y) => x.mPrice.CompareTo(y.mPrice));
                    catalogueSort = "     PRICE ⇩";
                    break;
                case 5:
                    masterList.Sort(SortByWeather);
                    catalogueSort = "   WEATHER ⇩";
                    break;
                case 6:
                    masterList.Sort(SortByDifficulty);
                    catalogueSort = "DIFFICULTY ⇩";
                    break;
                case 7:
                    masterList.Sort((x,y) => x.mID.CompareTo(y.mID));
                    masterList.Reverse();
                    catalogueSort = "   DEFAULT ⇧";
                    break;
                case 8:
                    masterList.Sort((x, y) => x.mName.CompareTo(y.mName));
                    masterList.Reverse();
                    catalogueSort = "      NAME ⇧";
                    break;
                case 9:
                    masterList.Sort(SortByPrefix);
                    masterList.Reverse();
                    catalogueSort = "    PREFIX ⇧";
                    break;
                case 10:
                    masterList.Sort(SortByGrade);
                    masterList.Reverse();
                    catalogueSort = "     GRADE ⇧";
                    break;
                case 11:
                    masterList.Sort((x, y) => x.mPrice.CompareTo(y.mPrice));
                    masterList.Reverse();
                    catalogueSort = "     PRICE ⇧";
                    break;
                case 12:
                    masterList.Sort(SortByWeather);
                    masterList.Reverse();
                    catalogueSort = "   WEATHER ⇧";
                    break;
                case 13:
                    masterList.Sort(SortByDifficulty);
                    masterList.Reverse();
                    catalogueSort = "DIFFICULTY ⇧";
                    break;
                default:
                    masterList.Sort((x, y) => x.mID.CompareTo(y.mID));
                    catalogueSort = "   DEFAULT ⇩";
                    break;
            }

            mls.LogDebug("SETUP END");

            return;
        }
    }
}
