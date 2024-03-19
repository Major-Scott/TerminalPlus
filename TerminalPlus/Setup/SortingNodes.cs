using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace TerminalPlus
{
    public partial class Nodes
    {
        //RETURNS GUIDE:
        //  0 = x and y are same
        //  1 = y then x
        // -1 = x then y (yeah its backwards and stupid and gave me a headache)

        //QUICK SORTS REFERENCE:
        // By Name: moonsList.Sort((x, y) => moonNames[x.levelID].CompareTo(moonNames[y.levelID]));
        // By Price: moonsList.Sort((x, y) => moonsPrice[x.levelID].CompareTo(moonsPrice[y.levelID]));
        // By Weather: moonsList.Sort((x, y) => x.currentWeather.CompareTo(y.currentWeather));

        static readonly List<string> nounList = new List<string>();
        public static readonly Terminal terminal = UnityEngine.Object.FindAnyObjectByType<Terminal>();

        internal static TerminalNode CreateNode()
        {
            TerminalNode terminalNode = ScriptableObject.CreateInstance<TerminalNode>();
            terminalNode.displayText = string.Empty;
            terminalNode.maxCharactersToType = 25;

            terminalNode.displayPlanetInfo = -1;
            terminalNode.buyRerouteToMoon = -1;
            terminalNode.buyItemIndex = -1;

            terminalNode.shipUnlockableID = -1;
            terminalNode.creatureFileID = -1;
            terminalNode.storyLogFileID = -1;

            terminalNode.playSyncedClip = -1;
            terminalNode.terminalEvent = string.Empty;
            terminalNode.terminalOptions = new CompatibleNoun[0];

            return terminalNode;
        }
        internal static TerminalKeyword CreateKeyword()
        {
            TerminalKeyword terminalKeyword = ScriptableObject.CreateInstance<TerminalKeyword>();
            terminalKeyword.defaultVerb = null;
            return terminalKeyword;
        }

        internal static void CreateSortingNodes()
        {
            nounList.AddRange(new string[] { "id", "name", "prefix", "grade", "price", "weather", "difficulty", "info", "help", "list", "current" });
            TerminalKeyword sortKeyword = CreateKeyword();
            TerminalKeyword reverseKeyword = CreateKeyword();

            if (terminal.terminalNodes.allKeywords.FirstOrDefault(keyword => keyword.word == "sort" || keyword.name == "sortKeyword") != null)
            {
                mls.LogInfo("'sort' keyword already exists, skipping");
                sortKeyword = terminal.terminalNodes.allKeywords.FirstOrDefault(keyword => keyword.word == "sort" || keyword.name == "sortKeyword");
            }
            else
            {
                sortKeyword.word = "sort";
                sortKeyword.name = "sortKeyword";
                sortKeyword.isVerb = true;
                terminal.terminalNodes.allKeywords = terminal.terminalNodes.allKeywords.AddItem(sortKeyword).ToArray();
            }
            if (terminal.terminalNodes.allKeywords.FirstOrDefault(keyword => keyword.word == "reverse" || keyword.name == "reverseKeyword") != null)
            {
                mls.LogInfo("'reverse' keyword already exists, skipping");
                reverseKeyword = terminal.terminalNodes.allKeywords.FirstOrDefault(keyword => keyword.word == "reverse" || keyword.name == "reverseKeyword");
            }
            else
            {
                reverseKeyword.word = "reverse";
                reverseKeyword.name = "reverseKeyword";
                reverseKeyword.isVerb = true;
                terminal.terminalNodes.allKeywords = terminal.terminalNodes.allKeywords.AddItem(reverseKeyword).ToArray();
            }
            sortKeyword.compatibleNouns = new CompatibleNoun[11];
            reverseKeyword.compatibleNouns = new CompatibleNoun[11];

            for (int i = 0; i < nounList.Count; i++)
            {
                TerminalKeyword nounKeyword = CreateKeyword();
                TerminalNode terminalNode = CreateNode();
                TerminalNode revTerminalNode = CreateNode();

                if (terminal.terminalNodes.allKeywords.FirstOrDefault(keynoun => keynoun.word == nounList[i]) != null && nounList[i] != "help" && nounList[i] != "info")
                {
                    nounKeyword = terminal.terminalNodes.allKeywords.FirstOrDefault(keynoun => keynoun.word == nounList[i]);
                }

                nounKeyword.word = nounList[i];
                nounKeyword.name = $"{nounList[i]}TPsortKeyword";
                nounKeyword.defaultVerb = sortKeyword;
                terminal.terminalNodes.allKeywords = terminal.terminalNodes.allKeywords.AddItem(nounKeyword).ToArray();
                terminalNode.terminalEvent = nounKeyword.word;
                terminalNode.name = $"{nounList[i]}TPsortNode";
                CompatibleNoun compatibleNoun = new CompatibleNoun();
                compatibleNoun.noun = nounKeyword;
                compatibleNoun.result = terminalNode;
                sortKeyword.compatibleNouns[i] = compatibleNoun;
                reverseKeyword.compatibleNouns[i] = compatibleNoun;
            }

        }

        [HarmonyPatch(typeof(RoundManager), "Start")]
        [HarmonyPostfix]
        [HarmonyPriority(10)]
        public static void CreateNodes()
        {
            CreateSortingNodes();
        }

//============================================================================================

        public static int SortByID(SelectableLevel x, SelectableLevel y)
        {
            if (x == null && y == null) return 0;
            else if (x == null) return -1;
            else if (y == null) return 1;
            else
            {
                int retval = x.levelID.CompareTo(y.levelID);
                if (retval != 0) return retval;
                else return 0;
            }
        }

        public static int SortByPrefix(SelectableLevel x, SelectableLevel y)
        {
            if (x == null && y == null) return 0;
            else if (x == null) return -1;
            else if (y == null) return 1;
            else
            {
                string xInput = moonPrefixes[x.levelID];
                string yInput = moonPrefixes[y.levelID];

                int xNum, yNum;
                if (int.TryParse(xInput, out xNum) && int.TryParse(yInput, out yNum)) return xNum.CompareTo(yNum);
                else if (int.TryParse(xInput, out xNum)) return 1;
                else if (int.TryParse(yInput, out yNum)) return -1;
                else return 0;
            }
        }

        public static int SortByGrade(SelectableLevel y, SelectableLevel x)
        {
            
            if (x == null && y == null) return 0;
            else if (x == null) return -1;
            else if (y == null) return 1;
            else
            {

                if (x.riskLevel == "Safe" && y.riskLevel != "Safe") return 1;
                else if (x.riskLevel != "Safe" && y.riskLevel == "Safe") return -1;
                else if (x.riskLevel == "Safe" && y.riskLevel == "Safe") return 0;

                if (x.riskLevel[0] == y.riskLevel[0])
                {

                    if (x.riskLevel.Contains('+') && !y.riskLevel.Contains("+")) return -1;
                    else if (!x.riskLevel.Contains('+') && y.riskLevel.Contains("+")) return 1;
                    else if (!x.riskLevel.Contains('-') && y.riskLevel.Contains("-")) return -1;
                    else if (x.riskLevel.Contains('-') && !y.riskLevel.Contains("-")) return 1;
                    else return 0;
                }
                if (x.riskLevel[0] == 'S' && y.riskLevel[0] != 'S') return -1;
                else if (x.riskLevel[0] != 'S' && y.riskLevel[0] == 'S') return 1;
                else if (x.riskLevel.Contains("SS") && !y.riskLevel.Contains("SS")) return -1;
                else if (!x.riskLevel.Contains("SS") && y.riskLevel.Contains("SS")) return 1;

                if (x.riskLevel.CompareTo(y.riskLevel) != 0) return x.riskLevel.CompareTo(y.riskLevel);
                else return 0;
            }
        }

        public static int SortByDifficulty(SelectableLevel y, SelectableLevel x)
        {
            if (x == null && y == null) return 0;
            else if (x == null) return -1;
            else if (y == null) return 1;
            else
            {
                if (x.riskLevel == "Safe" && y.riskLevel != "Safe") return 1;
                else if (x.riskLevel != "Safe" && y.riskLevel == "Safe") return -1;

                float xDif = 2 * x.maxDaytimeEnemyPowerCount + 4 * x.maxOutsideEnemyPowerCount + 5 * x.maxEnemyPowerCount * x.factorySizeMultiplier + 
                    5 * ((int)x.currentWeather + 2);
                float yDif = 2 * y.maxDaytimeEnemyPowerCount + 4 * y.maxOutsideEnemyPowerCount + 5 * y.maxEnemyPowerCount * y.factorySizeMultiplier +
                    5 * ((int)y.currentWeather + 2);

                if (x.currentWeather == LevelWeatherType.Eclipsed) xDif = xDif + 2 * x.maxOutsideEnemyPowerCount;
                if (y.currentWeather == LevelWeatherType.Eclipsed) yDif = yDif + 2 * y.maxOutsideEnemyPowerCount;

                //manualLogSource.LogMessage($"X: {x.PlanetName}, TOTAL DIFFICULTY: {xDif}");
                //manualLogSource.LogMessage($"Y: {y.PlanetName}, TOTAL DIFFICULTY: {yDif}");
                //manualLogSource.LogMessage($"RESULT Y-X: {xDif.CompareTo(yDif)}");
                //manualLogSource.LogMessage("-----------------------------------------------------------------");

                int retval = yDif.CompareTo(xDif);
                if (retval != 0) return retval;
                else return 0;
            }
        }
    }
}

