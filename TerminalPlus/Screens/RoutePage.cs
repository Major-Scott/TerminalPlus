using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TerminalPlus
{
    public partial class Nodes
    {
        public string RoutePage(TerminalNode terminalNode, Terminal terminal)
        {
            StringBuilder pageChart = new StringBuilder();
            int routeID = terminalNode.displayPlanetInfo;

            SelectableLevel currentMoon = moonsList.FirstOrDefault(m => m.levelID == routeID);

            string routeName = moonPrefixes[routeID].Length > 0 ? $"{moonPrefixes[routeID]}-{moonNames[routeID]}" : moonNames[routeID];

            routeName = routeName.Length <= 26 ? routeName.ToUpper().PadRight(26) : routeName.Substring(0, 26).ToUpper();
            //string cWeather = currentMoon.currentWeather == LevelWeatherType.None  ? "Clear" : currentMoon.currentWeather.ToString();

            pageChart.AppendLine("\n<line-height=100%>                                                    ");
            pageChart.AppendLine("<line-height=100%>  ╔═══════════════════╦═══════════─════─═══──═─-- -");
            pageChart.AppendLine(@"  ║ ╦╗╔╗╦╗╔╗╦╦╔╦╗╔╗╔╗ ║ <voffset=-3>Preparing reroute to:</voffset>    ");
            pageChart.AppendLine($"  ║ ║╣╠ ║╣║║║║ ║ ╠ ╔╝ ║ <voffset=-18.5><size=125%>{routeName}</size></voffset>");
            pageChart.AppendLine(@"  ║ ╩╚╚╝╩╚╚╝╚╝ ╩ ╚╝<voffset=3><space=2>□<space=-2></voffset>  ║             ");
            pageChart.AppendLine( "  ╚═══════════════════╝                            \n");
            pageChart.AppendLine($"  +-──-");
            pageChart.AppendLine($"  │ Current  Weather:  {fullWeather[routeID]}");
            pageChart.AppendLine($"  │ Hazard Lvl/Grade:  {currentMoon.riskLevel}");
            pageChart.AppendLine($"  +       ");
            pageChart.AppendLine($"  │ Rerouting to {moonNames[routeID]} costs ${moonsPrice[routeID]}.");
            pageChart.AppendLine($"  │ Your current balance is ${terminal.groupCredits}.");
            pageChart.AppendLine($"  +-──-\n\n");
            pageChart.AppendLine($"       <space=0.2en>Please <size=120%>CONFIRM</size> (\"C\") or <size=120%>DENY</size> (\"D\")");

            return pageChart.ToString();
        }
    }
}
