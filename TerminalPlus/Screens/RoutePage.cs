using LethalLevelLoader;
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
            //int routeID = terminalNode.displayPlanetInfo;

            MoonMaster currentMoon = moonMasters[terminalNode.displayPlanetInfo];

            string routeName = currentMoon.mPrefix.Length > 0 ? $"{currentMoon.mPrefix}-{currentMoon.mName}" : currentMoon.mName;

            routeName = routeName.Length <= 26 ? routeName.ToUpper().PadRight(26) : routeName.Substring(0, 26).ToUpper();
            string cWeather = currentMoon.mWeather != string.Empty ? currentMoon.mWeather : "Clear"; //.Replace(" ", string.Empty)

            pageChart.AppendLine("\n<line-height=100%>                                                    ");
            pageChart.AppendLine("<line-height=100%>  ╔═══════════════════╦═══════════─════─═══──═─-- -");
            pageChart.AppendLine(@"  ║ ╦╗╔╗╦╗╔╗╦╦╔╦╗╔╗╔╗ ║ <voffset=-3>Preparing reroute to:</voffset>    ");
            pageChart.AppendLine($"  ║ ║╣╠ ║╣║║║║ ║ ╠ ╔╝ ║ <voffset=-18.5><size=125%>{routeName}</size></voffset>");
            pageChart.AppendLine(@"  ║ ╩╚╚╝╩╚╚╝╚╝ ╩ ╚╝<voffset=3><space=2>□<space=-2></voffset>  ║             ");
            pageChart.AppendLine( "  ╚═══════════════════╝                            \n");
            pageChart.AppendLine($"  +-──-");
            pageChart.AppendLine($"  │ Current  Weather:  {cWeather}");
            pageChart.AppendLine($"  │ Hazard Lvl/Grade:  {currentMoon.mGrade}");
            pageChart.AppendLine($"  +       ");
            pageChart.AppendLine($"  │ Rerouting to {currentMoon.mName} costs ${currentMoon.mPrice}.");
            pageChart.AppendLine($"  │ Your current balance is ${terminal.groupCredits}.");
            pageChart.AppendLine($"  +-──-\n\n");
            pageChart.AppendLine($"       <space=0.2en>Please <size=120%>CONFIRM</size> (\"C\") or <size=120%>DENY</size> (\"D\")");

            return pageChart.ToString();
        }
    }
}
