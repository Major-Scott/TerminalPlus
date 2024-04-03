using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TerminalPlus
{
    partial class Nodes
    {
        public string MainHelpPage()
        {
            StringBuilder pageChart = new StringBuilder();

            pageChart.AppendLine("\n\n>MOONS\nTo see the list of moons your ship can route to.\n");
            pageChart.AppendLine(">STORE\nTo see The Company Store's selection of items.\n");
            pageChart.AppendLine(">BESTIARY\nTo see the list of wildlife on record.\n");
            pageChart.AppendLine(">STORAGE\nTo access objects placed into storage.\n");
            pageChart.AppendLine(">SORT [sort setting]\nTo sort the moon catalogue by one of several possible settings. Type \"sort info\" for settings.\n");
            pageChart.AppendLine(">REVERSE [sort setting]\nTo sort and reverse the moon catalogue. \n");
            pageChart.AppendLine(">OTHER\nTo see the list of other commands.");

            return pageChart.ToString();
        }

        public string HelpInfoPage()
        {
            StringBuilder pageChart2 = new StringBuilder();

            pageChart2.AppendLine("\n\n         <size=120%><color=yellow>\"SORT\"</color> and <color=red>\"REVERSE\"</color> Settings</size>          ");
            pageChart2.AppendLine("<line-height=70%>        <size=120%>‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾          ");
            pageChart2.AppendLine("</line-height></size>>\"DEFAULT\" or \"ID\" <size=90%><color=yellow>(LOWEST first)  <color=red>(HIGHEST first)</color></color></size>");
            pageChart2.AppendLine("Sorts moons by internal ID; the default setting.<line-height=50%>\n</line-height>");
            pageChart2.AppendLine(">\"NAME\" <size=90%><color=yellow>('A' first)  <color=red>('Z' first)</color></color></size>");
            pageChart2.AppendLine("Sorts moons alphabetically by name.<line-height=50%>\n</line-height>");
            pageChart2.AppendLine(">\"PREFIX\" <size=90%><color=yellow>(LOWEST first)  <color=red>(HIGHEST first)</color></color></size>");
            pageChart2.AppendLine("Sorts moons by prefix (number left of the name).<line-height=50%>\n</line-height>");
            pageChart2.AppendLine(">\"GRADE\" <size=90%><color=yellow>(WORST first)  <color=red>(BEST first)</color></color></size>");
            pageChart2.AppendLine("Sorts moons by hazard level / grade (\"GD\").<line-height=50%>\n</line-height>");
            pageChart2.AppendLine(">\"PRICE\" <size=90%><color=yellow>(LOWEST first)  <color=red>(HIGHEST first)</color></color></size>");
            pageChart2.AppendLine("Sorts moons by price.<line-height=50%>\n</line-height>");
            pageChart2.AppendLine(">\"WEATHER\" <size=90%><color=yellow>(CLEAR first)  <color=red>(ECLIPSED first)</color></color></size>");
            pageChart2.AppendLine("Sorts moons by current weather conditions.<line-height=50%>\n</line-height>");
            pageChart2.AppendLine(">\"DIFFICULTY\" <size=90%><color=yellow>(EASIEST first)  <color=red>(HARDEST first)</color></color></size>");
            pageChart2.AppendLine("Sorts moons by calculated difficulty.<line-height=50%>\n</line-height>");
            pageChart2.AppendLine(">\"CURRENT\", \"LIST\", or NONE <size=90%><color=yellow>(N/A)  <color=red>(Reverse current)</color></color></size>");
            pageChart2.AppendLine("Reverse the current page.");

            return pageChart2.ToString();
        }

    }
}
