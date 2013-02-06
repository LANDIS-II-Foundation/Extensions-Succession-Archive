//  Copyright 2007-2010 Portland State University, University of Wisconsin-Madison
//  Author: Robert Scheller, Ben Sulman

using Landis.Core;
using Landis.SpatialModeling;
using Edu.Wisc.Forest.Flel.Util;
using Landis.Library.Succession;
using Landis.Library.Climate;

using System.Collections.Generic;
using System;


namespace Landis.Extension.Succession.Century
{
    public class EcoregionData
    {

        //user-defined by ecoregion
        public static Ecoregions.AuxParm<double> PercentClay;  
        public static Ecoregions.AuxParm<double> PercentSand;  
        public static Ecoregions.AuxParm<int>    SoilDepth;
        public static Ecoregions.AuxParm<double> FieldCapacity;
        public static Ecoregions.AuxParm<double> WiltingPoint;
        public static Ecoregions.AuxParm<double> StormFlowFraction;
        public static Ecoregions.AuxParm<double> BaseFlowFraction;
        public static Ecoregions.AuxParm<double> Drain;
        public static Ecoregions.AuxParm<double> AtmosNslope;
        public static Ecoregions.AuxParm<double> AtmosNintercept;
        public static Ecoregions.AuxParm<double> Latitude;
        public static Ecoregions.AuxParm<double> DecayRateSurf; 
        public static Ecoregions.AuxParm<double> DecayRateSOM1;
        public static Ecoregions.AuxParm<double> DecayRateSOM2;
        public static Ecoregions.AuxParm<double> DecayRateSOM3;
        public static Ecoregions.AuxParm<double> Denitrif;
        public static Ecoregions.AuxParm<double> ActiveSiteCount;
        public static Ecoregions.AuxParm<Percentage>[] ShadeBiomass;
        public static Ecoregions.AuxParm<int> B_MAX;
        
        // AnnualClimateArray contains climates for N years whereby N is the succession time step.
        // AnnualClimate is the active (current) year's climate, one of the elements in AnnualClimateArray.
        public static Ecoregions.AuxParm<AnnualClimate[]> AnnualClimateArray;
        public static Ecoregions.AuxParm<AnnualClimate> AnnualWeather;
        public static Ecoregions.AuxParm<bool[]> ClimateUpdates;

        //---------------------------------------------------------------------
        public static void Initialize(IInputParameters parameters)
        {
        
            PercentClay         = parameters.PercentClay; 
            PercentSand         = parameters.PercentSand; 
            SoilDepth           = parameters.SoilDepth;
            FieldCapacity       = parameters.FieldCapacity;
            WiltingPoint        = parameters.WiltingPoint;
            StormFlowFraction   = parameters.StormFlowFraction;
            BaseFlowFraction    = parameters.BaseFlowFraction;
            Drain               = parameters.Drain;
            //MonthlyNDeposition = parameters.MonthlyNDeposition;
            AtmosNslope         = parameters.AtmosNslope;
            AtmosNintercept     = parameters.AtmosNintercept;
            Latitude            = parameters.Latitude;
            DecayRateSurf       = parameters.DecayRateSurf;
            DecayRateSOM1       = parameters.DecayRateSOM1;
            DecayRateSOM2       = parameters.DecayRateSOM2;
            DecayRateSOM3       = parameters.DecayRateSOM3;
            Denitrif             = parameters.Denitrif;
            
            ShadeBiomass = parameters.MinRelativeBiomass;
    
            ActiveSiteCount = new Ecoregions.AuxParm<double>(PlugIn.ModelCore.Ecoregions);
            ClimateUpdates  = new Ecoregions.AuxParm<bool[]>(PlugIn.ModelCore.Ecoregions);
            
            foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
            {
                IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];
                //PlugIn.ModelCore.UI.WriteLine("Latitude for {0} = {1}.", ecoregion.Name, parameters.Latitude[ecoregion]);
                
                SiteVars.SOM1surface[site].Carbon     = parameters.InitialSOM1surfC[ecoregion];
                SiteVars.SOM1surface[site].Nitrogen   = parameters.InitialSOM1surfN[ecoregion];

                SiteVars.SOM1soil[site].Carbon          = parameters.InitialSOM1soilC[ecoregion];
                SiteVars.SOM1soil[site].Nitrogen        = parameters.InitialSOM1soilN[ecoregion];

                SiteVars.SOM2[site].Carbon              = parameters.InitialSOM2C[ecoregion];
                SiteVars.SOM2[site].Nitrogen            = parameters.InitialSOM2N[ecoregion];
                
                SiteVars.SOM3[site].Carbon              = parameters.InitialSOM3C[ecoregion];
                SiteVars.SOM3[site].Nitrogen            = parameters.InitialSOM3N[ecoregion];
                
                SiteVars.MineralN[site]               = parameters.InitialMineralN[ecoregion];
                
                ActiveSiteCount[ecoregion]++;
            }


            GenerateNewClimate(0, parameters.Timestep);
            
            AnnualWeather = new Ecoregions.AuxParm<AnnualClimate>(PlugIn.ModelCore.Ecoregions);
            foreach(IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions) 
                if(ecoregion.Active)
                {
                    SetAnnualClimate(ecoregion, 0);
                    ClimateUpdates[ecoregion] = new bool[PlugIn.ModelCore.EndTime + parameters.Timestep + 1];
                    ClimateUpdates[ecoregion][0] = true;
                }
            
        }
        //---------------------------------------------------------------------
        public static void ChangeParameters(Dynamic.IParameters parameters)
        {
            B_MAX               = new Ecoregions.AuxParm<int>(PlugIn.ModelCore.Ecoregions);
            
            //  Fill in B_MAX array
            foreach (IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions) 
            {
                if(ecoregion.Active)
                {
                    int largest_B_MAX_Spp = 0;
                    foreach (ISpecies species in PlugIn.ModelCore.Species) 
                    {
                        largest_B_MAX_Spp = Math.Max(largest_B_MAX_Spp, SpeciesData.B_MAX_Spp[species][ecoregion]);
                        //PlugIn.ModelCore.UI.WriteLine("B_MAX={0}. species={1}, ecoregion={2}", largest_B_MAX_Spp, species.Name, ecoregion.Name);
                    }
                    B_MAX[ecoregion] = largest_B_MAX_Spp;
                }
            }
         
        }

        //---------------------------------------------------------------------
        // Generates new climate parameters at an annual time step.
        // Note:  During the spin-up phase of growth, the same annual climates will
        // be used repeatedly in order.
        public static void SetAnnualClimate(IEcoregion ecoregion, int year)
        {
            int actualYear = PlugIn.ModelCore.CurrentTime + year;
            
            if(actualYear == 0 || actualYear != AnnualWeather[ecoregion].Year)
            {
                //PlugIn.ModelCore.UI.WriteLine("  SETTING ANNAUL CLIMATE:  Yr={0}, SimYr={1}, Eco={2}.", year, actualYear, ecoregion.Name);
                
                AnnualWeather[ecoregion] = AnnualClimateArray[ecoregion][year];
                AnnualWeather[ecoregion].SetAnnualN(EcoregionData.AtmosNslope[ecoregion], EcoregionData.AtmosNintercept[ecoregion]);

                string weatherWrite = AnnualWeather[ecoregion].Write();
                //PlugIn.ModelCore.UI.WriteLine("{0}", weatherWrite);
            }
        }

        

        //---------------------------------------------------------------------
        // Generates new climate parameters at an annual time step.
        // 
        public static void GenerateNewClimate(int year, int years)
        {
        
            //PlugIn.ModelCore.UI.WriteLine("   Generating new climate for simulation year {0}.", year);

            AnnualClimateArray = new Ecoregions.AuxParm<AnnualClimate[]>(PlugIn.ModelCore.Ecoregions);
            
            // Issues with this approach:  Each ecoregion will have unique variability associated with 
            // temperature and precipitation.  In reality, we expect some regional synchronicity.  An 
            // easy-ish solution would be to use the same random number in combination with standard 
            // deviations for all ecoregions.  The converse problem is over synchronization of climate, but
            // that would certainly be preferrable over smaller regions.
            
            
            
            foreach(IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions) 
            {
                if(ecoregion.Active)
                {            
                    AnnualClimate[] tempClimate = new AnnualClimate[years];
            
                    for (int y = 0; y < years; y++)
                    {
                
                        int actualYear = year + y;
            
                        if(Climate.AllData.ContainsKey(actualYear))
                        {
                            Climate.TimestepData = Climate.AllData[actualYear];
                            //PlugIn.ModelCore.UI.WriteLine("  Changing TimestepData:  Yr={0}, Eco={1}.", actualYear, ecoregion.Name);
                            //PlugIn.ModelCore.UI.WriteLine("  Changing TimestepData:  AllData  Jan Ppt = {0:0.00}.", Climate.AllData[actualYear][ecoregion.Index,0].AvgPpt);
                            //PlugIn.ModelCore.UI.WriteLine("  Changing TimestepData:  Timestep Jan Ppt = {0:0.00}.", Climate.TimestepData[ecoregion.Index,0].AvgPpt);
                        }

                        AnnualClimate.AnnualClimateInitialize();
                        tempClimate[y] = new AnnualClimate(ecoregion, actualYear, Latitude[ecoregion]); 
                    
                    }
                
                    AnnualClimateArray[ecoregion] = tempClimate;
                }
            }
        }
        
    }
}
