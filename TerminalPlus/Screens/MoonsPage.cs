using System;
using System.Collections.Generic;
using System.Text;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace TerminalPlus
{
    public partial class Nodes
    {
        readonly string symDollar2 = "<space=0.256em><size=92%><voffset=0.72><rotate=-90><space=-0.39em>▲</rotate></voffset><size=74%><voffset=0.54><space=-0.6em>■</voffset></size></size><space=0.19em>";
        //readonly string symDollar3 = "<voffset=-0.6><size=86%>▘</size></voffset>";
        public string MoonsPage()
        {
            StringBuilder pageChart = new StringBuilder();

            if (ConfigManager.activeKilroy)
            {
                pageChart.AppendLine("<line-height=100%>                                         _____     ");
                pageChart.AppendLine("                                        | . . |    ");
                pageChart.AppendLine("  ╔═══════════════════════════════════ooO═| |═Ooo═╗");
                pageChart.AppendLine(@"  ║ ╦╦╗╔╗╔╗╦╗  ╔╗╔╗╔╦╗╔╗╦ ╔╗╔╗╦╦╔╗        ╰—╯     ║");
            }
            else
            {
                pageChart.AppendLine("<line-height=100%>\n                                                   ");
                pageChart.AppendLine(@"  ╔═══════════════════════════════════════════════╗");
                pageChart.AppendLine(@"  ║ ╦╦╗╔╗╔╗╦╗  ╔╗╔╗╔╦╗╔╗╦ ╔╗╔╗╦╦╔╗                ║");
            }
            pageChart.AppendLine(@"  ║ ║║║║║║║║║  ║ ╠╣ ║ ╠╣║ ║║║╦║║╠     SORTING BY: ║");
            pageChart.AppendLine($"  ║ ╩ ╩╚╝╚╝╩╚  ╚╝╩╩ ╩ ╩╩╩╝╚╝╚╝╚╝╚╝   {catalogueSort} ║");
            pageChart.AppendLine(@"  ╠═════════════════════╦════╦═══════╦════════════╣");
            pageChart.AppendLine(@"  ║    MOON REGISTRY    ║ GD ║ PRICE ║<cspace=0.85>  WEATHER  </cspace>║");
            pageChart.AppendLine(@"  ╠═════════════════════╩════╩═══════╩════════════╣");

            int counter = 0;
            bool firstNode = true;
            SelectableLevel slRef = ScriptableObject.CreateInstance<SelectableLevel>();
            MoonMaster sortRef = new MoonMaster(slRef);
            //List<SelectableLevel> displayLevels = PluginMain.LLLExists ? CompatibilityInfo.visibleLevels : moonsList;

            foreach (MoonMaster moonMP in masterList)
            {
                if (!moonMP.mVis) continue;

                if (!ConfigManager.evenSort && catalogueSort.Contains("GRADE") && !firstNode && moonMP.mGrade[0] != sortRef.mGrade[0]) { }
                else if (!ConfigManager.evenSort && catalogueSort.Contains("PRICE") && !firstNode && (Math.Abs(moonMP.mPrice - sortRef.mPrice) > 400)) { }
                else if (!ConfigManager.evenSort && catalogueSort.Contains("WEATHER") && !firstNode && moonMP.dispWeather != sortRef.dispWeather) { }
                else if (counter >= 3) { }
                else
                {
                    pageChart.AppendLine($"  ║ {moonMP.dispPrefix} {moonMP.dispName} |{moonMP.dispGrade}|" +
                    $" {symDollar2}{moonMP.mPrice,-4} | {moonMP.dispWeather,-10} ║");
                    if (moonMP.halfWeather != string.Empty) pageChart.AppendLine($"  ╠                     |    |       | {moonMP.halfWeather,-10} ╣");

                    sortRef = moonMP;
                    firstNode = false;
                    counter++;
                    continue;
                }

                pageChart.AppendLine("  ╠-----—---------------|----|-------|------------╣");

                pageChart.AppendLine($"  ║ {moonMP.dispPrefix} {moonMP.dispName} |{moonMP.dispGrade}|" +
                    $" {symDollar2}{moonMP.mPrice,-4} | {moonMP.dispWeather,-10} ║");
                if (moonMP.halfWeather != string.Empty) pageChart.AppendLine($"  ╠                     |    |       | {moonMP.halfWeather,-10} ╣");

                counter = 1;
                sortRef = moonMP;
            }

            pageChart.AppendLine("  ╠═══════════════════════════════════════════════╣");
            pageChart.AppendLine($"  ║    The Company is currently buying at {((int)(StartOfRound.Instance.companyBuyingRate * 100)).ToString() + "%",-5}   ║");
            pageChart.AppendLine("  ║           -------------------------           ║");
            pageChart.AppendLine($"  ║  You have {(int)Mathf.Floor(TimeOfDay.Instance.timeUntilDeadline / TimeOfDay.Instance.totalTime)} days left to complete your quota  ║");
            pageChart.AppendLine("  ╚═══════════════════════════════════════════════╝");

            return pageChart.ToString();
        }
    }
}

//foreach (SelectableLevel selectableLevel in moonsList)
//{
//    if (ConfigManager.overrideVisibility && selectableLevel.name == "CompanyBuildingLevel") continue;

//    if (!ConfigManager.evenSort && catalogueSort.Contains("GRADE") && !firstNode && selectableLevel.riskLevel[0] != sortRef.riskLevel[0]) { }
//    else if (!ConfigManager.evenSort && catalogueSort.Contains("PRICE") && !firstNode && (Math.Abs(moonsPrice[selectableLevel.levelID] - moonsPrice[sortRef.levelID]) > 400)) { }
//    else if (!ConfigManager.evenSort && catalogueSort.Contains("WEATHER") && !firstNode && displayWeather[selectableLevel.levelID] != displayWeather[sortRef.levelID]) { }
//    else if (counter >= 3) { }
//    else
//    {
//        pageChart.AppendLine($"  ║ {displayPrefixes[selectableLevel.levelID]} {displayNames[selectableLevel.levelID]} |{displayGrades[selectableLevel.levelID]}|" +
//        $" {symDollar2}{moonsPrice[selectableLevel.levelID],-4} | {displayWeather[selectableLevel.levelID],-10} ║");
//        if (halfWeather[selectableLevel.levelID] != string.Empty) pageChart.AppendLine($"  ╠                     |    |       | {halfWeather[selectableLevel.levelID],-10} ╣");

//        sortRef = selectableLevel;
//        firstNode = false;
//        counter++;
//        continue;
//    }

//    pageChart.AppendLine("  ╠-----—---------------|----|-------|------------╣");

//    pageChart.AppendLine($"  ║ {displayPrefixes[selectableLevel.levelID]} {displayNames[selectableLevel.levelID]} |{displayGrades[selectableLevel.levelID]}|" +
//        $" {symDollar2}{moonsPrice[selectableLevel.levelID],-4} | {displayWeather[selectableLevel.levelID],-10} ║");
//    if (halfWeather[selectableLevel.levelID] != string.Empty) pageChart.AppendLine($"  ╠                     |    |       | {halfWeather[selectableLevel.levelID],-10} ╣");

//    counter = 1;
//    sortRef = selectableLevel;
//}