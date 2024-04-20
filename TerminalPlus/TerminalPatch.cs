using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameNetcodeStuff;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.UI;
using static TerminalPlus.Nodes;

namespace TerminalPlus
{  
    [HarmonyPatch(typeof(Terminal))]
    public class TerminalPatches
    {
        public static string playerSubmit = string.Empty;
        public static TextMeshProUGUI terminalClock = new TextMeshProUGUI();
        public static Transform clockBG;
        public static float timeSinceSubmit = 0f;

        // EXECUTION ORDER:
        // 1. PatchMoonInfo
        // 2. NameSeparator
        // 3. MakeConfig
        // 4. MoonCatalogueSetup
        // 5. CreateNodes


        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.First)]
        public static void PatchHelpPage(Terminal __instance)
        {
            terminal = __instance;
            //terminal.terminalNodes.specialNodes[13].displayText = new Nodes().MainHelpPage();
            //terminal.screenText.caretBlinkRate = 1f; //__instance.screenText.caretBlinkRate = 1f;
            __instance.scrollBarCanvasGroup.transform.localPosition = new Vector3(246, 196.5f, 0); // default: 245ish, 208ish, 0
            Resources.FindObjectsOfTypeAll<TerminalNode>().FirstOrDefault(tn => tn.name == "OtherCommands").displayText = new Nodes().OtherHelpPage();

            terminalClock = UnityEngine.Object.Instantiate(terminal.topRightText);
            clockBG = UnityEngine.Object.Instantiate(terminal.topRightText.transform.parent.GetChild(5));

            terminalClock.transform.parent = clockBG.parent = __instance.topRightText.transform.parent;
            terminalClock.text = string.Empty;
            terminalClock.horizontalAlignment = HorizontalAlignmentOptions.Right;
            terminalClock.alignment = TextAlignmentOptions.TopRight;
            terminalClock.transform.localPosition = new Vector3(132f, 199f, -1f);
            terminalClock.transform.localRotation = clockBG.localRotation = new Quaternion(0f, 0f, 0f, 1f);
            terminalClock.transform.localScale = Vector3.one;
            clockBG.localScale = Vector3.zero;
        }

        [HarmonyPostfix]
        [HarmonyPatch("ParsePlayerSentence")]
        public static void ParsePatch(Terminal __instance)
        {
            playerSubmit = __instance.screenText.text.Substring(__instance.screenText.text.Length - __instance.textAdded);
            mls.LogDebug("Player entered: " + playerSubmit);
            mls.LogWarning("----------PARSEPLAYER----------");
        }

        [HarmonyPatch("OnSubmit")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        public static void NodeConsoleInfo(Terminal __instance)
        {
            timeSinceSubmit = 0f;
            __instance.scrollBarVertical.value = 1f;

            //if (__instance.currentNode != null)
            //{
            //    mls.LogWarning("CURRENT NODE: " + __instance.currentNode);
            //    mls.LogWarning("CURRENT NAME: " + __instance.currentNode.name);
            //    mls.LogMessage("NODE CNAME: " + __instance.currentNode.creatureName);
            //    mls.LogMessage("  NODE CID: " + __instance.currentNode.creatureFileID);
            //    if (__instance.currentNode.displayVideo != null) mls.LogMessage("NODE VIDEO: " + __instance.currentNode.displayVideo.name);
            //}
        }

        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        [HarmonyPatch("RunTerminalEvents")]
        public static void TerminalEventPostfix(TerminalNode node, Terminal __instance) 
        {
            string newDisplayText = null;
            if (node == null) return;

            if (node.name == "helpTPsortNode" || node.name == "infoTPsortNode") newDisplayText = new Nodes().HelpInfoPage();
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
                else PluginMain.mls.LogDebug($"not reverse :(");

                newDisplayText = new Nodes().MoonsPage();
            }
            else if (node.name == "0_StoreHub") newDisplayText = new Nodes().StorePage(__instance);
            else if (node.name == "ScanInfo") newDisplayText = new Nodes().ScanMoonPage();
            else if (node.name == "scanShipNode") newDisplayText = new Nodes().ScanShipPage();
            else if (node.displayPlanetInfo >= 0 && node.name.ToLower().Contains("route") && moonMasters[node.displayPlanetInfo] != null) newDisplayText = new Nodes().RoutePage(node, __instance);
            else if (node.name == "CancelRoute") newDisplayText = "\n\n\nThe reroute has been cancelled.\n\n";
            //else if (node.name == "OtherCommands") newDisplayText = new Nodes().OtherHelpPage();

            if (newDisplayText != null)
            {
                __instance.screenText.textComponent.enableKerning = false;
                __instance.screenText.textComponent.enableWordWrapping = false;

                StringBuilder builder = new StringBuilder();
                if ((bool)__instance.displayingPersistentImage) builder.Append("\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n");

                builder.Append($"{newDisplayText}\n");

                __instance.screenText.text = builder.ToString();
                __instance.currentText = builder.ToString();
                __instance.textAdded = 0;
            }
            else __instance.screenText.textComponent.enableWordWrapping = true;
        }
    }
}