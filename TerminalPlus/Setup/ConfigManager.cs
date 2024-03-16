using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using HarmonyLib;
using BepInEx.Logging;
using UnityEngine;

namespace TerminalPlus
{
    public class ConfigManager
    {
        public enum sortingOptions
        {
            ID, Name, Prefix, Grade, Price, Weather, Difficulty,
            IDReversed, NameReversed, PrefixReversed, GradeReversed, PriceReversed, WeatherReversed, DifficultyReversed
        }
        public static char padChar = ' ';
        public static int defaultSort;
        public static bool activeKilroy = false;
        public static bool hiddenCompany = true;
        public static SelectableLevel companyLocation;

        static ConfigEntry<bool> enableCustom;

        static ConfigEntry<bool> padPrefixes;
        static ConfigEntry<sortingOptions> defaultSorting;
        static ConfigEntry<bool> hideCompany;
        static ConfigEntry<bool> configKilroy;
        
        static ConfigEntry<string> setCustomName;
        static ConfigEntry<int> setCustomPrefix;
        static ConfigEntry<string> setCustomGrade;
        static ConfigEntry<int> setCustomPrice;

        static ConfigEntry<string> setCustomDescription;
        static ConfigEntry<string> setCustomInfo;


        [HarmonyPatch(typeof(RoundManager), "Start")]
        [HarmonyPrefix]
        [HarmonyPriority(8)]
        public static void MakeConfig()
        {
            ManualLogSource mls = BepInEx.Logging.Logger.CreateLogSource("TerminalPlus");
            mls.LogDebug("CONFIG START");

            TerminalNode[] confirmNodes = Resources.FindObjectsOfTypeAll<TerminalNode>().Where((TerminalNode k) => k.buyRerouteToMoon >= 0).ToArray();
            TerminalNode[] infoNodes = Resources.FindObjectsOfTypeAll<TerminalNode>().Where((TerminalNode n) =>
            n.name.ToLower().Contains("info") && n.displayText.Contains("\n------------------")).ToArray();

            ConfigFile configFile = PluginMain.configFile;

            defaultSorting = configFile.Bind("00. General", "Default Sorting", sortingOptions.ID,
                "The default method for sorting moons in the terminal catalogue.");
            hideCompany = configFile.Bind("00. General", "Hide The Company", true,
                "Hides \"71 Gordion\" (The Company Building) from the Moon Catalogue page.");
            padPrefixes = configFile.Bind("00. General", "Pad Prefixes", false,
                "Pads all prefixes with zeros to be the same length (e.g. \"7 Dine\" would become \"007 Dine\").");
            string settingEntryNum;

            foreach (SelectableLevel selectableLevel in Nodes.moonsList)
            {
                int cuID = selectableLevel.levelID;
                if (cuID < 9) { settingEntryNum = $"0{cuID + 1}"; }
                else { settingEntryNum = $"{cuID + 1}"; }
                if (int.TryParse(Nodes.moonPrefixes[cuID], out int tempPrefix)) { }
                else tempPrefix = -1;

                enableCustom = configFile.Bind($"{settingEntryNum}. {selectableLevel.PlanetName} Settings",
                    $"{selectableLevel.PlanetName} - Enable Moon Config", false, $"Enables the customization options below for {selectableLevel.PlanetName}.");

                setCustomName = configFile.Bind($"{settingEntryNum}. {selectableLevel.PlanetName} Settings",
                    $"{selectableLevel.PlanetName} - Set Custom Name", Nodes.moonNames[cuID], "Set a custom moon name (not including the prefix).");

                setCustomPrefix = configFile.Bind($"{settingEntryNum}. {selectableLevel.PlanetName} Settings",
                    $"{selectableLevel.PlanetName} - Set Custom Prefix", tempPrefix, $"Set a custom prefix number (e.g. the \"8\" in \"8 Titan\"). Set to \"-1\" to remove prefix entirely.");

                setCustomGrade = configFile.Bind($"{settingEntryNum}. {selectableLevel.PlanetName} Settings",
                    $"{selectableLevel.PlanetName} - Set Custom Grade", selectableLevel.riskLevel, "Set a custom grade/hazard level (e.g. \"A+\", \"D\", \"SS\", etc). Will be trimmed to two characters.");

                setCustomPrice = configFile.Bind($"{settingEntryNum}. {selectableLevel.PlanetName} Settings",
                    $"{selectableLevel.PlanetName} - Set Custom Price", Nodes.moonsPrice[cuID], "Set a custom price. (NOTE: may break if another mod reassigns level IDs during runtime)");

                setCustomDescription = configFile.Bind($"{settingEntryNum}. {selectableLevel.PlanetName} Settings",
                    $"{selectableLevel.PlanetName} - Set Custom Description", selectableLevel.LevelDescription, "Set a custom description (i.e. the \"POPULATION\", \"CONDITIONS\", and \"FAUNA\" subtext).");

                setCustomInfo = configFile.Bind($"{settingEntryNum}. {selectableLevel.PlanetName} Settings",
                    $"{selectableLevel.PlanetName} - Set Custom Info Panel", "placeholder (I tried my darndest to get the default here, changing it should still work)", "Set custom info display text (for when you enter \"[moon name] info\" into the terminal)\n(NOTE: may not work on modded moons depending on their default formatting).");

                if (enableCustom.Value)
                {
                    foreach (TerminalNode infoNode in infoNodes)
                    {
                        if (infoNode.displayText.StartsWith(selectableLevel.PlanetName) ||
                            infoNode.displayText.Contains($"{Nodes.moonPrefixes[cuID]}-{Nodes.moonNames[cuID]}") ||
                            infoNode.name.ToLower().Contains(Nodes.moonNames[cuID].ToLower()))
                            infoNode.displayText = setCustomInfo.Value;
                    }


                    mls.LogInfo($"Config enabled for {selectableLevel.PlanetName}. Running...");

                    Nodes.moonNames[cuID] = setCustomName.Value;
                    if (setCustomPrefix.Value == -1) Nodes.moonPrefixes[cuID] = string.Empty;
                    else Nodes.moonPrefixes[cuID] = setCustomPrefix.Value.ToString();
                    selectableLevel.PlanetName = setCustomPrefix.Value + " " + setCustomName.Value;

                    selectableLevel.riskLevel = setCustomGrade.Value;
                    if (setCustomPrice.Value < 0) setCustomPrice.Value = 0;
                    Nodes.moonsPrice[cuID] = setCustomPrice.Value;
                    selectableLevel.LevelDescription = setCustomDescription.Value;

                    foreach (CompatibleNoun nounFinder in Nodes.routeNouns)
                    {
                        if (nounFinder.result.displayPlanetInfo == cuID)
                        {
                            nounFinder.noun.word = setCustomName.Value.ToLower();
                            nounFinder.result.displayText = $"The cost to route to {Nodes.moonPrefixes[cuID]}-{Nodes.moonNames[cuID]} is [totalCost]. It is currently [currentPlanetTime] on this moon.\n\nPlease CONFIRM or DENY.";
                            nounFinder.result.itemCost = setCustomPrice.Value;
                        }
                    }
                    foreach (TerminalNode confirmNode in confirmNodes)
                    {
                        if (confirmNode.buyRerouteToMoon == cuID)
                        {
                            confirmNode.displayText = $"Routing autopilot to {Nodes.moonPrefixes[cuID]}-{Nodes.moonNames[cuID]}.\nYour new balance is [playerCredits].";
                            confirmNode.itemCost = setCustomPrice.Value;
                        }
                    }
                }
            }
            configKilroy = configFile.Bind($"{Nodes.moonsList.Count + 1}. Misc.", "Kilroy", false, "Kilroy is here.");
            if (padPrefixes.Value) padChar = '0';
            else padChar = ' ';
            defaultSort = (int)defaultSorting.Value;
            activeKilroy = configKilroy.Value;
            hiddenCompany = hideCompany.Value;
            mls.LogInfo($"Default sorting method: {defaultSorting.Value}");
            mls.LogDebug("CONFIG END");
        }
    }
}