using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace TerminalPlus
{
    partial class Nodes
    {
        public string StorePage(Terminal terminal)
        {
            var instanceOnShip = GameObject.Find("/Environment/HangarShip").GetComponentsInChildren<GrabbableObject>().ToList();

            List<Item> storeItems = terminal.buyableItemsList.ToList();
            List<TerminalNode> storeDecorations = terminal.ShipDecorSelection.OrderBy(x => x.creatureName).ToList();
            List<UnlockableItem> storeShipUpgrades = StartOfRound.Instance.unlockablesList.unlockables.Where(x => x.unlockableType == 1 && x.alwaysInStock == true).ToList();
            storeItems.OrderBy(x => x.itemName);
            StringBuilder pageChart = new StringBuilder();

            pageChart.AppendLine("\n\n  ╔═══════════════════════════════════════════════╗");
            pageChart.AppendLine(@"  ║ ♥-  ╔╦╗╗╔╔╗  ╔╗╔╗╦╦╗╔╗╔╗╦╗╗╔  ╔╗╔╦╗╔╗╦╗╔╗  -♣ ║");
            pageChart.AppendLine(@"  ║ |    ║ ╠╣╠   ║ ║║║║║╠╝╠╣║║╠╝  ╚╗ ║ ║║║╣╠    | ║");
            pageChart.AppendLine(@"  ║ ♠-   ╩ ╝╚╚╝  ╚╝╚╝╩ ╩╩ ╩╩╩╚╚   ╚╝ ╩ ╚╝╩╚╚╝  -♦ ║");
            pageChart.AppendLine(@"  ╠══════════════════════╦═══════╦═════╦══════════╣");
            pageChart.AppendLine(@"  ║   <space=0.265en>ITEM/UNLOCKABLE<space=0.265en>   ║ PRICE ║<space=0.265en>SALE<space=0.265en>║  STATUS  ║");
            pageChart.AppendLine(@"  ╠══════════════════════╩═══════╩═════╩══════════╣");
            foreach (var item in storeItems)
            {
                if (terminal.buyableItemsList.ToList().IndexOf(item) < 0) continue;
                string cName = item.itemName.Length > 20 ? item.itemName.Substring(0, 17) + "..." : item.itemName.PadRight(20);

                int percentSale = terminal.itemSalesPercentages[terminal.buyableItemsList.ToList().IndexOf(item)];
                string itemSale = $"{100 - percentSale}%".PadLeft(3);
                if (itemSale == " 0%") itemSale = "   ";
                else if (itemSale == "100%") itemSale = "$$$";
                int itemPrice = (int)Math.Round(item.creditsWorth * (percentSale / 100f));
                if (itemPrice > 9999) itemPrice = 9999;

                int numOnShip = instanceOnShip.FindAll(x => x.itemProperties.itemName == item.itemName).Count;

                string displayNumOnShip = numOnShip.ToString();
                if (numOnShip > 99) displayNumOnShip = "+! Owned";
                else if (numOnShip <= 0) displayNumOnShip = "        ";
                else if (numOnShip < 10) displayNumOnShip = numOnShip.ToString().PadLeft(2, '0') + " Owned";
                else displayNumOnShip = numOnShip.ToString().PadRight(2) + " Owned";

                pageChart.AppendLine($"  ║ {cName} |<cspace=-2> $</cspace>{itemPrice,-4}<cspace=-0.6> |</cspace> {itemSale} | {displayNumOnShip} ║");
            }
            pageChart.AppendLine("  ╠═══════════════════════════════════════════════╣"); //╠═══════════════════╬═══════╬══════╬════════════╣

            storeShipUpgrades.OrderBy(x => x.unlockableName).ToList();
            Dictionary<string, string> defaultUpgrades = new Dictionary<string, string>()
            {
                { "Teleporter", "375 " },
                { "Signal translator", "255 " },
                { "Loud horn", "100 " },
                { "Inverse Teleporter", "425 " },
            };

            foreach (UnlockableItem upgrade in storeShipUpgrades)
            {
                TerminalNode upgradeNode = upgrade.shopSelectionNode;
                string upgradePrice = string.Empty;
                string upgradeName = upgrade.unlockableName;

                if (defaultUpgrades.ContainsKey(upgrade.unlockableName)) upgradePrice = defaultUpgrades[upgrade.unlockableName];
                else if (upgradeNode != null) upgradePrice = upgradeNode.itemCost.ToString().PadRight(4);
                else
                {
                    upgradeNode = UnityEngine.Object.FindObjectsOfType<TerminalNode>().ToList().Find(x => x.shipUnlockableID ==
                    StartOfRound.Instance.unlockablesList.unlockables.ToList().IndexOf(upgrade));
                    if (upgradeNode == null) upgradePrice = "??? ";
                }
                upgradeName = upgradeName.Length > 20 ? upgradeName.Substring(0, 17) + "..." : upgradeName.PadRight(20);

                if (upgrade.alreadyUnlocked || upgrade.hasBeenUnlockedByPlayer)
                {
                    pageChart.AppendLine($"  ║ {upgradeName} |<cspace=-2> $</cspace>{upgradePrice}<cspace=-0.6> |</cspace>     | UNLOCKED ║");
                }
                else
                {
                    pageChart.AppendLine($"  ║ {upgradeName} |<cspace=-2> $</cspace>{upgradePrice}<cspace=-0.6> |</cspace>     |          ║");
                }
            }
            pageChart.AppendLine("  ╠═══════════════════════════════════════════════╣");

            foreach (TerminalNode decoration in storeDecorations)
            {
                UnlockableItem unlockable = StartOfRound.Instance.unlockablesList.unlockables[decoration.shipUnlockableID];
                decoration.creatureName = decoration.creatureName.Length > 20 ? decoration.creatureName.Substring(0, 17) + "..." : decoration.creatureName.PadRight(20);
                
                if (unlockable != null && (unlockable.alreadyUnlocked || unlockable.hasBeenUnlockedByPlayer))
                {
                    pageChart.AppendLine($"  ║ {decoration.creatureName} |<cspace=-2> $</cspace>{decoration.itemCost,-4}<cspace=-0.6> |</cspace>     | UNLOCKED ║");
                }
                else if (unlockable != null)
                {
                    pageChart.AppendLine($"  ║ {decoration.creatureName} |<cspace=-2> $</cspace>{decoration.itemCost,-4}<cspace=-0.6> |</cspace>     |          ║");
                }
            }
            pageChart.AppendLine("  ╚═══════════════════════════════════════════════╝");

            return pageChart.ToString();
        }
    }
}
