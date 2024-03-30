using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace TerminalPlus
{
    public partial class Nodes
    {
        public static List<SelectableLevel> moonsList = new List<SelectableLevel>();

        public static int[] moonsPrice;
        public static List<string> moonNames = new List<string>();
        public static List<string> moonPrefixes = new List<string>();

        private static string[] displayNames = new string[0];
        private static string[] displayPrefixes = new string[0];
        private static string[] displayGrades = new string[0];
        private static string[] displayWeather = new string[0];

        private static string[] halfWeather = new string[0];
        public static CompatibleNoun[] routeNouns = new CompatibleNoun[0];
        public static TerminalNode[] confirmNodes = Resources.FindObjectsOfTypeAll<TerminalNode>().Where((TerminalNode k) => k.buyRerouteToMoon >= 0).ToArray();
        //public static TerminalKeyword[] moonKeywords = new TerminalKeyword[0];

        public static string catalogueSort = "   DEFAULT ⇩";

        public static ManualLogSource mls = BepInEx.Logging.Logger.CreateLogSource("TerminalPlus");

        [HarmonyPatch(typeof(RoundManager), "Start")]
        [HarmonyPostfix]
        [HarmonyPriority(100)]
        private static void PatchMoonInfo()
        {
            List<int> IDChecker = new List<int>();
            List<SelectableLevel> errorArray = new List<SelectableLevel>();
            routeNouns = terminal.terminalNodes.allKeywords[26].compatibleNouns;
            moonsList.Clear();
            moonsList = Resources.FindObjectsOfTypeAll<SelectableLevel>().ToList();

            foreach (SelectableLevel selectableLevel in moonsList)
            {
                mls.LogInfo($"Checking level {selectableLevel.PlanetName}'s ID: {selectableLevel.levelID}");
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
                IDChecker.Add(selectableLevel.levelID);
            }

            foreach (SelectableLevel item in errorArray) moonsList.Remove(item);//-----------------fixes arrays

            for(int i = errorArray.Count - 1; i >= 0; i--)
            {
                moonsList.Remove(errorArray[i]);
                UnityEngine.Object.Destroy(errorArray[i]);
            }

            Array.Resize(ref moonsPrice, moonsList.Count);
            moonsList.Sort(SortByID);
            IDChecker.Clear();

            foreach (SelectableLevel slMatcher in moonsList)
            {
                foreach (CompatibleNoun compatibleNoun in routeNouns)
                {
                    if (compatibleNoun.result.displayPlanetInfo == slMatcher.levelID)
                    {
                        moonsPrice[slMatcher.levelID] = compatibleNoun.result.itemCost;
                        mls.LogInfo($"Found price of {slMatcher.PlanetName}: ${moonsPrice[slMatcher.levelID]}");
                    }
                    
                }
            }
        }


        [HarmonyPatch(typeof(RoundManager), "Start")]
        [HarmonyPostfix]
        [HarmonyPriority(100)]
        public static void NameSeparator()
        {
            moonNames.Clear();
            moonPrefixes.Clear();
            
            foreach (SelectableLevel selectableLevel in moonsList)
            {
                string namePrefix = string.Empty;
                string nameTrimmed;
                if (!char.IsDigit(selectableLevel.PlanetName.First())) nameTrimmed = selectableLevel.PlanetName;
                else
                {
                    namePrefix = selectableLevel.PlanetName.Substring(0, selectableLevel.PlanetName.IndexOf(' '));
                    nameTrimmed = selectableLevel.PlanetName.Substring(selectableLevel.PlanetName.IndexOf(' ') + 1);
                }
                moonNames.Insert(selectableLevel.levelID, nameTrimmed);
                moonPrefixes.Insert(selectableLevel.levelID, namePrefix);
            }
        }


        [HarmonyPatch(typeof(RoundManager), "Start")]
        [HarmonyPostfix]
        [HarmonyPriority(98)]
        public static void MoonCatalogueSetup()
        {
            mls.LogDebug("SETUP START");
            displayNames = new string[moonsList.Count];
            displayPrefixes = new string[moonsList.Count];
            displayGrades = new string[moonsList.Count];
            displayWeather = new string[moonsList.Count];
            halfWeather = new string[moonsList.Count];

            foreach (SelectableLevel slSetup in moonsList)
            {
                int cID = slSetup.levelID;
                bool isLongPrefix = moonPrefixes.Any(p => p.Length == 4);
                string cWeather = slSetup.currentWeather.ToString();
                displayNames[cID] = moonNames[cID];
                displayPrefixes[cID] = moonPrefixes[cID];
                displayGrades[cID] = slSetup.riskLevel;
                halfWeather[cID] = string.Empty;

                displayNames[cID] = moonNames[cID].Length <= 15 ? moonNames[cID].PadRight(15) : moonNames[cID].Substring(0, 12) + "...";

                if (isLongPrefix) { displayPrefixes[cID] = moonPrefixes[cID].Length <= 4 ? moonPrefixes[cID].PadLeft(4, ConfigManager.padChar) : moonPrefixes[cID].Substring(0, 4); }
                else { displayPrefixes[cID] = displayPrefixes[cID].Length <= 3 ? moonPrefixes[cID].PadLeft(3, ConfigManager.padChar) : moonPrefixes[cID].Substring(0, 3); }

                if (displayGrades[cID].ToLower() == "unknown") displayGrades[cID] = "??";
                displayGrades[cID] = displayGrades[cID].Length <= 2 || displayGrades[cID] == "Safe" ? displayGrades[cID].PadRight(3).PadLeft(4) : displayGrades[cID].Substring(0, 2).PadRight(3).PadLeft(4);

                if (slSetup.currentWeather == LevelWeatherType.None && ConfigManager.showClear) displayWeather[cID] = "Clear";
                else if (slSetup.currentWeather == LevelWeatherType.None) displayWeather[cID] = string.Empty;
                else if (cWeather.Length > 10 && !ConfigManager.longWeather) displayWeather[cID] = "Complex";
                else if (cWeather.Length > 10 && cWeather.Contains("/") && cWeather.IndexOf('/') <= 10)
                {
                    displayWeather[cID] = cWeather.Substring(0, cWeather.IndexOf('/') + 1);
                    halfWeather[cID] = cWeather.Substring(cWeather.IndexOf("/") + 1);
                }
                else if (cWeather.Length > 10)
                {
                    displayWeather[cID] = cWeather.Substring(0, 9) + "-";
                    halfWeather[cID] = cWeather.Substring(9);
                }
                else displayWeather[cID] = cWeather;

                if (halfWeather[cID].Length > 10) halfWeather[cID] = halfWeather[cID].Substring(0, 7) + "...";
            }
            
            switch (ConfigManager.defaultSort)
            {
                case 1:
                    moonsList.Sort((x, y) => moonNames[x.levelID].CompareTo(moonNames[y.levelID]));
                    catalogueSort = "      NAME ⇩";
                    break;
                case 2:
                    moonsList.Sort(SortByPrefix);
                    catalogueSort = "    PREFIX ⇩";   //⇧⇩
                    break;
                case 3:
                    moonsList.Sort(SortByGrade);
                    catalogueSort = "     GRADE ⇩";
                    break;
                case 4:
                    moonsList.Sort((x, y) => moonsPrice[x.levelID].CompareTo(moonsPrice[y.levelID]));
                    catalogueSort = "     PRICE ⇩";
                    break;
                case 5:
                    moonsList.Sort((x, y) => x.currentWeather.CompareTo(y.currentWeather));
                    catalogueSort = "   WEATHER ⇩";
                    break;
                case 6:
                    moonsList.Sort(SortByDifficulty);
                    catalogueSort = "DIFFICULTY ⇩";
                    break;
                case 7:
                    moonsList.Sort(SortByID);
                    moonsList.Reverse();
                    catalogueSort = "   DEFAULT ⇧";
                    break;
                case 8:
                    moonsList.Sort((x, y) => moonNames[x.levelID].CompareTo(moonNames[y.levelID]));
                    moonsList.Reverse();
                    catalogueSort = "      NAME ⇧";
                    break;
                case 9:
                    moonsList.Sort(SortByPrefix);
                    moonsList.Reverse();
                    catalogueSort = "    PREFIX ⇧";
                    break;
                case 10:
                    moonsList.Sort(SortByGrade);
                    moonsList.Reverse();
                    catalogueSort = "     GRADE ⇧";
                    break;
                case 11:
                    moonsList.Sort((x, y) => moonsPrice[x.levelID].CompareTo(moonsPrice[y.levelID]));
                    moonsList.Reverse();
                    catalogueSort = "     PRICE ⇧";
                    break;
                case 12:
                    moonsList.Sort((x, y) => x.currentWeather.CompareTo(y.currentWeather));
                    moonsList.Reverse();
                    catalogueSort = "   WEATHER ⇧";
                    break;
                case 13:
                    moonsList.Sort(SortByDifficulty);
                    moonsList.Reverse();
                    catalogueSort = "DIFFICULTY ⇧";
                    break;
                default:
                    moonsList.Sort(SortByID);
                    catalogueSort = "   DEFAULT ⇩";
                    break;
            }
            mls.LogDebug("SETUP END");


            return;

        }
    }
}