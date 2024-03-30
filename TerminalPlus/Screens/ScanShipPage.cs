using HarmonyLib;
using System;
using System.IO;
using System.Drawing.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEditor;
using BepInEx;
using System.Collections;
using System.Reflection;

namespace TerminalPlus
{
    public partial class Nodes
    {
        readonly string symTwoHand = "<voffset=-1><space=-2.73><size=115%>I<space=-5.5>I</size><space=-2.73></voffset>";
        readonly string symConduct = "<voffset=-0.9><space=-0.64><size=115%><rotate=-45>Ϟ</rotate></size><space=-0.64></voffset>";
        readonly string symWeapon = "<voffset=-2.4><space=-0.83><size=135%><rotate=145>†</rotate></size><space=-2.12></voffset>";
        readonly string symBattery = "<voffset=-1.8><space=-0.63><size=115%>±</size><space=-0.63></voffset>";
        readonly string symValue = "<voffset=-1.5><space=-0.6><size=115%>$</size><space=-3.6></voffset>";

        public string ScanShipPage()
        {
            List<GrabbableObject> itemList = new List<GrabbableObject>();

            string scanHeader = string.Empty;

            itemList = Resources.FindObjectsOfTypeAll<GrabbableObject>().Where((GrabbableObject g) => !g.itemProperties.isScrap
            && g.itemProperties.name != "Clipboard" && g.itemProperties.name != "StickyNote" && (g.isInShipRoom || g.isInElevator)).ToList();
            itemList.Sort((a, b) => a.itemProperties.creditsWorth.CompareTo(b.itemProperties.creditsWorth));

            int totConduct = itemList.FindAll(c => c.itemProperties.isConductiveMetal).Count;
            int totTwoHand = itemList.FindAll(t => t.itemProperties.twoHanded).Count;
            int totWeapon = itemList.FindAll(w => w.itemProperties.isDefensiveWeapon).Count;
            int totBattery = itemList.FindAll(b => b.itemProperties.requiresBattery).Count;
            int totValue = itemList.Sum(v => v.scrapValue);

            string barConduct = totConduct <= 19 ? string.Empty.PadLeft(totConduct, '█').PadRight(19, '.') : string.Empty.PadLeft(19, '█');
            string barTwoHand = totTwoHand <= 19 ? string.Empty.PadLeft(totTwoHand, '█').PadRight(19, '.') : string.Empty.PadLeft(19, '█');
            string barWeapon = totWeapon <= 19 ? string.Empty.PadLeft(totWeapon, '█').PadRight(19, '.') : string.Empty.PadLeft(19, '█');
            string barBattery = totBattery <= 19 ? string.Empty.PadLeft(totBattery, '█').PadRight(19, '.') : string.Empty.PadLeft(19, '█');
            string barValue = totValue <= 1949 ? string.Empty.PadLeft(Mathf.RoundToInt(totValue/100), '█').PadRight(19, '.') : string.Empty.PadLeft(19, '█');
            string barTotal = itemList.Count <= 19 ? string.Empty.PadLeft(itemList.Count, '█').PadRight(19, '.') : string.Empty.PadLeft(19, '█');

            string disConduct = totConduct <= 999 ? totConduct.ToString().PadLeft(2, '0').PadRight(5) : "999+ ";
            string disTwoHand = totTwoHand <= 999 ? totTwoHand.ToString().PadLeft(2, '0').PadRight(5) : "999+ ";
            string disWeapon = totWeapon <= 999 ? totWeapon.ToString().PadLeft(2, '0').PadRight(5) : "999+ ";
            string disBattery = totBattery <= 999 ? totBattery.ToString().PadLeft(2, '0').PadRight(5) : "999+ ";
            string disValue = totValue <= 9999 ? totValue.ToString().PadRight(5) : "9999+";
            string disTotal = itemList.Count <= 999 ? itemList.Count.ToString().PadLeft(2, '0').PadRight(5) : "999+ ";

            StringBuilder pageChart = new StringBuilder();
            pageChart.AppendLine("\n<line-height=100%>                                                   ");
            pageChart.AppendLine("<line-height=100%>  ╔════╗_╔════╦═══════════════════════╗_╔═══════╗  ");
            pageChart.AppendLine("  ║    │‾     ║ <voffset=-0.4em>╔═╗╔═╗╔═╗╔╗╔</voffset> ┌────┐   ║‾        ║  ");
            pageChart.AppendLine("  ║    │      ║ <voffset=-0.4em>╚═╗║  ╠═╣║║║</voffset> │+--+│   ║         ╚═╗");
            pageChart.AppendLine("  ║    │      ║ <voffset=-0.4em>╚═╝╚═╝╩ ╩╝╚╝</voffset> │+-+-│   ║           ║");
            pageChart.AppendLine("  ║    │      ║ <voffset=-0.4em>ITEM SUMMARY</voffset> │-+-+│   ║           ║");
            pageChart.AppendLine("  ║    │      ║              └────┘   ║           ║");
            pageChart.AppendLine("  ║    ╰──────╚═══════════════════════╝           ║");
            pageChart.AppendLine("  ║                                               ║");
            pageChart.AppendLine("  ║    ╭─────────────────────────────────────╮    ║");
            pageChart.AppendLine($"  ║[--]│  SCANNING: Company-Ship™  Equipment │[--]║");
            pageChart.AppendLine("  ║[---╯ ----------------------------------- ╰---]║");
            pageChart.AppendLine("  ║]                 ╔═══════════════════╗       [║");
            pageChart.AppendLine($"  ║]   Two-Handed [{symTwoHand}]║{barTwoHand}║{disTwoHand}  [║");
            pageChart.AppendLine($"  ║]   Conductive [{symConduct}]║{barConduct}║{disConduct}  [║");
            pageChart.AppendLine($"  ║]    Is Weapon [{symWeapon}]║{barWeapon}║{disWeapon}  [║");
            pageChart.AppendLine($"  ║]  Has Battery [{symBattery}]║{barBattery}║{disBattery}  [║");
            pageChart.AppendLine($"  ║]  Total Items [∑]║{barTotal}║{disTotal}  [║");
            pageChart.AppendLine($"  ║]  Total Value [{symValue}]║{barValue}║{disValue}  [║");
            pageChart.AppendLine("  ║]                 ╠╤╤╤╤╦╤╤╤╤╦╤╤╤╤╦╤╤╤╤╣       [║");
            pageChart.AppendLine("  ║[-╮               0    5   10   15   20+    ╭-]║");


            if (itemList.Count > 0)
            {
                pageChart.AppendLine("  ╠══╧══════════════════╦═══════╦═══════╦══════╧══╣");
                pageChart.AppendLine("  ║   EQUIPMENT  NAME   ║ OWNED ║  WGT. ║  NOTES  ║");
                pageChart.AppendLine("  ╠═════════════════════╩═══════╩═══════╩═════════╣");

                foreach (GrabbableObject item in itemList)
                {
                    string cName = item.itemProperties.itemName;
                    string cValue = item.itemProperties.creditsWorth.ToString();
                    string cWeight = Mathf.RoundToInt(Mathf.Clamp(item.itemProperties.weight - 1f, 0f, 100f) * 105f).ToString();

                    string cTwoHand = item.itemProperties.twoHanded ? symTwoHand : " ";
                    string cConduct = item.itemProperties.isConductiveMetal ? symConduct : " ";
                    string cWeapon = item.itemProperties.isDefensiveWeapon ? symWeapon : " ";
                    string cBattery = item.itemProperties.requiresBattery ? symBattery : " ";
                    string cProperties = $" {cTwoHand} {cConduct} {cWeapon} {cBattery} ";

                    cName = cName.Length <= 19 ? cName.PadRight(19) : cName.Substring(0, 16) + "...";
                    cValue = cValue.Length <= 4 ? cValue.PadRight(4) : "9999";
                    cWeight = cWeight.Length <= 3 ? cWeight.PadRight(3) : "999";

                    pageChart.AppendLine($"  ║ {cName} | <space=-1.5>$<space=-1>{cValue} | {cWeight}lb |{cProperties}║");
                }
            }
            else
            {
                pageChart.AppendLine("  ╠══╧═════════════════════════════════════════╧══╣");
                pageChart.AppendLine("  ║             NO EQUIPMENT FOUND :(             ║");
            }

            pageChart.AppendLine("  ╠═══════════════════════════════════════════════╣");
            pageChart.AppendLine("  ║└──┘╰─────────────────────────────────────╯└──┘║");
            pageChart.AppendLine("  ║    │                                     │    ║");
            pageChart.AppendLine("  ╚════╧═════════════════════════════════════╧════╝");

            return pageChart.ToString();
        }
    }
}