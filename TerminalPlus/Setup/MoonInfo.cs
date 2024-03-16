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
        private static SelectableLevel[] moonsArray = Resources.FindObjectsOfTypeAll<SelectableLevel>();
        public static List<SelectableLevel> moonsList = moonsArray.ToList();

        public static int[] moonsPrice;
        public static List<string> moonNames = new List<string>();
        public static List<string> moonPrefixes = new List<string>();

        private static string[] displayNames = new string[0];
        private static string[] displayPrefixes = new string[0];
        private static string[] displayGrades = new string[0];

        public static CompatibleNoun[] routeNouns = Resources.FindObjectsOfTypeAll<TerminalKeyword>().FirstOrDefault((TerminalKeyword w) => w.name == "Route")?.compatibleNouns;
        public static string catalogueSort = "        ID ⇩";

        public static ManualLogSource mls = BepInEx.Logging.Logger.CreateLogSource("TerminalPlus");

        [HarmonyPatch(typeof(RoundManager), "Start")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.HigherThanNormal)]
        private static void PatchMoonInfo()
        {
            List<SelectableLevel> errorArray = new List<SelectableLevel>();

            foreach (SelectableLevel selectableLevel in moonsList)
            {
                if (selectableLevel.levelID < 0)//-----------------------------------------------------confirms IDs
                {
                    mls.LogInfo("Found ID below zero. Removing...");
                    errorArray.Add(selectableLevel);
                    continue;
                }
            }

            foreach (SelectableLevel item in errorArray) { moonsList.Remove(item); }//-----------------fixes arrays
            Array.Resize(ref moonsPrice, moonsList.Count);

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
            moonsList.Sort(SortByID);
        }


        [HarmonyPatch(typeof(RoundManager), "Start")]
        [HarmonyPrefix]
        [HarmonyPriority(10)]
        public static void NameSeparator()
        {
            moonNames.Clear();
            moonPrefixes.Clear();

            
            foreach (SelectableLevel selectableLevel in moonsList)
            {
                string nameTrimmed = selectableLevel.PlanetName.Substring(selectableLevel.PlanetName.IndexOf(' ') + 1);
                string namePrefix = string.Empty;

                if (selectableLevel.PlanetName.Length > nameTrimmed.Length)
                {
                    namePrefix = selectableLevel.PlanetName.Substring(0, selectableLevel.PlanetName.Length - nameTrimmed.Length - 1);
                }

                moonNames.Insert(selectableLevel.levelID, nameTrimmed);
                moonPrefixes.Insert(selectableLevel.levelID, namePrefix);
            }
        }


        [HarmonyPatch(typeof(RoundManager), "Start")]
        [HarmonyPostfix]
        [HarmonyPriority(1)]
        public static void MoonCatalogueSetup()
        {
            mls.LogDebug("SETUP START");
            displayNames = new string[moonsList.Count];
            displayPrefixes = new string[moonsList.Count];
            displayGrades = new string[moonsList.Count];

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