//  Copyright 2007 Conservation Biology Institute
//  Authors:  Robert M. Scheller
//  License:  Available at  
//  http://www.landis-ii.org/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Util;
using System;
using System.Collections.Generic;

using Landis.Ecoregions;
using Landis.Landscape;
using Landis.Species;
using Landis.Library.Climate;

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
        //public static Ecoregions.AuxParm<double> MonthlyNDeposition;
        public static Ecoregions.AuxParm<double> AtmosNslope;
        public static Ecoregions.AuxParm<double> AtmosNintercept;
        public static Ecoregions.AuxParm<double> Latitude;
        public static Ecoregions.AuxParm<double> ActiveSiteCount;
        public static Ecoregions.AuxParm<Percentage>[] ShadeBiomass;
        public static Ecoregions.AuxParm<int> B_MAX;
        
        // AnnualClimateArray contains climates for N years whereby N is the succession time step.
        // AnnualClimate is the active (current) year's climate, one of the elements in AnnualClimateArray.
        public static Ecoregions.AuxParm<AnnualClimate[]> AnnualClimateArray;
        public static Ecoregions.AuxParm<AnnualClimate> AnnualWeather;
        public static Ecoregions.AuxParm<bool[]> ClimateUpdates;

        //---------------------------------------------------------------------
        public static void Initialize(IParameters parameters)
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
            ShadeBiomass        = parameters.MinRelativeBiomass; 
    
            ActiveSiteCount = new Ecoregions.AuxParm<double>(Model.Core.Ecoregions);
            ClimateUpdates  = new Ecoregions.AuxParm<bool[]>(Model.Core.Ecoregions);
            
            foreach (ActiveSite site in Model.Core.Landscape)
            {
                IEcoregion ecoregion = Model.Core.Ecoregion[site];
                
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
            
            AnnualWeather = new Ecoregions.AuxParm<AnnualClimate>(Model.Core.Ecoregions);
            foreach(IEcoregion ecoregion in Model.Core.Ecoregions) 
                if(ActiveSiteCount[ecoregion] > 0)
                {
                    SetAnnualClimate(ecoregion, 0);
                    ClimateUpdates[ecoregion] = new bool[Model.Core.EndTime + parameters.Timestep + 1];
                    ClimateUpdates[ecoregion][0] = true;
                }
            
        }
        //---------------------------------------------------------------------
        public static void ChangeParameters(Dynamic.IParameters parameters)
        {
            B_MAX               = new Ecoregions.AuxParm<int>(Model.Core.Ecoregions);
            
            //  Fill in B_MAX array
            foreach (IEcoregion ecoregion in Model.Core.Ecoregions) 
            {
                if(ActiveSiteCount[ecoregion] > 0)
                {
                    int largest_B_MAX_Spp = 0;
                    foreach (ISpecies species in Model.Core.Species) 
                    {
                        largest_B_MAX_Spp = Math.Max(largest_B_MAX_Spp, SpeciesData.B_MAX_Spp[species][ecoregion]);
                        //UI.WriteLine("B_MAX={0}. species={1}, ecoregion={2}", largest_B_MAX_Spp, species.Name, ecoregion.Name);
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
            int actualYear = Model.Core.CurrentTime + year;
            
            if(actualYear == 0 || actualYear != AnnualWeather[ecoregion].Year)
            {
                //UI.WriteLine("  SETTING ANNAUL CLIMATE:  Yr={0}, SimYr={1}, Eco={2}.", year, actualYear, ecoregion.Name);
                AnnualWeather[ecoregion] = AnnualClimateArray[ecoregion][year];
                AnnualWeather[ecoregion].SetAnnualN(EcoregionData.AtmosNslope[ecoregion], EcoregionData.AtmosNintercept[ecoregion]);

                string weatherWrite = AnnualWeather[ecoregion].Write();
                //UI.WriteLine("{0}", weatherWrite);
            }
        }

        

        //---------------------------------------------------------------------
        // Generates new climate parameters at an annual time step.
        // 
        public static void GenerateNewClimate(int year, int years)
        {
        
            //UI.WriteLine("   Generating new climate for simulation year {0}.", year);

            AnnualClimateArray = new Ecoregions.AuxParm<AnnualClimate[]>(Model.Core.Ecoregions);
            
            // Issues with this approach:  Each ecoregion will have unique variability associated with 
            // temperature and precipitation.  In reality, we expect some regional synchronicity.  An 
            // easy-ish solution would be to use the same random number in combination with standard 
            // deviations for all ecoregions.  The converse problem is over synchronization of climate, but
            // that would certainly be preferrable over smaller regions.
            
            
            
            foreach(IEcoregion ecoregion in Model.Core.Ecoregions) 
            {
                if(ActiveSiteCount[ecoregion] > 0)
                {            
                    AnnualClimate[] tempClimate = new AnnualClimate[years];
            
                    for (int y = 0; y < years; y++)
                    {
                
                        int actualYear = year + y;
            
                        if(Climate.AllData.ContainsKey(actualYear))
                        {
                            Climate.TimestepData = Climate.AllData[actualYear];
                            //UI.WriteLine("  Changing TimestepData:  Yr={0}, Eco={1}.", actualYear, ecoregion.Name);
                            //UI.WriteLine("  Changing TimestepData:  AllData  Jan Ppt = {0:0.00}.", Climate.AllData[actualYear][ecoregion.Index,0].AvgPpt);
                            //UI.WriteLine("  Changing TimestepData:  Timestep Jan Ppt = {0:0.00}.", Climate.TimestepData[ecoregion.Index,0].AvgPpt);
                        }
                    
                        tempClimate[y] = new AnnualClimate(ecoregion.Index, actualYear, Latitude[ecoregion]); 
                    
                    }
                
                    AnnualClimateArray[ecoregion] = tempClimate;
                }
            }
        }
        
    }
}
