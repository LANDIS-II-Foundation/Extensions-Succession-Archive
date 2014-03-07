//  Copyright 2007-2010 Portland State University, University of Wisconsin-Madison
//  Author: Robert Scheller, Ben Sulman

using Landis.Core;
using Landis.SpatialModeling;
using Edu.Wisc.Forest.Flel.Util;
using Landis.Library.Succession;
using Landis.Library.Climate;
using System.Collections.Generic;
using System.Linq;
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
        public static Ecoregions.AuxParm<int> ActiveSiteCount;
        public static Ecoregions.AuxParm<Percentage>[] ShadeBiomass;
        public static Ecoregions.AuxParm<int> B_MAX;
        private static Ecoregions.AuxParm<int> LastYearUpdated;

        
        public static Ecoregions.AuxParm<AnnualClimate_Monthly> AnnualWeather;
        // AnnualClimateArray contains climates for N years whereby N is the succession time step.
        // AnnualClimate is the active (current) year's climate, one of the elements in AnnualClimateArray.
        //public static AnnualClimate_Monthly[] AnnualWeather;  //index by ecoregion
        //public static Ecoregions.AuxParm<bool[]> ClimateUpdates;

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
    
            ActiveSiteCount = new Ecoregions.AuxParm<int>(PlugIn.ModelCore.Ecoregions);
            LastYearUpdated = new Ecoregions.AuxParm<int>(PlugIn.ModelCore.Ecoregions);
            AnnualWeather = new Ecoregions.AuxParm<AnnualClimate_Monthly>(PlugIn.ModelCore.Ecoregions);
            //ClimateUpdates  = new Ecoregions.AuxParm<bool[]>(PlugIn.ModelCore.Ecoregions);
            
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

            
            //GenerateNewClimate(0, parameters.Timestep, Climate.Phase.SpinUp_Climate);
            //AnnualWeather = Climate.Future_MonthlyData[0];

            foreach (IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions)
            {
                if (ecoregion.Active)
                {
                    Climate.GenerateEcoregionClimateData(ecoregion, 0, Latitude[ecoregion], FieldCapacity[ecoregion], WiltingPoint[ecoregion]);
                    SetSingleAnnualClimate(ecoregion, 0, Climate.Phase.SpinUp_Climate);  // Some placeholder data to get things started.
                }
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
        // Generates new climate parameters for a SINGLE ECOREGION at an annual time step.
        public static void SetSingleAnnualClimate(IEcoregion ecoregion, int year, Climate.Phase spinupOrfuture)
        {
            int actualYear = PlugIn.ModelCore.CurrentTime + year;

            if (spinupOrfuture == Climate.Phase.Future_Climate)
            {
                actualYear += Climate.Future_MonthlyData.First().Key;
                //PlugIn.ModelCore.UI.WriteLine("Retrieving {0} for year {1}.", spinupOrfuture.ToString(), actualYear);
                if (Climate.Future_MonthlyData.ContainsKey(actualYear))
                {
                    AnnualWeather[ecoregion] = Climate.Future_MonthlyData[actualYear][ecoregion.Index];
                    //AnnualWeather[ecoregion].WriteToLandisLogFile();
                }
                //else
                //    PlugIn.ModelCore.UI.WriteLine("Key is missing: Retrieving {0} for year {1}.", spinupOrfuture.ToString(), actualYear);
            }
            else
            {
                if (LastYearUpdated[ecoregion] == year+1)
                    return;

                actualYear += Climate.Spinup_MonthlyData.First().Key;
                //PlugIn.ModelCore.UI.WriteLine("Retrieving {0} for year {1}.", spinupOrfuture.ToString(), actualYear);
                if (Climate.Spinup_MonthlyData.ContainsKey(actualYear))
                {
                    AnnualWeather[ecoregion] = Climate.Spinup_MonthlyData[actualYear][ecoregion.Index];
                    LastYearUpdated[ecoregion] = year+1;
                }
            }
            
        }

        //---------------------------------------------------------------------
        // Generates new climate parameters for a SINGLE ECOREGION at an annual time step.
        public static void SetAllFutureAnnualClimates(int year)
        {
            int actualYear = PlugIn.ModelCore.CurrentTime + year;

            foreach (IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions)
            {
                if (ecoregion.Active)
                {
                    actualYear += Climate.Future_MonthlyData.First().Key;
                    //PlugIn.ModelCore.UI.WriteLine("Retrieving {0} for year {1}.", spinupOrfuture.ToString(), actualYear);
                    if (Climate.Future_MonthlyData.ContainsKey(actualYear))
                    {
                        AnnualWeather[ecoregion] = Climate.Future_MonthlyData[actualYear][ecoregion.Index];
                        //AnnualWeather[ecoregion].WriteToLandisLogFile();
                    }
                }
            }
        }
        

        //***Amin's NOTE:***
        //this function has been preserved because of the extensive use of this function and specially EcoregionData.AnnualClimateArray which is filled out here in this function. However, 
        //since the "new AnnualClimate(...)" can provide the required climate for each ecoregion-timestep, the EcoregionData.AnnualClimateArray is no longer required and the "new AnnualClimate(...)" can be used instead.
        //The advantage of the "new AnnualClimate(...)" is that it does not requre to iterate in ecoregions and store their coresponding climates and it's running time is a constant time. 
        // Also, it is encapsulated in Climate library and is more maintainable and extendable.
        //---------------------------------------------------------------------
        // Generates new climate parameters at an annual time step.
        // 
        //public static void GenerateNewClimate(int startingTimestep, int timeStepCount, Climate.Phase spinupOrfuture) //Pass false for spin-up (historic)
        //{
        //    //PlugIn.ModelCore.UI.WriteLine("   Generating new climate for simulation year {0}.", year);

        //    AnnualClimateArray = new Ecoregions.AuxParm<AnnualClimate_Monthly[]>(PlugIn.ModelCore.Ecoregions);
                                 
        //    PlugIn.ModelCore.UI.WriteLine("    Loading {0}, Year: {1}", spinupOrfuture.ToString(), startingTimestep);
            
        //    foreach(IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions) 
        //    {
        //        if(ecoregion.Active)
        //        {            
        //            AnnualClimate_Monthly[] tempClimate = new AnnualClimate_Monthly[timeStepCount];
            
        //            for (int y = 0; y < timeStepCount; y++)
        //            {
        //                int actualYear = startingTimestep + y;
            
        //                //if(Climate.AllData.ContainsKey(actualYear))
        //                //{
        //                //    Climate.TimestepData = Climate.AllData[actualYear];
        //                //    //PlugIn.ModelCore.UI.WriteLine("  Changing TimestepData:  Yr={0}, Eco={1}.", actualYear, ecoregion.Name);
        //                //    //PlugIn.ModelCore.UI.WriteLine("  Changing TimestepData:  AllData  Jan Ppt = {0:0.00}.", Climate.AllData[actualYear][ecoregion.Index,0].AvgPpt);
        //                //    //PlugIn.ModelCore.UI.WriteLine("  Changing TimestepData:  Timestep Jan Ppt = {0:0.00}.", Climate.TimestepData[ecoregion.Index,0].AvgPpt);
        //                //}

        //                AnnualClimate.AnnualClimateInitialize();  // Synchronizes temperature and precipitation deviation across ecoregions.
        //                tempClimate[y] = new AnnualClimate_Monthly(ecoregion, actualYear, Latitude[ecoregion], spinupOrfuture, actualYear); //actual year and timeStep here have been set to be identity
                        
        //                //Console.WriteLine("---{0} , for eco:{1} actualYear/timeStep: {2}  ", spinuOrfuture.ToString(), ecoregion.Index, actualYear);
        //            }
        //            AnnualClimateArray[ecoregion] = tempClimate;
        //        }
        //    }
        //}

        //public static void GenerateNewClimate_OLD(int year, int years) //Pass false for spin-up (historic)
        //{

        //    //PlugIn.ModelCore.UI.WriteLine("   Generating new climate for simulation year {0}.", year);

        //    AnnualClimateArray = new Ecoregions.AuxParm<AnnualClimate_Monthly[]>(PlugIn.ModelCore.Ecoregions);

        //    // Issues with this approach:  Each ecoregion will have unique variability associated with 
        //    // temperature and precipitation.  In reality, we expect some regional synchronicity.  An 
        //    // easy-ish solution would be to use the same random number in combination with standard 
        //    // deviations for all ecoregions.  The converse problem is over synchronization of climate, but
        //    // that would certainly be preferrable over smaller regions.



        //    foreach (IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions)
        //    {
        //        if (ecoregion.Active)
        //        {
        //            AnnualClimate_Monthly[] tempClimate = new AnnualClimate_Monthly[years];

        //            for (int y = 0; y < years; y++)
        //            {

        //                int actualYear = year + y;

        //                if (Climate.AllData.ContainsKey(actualYear))
        //                {
        //                    Climate.TimestepData = Climate.AllData[actualYear];
        //                    //PlugIn.ModelCore.UI.WriteLine("  Changing TimestepData:  Yr={0}, Eco={1}.", actualYear, ecoregion.Name);
        //                    //PlugIn.ModelCore.UI.WriteLine("  Changing TimestepData:  AllData  Jan Ppt = {0:0.00}.", Climate.AllData[actualYear][ecoregion.Index,0].AvgPpt);
        //                    //PlugIn.ModelCore.UI.WriteLine("  Changing TimestepData:  Timestep Jan Ppt = {0:0.00}.", Climate.TimestepData[ecoregion.Index,0].AvgPpt);
        //                }

        //                AnnualClimate.AnnualClimateInitialize();
        //                tempClimate[y] = new AnnualClimate_Monthly(ecoregion, actualYear, Latitude[ecoregion]);

        //            }

        //            AnnualClimateArray[ecoregion] = tempClimate;
        //        }
        //    }
        //}
        
    }
}
