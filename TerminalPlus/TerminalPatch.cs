using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameNetcodeStuff;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore;
using static TerminalPlus.Nodes;

namespace TerminalPlus
{  
    [HarmonyPatch(typeof(Terminal))]
    public class TerminalPatches
    {
        public static string playerSubmit = string.Empty;

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
            //PluginMain.mls.LogDebug("PARSEPLAYER PATCH");
            playerSubmit = __instance.screenText.text.Substring(__instance.screenText.text.Length - __instance.textAdded);
        }
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        [HarmonyPatch("RunTerminalEvents")]
        public static void TerminalEventPostfix(TerminalNode node, Terminal __instance) 
        {
            string newDisplayText = null;
            if (node == null) return;

            if (node == terminal.terminalNodes.specialNodes[13]) newDisplayText = new Nodes().MainHelpPage();
            else if (node.name == "helpTPsortNode" || node.name == "infoTPsortNode") newDisplayText = new Nodes().HelpInfoPage();

            else if (node.name == "MoonsCatalogue" || node.name.Contains("TPsort"))
            {
                switch (node.terminalEvent)
                {
                    case "default":
                        masterList.Sort((x, y) => x.mID.CompareTo(y.mID));
                        catalogueSort = "   DEFAULT ⇩";
                        break;
                    case "id":
                        masterList.Sort(SortByID);
                        catalogueSort = "   DEFAULT ⇩";
                        break;
                    case "name":
                        masterList.Sort((x, y) => x.mName.CompareTo(y.mName));
                        catalogueSort = "      NAME ⇩";
                        break;
                    case "prefix":
                        masterList.Sort(SortByPrefix);
                        catalogueSort = "    PREFIX ⇩";
                        break;
                    case "grade":
                        masterList.Sort(SortByGrade);
                        catalogueSort = "     GRADE ⇩";
                        break;
                    case "price":
                        masterList.Sort((x, y) => x.mPrice.CompareTo(y.mPrice));
                        catalogueSort = "     PRICE ⇩";
                        break;
                    case "weather":
                        masterList.Sort(SortByWeather);
                        catalogueSort = "   WEATHER ⇩";
                        break;
                    case "difficulty":
                        masterList.Sort(SortByDifficulty);
                        catalogueSort = "DIFFICULTY ⇩";
                        break;
                    case "list":
                        break;
                    case "current":
                        break;
                    case "rev":
                        break;
                    default:
                        break;
                }
                if (node.name.Contains("TPsort") && (playerSubmit.Contains("rev") || node.terminalEvent == "rev"))
                {
                    masterList.Reverse();
                    if (catalogueSort.Contains('⇧')) catalogueSort = catalogueSort.Substring(0, 11) + '⇩';
                    else catalogueSort = catalogueSort.Substring(0, 11) + '⇧';
                }
                else PluginMain.mls.LogInfo($"not reverse :(");

                newDisplayText = new Nodes().MoonsPage();
            }
            else if (node.name == "0_StoreHub") newDisplayText = new Nodes().StorePage(__instance);
            else if (node.name == "ScanInfo") newDisplayText = new Nodes().ScanMoonPage();
            else if (node.name == "scanShipNode") newDisplayText = new Nodes().ScanShipPage();
            else if (node.displayPlanetInfo >= 0 && node.name.ToLower().Contains("route")) newDisplayText = new Nodes().RoutePage(node, __instance);
            else if (node.name == "CancelRoute") newDisplayText = "\n\n\nThe reroute has been cancelled.\n\n";

            if (newDisplayText != null)
            {
                __instance.screenText.caretBlinkRate = 2f;
                __instance.screenText.textComponent.enableKerning = false;
                //__instance.screenText.textComponent.enableWordWrapping = false;

                StringBuilder builder = new StringBuilder();
                if ((bool)__instance.displayingPersistentImage) builder.Append("\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n");

                builder.Append($"{newDisplayText}\n");

                __instance.screenText.text = builder.ToString();
                __instance.currentText = builder.ToString();
                __instance.textAdded = 0;
            }
        }
    }
}