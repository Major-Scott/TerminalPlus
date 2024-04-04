using System;
using System.Collections.Generic;
using BepInEx;
using System.Linq;
using BepInEx.Configuration;
using HarmonyLib;
using BepInEx.Logging;
using UnityEngine;
using System.Diagnostics.Eventing.Reader;
using System.Reflection;

namespace TerminalPlus
{
    public class ConfigManager
    {
        public enum sortingOptions
        {
            Default, Name, Prefix, Grade, Price, Weather, Difficulty,
            DefaultReversed, NameReversed, PrefixReversed, GradeReversed, PriceReversed, WeatherReversed, DifficultyReversed
        }
        //public enum detailedScan
        //{
        //    Vanilla, Low, Medium, High
        //}
        public static char padChar = ' ';
        public static int defaultSort;
        public static bool activeKilroy = false;
        public static bool overrideVisibility = false;
        public static bool showClear = false;
        public static bool evenSort = false;
        public static bool longWeather = false;
        public static bool detailedScan = true;
        public static SelectableLevel companyLocation;
        private static readonly Dictionary<int, string> originalNames = new Dictionary<int, string>();

        static ConfigEntry<bool> enableCustom;

        static ConfigEntry<bool> padPrefixes;
        static ConfigEntry<sortingOptions> defaultSorting;
        static ConfigEntry<bool> overrideVisibilityConfig;
        static ConfigEntry<bool> showClearConfig;
        static ConfigEntry<bool> longWeatherConfig;
        static ConfigEntry<bool> evenSortConfig;
        static ConfigEntry<bool> detailedScanConfig;
        static ConfigEntry<string> customDifficultyConfig;
        static ConfigEntry<bool> configKilroy;
        
        static ConfigEntry<string> setCustomName;
        static ConfigEntry<int> setCustomPrefix;
        static ConfigEntry<string> setCustomGrade;
        static ConfigEntry<int> setCustomPrice;

        static ConfigEntry<string> setCustomDescription;
        static ConfigEntry<string> setCustomInfo;

        public static PropertyInfo orphanedEntriesProp = PluginMain.configFile.GetType().GetProperty("OrphanedEntries", BindingFlags.NonPublic | BindingFlags.Instance);

        private static ManualLogSource mls = BepInEx.Logging.Logger.CreateLogSource("TerminalPlus");

        [HarmonyPatch(typeof(RoundManager), "Start")]
        [HarmonyPostfix]
        [HarmonyPriority(2)]
        public static void MakeConfig()
        {         
            mls.LogDebug("CONFIG START");

            TerminalNode[] infoNodes = Resources.FindObjectsOfTypeAll<TerminalNode>().Where((TerminalNode n) =>
            n.name.ToLower().Contains("info") && n.displayText.Contains("\n------------------")).ToArray();

            ConfigFile configFile = PluginMain.configFile;

            defaultSorting = configFile.Bind("00. General", "Default Sorting", sortingOptions.Default,
                "The default method for sorting moons in the terminal catalogue.");
            //hideCompanyConfig = configFile.Bind("00. General", "Hide The Company", true,
            //    "Hides \"71 Gordion\" (The Company Building) from the Moon Catalogue page.");
            padPrefixes = configFile.Bind("00. General", "Pad Prefixes", false,
                "Pads all prefixes with zeros to be the same length (e.g. \"7 Dine\" would become \"007 Dine\").");
            showClearConfig = configFile.Bind("00. General", "Show Clear Weather", false,
                "Adds \"(Clear)\" to the weather column of the moon catalogue when the moon has no weather.");
            evenSortConfig = configFile.Bind("00. General", "Uniform Catalogue Listing", false,
                "Displays all moons in groups of three, regardless of current sorting.");
            longWeatherConfig = configFile.Bind("00. General", "Show Full Weather Name", false,
                "Will create multi-line entries in the moon catalogue for longer weather names (like the multiple weathers of \"WeatherTweaks\"). If set to false, longer weathers will be truncated to \"Complex\" in the catalogue.");
            detailedScanConfig = configFile.Bind("00. General", "Detailed Terminal Scan", true,
                "Displays a more detailed  \"scan\" terminal page including a list of all items and their most important properties.");
            customDifficultyConfig = configFile.Bind("00. General", "Custom Difficulty Equation", "DayPower:2,NightPower:4,InsidePower:5,Size:2,Weather:3",
                "Creates a custom equation on which the \"difficulty\" sorting method is calculated.\nPOSSIBLE VARIABLES:\nDayPower (daytime enemy power), NightPower (nighttime enemy power)," +
                " InsidePower (inside enemy power),\nDayEnemies (total daytime enemy types), NightEnemies (total nighttime enemy types), InsideEnemies (total inside enemy types)," +
                "\nSize (facility/interior size), Weather (current weather), Value (average scrap value)\nPlease provide variables in the \"variable:weight\" format shown in the default.");

            string settingEntryNum;
            originalNames.Clear();
            Nodes.priceOverride = new bool[Nodes.moonMasters.Count];

            foreach (MoonMaster moonCFG in Nodes.moonMasters.Values)
            {
                mls.LogWarning($"CURRENT LEVEL: {moonCFG.mLevelName}");

                int cuID = moonCFG.mID;
                int saveID = -1;
                string tempInfo = "PLACEHOLDER TEXT (could not find info text for config, setting may not work)";
                string configName = moonCFG.origName;
                if (cuID < 9) { settingEntryNum = $"0{cuID + 1}"; }
                else { settingEntryNum = $"{cuID + 1}"; }
                if (int.TryParse(moonCFG.mPrefix, out int tempPrefix)) { }
                else tempPrefix = -1;

                //originalNames.Add(selectableLevel.levelID, selectableLevel.PlanetName);

                for (int i = 0; i < infoNodes.Length; i++)
                {
                    if (infoNodes[i].displayText.ToLower().StartsWith(moonCFG.origName.ToLower()) ||
                        infoNodes[i].displayText.ToLower().StartsWith((moonCFG.mPrefix + "-" + moonCFG.mName).ToLower()) ||
                        infoNodes[i].displayText.ToLower().StartsWith(moonCFG.mName.ToLower()) ||
                        infoNodes[i].name.ToLower().Contains(moonCFG.mName.ToLower()))
                    {
                        tempInfo = infoNodes[i].displayText;
                        saveID = i;
                        break;
                    }
                }

                if (moonCFG.mLevelName == "CompanyBuildingLevel")
                {
                    tempInfo = Resources.FindObjectsOfTypeAll<TerminalNode>().FirstOrDefault((TerminalNode n) => n.name == "CompanyBuildingInfo").displayText;
                    configName = "The Company Building";
                    moonCFG.mName = "Company Building";
                    moonCFG.mPrefix = "The";

                    enableCustom = configFile.Bind($"{settingEntryNum}. {configName} Settings",
                        $"{configName} - Enable Moon Config", false, $"Enables the customization options below for {configName}.");

                    overrideVisibility = true;
                    overrideVisibilityConfig = configFile.Bind($"{settingEntryNum}. {configName} Settings",
                        $"{configName} - Hide Moon in Terminal", true, $"Hides The Company in the moon catalogue. " +
                        $"Set to \"true\" to hide; visibility may be overridden by other mods .\nNOTE: The moon will still be active and usable, just not visible.");
                    setCustomName = configFile.Bind($"{settingEntryNum}. {configName} Settings",
                        $"{configName} - Set Custom Name", "Company Building", "Set a custom moon name (not including the prefix).");
                    setCustomPrefix = configFile.Bind($"{settingEntryNum}. {configName} Settings",
                        $"{configName} - Set Custom Prefix", -2, $"Set a custom prefix number (e.g. the \"8\" in \"8 Titan\"). " +
                        $"Set to \"-1\" to remove prefix entirely. Set to \"-2\" to use \"The\" for \"The Company Building\".");
                }
                else
                {
                    enableCustom = configFile.Bind($"{settingEntryNum}. {configName} Settings",
                        $"{configName} - Enable Moon Config", false, $"Enables the customization options below for {configName}.");

                    overrideVisibilityConfig = configFile.Bind($"{settingEntryNum}. {configName} Settings",
                    $"{configName} - Hide Moon in Terminal", false, $"Hide the moon in the catalogue. " +
                        $"If true, it will be hidden, if false, it will be whatever it would be without TerminalPlus.\nNOTE: The moon will still be active and usable, just not visible.");
                    setCustomName = configFile.Bind($"{settingEntryNum}. {configName} Settings",
                        $"{configName} - Set Custom Name", moonCFG.mName, "Set a custom moon name (not including the prefix).");
                    setCustomPrefix = configFile.Bind($"{settingEntryNum}. {configName} Settings",
                        $"{configName} - Set Custom Prefix", tempPrefix, $"Set a custom prefix number (e.g. the \"8\" in \"8 Titan\"). Set to \"-1\" to remove prefix entirely.");
                }

                setCustomGrade = configFile.Bind($"{settingEntryNum}. {configName} Settings",
                    $"{configName} - Set Custom Grade", moonCFG.mGrade, "Set a custom grade/hazard level (e.g. \"A+\", \"D\", \"SS\", etc). Will be trimmed to two characters.");
                setCustomPrice = configFile.Bind($"{settingEntryNum}. {configName} Settings",
                    $"{configName} - Set Custom Price", moonCFG.mPrice, "Set a custom price. Set to \"-1\" to ignore for compatibility with other price-changing mods.\n(NOTE: may break if another mod reassigns level IDs during runtime)");

                setCustomDescription = configFile.Bind($"{settingEntryNum}. {configName} Settings",
                    $"{configName} - Set Custom Description", moonCFG.mDesc, "Set a custom description (i.e. the \"POPULATION\", \"CONDITIONS\", and \"FAUNA\" subtext).");
                setCustomInfo = configFile.Bind($"{settingEntryNum}. {configName} Settings",
                    $"{configName} - Set Custom Info Panel", tempInfo, "Set custom info display text (for when you enter \"[moon name] info\" into the terminal)\n(NOTE: may not work on modded moons depending on their default formatting).");

                if (enableCustom.Value)
                {
                    mls.LogInfo($"Config enabled for {moonCFG.mLevelName}. Running...");

                    moonCFG.mName = setCustomName.Value;

                    if (setCustomPrefix.Value < 0) moonCFG.mPrefix = string.Empty;
                    else moonCFG.mPrefix = setCustomPrefix.Value.ToString();
                    moonCFG.mLevel.PlanetName = setCustomPrefix.Value + " " + setCustomName.Value;
                    moonCFG.mLevel.riskLevel = moonCFG.mGrade = setCustomGrade.Value;

                    //if (setCustomPrice.Value < 0) { moonCFG.mPriceOR = true; setCustomPrice.Value = moonCFG.mPrice; }
                    //else moonCFG.mPriceOR = false;
                    //moonCFG.mPrice = setCustomPrice.Value;

                    if (setCustomPrice.Value >= 0) {
                        moonCFG.mPriceOR = false;
                        moonCFG.mPrice = setCustomPrice.Value;
                    }
                    else moonCFG.mPriceOR = true;

                    moonCFG.mDesc = setCustomDescription.Value;

                    foreach (CompatibleNoun nounFinder in Nodes.routeNouns)
                    {
                        if (nounFinder.result.displayPlanetInfo == cuID)
                        {
                            nounFinder.noun.word = setCustomName.Value.ToLower();
                            //if (Nodes.moonPrefixes[cuID].Length > 0)
                            //{
                            //    nounFinder.result.displayText = $"The cost to route to {Nodes.moonPrefixes[cuID]}-{Nodes.moonNames[cuID]} is [totalCost]. It is currently [currentPlanetTime] on this moon.\n\nPlease CONFIRM or DENY.";
                            //}
                            //else nounFinder.result.displayText = $"The cost to route to {Nodes.moonNames[cuID]} is [totalCost]. It is currently [currentPlanetTime] on this moon.\n\nPlease CONFIRM or DENY.";
                            nounFinder.result.itemCost = setCustomPrice.Value;
                        }
                    }
                    foreach (TerminalNode confirmNode in Nodes.confirmNodes)
                    {
                        if (confirmNode.buyRerouteToMoon == cuID)
                        {
                            confirmNode.displayText = $"Routing autopilot to {moonCFG.mPrefix}-{moonCFG.mName}.\nYour new balance is [playerCredits].";
                            confirmNode.itemCost = setCustomPrice.Value;
                        }
                    }
                    if (saveID >= 0) infoNodes[saveID].displayText = setCustomInfo.Value;
                    moonCFG.mVis = !overrideVisibilityConfig.Value;
                }
                if (moonCFG.mLevelName == "CompanyBuildingLevel" && setCustomPrefix.Value == -2) moonCFG.mPrefix = "The";
                //var orphanedEntries = (Dictionary<ConfigDefinition, string>)orphanedEntriesProp.GetValue(configFile, null);
                CheckForOrphans(configFile, configName);
            }
            configKilroy = configFile.Bind($"{Nodes.moonMasters.Count + 1}. Misc.", "Kilroy", false, "Kilroy is here.");
            if (padPrefixes.Value) padChar = '0';
            else padChar = ' ';
            defaultSort = (int)defaultSorting.Value;
            showClear = showClearConfig.Value;
            evenSort = evenSortConfig.Value;
            longWeather = longWeatherConfig.Value;
            detailedScan = detailedScanConfig.Value;
            mls.LogInfo($"Default sorting method: {defaultSorting.Value}");

            var orphanedEntries2 = (Dictionary<ConfigDefinition, string>)orphanedEntriesProp.GetValue(configFile, null);
            if (orphanedEntries2.Where(e => e.Key.Key == "Kilroy" && e.Value == "true") != null) configKilroy.Value = true;
            activeKilroy = configKilroy.Value;
            CalculateDifficulty();

            orphanedEntries2.Clear();
            configFile.Save();

            mls.LogDebug("CONFIG END");
        }

        public static void CheckForOrphans(ConfigFile configFile, string configName)
        {
            var orphanedEntries = (Dictionary<ConfigDefinition, string>)orphanedEntriesProp.GetValue(configFile, null);

            if (orphanedEntries.Where(c => c.Key.Key == $"{configName} - Enable Moon Config").FirstOrDefault().Value == "true")
            {
                mls.LogInfo($"Active orphaned entries for {configName}. Replacing...");
                enableCustom.Value = true;
            }
            else return;
                
            foreach (var entry in orphanedEntries.Where(e => e.Key.Key.Contains(configName)))
            {
                if (entry.Key.Key.Contains(" - Set Custom Name" ) && entry.Value != setCustomName.DefaultValue.ToString()) setCustomName.Value = entry.Value;

                else if (entry.Key.Key.Contains(" - Set Custom Prefix") && entry.Value != setCustomPrefix.DefaultValue.ToString())
                {
                    setCustomPrefix.Value = int.TryParse(entry.Value, out int value) ? value : -1;
                }
                else if (entry.Key.Key.Contains(" - Set Custom Grade") && entry.Value != setCustomPrefix.DefaultValue.ToString()) setCustomGrade.Value = entry.Value;

                else if (entry.Key.Key.Contains(" - Set Custom Price") && entry.Value != setCustomPrefix.DefaultValue.ToString())
                {
                    setCustomPrice.Value = int.TryParse(entry.Value, out int value) ? value : -1;
                }
                else if (entry.Key.Key.Contains(" - Set Custom Description") && entry.Value != setCustomPrefix.DefaultValue.ToString()) setCustomDescription.Value = entry.Value;

                else if (entry.Key.Key.Contains(" - Set Custom Info Panel") && entry.Value != setCustomPrefix.DefaultValue.ToString()) setCustomInfo.Value = entry.Value;
            }
        }

        public static void CalculateDifficulty()
        {
            string cDiff = customDifficultyConfig.Value + ",";

            if (cDiff.ToLower().Contains("daypower:") && cDiff.ToLower().IndexOf("daypower:") + 9 < cDiff.Length)
            {
                string dayValue = cDiff.Substring(cDiff.ToLower().IndexOf("daypower:") + 9).ToString();
                if (dayValue.IndexOf(',') > 0) int.TryParse(dayValue.Substring(0, dayValue.IndexOf(',')), out Nodes.dayPowerMult);
            }
            if (cDiff.ToLower().Contains("nightpower:") && cDiff.ToLower().IndexOf("nightpower:") + 11 < cDiff.Length)
            {
                string nightValue = cDiff.Substring(cDiff.ToLower().IndexOf("nightpower:") + 11).ToString();
                if (nightValue.IndexOf(',') > 0) int.TryParse(nightValue.Substring(0, nightValue.IndexOf(',')), out Nodes.nightPowerMult);
            }
            if (cDiff.ToLower().Contains("insidepower:") && cDiff.ToLower().IndexOf("insidepower:") + 12 < cDiff.Length)
            {
                string insideValue = cDiff.Substring(cDiff.ToLower().IndexOf("insidepower:") + 12).ToString();
                if (insideValue.IndexOf(',') > 0) int.TryParse(insideValue.Substring(0, insideValue.IndexOf(',')), out Nodes.insidePowerMult);
            }
            //-------------------------------------------------------------------------------------------------------------------------------------------
            if (cDiff.ToLower().Contains("dayenemies:") && cDiff.ToLower().IndexOf("dayenemies:") + 11 < cDiff.Length)
            {
                string dayValue = cDiff.Substring(cDiff.ToLower().IndexOf("dayenemies:") + 11).ToString();
                if (dayValue.IndexOf(',') > 0) int.TryParse(dayValue.Substring(0, dayValue.IndexOf(',')), out Nodes.dayCountMult);
            }
            if (cDiff.ToLower().Contains("nightenemies:") && cDiff.ToLower().IndexOf("nightenemies:") + 13 < cDiff.Length)
            {
                string nightValue = cDiff.Substring(cDiff.ToLower().IndexOf("nightenemies:") + 13).ToString();
                if (nightValue.IndexOf(',') > 0) int.TryParse(nightValue.Substring(0, nightValue.IndexOf(',')), out Nodes.nightCountMult);
            }
            if (cDiff.ToLower().Contains("insideenemies:") && cDiff.ToLower().IndexOf("insideenemies:") + 14 < cDiff.Length)
            {
                string insideValue = cDiff.Substring(cDiff.ToLower().IndexOf("insideenemies:") + 14).ToString();
                if (insideValue.IndexOf(',') > 0) int.TryParse(insideValue.Substring(0, insideValue.IndexOf(',')), out Nodes.insideCountMult);
            }
            //--------------------------------------------------------------------------------------------------------------------------------------------
            if (cDiff.ToLower().Contains("size:") && cDiff.ToLower().IndexOf("size:") + 5 < cDiff.Length)
            {
                string sizeValue = cDiff.Substring(cDiff.ToLower().IndexOf("size:") + 5).ToString();
                if (sizeValue.IndexOf(',') > 0) int.TryParse(sizeValue.Substring(0, sizeValue.IndexOf(',')), out Nodes.sizeMult);
            }
            if (cDiff.ToLower().Contains("weather:") && cDiff.ToLower().IndexOf("weather:") + 8 < cDiff.Length)
            {
                string weatherValue = cDiff.Substring(cDiff.ToLower().IndexOf("weather:") + 8).ToString();
                if (weatherValue.IndexOf(',') > 0) int.TryParse(weatherValue.Substring(0, weatherValue.IndexOf(',')), out Nodes.weatherMult);
            }
            if (cDiff.ToLower().Contains("scrapvalue:") && cDiff.ToLower().IndexOf("scrapvalue:") + 11 < cDiff.Length)
            {
                string scrapValue = cDiff.Substring(cDiff.ToLower().IndexOf("scrapvalue:") + 11).ToString();
                if (scrapValue.IndexOf(',') > 0) int.TryParse(scrapValue.Substring(0, scrapValue.IndexOf(',')), out Nodes.valueMult);
            }
        }

        [HarmonyPatch(typeof(GameNetworkManager), "StartDisconnect")]
        [HarmonyPostfix]
        [HarmonyPriority(0)]
        private static void ResetNames()
        {
            mls.LogMessage("Disconnecting and resetting names:");
            //foreach (SelectableLevel slReset in Nodes.moonsList)
            //{
            //    slReset.PlanetName = originalNames[slReset.levelID];
            //    mls.LogDebug("Reset: " + slReset.PlanetName);
            //}
            foreach (MoonMaster moonRN in Nodes.moonMasters.Values)
            {
                moonRN.mLevel.PlanetName = moonRN.origName;
                mls.LogDebug("Reset: " + moonRN.origName);
            }

        }

    }
}






/*foreach (SelectableLevel selectableLevel in Nodes.moonsList)
            {
                mls.LogWarning($"CURRENT LEVEL: {selectableLevel.name}");

                int cuID = selectableLevel.levelID;
                int saveID = -1;
                string tempInfo = "PLACEHOLDER TEXT (could not find info text for config, setting may not work)";
                string configName = selectableLevel.PlanetName;
                if (cuID < 9) { settingEntryNum = $"0{cuID + 1}"; }
                else { settingEntryNum = $"{cuID + 1}"; }
                if (int.TryParse(Nodes.moonPrefixes[cuID], out int tempPrefix)) { }
                else tempPrefix = -1;
                
                originalNames.Add(selectableLevel.levelID, selectableLevel.PlanetName);

                for (int i = 0; i < infoNodes.Length; i++)
                {
                    if (infoNodes[i].displayText.ToLower().StartsWith(selectableLevel.PlanetName.ToLower()) ||
                        infoNodes[i].displayText.ToLower().StartsWith((Nodes.moonPrefixes[cuID] + "-" + Nodes.moonNames[cuID]).ToLower()) ||
                        infoNodes[i].displayText.ToLower().StartsWith(Nodes.moonNames[cuID].ToLower()) ||
                        infoNodes[i].name.ToLower().Contains(Nodes.moonNames[cuID].ToLower()))
                    {
                        tempInfo = infoNodes[i].displayText;
                        saveID = i;
                        break;
                    }
                }
                //if (selectableLevel.name == "CompanyBuildingLevel")
                //{
                //    //tempInfo = Resources.FindObjectsOfTypeAll<TerminalNode>().Where((TerminalNode n) => n.name == "CompanyBuildingInfo").FirstOrDefault().displayText;
                //    tempInfo = Resources.FindObjectsOfTypeAll<TerminalNode>().FirstOrDefault((TerminalNode n) => n.name == "CompanyBuildingInfo").displayText;
                //    configName = "The Company Building";
                //}

                enableCustom = configFile.Bind($"{settingEntryNum}. {configName} Settings",
                        $"{configName} - Enable Moon Config", false, $"Enables the customization options below for {configName}.");
                
                if (selectableLevel.name == "CompanyBuildingLevel")
                {
                    tempInfo = Resources.FindObjectsOfTypeAll<TerminalNode>().FirstOrDefault((TerminalNode n) => n.name == "CompanyBuildingInfo").displayText;
                    configName = "The Company Building";
                    Nodes.moonNames[cuID] = "Company Building";
                    Nodes.moonPrefixes[cuID] = "The";

                    overrideVisibility = true;
                    overrideVisibilityConfig = configFile.Bind($"{settingEntryNum}. {configName} Settings",
                        $"{configName} - Hide Moon in Terminal", true, $"Override the moon visibility in the catalogue. " +
                        $"If true, it will be hidden, if false, it will be whatever it would be without TerminalPlus.\nNOTE: The moon will still be active and usable, just not visible.");
                    setCustomName = configFile.Bind($"{settingEntryNum}. {configName} Settings",
                        $"{configName} - Set Custom Name", "Company Building", "Set a custom moon name (not including the prefix).");
                    setCustomPrefix = configFile.Bind($"{settingEntryNum}. {configName} Settings",
                        $"{configName} - Set Custom Prefix", -2, $"Set a custom prefix number (e.g. the \"8\" in \"8 Titan\"). " +
                        $"Set to \"-1\" to remove prefix entirely. Set to \"-2\" to use \"The\" for \"The Company Building\".");
                }
                else
                {
                    overrideVisibilityConfig = configFile.Bind($"{settingEntryNum}. {configName} Settings",
                    $"{configName} - Hide Moon in Terminal", false, $"Override the moon visibility in the catalogue. " +
                        $"If true, it will be hidden, if false, it will be whatever it would be without TerminalPlus.\nNOTE: The moon will still be active and usable, just not visible.");
                    setCustomName = configFile.Bind($"{settingEntryNum}. {configName} Settings",
                        $"{configName} - Set Custom Name", Nodes.moonNames[cuID], "Set a custom moon name (not including the prefix).");
                    setCustomPrefix = configFile.Bind($"{settingEntryNum}. {configName} Settings",
                        $"{configName} - Set Custom Prefix", tempPrefix, $"Set a custom prefix number (e.g. the \"8\" in \"8 Titan\"). Set to \"-1\" to remove prefix entirely.");
                }
                setCustomGrade = configFile.Bind($"{settingEntryNum}. {configName} Settings",
                    $"{configName} - Set Custom Grade", selectableLevel.riskLevel, "Set a custom grade/hazard level (e.g. \"A+\", \"D\", \"SS\", etc). Will be trimmed to two characters.");
                setCustomPrice = configFile.Bind($"{settingEntryNum}. {configName} Settings",
                    $"{configName} - Set Custom Price", Nodes.moonsPrice[cuID], "Set a custom price. Set to \"-1\" to ignore for compatibility with other price-changing mods.\n(NOTE: may break if another mod reassigns level IDs during runtime)");

                setCustomDescription = configFile.Bind($"{settingEntryNum}. {configName} Settings",
                    $"{configName} - Set Custom Description", selectableLevel.LevelDescription, "Set a custom description (i.e. the \"POPULATION\", \"CONDITIONS\", and \"FAUNA\" subtext).");
                setCustomInfo = configFile.Bind($"{settingEntryNum}. {configName} Settings",
                    $"{configName} - Set Custom Info Panel", tempInfo, "Set custom info display text (for when you enter \"[moon name] info\" into the terminal)\n(NOTE: may not work on modded moons depending on their default formatting).");

                var orphanedEntries = (Dictionary<ConfigDefinition, string>)orphanedEntriesProp.GetValue(configFile, null);
                CheckForOrphans(configFile, configName);

                if (enableCustom.Value)
                {
                    mls.LogInfo($"Config enabled for {selectableLevel.PlanetName}. Running...");

                    Nodes.moonNames[cuID] = setCustomName.Value;

                    if (setCustomPrefix.Value < 0) Nodes.moonPrefixes[cuID] = string.Empty;
                    else Nodes.moonPrefixes[cuID] = setCustomPrefix.Value.ToString();
                    selectableLevel.PlanetName = setCustomPrefix.Value + " " + setCustomName.Value;
                    selectableLevel.riskLevel = setCustomGrade.Value;

                    if (setCustomPrice.Value < 0) { Nodes.priceOverride[cuID] = true; setCustomPrice.Value = Nodes.moonsPrice[cuID]; }
                    else Nodes.priceOverride[cuID] = false;
                    Nodes.moonsPrice[cuID] = setCustomPrice.Value;
                    selectableLevel.LevelDescription = setCustomDescription.Value;

                    foreach (CompatibleNoun nounFinder in Nodes.routeNouns)
                    {
                        if (nounFinder.result.displayPlanetInfo == cuID)
                        {
                            nounFinder.noun.word = setCustomName.Value.ToLower();
                            //if (Nodes.moonPrefixes[cuID].Length > 0)
                            //{
                            //    nounFinder.result.displayText = $"The cost to route to {Nodes.moonPrefixes[cuID]}-{Nodes.moonNames[cuID]} is [totalCost]. It is currently [currentPlanetTime] on this moon.\n\nPlease CONFIRM or DENY.";
                            //}
                            //else nounFinder.result.displayText = $"The cost to route to {Nodes.moonNames[cuID]} is [totalCost]. It is currently [currentPlanetTime] on this moon.\n\nPlease CONFIRM or DENY.";
                            nounFinder.result.itemCost = setCustomPrice.Value;
                        }
                    }
                    foreach (TerminalNode confirmNode in Nodes.confirmNodes)
                    {
                        if (confirmNode.buyRerouteToMoon == cuID)
                        {
                            confirmNode.displayText = $"Routing autopilot to {Nodes.moonPrefixes[cuID]}-{Nodes.moonNames[cuID]}.\nYour new balance is [playerCredits].";
                            confirmNode.itemCost = setCustomPrice.Value;
                        }
                    }
                    if (saveID >= 0) infoNodes[saveID].displayText = setCustomInfo.Value;
                }
                if (selectableLevel.name == "CompanyBuildingLevel" && setCustomPrefix.Value == -2) Nodes.moonPrefixes[cuID] = "The";
            }*/