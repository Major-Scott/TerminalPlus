using System;
using System.Collections.Generic;
using System.Text;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace TerminalPlus
{
    partial class Nodes
    {
        //TERMINAL WIDTH: 51 for some reason & also off-center. padleft 3 for centered and width = 48
        //planetNameWidth = 20;  //prefix is added 4, longest is "041 Experimentation" 20 total
        //planetWeatherWidth = 10; //longest it would be is 10: "(Eclipsed)"
        public string MoonCataloguePage()
        {
            StringBuilder pageChart = new StringBuilder();

            if (ConfigManager.activeKilroy)
            {
                pageChart.AppendLine(@"                                         _____     ");
                pageChart.AppendLine(@"                                        | . . |    ");
                pageChart.AppendLine(@"  ╔═══════════════════════════════════ooO═| |═Ooo═╗");
                pageChart.AppendLine(@"  ║ ╦╦╗╔╗╔╗╦╗  ╔╗╔╗╔╦╗╔╗╦ ╔╗╔╗╦╦╔╗        ╰—╯     ║");
            }
            else
            {
                pageChart.AppendLine(@"                                                   ");
                pageChart.AppendLine(@"                                                   ");
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
            SelectableLevel sortRef = ScriptableObject.CreateInstance<SelectableLevel>();

            foreach (SelectableLevel selectableLevel in moonsList)
            {
                if (ConfigManager.hiddenCompany && selectableLevel.name == "CompanyBuildingLevel") continue;

                if (catalogueSort == "     GRADE ⇩" && !firstNode && selectableLevel.riskLevel[0] != sortRef.riskLevel[0]) { }
                else if (catalogueSort == "     PRICE ⇩" && !firstNode && (Math.Abs(moonsPrice[selectableLevel.levelID] - moonsPrice[sortRef.levelID]) > 400)) { }
                else if (catalogueSort == "   WEATHER ⇩" && !firstNode && selectableLevel.currentWeather != sortRef.currentWeather) { }
                else if (counter >= 3) { }
                else
                {
                    pageChart.AppendLine($"  ║ {displayPrefixes[selectableLevel.levelID]} {displayNames[selectableLevel.levelID]} |" +
                    $"{displayGrades[selectableLevel.levelID]}|" +
                    $"<cspace=-1> $</cspace>{moonsPrice[selectableLevel.levelID],-4}<cspace=-0.8> |</cspace> " +
                    $"({selectableLevel.currentWeather})".PadRight(10) + "<cspace=-1> ║</cspace>");
                    sortRef = selectableLevel;
                    firstNode = false;
                    counter++;
                    continue;
                }

                pageChart.AppendLine("  ╠-----—---------------|----|-------|------------╣");

                pageChart.AppendLine($"  ║ {displayPrefixes[selectableLevel.levelID]} {displayNames[selectableLevel.levelID]} |" +
                    $"{displayGrades[selectableLevel.levelID]}|" +
                    $"<cspace=-1> $</cspace>{moonsPrice[selectableLevel.levelID],-4}<cspace=-0.8> |</cspace> " + 
                    $"({selectableLevel.currentWeather})".PadRight(10) + "<cspace=-1> ║</cspace>");
                counter = 1;
                sortRef = selectableLevel;
            }
            pageChart.AppendLine(@"  ╠═══════════════════════════════════════════════╣");
            pageChart.AppendLine($"  ║    The Company is currently buying at {((int)(StartOfRound.Instance.companyBuyingRate * 100)).ToString() + "%",-4}    ║");
            pageChart.AppendLine(@"  ╚═══════════════════════════════════════════════╝");

            return pageChart.ToString();
        }

    }
}