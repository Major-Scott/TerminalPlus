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
        StringBuilder pageChart = new StringBuilder();

        public string IconPage()
        {
            pageChart.AppendLine( "\n\n\n\n\n\n<size=90%>     ╔═════╗         ╔═╗       ╔═╗                     ");
            pageChart.AppendLine( "     ╚═╗ ╔═╝═╗══╗════╬═╣═══╗═══╣ ║                     ");
            pageChart.AppendLine( "       ║ ║ '═╣ ╔╝    ║ ║   ║ ' ║ ║                     ");
            pageChart.AppendLine( "       ╚═╝═══╝═╝═╩═╩═╝═╝═╩═╝═╩═╝═╝                     <line-height=40%>");
            pageChart.AppendLine(@"</line-height>     ╔══╗══╗══════════════════════════╗══════════════╗╗");
            pageChart.AppendLine(@"     ║ ╔════════════════════╗══════════════════════╗╗ ║");//
            pageChart.AppendLine(@"     ║ ║  ╔═══════╗╗╔════╗╗  ╔══╗╗ ╔══╗╗╔═══════╗╗  ║ ║");//    ╔═══════╗╗╔════╗╗  ╔══╗╗ ╔══╗╗╔═══════╗╗
            pageChart.AppendLine(@"     ║ ║  ╚═╗ ╔══╗ ║╚═╗ ╔═╝  ╚╗ ╔╝ ╚╗ ╔╝║ ╔════╗ ║  ║ ║");//    ╚═╗ ╔══╗ ║╚═╗ ╔═╝  ╚╗ ╔╝ ╚╗ ╔╝║ ╔════╗ ║
            pageChart.AppendLine(@"     ║ ║    ║ ╚══╝ ║  ║ ║     ║ ║   ║ ║ ║ ╚╗═══╩╗╣  ║ ║");//      ║ ╚══╝ ║  ║ ║     ║ ║   ║ ║ ║ ╚════╩╗╣
            pageChart.AppendLine(@"     ║ ║    ║ ╔══╗═╝  ║ ║  ╔╗╗║ ║   ║ ║ ╠═╗════╗ ║  ║ ║");//      ║ ╔════╝  ║ ║  ╔╗╗║ ║   ║ ║ ╠═╗════╗ ║
            pageChart.AppendLine(@"     ║ ║  ╔═╝ ╚╗╗   ╔═╝ ╚══╝ ║║ ╚╗══╝ ║ ║ ╚════╝ ║  ║ ║");//    ╔═╝ ╚╗╗   ╔═╝ ╚══╝ ║║ ╚═══╝ ║ ║ ╚════╝ ║
            pageChart.AppendLine(@"     ║ ║  ╚╗════╝   ╚═╗═╗════╝╚═════╗═╝ ╚═══╗══╗═╝  ║ ║");//    ╚═════╝   ╚════════╝╚═══════╝ ╚════════╝
            pageChart.AppendLine(@"     ║ ╚═════════════════════════════════╗══════════╝ ║");//
            pageChart.AppendLine(@"     ╚══════╗═══════════════╗════════╗═══════════╗════╝");//

            return pageChart.ToString();
        }


        

    }
}
