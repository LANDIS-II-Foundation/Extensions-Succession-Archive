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
        public static Ecoregions.AuxParm<double> AnnualNDeposition;    
        public static Ecoregions.AuxParm<double[]> MonthlyNDeposition; 
        private static Ecoregions.AuxParm<int> LastYearUpdated;

        public static Ecoregions.AuxParm<AnnualClimate_Monthly> AnnualWeather;

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
            MonthlyNDeposition = new Ecoregions.AuxParm<double[]>(PlugIn.ModelCore.Ecoregions);

            AnnualNDeposition = new Ecoregions.AuxParm<double>(PlugIn.ModelCore.Ecoregions);
            
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

            foreach (IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions)
            {
                MonthlyNDeposition[ecoregion] = new double[12];

                //for (int i = 0; i < 12; i++)
                //    MonthlyNDeposition[ecoregion][i] = 0.0;

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
            int actualYear = Climate.Future_MonthlyData.Keys.Min() + year;

            if (spinupOrfuture == Climate.Phase.Future_Climate)
            {
                //PlugIn.ModelCore.UI.WriteLine("Retrieving {0} for year {1}.", spinupOrfuture.ToString(), actualYear);
                if (Climate.Future_MonthlyData.ContainsKey(actualYear))
                {
                    AnnualWeather[ecoregion] = Climate.Future_MonthlyData[actualYear][ecoregion.Index];
                }
                //else
                //    PlugIn.ModelCore.UI.WriteLine("Key is missing: Retrieving {0} for year {1}.", spinupOrfuture.ToString(), actualYear);
            }
            else
            {
                //PlugIn.ModelCore.UI.WriteLine("Retrieving {0} for year {1}.", spinupOrfuture.ToString(), actualYear);
                if (Climate.Spinup_MonthlyData.ContainsKey(actualYear))
                {
                    AnnualWeather[ecoregion] = Climate.Spinup_MonthlyData[actualYear][ecoregion.Index];
                }
            }
           
        }

        //---------------------------------------------------------------------
        // Generates new climate parameters for all ecoregions at an annual time step.
        public static void SetAllEcoregions_FutureAnnualClimate(int year)
        {
            int actualYear = Climate.Future_MonthlyData.Keys.Min() + year - 1;
            foreach (IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions)
            {
                if (ecoregion.Active)
                {
                    //PlugIn.ModelCore.UI.WriteLine("Retrieving {0} for year {1}.", spinupOrfuture.ToString(), actualYear);
                    if (Climate.Future_MonthlyData.ContainsKey(actualYear))
                    {
                        AnnualWeather[ecoregion] = Climate.Future_MonthlyData[actualYear][ecoregion.Index];
                    }

                    PlugIn.ModelCore.UI.WriteLine("Utilizing Climate Data: Simulated Year = {0}, actualClimateYearUsed = {1}.", actualYear, AnnualWeather[ecoregion].Year);
                }

            }
        }
        

        
    }
}
