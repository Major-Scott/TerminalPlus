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
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;

namespace TerminalPlus
{
    public partial class Nodes
    {
        public string ScanMoonPage()
        {
            List<GrabbableObject> scrapList = new List<GrabbableObject>();

            string scanHeader = string.Empty;

            string symTwoHand = "<voffset=-1><space=-2.73><size=115%>I<space=-5.5>I</size><space=-2.73></voffset>";
            string symConduct = "<voffset=-0.9><space=-0.64><size=115%><rotate=-45>Ϟ</rotate></size><space=-0.64></voffset>";
            string symWeapon = "<voffset=-2.4><space=-0.83><size=135%><rotate=145>†</rotate></size><space=-2.12></voffset>";
            string symBattery = "<voffset=-1.8><space=-0.63><size=115%>±</size><space=-0.63></voffset>";
            string symValue = "<voffset=-1.5><space=-0.6><size=115%>$</size><space=-3.6></voffset>";

            if (StartOfRound.Instance.currentLevel.name == "CompanyBuildingLevel" || StartOfRound.Instance.inShipPhase)
            {
                scrapList = Resources.FindObjectsOfTypeAll<GrabbableObject>().Where((GrabbableObject g) => g.itemProperties.isScrap && (g.isInShipRoom || g.isInElevator)).ToList();
                scanHeader = " Company-Ship™ Interior".PadRight(24);
            }
            else
            {
                //scrapList = Resources.FindObjectsOfTypeAll<GrabbableObject>().Where((GrabbableObject g) => g.itemProperties.isScrap && !g.isInShipRoom && !g.isInElevator).ToList();
                scrapList = UnityEngine.Object.FindObjectsOfType<GrabbableObject>().Where((GrabbableObject g) => g.itemProperties.isScrap && !g.isInShipRoom && !g.isInElevator).ToList();

                //scrapList = (from item in UnityEngine.Object.FindObjectsOfType<GrabbableObject>() where item.itemProperties.isScrap && !item.isInShipRoom && !item.isInElevator select item).ToList();

                scanHeader = StartOfRound.Instance.currentLevel.PlanetName.Length <= 22 ?
                    $"\"{StartOfRound.Instance.currentLevel.PlanetName}\"".PadRight(24) : $"\"{StartOfRound.Instance.currentLevel.PlanetName.Substring(0, 19)}...\"";
            }
            scrapList.Sort((a, b) => a.scrapValue.CompareTo(b.scrapValue));

            int totConduct = scrapList.FindAll(c => c.itemProperties.isConductiveMetal).Count;
            int totTwoHand = scrapList.FindAll(t => t.itemProperties.twoHanded).Count;
            int totWeapon = scrapList.FindAll(w => w.itemProperties.isDefensiveWeapon).Count;
            int totBattery = scrapList.FindAll(b => b.itemProperties.requiresBattery).Count;
            int totValue = scrapList.Sum(v => v.scrapValue);

            string barConduct = totConduct <= 19 ? string.Empty.PadLeft(totConduct, '█').PadRight(19, '.') : string.Empty.PadLeft(19, '█');
            string barTwoHand = totTwoHand <= 19 ? string.Empty.PadLeft(totTwoHand, '█').PadRight(19, '.') : string.Empty.PadLeft(19, '█');
            string barWeapon = totWeapon <= 19 ? string.Empty.PadLeft(totWeapon, '█').PadRight(19, '.') : string.Empty.PadLeft(19, '█');
            string barBattery = totBattery <= 19 ? string.Empty.PadLeft(totBattery, '█').PadRight(19, '.') : string.Empty.PadLeft(19, '█');
            string barValue = totValue <= 1949 ? string.Empty.PadLeft(Mathf.RoundToInt(totValue/100), '█').PadRight(19, '.') : string.Empty.PadLeft(19, '█');
            string barTotal = scrapList.Count <= 19 ? string.Empty.PadLeft(scrapList.Count, '█').PadRight(19, '.') : string.Empty.PadLeft(19, '█');

            string disConduct = totConduct <= 999 ? totConduct.ToString().PadLeft(2, '0').PadRight(5) : "999+ ";
            string disTwoHand = totTwoHand <= 999 ? totTwoHand.ToString().PadLeft(2, '0').PadRight(5) : "999+ ";
            string disWeapon = totWeapon <= 999 ? totWeapon.ToString().PadLeft(2, '0').PadRight(5) : "999+ ";
            string disBattery = totBattery <= 999 ? totBattery.ToString().PadLeft(2, '0').PadRight(5) : "999+ ";
            string disValue = totValue <= 9999 ? totValue.ToString().PadRight(5) : "9999+";
            string disTotal = scrapList.Count <= 999 ? scrapList.Count.ToString().PadLeft(2, '0').PadRight(5) : "999+ ";

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
            pageChart.AppendLine($"  ║[--]│  SCANNING: {scanHeader} │[--]║");

            if (!ConfigManager.detailedScan)
            {
                string simpleCount = $"There is currently {scrapList.Count} total scrap".PadRight(35);
                int padLeft = (10 - totValue.ToString().Length)/2;
                int padRight = 10 - totValue.ToString().Length - padLeft;
                string simpleTotal = $"<size=120%>$</size><space=-3>{totValue}<space=-2>";
                pageChart.AppendLine($"  ║[  ]│ ----------------------------------- │[  ]║");
                pageChart.AppendLine($"  ║[--]│ {simpleCount} │[--]║");
                pageChart.AppendLine($"  ║[  ]│ with an approximate total value of: │[  ]║");
                pageChart.AppendLine($"  ║[--]│                                     │[--]║");
                pageChart.AppendLine($"  ║[  ]│ ----------  {string.Empty.PadLeft(padLeft)}{simpleTotal}{string.Empty.PadRight(padRight)}  ---------- │[  ]║");
                pageChart.AppendLine($"  ║[---╯                                     ╰---]║");
            }
            else
            {
                pageChart.AppendLine("  ║[---╯ ----------------------------------- ╰---]║");
                pageChart.AppendLine("  ║]                 ╔═══════════════════╗       [║");
                pageChart.AppendLine($"  ║]   Two-Handed [{symTwoHand}]║{barTwoHand}║{disTwoHand}  [║");
                pageChart.AppendLine($"  ║]   Conductive [{symConduct}]║{barConduct}║{disConduct}  [║");
                pageChart.AppendLine($"  ║]    Is Weapon [{symWeapon}]║{barWeapon}║{disWeapon}  [║");
                pageChart.AppendLine($"  ║]  Has Battery [{symBattery}]║{barBattery}║{disBattery}  [║");
                pageChart.AppendLine($"  ║]  Total Scrap [∑]║{barTotal}║{disTotal}  [║");
                pageChart.AppendLine($"  ║]  Total Value [{symValue}]║{barValue}║{disValue}  [║");
                pageChart.AppendLine("  ║]                 ╠╤╤╤╤╦╤╤╤╤╦╤╤╤╤╦╤╤╤╤╣       [║");
                pageChart.AppendLine("  ║[-╮               0    5   10   15   20+    ╭-]║");

                if (scrapList.Count > 0)
                {
                    pageChart.AppendLine("  ╠══╧══════════════════╦═══════╦═══════╦══════╧══╣");
                    pageChart.AppendLine("  ║      ITEM NAME      ║ VALUE ║  WGT. ║  NOTES  ║");
                    pageChart.AppendLine("  ╠═════════════════════╩═══════╩═══════╩═════════╣");

                    foreach (GrabbableObject scrap in scrapList)
                    {
                        string cName = scrap.itemProperties.itemName;
                        string cValue = scrap.scrapValue.ToString();
                        string cWeight = Mathf.RoundToInt(Mathf.Clamp(scrap.itemProperties.weight - 1f, 0f, 100f) * 105f).ToString();

                        string cTwoHand = scrap.itemProperties.twoHanded ? symTwoHand : " ";
                        string cConduct = scrap.itemProperties.isConductiveMetal ? symConduct : " ";
                        string cWeapon = scrap.itemProperties.isDefensiveWeapon ? symWeapon : " ";
                        string cBattery = scrap.itemProperties.requiresBattery ? symBattery : " ";
                        string cProperties = $" {cTwoHand} {cConduct} {cWeapon} {cBattery} ";

                        cName = cName.Length <= 19 ? cName.PadRight(19) : cName.Substring(0, 16) + "...";
                        cValue = cValue.Length <= 4 ? cValue.PadRight(4) : "9999";
                        cWeight = cWeight.Length <= 3 ? cWeight.PadLeft(3) : "999";
                        pageChart.AppendLine($"  ║ {cName} | <space=-1.5>$<space=-1>{cValue} | {cWeight}lb |{cProperties}║");
                    }
                }
                else
                {
                    pageChart.AppendLine("  ╠══╧═════════════════════════════════════════╧══╣");
                    pageChart.AppendLine("  ║               NO SCRAP FOUND :(               ║");
                }
            }

            pageChart.AppendLine("  ╠═══════════════════════════════════════════════╣");
            pageChart.AppendLine("  ║└──┘╰─────────────────────────────────────╯└──┘║");
            pageChart.AppendLine("  ║    │                                     │    ║");
            pageChart.AppendLine("  ╚════╧═════════════════════════════════════╧════╝");

            return pageChart.ToString();
        }
    }
}

//pageChart.AppendLine("  ╭────┬_.────┬───────────────────────┐_.───────.");
//pageChart.AppendLine("  │    │      │                       │          \\");
//pageChart.AppendLine("  │ /╲ │      │              ┌────┐   │           │");
//pageChart.AppendLine("  │ ┐┌`│      │              │    │   │           │");
//pageChart.AppendLine("  │    │      │              │    │   │           │");
//pageChart.AppendLine("  │    │      │              │    │   │           │");
//pageChart.AppendLine("  │    │      │              └────┘   │           │");
//pageChart.AppendLine("  │    ╰──────╰───────────────────────╯           │");
//pageChart.AppendLine("  │                                               │");
//pageChart.AppendLine("  │    ╭─────────────────────────────────────╮    │");
//pageChart.AppendLine("  │    │  SCANNING:  Company Ship™ Interior  │    │");
//pageChart.AppendLine("  │    │ ----------------------------------- │    │");
//pageChart.AppendLine("  │    │                                     │    │");
//pageChart.AppendLine("  │    │ ----------------------------------- │    │");
//pageChart.AppendLine("  │    │                                     │    │");
//pageChart.AppendLine("  │    │                                     │    │");
//pageChart.AppendLine("  │    │                                     │    │");
//pageChart.AppendLine("  │    │                                     │    │");
//pageChart.AppendLine("  │    │                                     │    │");
//pageChart.AppendLine("  │┌──┐│                                     │┌──┐│");
//pageChart.AppendLine("  │└──┘╰─────────────────────────────────────╯└──┘│");
//pageChart.AppendLine("  │    │                                     │    │");
//pageChart.AppendLine("  ╰───────────────────────────────────────────────╯");