using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
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

        public static readonly Terminal terminal = UnityEngine.Object.FindAnyObjectByType<Terminal>();
        private static string[] nounTPList = new string[0];

        public static int dayPowerMult = 0;
        public static int nightPowerMult = 0;
        public static int insidePowerMult = 0;
        public static int dayCountMult = 0;
        public static int nightCountMult = 0;
        public static int insideCountMult = 0;
        public static int sizeMult = 0;
        public static int valueMult = 0;
        public static int weatherMult = 0;

        internal static Dictionary<string, int> weathersDic = new Dictionary<string, int>()
        {
            { string.Empty, 0 },
            { "none", 0 },
            { "dustclouds", 1 },
            { "rainy", 2 },
            { "stormy", 3 },
            { "foggy", 4 },
            { "flooded", 5 },
            { "eclipsed", 6 },
            { "[unknown]", 7 },
            { "unknown", 7 }
        };

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
            //terminalNode.terminalOptions = new CompatibleNoun[0];

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
            mls.LogDebug("Start of CREATESORTNODES: " + nounTPList.Length);
            
            nounTPList = new string[13]{ "default", "id", "name", "prefix", "grade", "price", "weather", "difficulty", "info", "help", "list", "current", "rev" };
            TerminalKeyword sortKeyword = CreateKeyword();
            TerminalKeyword reverseKeyword = CreateKeyword();
            mls.LogDebug("created sort and reverse");

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
            sortKeyword.compatibleNouns = new CompatibleNoun[13];
            reverseKeyword.compatibleNouns = new CompatibleNoun[13];
            mls.LogDebug("TOTAL NOUN COUNT (should be 13): " + nounTPList.Length);
            for (int i = 0; i < nounTPList.Length; i++)
            {
                TerminalKeyword nounKeyword = CreateKeyword();
                TerminalNode terminalNode = CreateNode();
                TerminalNode revTerminalNode = CreateNode();
                if (terminal.terminalNodes.allKeywords.FirstOrDefault(keynoun => keynoun.word == nounTPList[i]) != null && nounTPList[i] != "help" && nounTPList[i] != "info")
                {
                    nounKeyword = terminal.terminalNodes.allKeywords.FirstOrDefault(keynoun => keynoun.word == nounTPList[i]);
                }
                nounKeyword.word = nounTPList[i];
                nounKeyword.name = $"{nounTPList[i]}TPsortKeyword";
                nounKeyword.defaultVerb = sortKeyword;
                terminal.terminalNodes.allKeywords = terminal.terminalNodes.allKeywords.AddItem(nounKeyword).ToArray();
                terminalNode.terminalEvent = nounKeyword.word;
                terminalNode.name = $"{nounTPList[i]}TPsortNode";
                CompatibleNoun compatibleNoun = new CompatibleNoun();
                compatibleNoun.noun = nounKeyword;
                compatibleNoun.result = terminalNode;
                sortKeyword.compatibleNouns[i] = compatibleNoun;
                reverseKeyword.compatibleNouns[i] = compatibleNoun;
                if (nounTPList[i] == "rev") reverseKeyword.specialKeywordResult = terminalNode;
            }
        }

        [HarmonyPatch(typeof(RoundManager), "Start")]
        [HarmonyPostfix]
        [HarmonyPriority(0)]
        public static void CreateNodes()
        {
            TerminalKeyword shipKeyword = CreateKeyword();
            TerminalNode shipNode = CreateNode();
            if (terminal.terminalNodes.allKeywords.FirstOrDefault(keynoun => keynoun.word == "ship") != null)
            {
                shipKeyword = terminal.terminalNodes.allKeywords.FirstOrDefault(keynoun => keynoun.word == "ship");
            }
            shipKeyword.word = "ship";
            shipKeyword.name = "scanShipKeyword";
            shipKeyword.defaultVerb = terminal.terminalNodes.allKeywords[73];
            terminal.terminalNodes.allKeywords = terminal.terminalNodes.allKeywords.AddItem(shipKeyword).ToArray();
            shipNode.terminalEvent = shipKeyword.word;
            shipNode.name = "scanShipNode";
            CompatibleNoun shipNoun = new CompatibleNoun { noun = shipKeyword, result = shipNode };
            terminal.terminalNodes.allKeywords[73].isVerb = true;
            terminal.terminalNodes.allKeywords[73].compatibleNouns = terminal.terminalNodes.allKeywords[73].compatibleNouns.AddItem(shipNoun).ToArray();
            CreateSortingNodes();
        }

//============================================================================================

        public static int SortByID(MoonMaster x, MoonMaster y)
        {
            if (x == null && y == null) return 0;
            else if (x == null) return -1;
            else if (y == null) return 1;
            else
            {
                int retval = x.mID.CompareTo(y.mID);
                if (retval != 0) return retval;
                else return 0;
            }
        }

        public static int SortByPrefix(MoonMaster x, MoonMaster y)
        {
            if (x == null && y == null) return 0;
            else if (x == null) return -1;
            else if (y == null) return 1;
            else
            {
                if (int.TryParse(x.mPrefix, out int xNum) && int.TryParse(y.mPrefix, out int yNum)) return xNum.CompareTo(yNum);
                else if (int.TryParse(x.mPrefix, out xNum)) return 1;
                else if (int.TryParse(y.mPrefix, out yNum)) return -1;
                else return 0;
            }
        }

        public static int SortByGrade(MoonMaster y, MoonMaster x)
        {
            
            if (x == null && y == null) return 0;
            else if (x == null) return -1;
            else if (y == null) return 1;
            else
            {
                string xGrade = x.mGrade;
                string yGrade = y.mGrade;

                if (xGrade == yGrade) return 0;
                else if (xGrade == "Safe" && yGrade != "Safe") return 1;
                else if (xGrade != "Safe" && yGrade == "Safe") return -1;
                else if (xGrade == "??" && yGrade != "??") return -1;
                else if (xGrade != "??" && yGrade == "??") return 1;

                if (xGrade[0] == 'S' && yGrade[0] != 'S') return -1;
                else if (xGrade[0] != 'S' && yGrade[0] == 'S') return 1;
                else if (xGrade.Contains("SS") && !yGrade.Contains("SS")) return -1;
                else if (!xGrade.Contains("SS") && yGrade.Contains("SS")) return 1;

                if (xGrade[0] == yGrade[0])
                {

                    if (xGrade.Contains('+') && !yGrade.Contains("+")) return -1;
                    else if (!xGrade.Contains('+') && yGrade.Contains("+")) return 1;
                    else if (!xGrade.Contains('-') && yGrade.Contains("-")) return -1;
                    else if (xGrade.Contains('-') && !yGrade.Contains("-")) return 1;
                    else return 0;
                }

                //if (x.riskLevel.CompareTo(y.riskLevel) != 0) return x.riskLevel.CompareTo(y.riskLevel);
                else return xGrade.CompareTo(y.mGrade);
            }
        }

        public static int SortByWeather(MoonMaster x, MoonMaster y)
        {
            if (x == null && y == null) return 0;
            else if (x == null) return -1;
            else if (y == null) return 1;
            else
            {
                string xWeather = x.mWeather.ToLower();
                string yWeather = y.mWeather.ToLower();

                if (xWeather == yWeather) return 0;
                else if ((xWeather == "none" || xWeather == string.Empty) && yWeather != "none" && yWeather != string.Empty) return 1;
                else if ((yWeather == "none" || yWeather == string.Empty) && xWeather != "none" && xWeather != string.Empty) return -1;
                else if (xWeather.Contains("?") && !yWeather.Contains("?")) return -1;
                else if (yWeather.Contains("?") && !xWeather.Contains("?")) return 1;
                else if (xWeather.Contains("/") && !yWeather.Contains("/")) return -1;
                else if (yWeather.Contains("/") && !xWeather.Contains("/")) return 1;
                else if (xWeather.Contains("/") && yWeather.Contains("/"))
                {
                    string x1 = xWeather.Substring(0, xWeather.IndexOf('/'));
                    string x2 = xWeather.Substring(xWeather.IndexOf('/') + 1);
                    string y1 = xWeather.Substring(0, yWeather.IndexOf('/'));
                    string y2 = xWeather.Substring(yWeather.IndexOf('/') + 1);
                    if (x1 == y1 && weathersDic.ContainsKey(x2) && weathersDic.ContainsKey(y2)) return weathersDic[x2].CompareTo(weathersDic[y2]);
                    else if (weathersDic.ContainsKey(x1) && weathersDic.ContainsKey(x2) && weathersDic.ContainsKey(y1) && weathersDic.ContainsKey(y2))
                    {
                        return (weathersDic[x1] + weathersDic[x2]).CompareTo(weathersDic[y1] + weathersDic[y2]);
                    }
                    else return xWeather.CompareTo(yWeather);
                }
                else if (weathersDic.ContainsKey(xWeather.Replace("?", string.Empty)) && weathersDic.ContainsKey(yWeather.Replace("?", string.Empty)))
                {
                    return weathersDic[xWeather.Replace("?", string.Empty)].CompareTo(weathersDic[yWeather.Replace("?", string.Empty)]);
                }
                else return 0;
                //int xNum, yNum;
                //if (int.TryParse(xInput, out xNum) && int.TryParse(yInput, out yNum)) return xNum.CompareTo(yNum);
                //else if (int.TryParse(xInput, out xNum)) return 1;
                //else if (int.TryParse(yInput, out yNum)) return -1;
            }
        }

        public static int SortByDifficulty(MoonMaster y, MoonMaster x)
        {
            if (x == null && y == null) return 0;
            else if (x == null) return -1;
            else if (y == null) return 1;
            else
            {
                int xDayEPC = x.mLevel.maxDaytimeEnemyPowerCount;
                int xDayEnemies = x.mLevel.DaytimeEnemies.Count;
                int xOutEPC = x.mLevel.maxOutsideEnemyPowerCount;
                int xOutEnemies = x.mLevel.OutsideEnemies.Count;
                int xInEPC = x.mLevel.maxEnemyPowerCount;
                int xInEnemies = x.mLevel.Enemies.Count;
                float xFactSize = x.mLevel.factorySizeMultiplier;
                int xMinScrap = x.mLevel.minTotalScrapValue;
                int xMaxScrap = x.mLevel.maxTotalScrapValue;
                int xWeather = weathersDic.ContainsKey(x.mWeather.Replace("?", string.Empty)) ? weathersDic[x.mWeather.Replace("?", string.Empty)] : 0;
                if (x.mWeather.ToLower().Contains("/"))
                {
                    string xHalf = x.mWeather.Substring(0, x.mWeather.IndexOf('/'));
                    xWeather = weathersDic.ContainsKey(xHalf) ? weathersDic[xHalf] : 0;
                }

                int yDayEPC = y.mLevel.maxDaytimeEnemyPowerCount;
                int yDayEnemies = y.mLevel.DaytimeEnemies.Count;
                int yOutEPC = y.mLevel.maxOutsideEnemyPowerCount;
                int yOutEnemies = y.mLevel.OutsideEnemies.Count;
                int yInEPC = y.mLevel.maxEnemyPowerCount;
                int yInEnemies = y.mLevel.Enemies.Count;
                float yFactSize = y.mLevel.factorySizeMultiplier;
                int yMinScrap = y.mLevel.minTotalScrapValue;
                int yMaxScrap = y.mLevel.maxTotalScrapValue;
                int yWeather = weathersDic.ContainsKey(y.mWeather.Replace("?", string.Empty)) ? weathersDic[y.mWeather.Replace("?", string.Empty)] : 0;
                if (y.mWeather.ToLower().Contains("/"))
                {
                    string yHalf = y.mWeather.Substring(0, y.mWeather.IndexOf('/'));
                    yWeather = weathersDic.ContainsKey(yHalf) ? weathersDic[yHalf] : 0;
                }

                if (x.mGrade == "Safe" && y.mGrade != "Safe") return 1;
                else if (x.mGrade != "Safe" && y.mGrade == "Safe") return -1;

                float xDiff = xDayEPC*dayPowerMult + xOutEPC*nightPowerMult + xInEPC*insidePowerMult + 
                    xDayEnemies*dayCountMult + xOutEnemies*nightCountMult + xInEnemies*insideCountMult +
                    xFactSize*sizeMult + (xWeather + 1)*weatherMult + ((xMinScrap + xMaxScrap)/2)*valueMult;

                float yDiff = yDayEPC * dayPowerMult + yOutEPC * nightPowerMult + yInEPC * insidePowerMult +
                    yDayEnemies * dayCountMult + yOutEnemies * nightCountMult + yInEnemies * insideCountMult +
                    yFactSize * sizeMult + (yWeather + 1) * weatherMult + ((yMinScrap + yMaxScrap) / 2) * valueMult;

                //mls.LogMessage($"X: {x.PlanetName}, TOTAL DIFFICULTY: {xDiff}");
                //mls.LogMessage($"Y: {y.PlanetName}, TOTAL DIFFICULTY: {yDiff}");
                //mls.LogMessage($"X TOT ENEMIES: {x.maxDaytimeEnemyPowerCount+x.maxOutsideEnemyPowerCount+x.maxEnemyPowerCount}");
                //mls.LogMessage($"Y TOT ENEMIES: {y.maxDaytimeEnemyPowerCount + y.maxOutsideEnemyPowerCount + y.maxEnemyPowerCount}");
                //mls.LogMessage($"X WEATHER: {(int)x.currentWeather + 1}  -  X SIZE: {x.factorySizeMultiplier}");
                //mls.LogMessage($"Y WEATHER: {(int)y.currentWeather + 1}  -  Y SIZE: {y.factorySizeMultiplier}");

                //mls.LogMessage("-----------------------------------------------------------------");

                int retval = yDiff.CompareTo(xDiff);
                if (retval != 0) return retval;
                else return 0;
            }
        }
    }
}

