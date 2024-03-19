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
        //private static SelectableLevel[] moonsArray = Resources.FindObjectsOfTypeAll<SelectableLevel>();
        public static List<SelectableLevel> moonsList;

        public static int[] moonsPrice;
        public static List<string> moonNames = new List<string>();
        public static List<string> moonPrefixes = new List<string>();

        private static string[] displayNames = new string[0];
        private static string[] displayPrefixes = new string[0];
        private static string[] displayGrades = new string[0];
        private static string[] displayWeather = new string[0];

        public static CompatibleNoun[] routeNouns = new CompatibleNoun[0];
        public static string catalogueSort = "        ID ⇩";

        public static ManualLogSource mls = BepInEx.Logging.Logger.CreateLogSource("TerminalPlus");

        [HarmonyPatch(typeof(RoundManager), "Start")]
        [HarmonyPostfix]
        [HarmonyPriority(100)]
        private static void PatchMoonInfo()
        {
            List<SelectableLevel> errorArray = new List<SelectableLevel>();
            routeNouns = terminal.terminalNodes.allKeywords[26].compatibleNouns;
            
            moonsList = Resources.FindObjectsOfTypeAll<SelectableLevel>().ToList();

            foreach (SelectableLevel selectableLevel in moonsList)
            {
                mls.LogInfo($"Checking level {selectableLevel.PlanetName}'s ID: {selectableLevel.levelID}");
                if (selectableLevel.levelID < 0)//-----------------------------------------------------confirms IDs
                {
                    mls.LogInfo("Found ID below zero. Removing...");
                    errorArray.Add(selectableLevel);
                    continue;
                }
            }

            foreach (SelectableLevel item in errorArray) { moonsList.Remove(item); }//-----------------fixes arrays
            Array.Resize(ref moonsPrice, moonsList.Count);
            moonsList.Sort(SortByID);

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
                
                string nameTrimmed = string.Empty;
                string namePrefix = string.Empty;
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

            foreach (SelectableLevel slSetup in moonsList)
            {
                int cID = slSetup.levelID;
                bool isLongPrefix = moonPrefixes.Any(p => p.Length == 4);

                displayNames[cID] = moonNames[cID];
                displayPrefixes[cID] = moonPrefixes[cID];
                displayGrades[cID] = slSetup.riskLevel;

                displayNames[cID] = displayNames[cID].Length <= 15 ? displayNames[cID].PadRight(15) : displayNames[cID].Substring(0, 12) + "...";

                if (isLongPrefix) { displayPrefixes[cID] = displayPrefixes[cID].Length <= 4 ? displayPrefixes[cID].PadLeft(4, ConfigManager.padChar) : displayPrefixes[cID].Substring(0, 4); }
                else { displayPrefixes[cID] = displayPrefixes[cID].Length <= 3 ? displayPrefixes[cID].PadLeft(3, ConfigManager.padChar) : displayPrefixes[cID].Substring(0, 3); }

                displayGrades[cID] = displayGrades[cID].Length <= 2 || displayGrades[cID] == "Safe" ? displayGrades[cID].PadRight(3).PadLeft(4) : displayGrades[cID].Substring(0, 2).PadRight(3).PadLeft(4);

                if (slSetup.currentWeather == LevelWeatherType.None && ConfigManager.showClear) displayWeather[cID] = "Clear";
                else if (slSetup.currentWeather == LevelWeatherType.None) displayWeather[cID] = string.Empty;
                else if (slSetup.currentWeather.ToString().Length > 10) displayWeather[cID] = "Complex";
                else displayWeather[cID] = slSetup.currentWeather.ToString();
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
                    catalogueSort = "        ID ⇧";
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
                    catalogueSort = "        ID ⇩";
                    break;
            }
            mls.LogDebug("SETUP END");


            return;

        }

    }
}