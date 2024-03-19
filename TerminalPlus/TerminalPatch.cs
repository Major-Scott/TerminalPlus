using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameNetcodeStuff;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace TerminalPlus
{  
    [HarmonyPatch(typeof(Terminal))]
    public class TerminalPatches
    {
        public static string playerSubmit = string.Empty;
        public static bool usedTerminal = false;

        // EXECUTION ORDER:
        // 1. PatchMoonInfo
        // 2. NameSeparator
        // 3. MakeConfig
        // 4. MoonCatalogueSetup
        // 5. CreateNodes

        [HarmonyPostfix]
        [HarmonyPatch("ParsePlayerSentence")]
        public static void ParsePatch(Terminal __instance)
        {
            PluginMain.mls.LogDebug("PARSEPLAYER PATCH");
            playerSubmit = __instance.screenText.text.Substring(__instance.screenText.text.Length - __instance.textAdded);
        }
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        [HarmonyPatch("RunTerminalEvents")]
        public static void TerminalEventPrefix(TerminalNode node, Terminal __instance) 
        {
            string newDisplayText = null;

            if (node == Nodes.terminal.terminalNodes.specialNodes[13]) newDisplayText = new Nodes().MainHelpPage();
            else if (node.name == "helpTPsortNode" || node.name == "infoTPsortNode") newDisplayText = new Nodes().HelpInfoPage();

            else if (node.name == "MoonsCatalogue" || node.name.Contains("TPsort"))
            {
                switch (node.terminalEvent)
                {
                    case "id":
                        Nodes.moonsList.Sort(Nodes.SortByID);
                        Nodes.catalogueSort = "        ID ⇩";
                        break;
                    case "name":
                        Nodes.moonsList.Sort((x, y) => Nodes.moonNames[x.levelID].CompareTo(Nodes.moonNames[y.levelID]));
                        Nodes.catalogueSort = "      NAME ⇩";
                        break;
                    case "prefix":
                        Nodes.moonsList.Sort(Nodes.SortByPrefix);
                        Nodes.catalogueSort = "    PREFIX ⇩";
                        break;
                    case "grade":
                        Nodes.moonsList.Sort(Nodes.SortByGrade);
                        Nodes.catalogueSort = "     GRADE ⇩";
                        break;
                    case "price":
                        Nodes.moonsList.Sort((x, y) => Nodes.moonsPrice[x.levelID].CompareTo(Nodes.moonsPrice[y.levelID]));
                        Nodes.catalogueSort = "     PRICE ⇩";
                        break;
                    case "weather":
                        Nodes.moonsList.Sort((x, y) => x.currentWeather.CompareTo(y.currentWeather));
                        Nodes.catalogueSort = "   WEATHER ⇩";
                        break;
                    case "difficulty":
                        Nodes.moonsList.Sort(Nodes.SortByDifficulty);
                        Nodes.catalogueSort = "DIFFICULTY ⇩";
                        break;
                    case "list":
                        break;
                    case "current":
                        break;
                    default:
                        break;
                }
                if (node.name.Contains("TPsort") && playerSubmit.Contains("rev"))
                {
                    Nodes.moonsList.Reverse();
                    if (Nodes.catalogueSort.Contains('⇧')) Nodes.catalogueSort = Nodes.catalogueSort.Substring(0, 11) + '⇩';
                    else Nodes.catalogueSort = Nodes.catalogueSort.Substring(0, 11) + '⇧';
                }
                else PluginMain.mls.LogInfo($"not reverse ):");

                newDisplayText = new Nodes().MoonsPage();
            }
            else if (node.name == "0_StoreHub") newDisplayText = new Nodes().StorePage(__instance);

            if (newDisplayText != null)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append($"{newDisplayText}\n");

                __instance.screenText.textComponent.enableKerning = false;
                //__instance.screenText.textComponent.wordWrappingRatios = 1;
                //__instance.screenText.textComponent.horizontalAlignment = HorizontalAlignmentOptions.Left;
                __instance.inputFieldText.autoSizeTextContainer = true;

                __instance.screenText.text = builder.ToString();
                __instance.currentText = builder.ToString();
                //__instance.currentNode.clearPreviousText = true;
                //__instance.textAdded = 0;
            }
            return;
        }
    }
}