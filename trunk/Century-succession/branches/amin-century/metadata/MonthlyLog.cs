using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Landis.Library.Metadata;

namespace Landis.Extension.Succession.Century
{
    public class MonthlyLog
    {
        [DataFieldAttribute(Unit = FiledUnits.Year, Desc = "Simulation Year")]
        public int Time {set; get;}

        [DataFieldAttribute(Unit = FiledUnits.Month, Desc = "Simulation Month")]
        public int Month { set; get; }

        [DataFieldAttribute(Desc = "Ecoregion Name")]
        public string EcoregionName { set; get; }

        [DataFieldAttribute(Unit = FiledUnits.Count, Desc = "Number of Sites")]
        public int NumSites { set; get; }

        [DataFieldAttribute(Unit = FiledUnits.cm, Desc = "Precipitation", Format = "0.0")]
        public double ppt {get; set;}

        [DataFieldAttribute(Unit = FiledUnits.DegreeC, Desc = "Air Temperature", Format = "0.0")]
        public double airtemp { get; set; }

        [DataFieldAttribute(Unit = FiledUnits.g_C_m2, Desc = "Aboveground NPP C", Format = "0.0")]
        public double avgNPPtc { get; set; }

        [DataFieldAttribute(Unit = FiledUnits.g_C_m2, Desc = "Aboveground Heterotrophic Respiration", Format = "0.0")]
        public double avgResp { get; set; }

        [DataFieldAttribute(Unit = FiledUnits.g_C_m2, Desc = "Net Ecosystem Exchange", Format = "0.0")]
        public double avgNEE { get; set; }

        [DataFieldAttribute(Unit = FiledUnits.g_C_m2, Desc = "N Deposition", Format = "0.0")]
        public double Ndep { get; set; }
    }
}
