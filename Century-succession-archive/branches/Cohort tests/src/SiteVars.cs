//  Copyright 2007-2010 Portland State University, University of Wisconsin-Madison
//  Author: Robert Scheller, Ben Sulman

using Landis.Core;
using Landis.SpatialModeling;
using Edu.Wisc.Forest.Flel.Util;
using Landis.Library.Succession;
using Landis.Library.LeafBiomassCohorts;  
using System.Collections.Generic;
using System;
using Landis.Library.Biomass;

namespace Landis.Extension.Succession.Century
{
    /// <summary>
    /// The pools of dead biomass for the landscape's sites.
    /// </summary>
    public static class SiteVars
    {
        // Time of last succession simulation:
        private static ISiteVar<int> timeOfLast;
        
        // Live biomass:        
        private static ISiteVar<Landis.Library.AgeOnlyCohorts.ISiteCohorts> baseCohortsSiteVar;
        private static ISiteVar<Landis.Library.BiomassCohorts.ISiteCohorts> biomassCohortsSiteVar;

        
        // Dead biomass:
        private static ISiteVar<Layer> surfaceDeadWood;
        private static ISiteVar<Layer> soilDeadWood;
        
        private static ISiteVar<Layer> surfaceStructural;
        private static ISiteVar<Layer> surfaceMetabolic;
        private static ISiteVar<Layer> soilStructural;
        private static ISiteVar<Layer> soilMetabolic;
                       
        // Soil layers
        private static ISiteVar<Layer> som1surface;
        private static ISiteVar<Layer> som1soil;
        private static ISiteVar<Layer> som2;
        private static ISiteVar<Layer> som3;
        
        // Similar to soil layers with respect to their pools:
        private static ISiteVar<Layer> stream;
        private static ISiteVar<Layer> sourceSink;
        
        // Other variables:
        private static ISiteVar<double> mineralN;
        private static ISiteVar<double> resorbedN;
        private static ISiteVar<double> waterMovement;  
        private static ISiteVar<double> availableWater;  
        private static ISiteVar<double> soilWaterContent;  
        private static ISiteVar<double> decayFactor;
        private static ISiteVar<double> soilTemperature;
        private static ISiteVar<double> anaerobicEffect;
        
        // Annual accumulators
        private static ISiteVar<double> grossMineralization;
        private static ISiteVar<double> ag_nppC;
        private static ISiteVar<double> bg_nppC;
        private static ISiteVar<double> litterfallC;
        private static ISiteVar<double> cohortLeafN;
        private static ISiteVar<double> cohortLeafC;
        private static ISiteVar<double> cohortWoodN;
        private static ISiteVar<double> cohortWoodC;
        private static ISiteVar<double[]> monthlyAGNPPC;
        private static ISiteVar<double[]> monthlyBGNPPC;
        private static ISiteVar<double[]> monthlyNEE;
        private static ISiteVar<double[]> monthlySoilWaterContent;
        public static ISiteVar<double> AnnualNEE;
        public static ISiteVar<double> FireCEfflux;
        public static ISiteVar<double> FireNEfflux;
        public static ISiteVar<double> Nvol;
        private static ISiteVar<double[]> monthlyResp;
        private static ISiteVar<double> totalNuptake;
        private static ISiteVar<double[]> monthlymineralN;
        private static ISiteVar<double> frassC;
        private static ISiteVar<double> lai;
                
        public static ISiteVar<double> TotalWoodBiomass;
        public static ISiteVar<int> PrevYearMortality;
        public static ISiteVar<byte> FireSeverity;
        public static ISiteVar<double> AgeMortality;
        public static ISiteVar<string> HarvestPrescriptionName;

        
        //---------------------------------------------------------------------

        /// <summary>
        /// Initializes the module.
        /// </summary>
        public static void Initialize()
        {
            cohorts = PlugIn.ModelCore.Landscape.NewSiteVar<Library.LeafBiomassCohorts.SiteCohorts>();
            biomassCohortsSiteVar = Landis.Library.Succession.CohortSiteVar<Landis.Library.BiomassCohorts.ISiteCohorts>.Wrap(cohorts);
            baseCohortsSiteVar = Landis.Library.Succession.CohortSiteVar<Landis.Library.AgeOnlyCohorts.ISiteCohorts>.Wrap(cohorts);
           
            timeOfLast = PlugIn.ModelCore.Landscape.NewSiteVar<int>();
            
            // Dead biomass:
            surfaceDeadWood     = PlugIn.ModelCore.Landscape.NewSiteVar<Layer>();
            soilDeadWood        = PlugIn.ModelCore.Landscape.NewSiteVar<Layer>();
            
            surfaceStructural   = PlugIn.ModelCore.Landscape.NewSiteVar<Layer>();
            surfaceMetabolic    = PlugIn.ModelCore.Landscape.NewSiteVar<Layer>();
            soilStructural      = PlugIn.ModelCore.Landscape.NewSiteVar<Layer>();
            soilMetabolic       = PlugIn.ModelCore.Landscape.NewSiteVar<Layer>();
            
            // Soil Layers
            som1surface         = PlugIn.ModelCore.Landscape.NewSiteVar<Layer>();
            som1soil            = PlugIn.ModelCore.Landscape.NewSiteVar<Layer>();
            som2                = PlugIn.ModelCore.Landscape.NewSiteVar<Layer>();
            som3                = PlugIn.ModelCore.Landscape.NewSiteVar<Layer>();
            
            // Other Layers
            stream              = PlugIn.ModelCore.Landscape.NewSiteVar<Layer>();
            sourceSink          = PlugIn.ModelCore.Landscape.NewSiteVar<Layer>();
            
            // Other variables
            mineralN            = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            resorbedN           = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            waterMovement       = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            availableWater      = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            soilWaterContent    = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            decayFactor         = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            soilTemperature     = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            anaerobicEffect     = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            
            // Annual accumulators
            grossMineralization = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            ag_nppC             = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            bg_nppC             = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            litterfallC         = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            monthlyAGNPPC       = PlugIn.ModelCore.Landscape.NewSiteVar<double[]>();
            monthlyBGNPPC       = PlugIn.ModelCore.Landscape.NewSiteVar<double[]>();
            monthlyNEE          = PlugIn.ModelCore.Landscape.NewSiteVar<double[]>();
            monthlySoilWaterContent = PlugIn.ModelCore.Landscape.NewSiteVar<double[]>();
            AnnualNEE           = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            FireCEfflux         = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            FireNEfflux         = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            monthlyResp         = PlugIn.ModelCore.Landscape.NewSiteVar<double[]>();

            cohortLeafN         = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            cohortLeafC         = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            cohortWoodN         = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            cohortWoodC         = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
                        
            TotalWoodBiomass    = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            AgeMortality        = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            Nvol                = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            PrevYearMortality   = PlugIn.ModelCore.Landscape.NewSiteVar<int>();
            totalNuptake        = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            monthlymineralN     = PlugIn.ModelCore.Landscape.NewSiteVar<double[]>();
            frassC              = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            lai                 = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            HarvestPrescriptionName = PlugIn.ModelCore.GetSiteVar<string>("Harvest.PrescriptionName");
            
                       

            PlugIn.ModelCore.RegisterSiteVar(cohorts, "Succession.LeafBiomassCohorts");
            PlugIn.ModelCore.RegisterSiteVar(baseCohortsSiteVar, "Succession.AgeCohorts");
            PlugIn.ModelCore.RegisterSiteVar(biomassCohortsSiteVar  , "Succession.BiomassCohorts");

            
            foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
            {
                
                surfaceDeadWood[site]       = new Layer(LayerName.Wood, LayerType.Surface);
                soilDeadWood[site]          = new Layer(LayerName.CoarseRoot, LayerType.Soil);
                
                surfaceStructural[site]     = new Layer(LayerName.Structural, LayerType.Surface);
                surfaceMetabolic[site]      = new Layer(LayerName.Metabolic, LayerType.Surface);
                soilStructural[site]        = new Layer(LayerName.Structural, LayerType.Soil);
                soilMetabolic[site]         = new Layer(LayerName.Metabolic, LayerType.Soil);
                
                som1surface[site]           = new Layer(LayerName.SOM1, LayerType.Surface);
                som1soil[site]              = new Layer(LayerName.SOM1, LayerType.Soil);
                
                som2[site]                  = new Layer(LayerName.SOM2, LayerType.Soil);
                som3[site]                  = new Layer(LayerName.SOM3, LayerType.Soil);
                
                stream[site]                = new Layer(LayerName.Other, LayerType.Other);
                sourceSink[site]            = new Layer(LayerName.Other, LayerType.Other);
                
                monthlyAGNPPC[site]           = new double[12];
                monthlyBGNPPC[site]           = new double[12];
                monthlyNEE[site]            = new double[12];
                monthlyResp[site]           = new double[12];
                monthlySoilWaterContent[site]       = new double[12];

                AvailableN.CohortResorbedNallocation = new Dictionary<int, Dictionary<int, double>>();
            }
            
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// Initializes for disturbances.
        /// </summary>
        public static void InitializeDisturbances()
        {
            FireSeverity        = PlugIn.ModelCore.GetSiteVar<byte>("Fire.Severity");
            HarvestPrescriptionName = PlugIn.ModelCore.GetSiteVar<string>("Harvest.PrescriptionName");

            //if(FireSeverity == null)
            //    throw new System.ApplicationException("TEST Error: Fire Severity NOT Initialized.");
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Biomass cohorts at each site.
        /// </summary>
        private static ISiteVar<SiteCohorts> cohorts;
        public static ISiteVar<SiteCohorts> Cohorts
        {
            get
            {
                return cohorts;
            }
            set
            {
                cohorts = value;
            }
        }
       
        //---------------------------------------------------------------------

        /// <summary>
        /// Computes the actual biomass at a site.  The biomass is the total
        /// of all the site's cohorts except young ones.  The total is limited
        /// to being no more than the site's maximum biomass less the previous
        /// year's mortality at the site.
        /// </summary>
        public static double ActualSiteBiomass(ActiveSite site)
        {
            IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];
            ISiteCohorts siteCohorts = SiteVars.Cohorts[site];

            if(siteCohorts == null)
                return 0.0;
            
            int youngBiomass;
            int totalBiomass = Library.LeafBiomassCohorts.Cohorts.ComputeBiomass(siteCohorts, out youngBiomass);
            double B_ACT = totalBiomass - youngBiomass;

            int lastMortality = SiteVars.PrevYearMortality[site];
            B_ACT = System.Math.Min(EcoregionData.B_MAX[ecoregion] - lastMortality, B_ACT);

            return B_ACT;
        }
        
        //---------------------------------------------------------------------
        public static void ResetAnnualValues(Site site)
        {
            
            // Reset these accumulators to zero:
            SiteVars.CohortLeafN[site] = 0.0;
            SiteVars.CohortLeafC[site] = 0.0;
            SiteVars.CohortWoodN[site] = 0.0;
            SiteVars.CohortWoodC[site] = 0.0;
            SiteVars.GrossMineralization[site] = 0.0;
            SiteVars.AGNPPcarbon[site] = 0.0;
            SiteVars.BGNPPcarbon[site] = 0.0;
            SiteVars.LitterfallC[site] = 0.0;
            
            SiteVars.Stream[site]          = new Layer(LayerName.Other, LayerType.Other);
            SiteVars.SourceSink[site]      = new Layer(LayerName.Other, LayerType.Other);
            
            SiteVars.SurfaceDeadWood[site].NetMineralization = 0.0;
            SiteVars.SurfaceStructural[site].NetMineralization = 0.0;
            SiteVars.SurfaceMetabolic[site].NetMineralization = 0.0;
            
            SiteVars.SoilDeadWood[site].NetMineralization = 0.0;
            SiteVars.SoilStructural[site].NetMineralization = 0.0;
            SiteVars.SoilMetabolic[site].NetMineralization = 0.0;
            
            SiteVars.SOM1surface[site].NetMineralization = 0.0;
            SiteVars.SOM1soil[site].NetMineralization = 0.0;
            SiteVars.SOM2[site].NetMineralization = 0.0;
            SiteVars.SOM3[site].NetMineralization = 0.0;
            SiteVars.AnnualNEE[site] = 0.0;
            SiteVars.Nvol[site] = 0.0;
            SiteVars.AnnualNEE[site] = 0.0;
            SiteVars.TotalNuptake[site] = 0.0;
            SiteVars.ResorbedN[site] = 0.0;
            SiteVars.FrassC[site] = 0.0;
            SiteVars.LAI[site] = 0.0;
            SiteVars.AgeMortality[site] = 0.0;

            //SiteVars.FireEfflux[site] = 0.0;
                        

        }

        //---------------------------------------------------------------------

        public static ISiteVar<int> TimeOfLast
        {
            get {
                return timeOfLast;
            }
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// The intact dead woody pools for the landscape's sites.
        /// </summary>
        public static ISiteVar<Layer> SurfaceDeadWood
        {
            get {
                return surfaceDeadWood;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The DEAD coarse root pool for the landscape's sites.
        /// </summary>
        public static ISiteVar<Layer> SoilDeadWood
        {
            get {
                return soilDeadWood;
            }
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// The dead surface pool for the landscape's sites.
        /// </summary>
        public static ISiteVar<Layer> SurfaceStructural
        {
            get {
                return surfaceStructural;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The dead surface pool for the landscape's sites.
        /// </summary>
        public static ISiteVar<Layer> SurfaceMetabolic
        {
            get {
                return surfaceMetabolic;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The fine root pool for the landscape's sites.
        /// </summary>
        public static ISiteVar<Layer> SoilStructural
        {
            get {
                return soilStructural;
            }
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// The fine root pool for the landscape's sites.
        /// </summary>
        public static ISiteVar<Layer> SoilMetabolic
        {
            get {
                return soilMetabolic;
            }
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// The soil organic matter (SOM1-Surface) for the landscape's sites.
        /// </summary>
        public static ISiteVar<Layer> SOM1surface
        {
            get {
                return som1surface;
            }
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// The soil organic matter (SOM1-Soil) for the landscape's sites.
        /// </summary>
        public static ISiteVar<Layer> SOM1soil
        {
            get {
                return som1soil;
            }
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// The soil organic matter (SOM2) for the landscape's sites.
        /// </summary>
        public static ISiteVar<Layer> SOM2
        {
            get {
                return som2;
            }
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// The soil organic matter (SOM3) for the landscape's sites.
        /// </summary>
        public static ISiteVar<Layer> SOM3
        {
            get {
                return som3;
            }
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// Leaching to a stream - using the soil layer object is a cheat
        /// </summary>
        public static ISiteVar<Layer> Stream
        {
            get {
                return stream;
            }
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// Water loss
        /// </summary>
        public static ISiteVar<double> WaterMovement
        {
            get {
                return waterMovement;
            }
            set {
                waterMovement = value;
            }
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// Water loss
        /// </summary>
        public static ISiteVar<double> AvailableWater
        {
            get {
                return availableWater;
            }
            set {
                availableWater = value;
            }
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// Water loss
        /// </summary>
        public static ISiteVar<double> SoilWaterContent
        {
            get {
                return soilWaterContent;
            }
            set {
                soilWaterContent = value;
            }
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// Available mineral Nitrogen
        /// </summary>
        public static ISiteVar<double> MineralN
        {
            get {
                return mineralN;
            }
            set {
                mineralN = value;
            }
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// The amount of N resorbed before leaf fall
        /// </summary>
        public static ISiteVar<double> ResorbedN
        {
            get
            {
                return resorbedN;
            }
            set
            {
                resorbedN = value;
            }
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// A generic decay factor determined by soil water and soil temperature.
        /// </summary>
        public static ISiteVar<double> DecayFactor
        {
            get {
                return decayFactor;
            }
            set {
                decayFactor = value;
            }
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// Soil temperature (C)
        /// </summary>
        public static ISiteVar<double> SoilTemperature
        {
            get {
                return soilTemperature;
            }
            set {
                soilTemperature = value;
            }
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// A generic decay factor determined by soil water and soil temperature.
        /// </summary>
        public static ISiteVar<double> AnaerobicEffect
        {
            get {
                return anaerobicEffect;
            }
            set {
                anaerobicEffect = value;
            }
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// A summary of all Nitrogen in the Cohorts.
        /// </summary>
        public static ISiteVar<double> CohortLeafN
        {
            get {
                return cohortLeafN;
            }
            set {
                cohortLeafN = value;
            }
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// A summary of all Carbon in the Cohorts.
        /// </summary>
        public static ISiteVar<double> CohortLeafC
        {
            get {
                return cohortLeafC;
            }
            set {
                cohortLeafC = value;
            }
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// A summary of all Nitrogen in the Cohorts.
        /// </summary>
        public static ISiteVar<double> CohortWoodN
        {
            get {
                return cohortWoodN;
            }
            set {
                cohortWoodN = value;
            }
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// A summary of all Carbon in the Cohorts.
        /// </summary>
        public static ISiteVar<double> CohortWoodC
        {
            get {
                return cohortWoodC;
            }
            set {
                cohortWoodC = value;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// A summary of Gross Mineraliztion.
        /// </summary>
        public static ISiteVar<double> GrossMineralization
        {
            get {
                return grossMineralization;
            }
            set {
                grossMineralization = value;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// A summary of Aboveground Net Primary Productivity (g C/m2)
        /// </summary>
        public static ISiteVar<double> AGNPPcarbon
        {
            get {
                return ag_nppC;
            }
        }
        
        //---------------------------------------------------------------------
        /// <summary>
        /// A summary of Belowground Net Primary Productivity (g C/m2)
        /// </summary>
        public static ISiteVar<double> BGNPPcarbon
        {
            get {
                return bg_nppC;
            }
        }

        
        
        
        //---------------------------------------------------------------------
        /// <summary>
        /// A summary of Litter fall (g C/m2).
        /// </summary>
        public static ISiteVar<double> LitterfallC
        {
            get {
                return litterfallC;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// A summary of Aboveground Net Primary Productivity (g C/m2)
        /// </summary>
        public static ISiteVar<double[]> MonthlyAGNPPcarbon
        {
            get {
                return monthlyAGNPPC;
            }
            set {
                monthlyAGNPPC = value;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// A summary of Belowground Net Primary Productivity (g C/m2)
        /// </summary>
        public static ISiteVar<double[]> MonthlyBGNPPcarbon
        {
            get {
                return monthlyBGNPPC;
            }
            set {
                monthlyBGNPPC = value;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// A summary of heterotrophic respiration, i.e. CO2 loss from decomposition (g C/m2)
        /// </summary>
        public static ISiteVar<double[]> MonthlyResp
        {
            get {
                return monthlyResp;
            }
            set {
                monthlyResp = value;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// A summary of Net Ecosystem Exchange (g C/m2), from a flux tower's perspective,
        /// whereby positive values indicate terrestrial C loss, negative values indicate C gain.
        /// Replace SourceSink?
        /// </summary>
        public static ISiteVar<double[]> MonthlyNEE
        {
            get {
                return monthlyNEE;
            }
            set {
                monthlyNEE = value;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// A summary of Net Ecosystem Exchange (g C/m2), from a flux tower's perspective,
        /// whereby positive values indicate terrestrial C loss, negative values indicate C gain.
        /// Replace SourceSink?
        /// </summary>
        public static ISiteVar<double[]> MonthlySoilWaterContent
        {
            get
            {
                return monthlySoilWaterContent;
            }
            set
            {
                monthlySoilWaterContent = value;
            }
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// Water loss
        /// </summary>
        public static ISiteVar<Layer> SourceSink
        {
            get {
                return sourceSink;
            }
            set {
                sourceSink = value;
            }
        }
        //---------------------------------------------------------------------
               /// <summary>
        /// A summary of N uptake (g N/m2)
        /// </summary>
        public static ISiteVar<double> TotalNuptake
        {
            get
            {
                return totalNuptake;
            }
            set 
            {
                totalNuptake = value;
            }
                
            
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// A summary of frass deposition (g C/m2)
        /// </summary>
        public static ISiteVar<double> FrassC
        {
            get
            {
                return frassC;
            }
            set
            {
                frassC = value;
            }


        }
        //---------------------------------------------------------------------
        /// <summary>
        /// A summary of LAI (m2/m2)
        /// </summary>
        public static ISiteVar<double> LAI
        {
            get
            {
                return lai;
            }
            set
            {
                lai = value;
            }


        }
    }
}
 
