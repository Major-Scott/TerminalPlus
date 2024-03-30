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
        public string Logo()
        {
            StringBuilder pageChart = new StringBuilder();
            pageChart.AppendLine( "     ╔═════╗         ╔═╗       ╔═╗                     ");
            pageChart.AppendLine( "     ╚═╗ ╔═╝═╗══╗════╬═╣═══╗═══╣ ║                     ");
            pageChart.AppendLine( "       ║ ║ '═╣ ╔╝    ║ ║   ║ ' ║ ║                     ");
            pageChart.AppendLine( "       ╚═╝═══╝═╝═╩═╩═╝═╝═╩═╝═╩═╝═╝                     <line-height=40%>");
            pageChart.AppendLine(@"</li ╔══╗══╗══════════════════════════╗══════════════╗╗");
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
