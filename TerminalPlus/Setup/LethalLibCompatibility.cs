using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using LethalLib;
using LethalLib.Modules;
using UnityEngine;
using static LethalLib.Modules.Items;
using static TerminalPlus.Nodes;

namespace TerminalPlus.Mods
{
    internal class LethalLibCompatibility
    {
        public static List<ShopItem> modItems = new List<ShopItem>();

        public static void RemoveHiddenStoreItems(Terminal terminal)
        {
            modItems.Clear();
            foreach (ShopItem shopItem in shopItems) modItems.Add(shopItem);
            foreach (ShopItem shopItem in modItems)
            {
                mls.LogDebug("PATCHITEM: " + shopItem.item.itemName);
                mls.LogDebug(" ENABLED?: " + !shopItem.wasRemoved);

                if (storeItems.Contains(shopItem.item) && shopItem.wasRemoved) storeItems.Remove(shopItem.item);

            }
        }
    }
}

namespace TerminalPlus.LGU
{
    using MoreShipUpgrades.Managers;
    using MoreShipUpgrades.Misc.TerminalNodes;
    
    internal class LGUCompatibility
    {
        public static StringBuilder LGUBuilder = new StringBuilder();
        public static List<CustomTerminalNode> upgradeNodes = new List<CustomTerminalNode>();

        [HarmonyPatch(typeof(UpgradeBus), "Reconstruct")]
        [HarmonyPostfix]
        public static void GetLGUUpgrades(ref List<CustomTerminalNode> ___terminalNodes)
        {
            if (___terminalNodes != null) upgradeNodes = ___terminalNodes;
        }

        public static string LGUString()
        {
            LGUBuilder.Clear();

            if (upgradeNodes.Count > 0)
            {
                LGUBuilder.AppendLine("  ║----------------------|-------|-----|----------║");

                foreach (CustomTerminalNode upgrade in upgradeNodes)
                {
                    int uLvl = upgrade.CurrentUpgrade;
                    int uMax = upgrade.MaxUpgrade;
                    string uName = upgrade.Name.Length > 20 ? upgrade.Name.Substring(0, 17) + "..." : upgrade.Name.PadRight(20);
                    string dispLevel = "        ";
                    string uSale = upgrade.salePerc < 1f ? $"{Mathf.RoundToInt((1f - upgrade.salePerc) * 100f)}%".PadLeft(3) : "   ";
                    if (uSale == "100%") uSale = "$$$";
                    int uPrice = Mathf.RoundToInt(upgrade.UnlockPrice * upgrade.salePerc);

                    if (!upgrade.Unlocked) { }
                    else if (uMax <= 0) dispLevel = "UNLOCKED";
                    else if (uLvl < uMax)
                    {
                        dispLevel = $"LEVEL: {uLvl + 1}";
                        uPrice = Mathf.RoundToInt(upgrade.Prices[uLvl] * upgrade.salePerc);
                    }
                    else
                    {
                        dispLevel = "<space=-0.5en>MAXED<space=1en>OUT<space=-0.5en>";
                        uPrice = Mathf.RoundToInt(upgrade.Prices[uLvl] * upgrade.salePerc);
                    }
                    LGUBuilder.AppendLine($"  ║ {uName} |<cspace=-2> $</cspace>{uPrice,-4}<cspace=-0.6> |</cspace> {uSale} | {dispLevel} ║");
                }
            }
            if (LGUBuilder != null) return LGUBuilder.ToString();
            else return string.Empty;
        }
    }
}
