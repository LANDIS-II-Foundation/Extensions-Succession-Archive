using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Landis.Library.Metadata;

namespace Landis.Extension.Succession.Century
{
    public class PrimaryLog
    {
            //log.WriteLine("");

        [DataFieldAttribute(Unit = FieldUnits.Year, Desc = "Simulation Year")]
        public int Time {set; get;}

        [DataFieldAttribute(Desc = "...")]
        public string EcoregionName { set; get; }

        [DataFieldAttribute(Desc = "Ecoregion Index")]
        public int EcoregionIndex { set; get; }

        [DataFieldAttribute(Unit = FieldUnits.Count, Desc = "Number of Sites")]
        public int NumSites { set; get; }

        [DataFieldAttribute(Unit = FieldUnits.g_C_m2, Desc = "Net Ecosystem Exchange C", Format = "0.0")]
        public double NEEC {get; set;}

        [DataFieldAttribute(Unit = FieldUnits.g_C_m2, Desc = "Total Soil Organic Carbon", Format = "0.0")]
        public double SOMTC { get; set; }

        [DataFieldAttribute(Unit = FieldUnits.g_B_m2, Desc = "Aboveground Biomass", Format = "0.0")]
        public double AGB { get; set; }
        
        //log.Write("AG_NPPC, BG_NPPC, LitterfallC, AgeMortality, ");
        [DataFieldAttribute(Unit = FieldUnits.g_C_m2_yr1, Desc = "Aboveground NPP C", Format = "0.0")]
        public double AG_NPPC { get; set; }
        
        [DataFieldAttribute(Unit = FieldUnits.g_C_m2_yr1, Desc = "Below ground NPP C", Format = "0.0")]
        public double BG_NPPC { get; set; }
        
        [DataFieldAttribute(Unit = FieldUnits.g_C_m2_yr1, Desc = "Litterfall C", Format = "0.0")]
        public double Litterfall { get; set; }

        [DataFieldAttribute(Unit = FieldUnits.g_B_m2_yr1, Desc = "Age Mortality Biomass", Format = "0.0")]
        public double AgeMortality { get; set; }
        
        //log.Write("MineralN, TotalN, GrossMineralization, ");
        [DataFieldAttribute(Unit = FieldUnits.g_N_m2, Desc = "Mineral N", Format = "0.0")]
        public double MineralN { get; set; }
        
        [DataFieldAttribute(Unit = FieldUnits.g_N_m2, Desc = "Total N", Format = "0.0")]
        public double TotalN { get; set; }
        
        [DataFieldAttribute(Unit = FieldUnits.g_N_m2, Desc = "Gross Mineralization", Format = "0.0")]
        public double GrossMineralization { get; set; }

        //log.Write("C:LeafFRoot, C:WoodCRoot, C:DeadWood, C:DeadCRoot, ");
        [DataFieldAttribute(Unit = FieldUnits.g_C_m2, Desc = "Leaf and Fine Root C", Format = "0.0")]
        public double C_LeafFRoot { get; set; }

        [DataFieldAttribute(Unit = FieldUnits.g_C_m2, Desc = "Wood and Coarse Root C", Format = "0.0")]
        public double C_WoodCRoot { get; set; }

        [DataFieldAttribute(Unit = FieldUnits.g_C_m2, Desc = "Dead Wood C", Format = "0.0")]
        public double C_DeadWood { get; set; }

        [DataFieldAttribute(Unit = FieldUnits.g_C_m2, Desc = "Dead Coarse Root C", Format = "0.0")]
        public double C_DeadCRoot { get; set; }

        //log.Write("C:SurfStruc, C:SurfMeta, C:SoilStruc, C:SoilMeta, ");
        [DataFieldAttribute(Unit = FieldUnits.g_C_m2, Desc = "Surface Structural C", Format = "0.0")]
        public double C_SurfStruc { get; set; }

        [DataFieldAttribute(Unit = FieldUnits.g_C_m2, Desc = "Surface Metabolic C", Format = "0.0")]
        public double C_SurfMeta { get; set; }

        [DataFieldAttribute(Unit = FieldUnits.g_C_m2, Desc = "Soil Structural C", Format = "0.0")]
        public double C_SoilStruc { get; set; }

        [DataFieldAttribute(Unit = FieldUnits.g_C_m2, Desc = "Soil Metabolic C", Format = "0.0")]
        public double C_SoilMeta { get; set; }

        //log.Write("C:SOM1surf, C:SOM1soil, C:SOM2, C:SOM3, ");
        [DataFieldAttribute(Unit = FieldUnits.g_C_m2, Desc = "SOM1 Surface C", Format = "0.0")]
        public double C_SOM1surf { get; set; }

        [DataFieldAttribute(Unit = FieldUnits.g_C_m2, Desc = "SOM1 Soil C", Format = "0.0")]
        public double C_SOM1soil { get; set; }

        [DataFieldAttribute(Unit = FieldUnits.g_C_m2, Desc = "SOM2 C", Format = "0.0")]
        public double C_SOM2 { get; set; }

        [DataFieldAttribute(Unit = FieldUnits.g_C_m2, Desc = "SOM3 C", Format = "0.0")]
        public double C_SOM3 { get; set; }

        //log.Write("N:CohortLeaf, N:CohortWood, N:DeadWood, N:DeadRoot, ");
        [DataFieldAttribute(Unit = FieldUnits.g_N_m2, Desc = "Leaf and Fine Root N", Format = "0.0")]
        public double N_LeafFRoot { get; set; }

        [DataFieldAttribute(Unit = FieldUnits.g_N_m2, Desc = "Wood and Coarse Root N", Format = "0.0")]
        public double N_WoodCRoot { get; set; }

        [DataFieldAttribute(Unit = FieldUnits.g_N_m2, Desc = "Dead Wood N", Format = "0.0")]
        public double N_DeadWood { get; set; }

        [DataFieldAttribute(Unit = FieldUnits.g_N_m2, Desc = "Dead Coarse Root N", Format = "0.0")]
        public double N_DeadCRoot { get; set; }

        //log.Write("N:SurfStruc, N:SurfMeta, N:SoilStruc, N:SoilMeta, ");
        [DataFieldAttribute(Unit = FieldUnits.g_N_m2, Desc = "Surface Structural N", Format = "0.0")]
        public double N_SurfStruc { get; set; }

        [DataFieldAttribute(Unit = FieldUnits.g_N_m2, Desc = "Surface Metabolic N", Format = "0.0")]
        public double N_SurfMeta { get; set; }

        [DataFieldAttribute(Unit = FieldUnits.g_N_m2, Desc = "Soil Structural N", Format = "0.0")]
        public double N_SoilStruc { get; set; }

        [DataFieldAttribute(Unit = FieldUnits.g_N_m2, Desc = "Soil Metabolic N", Format = "0.0")]
        public double N_SoilMeta { get; set; }

        //log.Write("N:SOM1surf, N:SOM1soil, N:SOM2, N:SOM3, ");
        [DataFieldAttribute(Unit = FieldUnits.g_N_m2, Desc = "SOM1 Surface N", Format = "0.0")]
        public double N_SOM1surf { get; set; }

        [DataFieldAttribute(Unit = FieldUnits.g_N_m2, Desc = "SOM1 Soil N", Format = "0.0")]
        public double N_SOM1soil { get; set; }

        [DataFieldAttribute(Unit = FieldUnits.g_N_m2, Desc = "SOM2 N", Format = "0.0")]
        public double N_SOM2 { get; set; }

        [DataFieldAttribute(Unit = FieldUnits.g_N_m2, Desc = "SOM3 N", Format = "0.0")]
        public double N_SOM3 { get; set; }

        //log.Write("SurfStrucNetMin, SurfMetaNetMin, SoilStrucNetMin, SoilMetaNetMin, ");
        [DataFieldAttribute(Unit = FieldUnits.g_N_m2, Desc = "Surface Structural Net Mineralization", Format = "0.0")]
        public double SurfStrucNetMin { get; set; }

        [DataFieldAttribute(Unit = FieldUnits.g_N_m2, Desc = "Surface Metabolic Net Mineralization", Format = "0.0")]
        public double SurfMetaNetMin { get; set; }

        [DataFieldAttribute(Unit = FieldUnits.g_N_m2, Desc = "Soil Structural Net Mineralization", Format = "0.0")]
        public double SoilStrucNetMin { get; set; }

        [DataFieldAttribute(Unit = FieldUnits.g_N_m2, Desc = "Soil Metabolic Net Mineralization", Format = "0.0")]
        public double SoilMetaNetMin { get; set; }

        //log.Write("SOM1surfNetMin, SOM1soilNetMin, SOM2NetMin, SOM3NetMin, ");
        [DataFieldAttribute(Unit = FieldUnits.g_N_m2, Desc = "SOM1 Surface Net Mineralization", Format = "0.0")]
        public double SOM1surfNetMin { get; set; }

        [DataFieldAttribute(Unit = FieldUnits.g_N_m2, Desc = "SOM1 Soil Net Mineralization", Format = "0.0")]
        public double SOM1soilNetMin { get; set; }

        [DataFieldAttribute(Unit = FieldUnits.g_N_m2, Desc = "SOM2 Net Mineralization", Format = "0.0")]
        public double SOM2NetMin { get; set; }

        [DataFieldAttribute(Unit = FieldUnits.g_N_m2, Desc = "SOM3 Net Mineralization", Format = "0.0")]
        public double SOM3NetMin { get; set; }
        
        //log.Write("StreamC, StreamN, FireCEfflux, FireNEfflux, ");
        [DataFieldAttribute(Unit = FieldUnits.g_C, Desc = "Stream C", Format = "0.0")]
        public double StreamC { get; set; }

        [DataFieldAttribute(Unit = FieldUnits.g_N, Desc = "Stream N", Format = "0.0")]
        public double StreamN { get; set; }

        [DataFieldAttribute(Unit = FieldUnits.g_C_m2, Desc = "Fire C Efflux", Format = "0.0")]
        public double FireCEfflux { get; set; }

        [DataFieldAttribute(Unit = FieldUnits.g_N_m2, Desc = "Fire N Efflux", Format = "0.0")]
        public double FireNEfflux { get; set; }
        
        //log.Write("Nuptake, Nresorbed, TotalSoilN, Nvol, avgfrassC,");
        [DataFieldAttribute(Unit = FieldUnits.g_N_m2, Desc = "N Uptake", Format = "0.0")]
        public double Nuptake { get; set; }
        
        [DataFieldAttribute(Unit = FieldUnits.g_N_m2, Desc = "N Resorbed", Format = "0.0")]
        public double Nresorbed { get; set; }

        [DataFieldAttribute(Unit = FieldUnits.g_N_m2, Desc = "Total Soil N", Format = "0.0")]
        public double TotalSoilN { get; set; }

        [DataFieldAttribute(Unit = FieldUnits.g_N_m2, Desc = "N Volatalized", Format = "0.0")]
        public double Nvol { get; set; }

        [DataFieldAttribute(Unit = FieldUnits.g_C_m2, Desc = "Frass C", Format = "0.0")]
        public double FrassC { get; set; }

    }
}
