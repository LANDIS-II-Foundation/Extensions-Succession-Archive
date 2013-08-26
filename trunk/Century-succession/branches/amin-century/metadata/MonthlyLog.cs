using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Landis.Library.Metadata;

namespace Landis.Extension.Succession.Century
{
    public class MonthlyLog
    {
        [DataFieldAttribute(Unit = FiledUnits.Year, Desc = "...")]
        public int Time {set; get;}

        [DataFieldAttribute(Unit = FiledUnits.Month, Desc = "...")]
        public int Month { set; get; }

        [DataFieldAttribute(Unit = FiledUnits.None, Desc = "...")]
        public string EcoregionName { set; get; }

        [DataFieldAttribute(Unit = FiledUnits.None, Desc = "...")]
        public int NumSites { set; get; }

        [DataFieldAttribute(Unit = FiledUnits.g_C_m_2, Desc = "...", Format = "{1:0.0}")]
        public double ppt {get; set;}// = new double[PlugIn.ModelCore.Ecoregions.Count];

        [DataFieldAttribute(Unit = FiledUnits.g_C_m_2, Desc = "...", Format = "{1:0.0}")]
        public double airtemp { get; set; }// = new double[PlugIn.ModelCore.Ecoregions.Count];

        [DataFieldAttribute(Unit = FiledUnits.g_C_m_2, Desc = "...", Format = "{1:0.0}")]
        public double avgNPPtc { get; set; }// = new double[PlugIn.ModelCore.Ecoregions.Count];

        [DataFieldAttribute(Unit = FiledUnits.g_C_m_2, Desc = "...", Format = "{1:0.0}")]
        public double avgResp { get; set; }// = new double[PlugIn.ModelCore.Ecoregions.Count];

        [DataFieldAttribute(Unit = FiledUnits.g_C_m_2, Desc = "...", Format = "{1:0.0}")]
        public double avgNEE { get; set; }// = new double[PlugIn.ModelCore.Ecoregions.Count];

        [DataFieldAttribute(Unit = FiledUnits.g_C_m_2, Desc = "...", Format = "{1:0.0}")]
        public double Ndep { get; set; }// = new double[PlugIn.ModelCore.Ecoregions.Count];
    }
}
