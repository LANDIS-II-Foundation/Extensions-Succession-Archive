//  Copyright 2007-2010 Portland State University, University of Wisconsin-Madison
//  Author: Robert Scheller, Ben Sulman, Melissa Lucash

using System.Collections.Generic;
using System.IO;
using System;
using Landis.Core;
using Landis.SpatialModeling;
using Landis.Library.Climate;

namespace Landis.Extension.Succession.Century
{

    public enum WaterType { Linear, Ratio }


    public class SoilWater
    {
        private static double H2Oinputs;
        private static double tave;
        private static double tmax;
        private static double tmin;
        private static double pet;

        public static void Run(int year, int month, double liveBiomass, Site site, out double baseFlow, out double stormFlow)
        {

            //Originally from h2olos.f of CENTURY model
            //Water Submodel for Century - written by Bill Parton
            //     Updated from Fortran 4 - rm 2/92
            //     Rewritten by Bill Pulliam - 9/94
            // Rewritten by Melissa Lucash- 11/2014

            //PlugIn.ModelCore.UI.WriteLine("month={0}.", Century.Month);
        
            //...Initialize Local Variables
            double addToSoil = 0.0;
            double bareSoilEvap = 0.0;
            baseFlow = 0.0;
            double relativeWaterContent = 0.0;
            double snow = 0.0;
            stormFlow = 0.0;
            double actualET = 0.0;
            double remainingPET = 0.0;
            double availableWaterMax = 0.0;  //amount of water available after precipitation and snowmelt (over-estimate of available water)
            double availableWaterMin = 0.0;   //amount of water available after stormflow (runoff) evaporation and transpiration, but before baseflow/leaching (under-estimate of available water)
            double availableWater = 0.0;     //amount of water deemed available to the trees, which will be the average between the max and min

            //...Calculate external inputs
            IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];

            double litterBiomass = (SiteVars.SurfaceStructural[site].Carbon + SiteVars.SurfaceMetabolic[site].Carbon) * 2.0;
            double deadBiomass = SiteVars.SurfaceDeadWood[site].Carbon / 0.47;
            double soilWaterContent = SiteVars.SoilWaterContent[site];
            double liquidSnowpack = SiteVars.LiquidSnowPack[site];

            H2Oinputs = EcoregionData.AnnualWeather[ecoregion].MonthlyPrecip[month]; //rain + irract in cm;
            //PlugIn.ModelCore.UI.WriteLine("SoilWater. WaterInputs={0:0.00}, .", H2Oinputs);
            tave = EcoregionData.AnnualWeather[ecoregion].MonthlyTemp[month];
            //PlugIn.ModelCore.UI.WriteLine("SoilWater. AvgTemp={0:0.00}, .", tave);
            tmax = EcoregionData.AnnualWeather[ecoregion].MonthlyMaxTemp[month];
            tmin = EcoregionData.AnnualWeather[ecoregion].MonthlyMinTemp[month];
            pet = EcoregionData.AnnualWeather[ecoregion].MonthlyPET[month];

            double wiltingPoint = EcoregionData.WiltingPoint[ecoregion];
            double soilDepth = EcoregionData.SoilDepth[ecoregion];
            double fieldCapacity = EcoregionData.FieldCapacity[ecoregion];
            double stormFlowFraction = EcoregionData.StormFlowFraction[ecoregion];
            double baseFlowFraction = EcoregionData.BaseFlowFraction[ecoregion];
            double drain = EcoregionData.Drain[ecoregion];
           
                      
            //...Calculating snow pack first. Occurs when mean monthly air temperature is equal to or below freezing,
            //     precipitation is in the form of snow.
            
            if (tmin <= 0.0) // Use tmin to dictate whether it snows or rains. 
            {
                snow = H2Oinputs; 
                H2Oinputs = 0.0;  
                liquidSnowpack += snow;  //only tracking liquidsnowpack (water equivalent) and not the actual amount of snow on the ground (i.e. not snowpack).
                //PlugIn.ModelCore.UI.WriteLine("Let it snow!! snow={0}, liquidsnowpack={1}.", snow, liquidSnowpack);
            }
            else
            {
                soilWaterContent += H2Oinputs;
                //PlugIn.ModelCore.UI.WriteLine("Let it rain and add it to soil! rain={0}, soilWaterContent={1}.", H2Oinputs, soilWaterContent);
            }

           
            //...Then melt snow if there is snow on the ground and air temperature (tmax) is above minimum.            
            if (liquidSnowpack > 0.0 && tmax > 0.0)
            {
                //...Calculate the amount of snow to melt:
                
                double snowMeltFraction = Math.Max((tmax * 0.05) + 0.024, 0.0);//This equation assumes a linear increase in the fraction of snow that melts as a function of air temp.  
                //This relationship ultimately derives from http://www.nps.gov/yose/planyourvisit/climate.htm which described the relationship between snow melting and air temp.
                //Documentation for the regression equation is in spreadsheet called WaterCalcs.xls by M. Lucash

               if (snowMeltFraction > 1.0)
                    snowMeltFraction = 1.0;

               addToSoil = liquidSnowpack * snowMeltFraction;  //Amount of liquidsnowpack that melts = liquidsnowpack multiplied by the fraction that melts.
              
                //Subtracted melted snow from snowpack and add it to the soil
               liquidSnowpack = liquidSnowpack - addToSoil;  
               soilWaterContent += addToSoil;
            }
            
            //Calculate the max amout of water available to trees, an over-estimate of the water available to trees.  It only reflects precip and melting of precip.
            availableWaterMax = soilWaterContent;
            
            //...Evaporate water from the snow pack (rewritten by Pulliam 9/94)
                  //...Coefficient 0.87 relates to heat of fusion for ice vs. liquid water
            if (liquidSnowpack > 0.0)
            {
                //...Calculate cm of snow that remaining pet energy can evaporate:
                double evaporatedSnow = pet * 0.87;

                //...Don't evaporate more snow than actually exists:
                if (evaporatedSnow > liquidSnowpack)
                    evaporatedSnow = liquidSnowpack;

                liquidSnowpack = liquidSnowpack - evaporatedSnow;

                //...Decrement remaining pet by energy used to evaporate snow:
                remainingPET = pet - evaporatedSnow;
                
                if (remainingPET < 0.0) 
                    remainingPET = 0.0;

                //Subtract evaporated snowfrom the soil water content
                soilWaterContent -= evaporatedSnow;
            }

            //Allow excess water to run off during storm events (stormflow)
            double waterFull = soilDepth * fieldCapacity;  //units of cm
            
            double waterMovement = 0.0;            

            if (soilWaterContent > waterFull)
            {

                waterMovement = Math.Max((soilWaterContent - waterFull), 0.0); // How much water should move during a storm event, which is based on how much water the soil can hold.
                soilWaterContent = waterFull;
                
                //...Compute storm flow.
                stormFlow = waterMovement * stormFlowFraction;

                //Subtract stormflow from soil water
                soilWaterContent -= stormFlow;
                //PlugIn.ModelCore.UI.WriteLine("Water Runs Off. stormflow={0}.", stormFlow);
            }

            
            
            //...Calculate bare soil water loss and interception  when air temperature is above freezing and no snow cover.
            //...Mofified 9/94 to allow interception when t < 0 but no snow cover, Pulliam
            if (liquidSnowpack <= 0.0)
            {
                //...Calculate total canopy cover and litter, put cap on effects:
                double standingBiomass = liveBiomass + deadBiomass;

                if (standingBiomass > 800.0) standingBiomass = 800.0;
                if (litterBiomass > 400.0) litterBiomass = 400.0;

                //...canopy interception, fraction of  precip (canopyIntercept):
                double canopyIntercept = ((0.0003 * litterBiomass) + (0.0006 * standingBiomass)) * OtherData.WaterLossFactor1;

                //...Bare soil evaporation, fraction of precip (bareSoilEvap):
                bareSoilEvap = 0.5 * System.Math.Exp((-0.002 * litterBiomass) - (0.004 * standingBiomass)) * OtherData.WaterLossFactor2;
                
                //...Calculate total surface evaporation losses, maximum allowable is 0.4 * pet. -rm 6/94
                remainingPET = pet;
                double soilEvaporation = System.Math.Min(((bareSoilEvap + canopyIntercept) * H2Oinputs), (0.4 * remainingPET));
                
                //Subtract soil evaporation from soil water content
               soilWaterContent -= soilEvaporation;
            }
                     
            // Calculate actual evapotranspiration.  This equation is derived from the stand equation for calculating AET from PET
            //http://www.civil.utah.edu/~mizukami/coursework/cveen7920/ETMeasurement.pdf 

            double waterEmpty = wiltingPoint * soilDepth;

            if (soilWaterContent > waterFull)
                actualET = remainingPET;
            else
            {
                actualET = Math.Max(remainingPET * ((soilWaterContent - waterEmpty) / (waterFull - waterEmpty)), 0.0);
            }

            if (actualET < 0.0)
                actualET = 0.0;

            //Subtract transpiration from soil water content
            soilWaterContent -= actualET;
            

            //Leaching occurs. Drain baseflow fraction from holding tank.
            baseFlow = soilWaterContent * baseFlowFraction;
            
            //Subtract baseflow from soil water
            soilWaterContent -= baseFlow;
                                                         
            //Calculate the amount of available water after all the evapotranspiration and leaching has taken place (minimum available water)           
            availableWaterMin = Math.Max(soilWaterContent - waterEmpty, 0.0);
                         

            //Calculate the final amount of available water to the trees, which is the average of the max and min          
            availableWater = (availableWaterMax + availableWaterMin)/ 2.0;
                       
            //// Compute the ratio of precipitation to PET
            double ratioPrecipPET = 0.0;
            //if (pet > 0.0) ratioPrecipPET = (availableWater + H2Oinputs) / pet; //old ratio used in previous versions of LANDIS-Century
            //if (pet > 0.0) ratioPrecipPET = H2Oinputs / pet;  //assumes that the ratio is the amount of incoming precip divided by PET.
            if (pet > 0.0) ratioPrecipPET = availableWater / pet;  //assumes that the ratio is the amount of incoming precip divided by PET.

            //SiteVars.NumberDryDays[site] = numberDryDays; //Calculated above using method below.
            SiteVars.LiquidSnowPack[site] = liquidSnowpack;
            SiteVars.WaterMovement[site] = waterMovement;
            SiteVars.AvailableWater[site] = availableWater;  //available to plants for growth     
            SiteVars.SoilWaterContent[site] = soilWaterContent;
            SiteVars.SoilTemperature[site] = CalculateSoilTemp(tmin, tmax, liveBiomass, litterBiomass, month);
            SiteVars.DecayFactor[site] = CalculateDecayFactor((int)OtherData.WType, SiteVars.SoilTemperature[site], relativeWaterContent, ratioPrecipPET, month);
            SiteVars.AnaerobicEffect[site] = CalculateAnaerobicEffect(drain, ratioPrecipPET, pet, tave);                             
                        
            return;
        }

        //---------------------------------------------------------------------------

        private static double CalculateDecayFactor(int idef, double soilTemp, double rwc, double ratioPrecipPET, int month)
        {
            // Decomposition factor relfecting the effects of soil temperature and moisture on decomposition
            // Originally revised from prelim.f of CENTURY
            // Irrigation is zero for natural forests
            double decayFactor = 0.0;   //represents defac in the original program defac.f
            double W_Decomp = 0.0;      //Water effect on decomposition

            //...where
            //      soilTemp;        //Soil temperature
            //      T_Decomp;     //Effect of soil temperature on decomposition
            //      W_Decomp;     //Effect of soil moisture on decompostion
            //      rwcf[10];     //Initial relative water content for 10 soil layers
            //      avh2o;        //Water available to plants for growth in soil profile
            //      precipitation;       //Precipitation of current month
            //      irract;       //Actual amount of irrigation per month (cm H2O/month)
            //      pet;          //Monthly potential evapotranspiration in centimeters (cm)

            //Option selection for wfunc depending on idef
            //      idef = 0;     // for linear option
            //      idef = 1;     // for ratio option


            if (idef == 0)
            {
                if (rwc > 13.0)
                    W_Decomp = 1.0;
                else
                    W_Decomp = 1.0 / (1.0 + 4.0 * System.Math.Exp(-6.0 * rwc));
            }
            else if (idef == 1)
            {
                if (ratioPrecipPET > 9)
                    W_Decomp = 1.0;
                else
                    W_Decomp = 1.0 / (1.0 + 30.0 * System.Math.Exp(-8.5 * ratioPrecipPET));
            }

            double tempModifier = T_Decomp(soilTemp);

            decayFactor = tempModifier * W_Decomp;

            //defac must >= 0.0
            if (decayFactor < 0.0) decayFactor = 0.0;

            //if (soilTemp < 0 && decayFactor > 0.01)
            //{
            //    PlugIn.ModelCore.UI.WriteLine("Yr={0},Mo={1}, PET={2:0.00}, MinT={3:0.0}, MaxT={4:0.0}, AveT={5:0.0}, H20={6:0.0}.", Century.Year, month, pet, tmin, tmax, tave, H2Oinputs);
            //    PlugIn.ModelCore.UI.WriteLine("Yr={0},Mo={1}, DecayFactor={2:0.00}, tempFactor={3:0.00}, waterFactor={4:0.00}, ratioPrecipPET={5:0.000}, soilT={6:0.0}.", Century.Year, month, decayFactor, tempModifier, W_Decomp, ratioPrecipPET, soilTemp);
            //}

            return decayFactor;   //Combination of water and temperature effects on decomposition
        }

        //---------------------------------------------------------------------------
        private static double T_Decomp(double soilTemp)
        {
            //Originally from tcalc.f
            //This function computes the effect of temperature on decomposition.
            //It is an exponential function.  Older versions of Century used a density function.
            //Created 10/95 - rm


            double Teff0 = OtherData.TemperatureEffectIntercept;
            double Teff1 = OtherData.TemperatureEffectSlope;
            double Teff2 = OtherData.TemperatureEffectExponent;

            double r = Teff0 + (Teff1 * System.Math.Exp(Teff2 * soilTemp));

            return r;
        }
        //---------------------------------------------------------------------------
        private static double CalculateAnaerobicEffect(double drain, double ratioPrecipPET, double pet, double tave)
        {

            //Originally from anerob.f of Century

            //...This function calculates the impact of soil anerobic conditions
            //     on decomposition.  It returns a multiplier 'anerob' whose value
            //     is 0-1.

            //...Declaration explanations:
            //     aneref[1] - ratio RAIN/PET with maximum impact
            //     aneref[2] - ratio RAIN/PET with minimum impact
            //     aneref[3] - minimum impact
            //     drain     - percentage of excess water lost by drainage
            //     newrat    - local var calculated new (RAIN+IRRACT+AVH2O[3])/PET ratio
            //     pet       - potential evapotranspiration
            //     rprpet    - actual (RAIN+IRRACT+AVH2O[3])/PET ratio

            double aneref1 = OtherData.RatioPrecipPETMaximum;  //This value is 1.5
            double aneref2 = OtherData.RatioPrecipPETMinimum;   //This value is 3.0
            double aneref3 = OtherData.AnerobicEffectMinimum;   //This value is 0.3

            double anerob = 1.0;

            //...Determine if RAIN/PET ratio is GREATER than the ratio with
            //     maximum impact.

            if ((ratioPrecipPET > aneref1) && (tave > 2.0))
            {
                double xh2o = (ratioPrecipPET - aneref1) * pet * (1.0 - drain);

                if (xh2o > 0)
                {
                    double newrat = aneref1 + (xh2o / pet);
                    double slope = (1.0 - aneref3) / (aneref1 - aneref2);
                    anerob = 1.0 + slope * (newrat - aneref1);
                    //PlugIn.ModelCore.UI.WriteLine("If higher threshold. newrat={0:0.0}, slope={1:0.00}, anerob={2:0.00}", newrat, slope, anerob);      
                }

                if (anerob < aneref3)
                    anerob = aneref3;
                //PlugIn.ModelCore.UI.WriteLine("Lower than threshold. Anaerobic={0}", anerob);      
            }
            //PlugIn.ModelCore.UI.WriteLine("ratioPrecipPET={0:0.0}, tave={1:0.00}, pet={2:0.00}, AnaerobicFactor={3:0.00}, Drainage={4:0.00}", ratioPrecipPET, tave, pet, anerob, drain);         
            //PlugIn.ModelCore.UI.WriteLine("Anaerobic Effect = {0:0.00}.", anerob);
            return anerob;
        }
        //---------------------------------------------------------------------------
        private static double CalculateSoilTemp(double tmin, double tmax, double liveBiomass, double litterBiomass, int month)
        {
            // ----------- Calculate Soil Temperature -----------
            double bio = liveBiomass + (OtherData.EffectLitterSoilT * litterBiomass);
            bio = Math.Min(bio, 600.0);

            //...Maximum temperature
            double maxSoilTemp = tmax + (25.4 / (1.0 + 18.0 * Math.Exp(-0.20 * tmax))) * (Math.Exp(OtherData.EffectBiomassMaxSurfT * bio) - 0.13);

            //...Minimum temperature
            double minSoilTemp = tmin + OtherData.EffectBiomassMinSurfT * bio - 1.78;

            //...Average surface temperature
            //...Note: soil temperature used to calculate potential production does not
            //         take into account the effect of snow (AKM)
            double soilTemp = (maxSoilTemp + minSoilTemp) / 2.0;

            //PlugIn.ModelCore.UI.WriteLine("Month={0}, Soil Temperature = {1}.", month+1, soilTemp);

            return soilTemp;
        }
        //--------------------------------------------------------------------------
        public static void Leach(Site site, double baseFlow, double stormFlow)
        {
           
            //  double minlch, double frlech[3], double stream[8], double basef, double stormf)
            //Originally from leach.f of CENTURY model
            //...This routine computes the leaching of inorganic nitrogen (potential for use with phosphorus, and sulfur)
            //...Written 2/92 -rm. Revised on 12/11 by ML
            // ML left out leaching intensity factor.  Cap on MAX leaching (MINLECH/OMLECH3) is poorly defined in CENTURY manual. Added a NO3frac factor to account 
            //for the fact that only NO3 (not NH4) is leached from soils.  

            //...Called From:   SIMSOM

            //...amtlea:    amount leached
            //...linten:    leaching intensity
            //...strm:      storm flow
            //...base:      base flow

            //Outputs:
            //minerl and stream are recomputed
            IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];
            double waterMove = SiteVars.WaterMovement[site];

            double amtNLeached = 0.0;

            //PlugIn.ModelCore.UI.WriteLine("WaterMove={0:0}, ", waterMove);         
           
         //...waterMove > 0. indicates a saturated water flow out of layer lyr
            if (waterMove > 0.0 && SiteVars.MineralN[site] > 0.0)
            {
                double textureEffect = OtherData.MineralLeachIntercept + OtherData.MineralLeachSlope * EcoregionData.PercentSand[ecoregion];
                //double leachIntensity = (1.0 - (OtherData.OMLeachWater - waterMove) / OtherData.OMLeachWater);
                //amtNLeached = textureEffect * SiteVars.MineralN[site] * OtherData.NfracLeachWater * OtherData.NO3frac;
                amtNLeached = textureEffect * SiteVars.MineralN[site] *  OtherData.NO3frac;
                
                //PlugIn.ModelCore.UI.WriteLine("amtNLeach={0:0.0}, textureEffect={1:0.0}, waterMove={2:0.0}, MineralN={3:0.00}", amtNLeached, textureEffect, waterMove, SiteVars.MineralN[site]);      
            }        
            
            double totalNleached = (baseFlow * amtNLeached) + (stormFlow * amtNLeached);
                        
            SiteVars.MineralN[site] -= totalNleached;
            //PlugIn.ModelCore.UI.WriteLine("AfterSoilWaterLeaching. totalNLeach={0:0.0}, MineralN={1:0.00}", totalNleached, SiteVars.MineralN[site]);         

            SiteVars.Stream[site].Nitrogen += totalNleached;
            SiteVars.MonthlyStreamN[site][Century.Month] += totalNleached;
            //PlugIn.ModelCore.UI.WriteLine("AfterSoilWaterLeaching. totalNLeach={0:0.0}, MineralN={1:0.00}", totalNleached, SiteVars.MineralN[site]);        

            return;
        }

        //******************************************************************************
        //This is the originally Century water budget which is used in versions of Century prior to v3.2
        //public static void Run(int year, int month, double liveBiomass, Site site, out double baseFlow, out double stormFlow)
        //{

        //    //PlugIn.ModelCore.UI.WriteLine("year = {0}, month = {1}", year, month);
        //    //Originally from h2olos.f of CENTURY model
        //    //Water Submodel for Century - written by Bill Parton
        //    //     Updated from Fortran 4 - rm 2/92
        //    //     Rewritten by Bill Pulliam - 9/94

        //    //...Description of variables
        //    //   deadBiomass        the average monthly standing dead biomass(gm/m..2)
        //    //   soilDepth          depth of the ith soil layer(cm)
        //    //   fieldCapacity      the field capacity of the ith soil layer(fraction)
        //    //   litterBiomass      the average monthly litter biomass(gm/m..2)
        //    //   liveBiomass        the average monthly live plant biomass(gm/m..2)
        //    //   waterMovement      the index for water movement(0-no flow,1-satruated flow)
        //    //   soilWaterContent   the soil water content of the ith soil layer(cm h2o)
        //    //   asnow              the snow pack water contint(cm-h2o)
        //    //   avh2o (1)          NA water available to plants for growth
        //    //   avh2o (2)          NA water available to plants for survival
        //    //                      (available water in the whole soil profile)
        //    //   availableWater     available water in current soil layer
        //    //   wiltingPoint       the wilting point of the  ith soil layer(fraction)
        //    //   transpLossFactor   the weight factor for transpiration water loss 
        //    //   totalEvaporated               the water evaporated from the  soil and vegetation(cm/mon)
        //    //   evaporatedSnow             snow evaporated
        //    //   inputs             rain + irrigation
        //    //   H2Oinputs            inputs which are water (not converted to snow)
        //    //   nlayer             NA number of soil layers with water available for plant survival
        //    //   nlaypg             NA number of soil layers with water available for plant growth
        //    //   remainingPET       remaining pet, updated after each incremental h2o loss
        //    //   potentialEvapTop               the potential evaporation rate from the top  soil layer (cm/day)
        //    //   rain               the total monthly rainfall (cm/month)
        //    //   relativeWaterContent        the relative water content of the ith soil layer(0-1)
        //    //   liquidSnowpack               the liquid water in the snow pack
        //    //   tav                average monthly air temperature (2m-        //)
        //    //   tran               transpriation water loss(cm/mon)
        //    //   transpirationLoss  transpiration water loss

        //    //...Initialize Local Variables
        //    double addToSoil = 0.0;
        //    double bareSoilEvap = 0.0;
        //    baseFlow = 0.0;
        //    double totalEvaporated = 0.0;
        //    double evaporativeLoss = 0.0;
        //    double potentialTrans = 0.0;
        //    double relativeWaterContent = 0.0;
        //    double snow = 0.0;
        //    double liquidSnowpack = 0.0;
        //    stormFlow = 0.0;
        //    //double tran = 0.0;
        //    double transpiration = 0.01;

        //    //...Calculate external inputs
        //    IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];

        //    double litterBiomass = (SiteVars.SurfaceStructural[site].Carbon + SiteVars.SurfaceMetabolic[site].Carbon) * 2.0;
        //    double deadBiomass = SiteVars.SurfaceDeadWood[site].Carbon * 2.0;
        //    double soilWaterContent = SiteVars.SoilWaterContent[site];

        //    H2Oinputs = EcoregionData.AnnualWeather[ecoregion].MonthlyPrecip[month]; //rain + irract;
        //    tave = EcoregionData.AnnualWeather[ecoregion].MonthlyTemp[month];
        //    tmax = EcoregionData.AnnualWeather[ecoregion].MonthlyMaxTemp[month];
        //    tmin = EcoregionData.AnnualWeather[ecoregion].MonthlyMinTemp[month];
        //    pet = EcoregionData.AnnualWeather[ecoregion].MonthlyPET[month];

        //    PlugIn.ModelCore.UI.WriteLine("Line 91. Really just the inputs. Year={0}, month={1}, AvgMonthlyTemp={2}, tmax={3}, tmin={4}, pet={5:0.0000}, ppt={6}, soilWaterContent={7:0.0000}.", Century.Year, Century.Month, tave, tmax, tmin, pet, H2Oinputs, soilWaterContent);
        //    //double soilTemp         = tave;    

        //    double wiltingPoint = EcoregionData.WiltingPoint[ecoregion];
        //    double soilDepth = EcoregionData.SoilDepth[ecoregion];
        //    double fieldCapacity = EcoregionData.FieldCapacity[ecoregion];
        //    double stormFlowFraction = EcoregionData.StormFlowFraction[ecoregion];
        //    double baseFlowFraction = EcoregionData.BaseFlowFraction[ecoregion];
        //    double drain = EcoregionData.Drain[ecoregion];

        //    //PlugIn.ModelCore.UI.WriteLine("Really just more inputs. wiltingpoint={0}, soildepth={1}, fieldcapacity={2}, stormflow={3}, baseflow={4}, drainage={5}.", wiltingPoint, soilDepth, fieldCapacity, stormFlowFraction, baseFlowFraction, drain);

        //    deadBiomass = 0.0;

        //    //...Throughout, uses remainingPET as remaining energy for pet after
        //    //     each melting and evaporation step.  Initially calculated
        //    //     pet is not modified.  Pulliam 9/94
        //    double remainingPET = pet;

        //    //...Determine the snow pack, melt snow, and evaporate from the snow pack
        //    //...When mean monthly air temperature is below freezing,
        //    //     precipitation is in the form of snow.
        //    if (tave < 0.0)
        //    {
        //        snow = H2Oinputs; //snow + inputs;
        //        H2Oinputs = 0.0;
        //        PlugIn.ModelCore.UI.WriteLine("Let it snow!! snow={0}, waterinputs={1}, remainingPET={2}.", snow, H2Oinputs, remainingPET);
        //    }

        //    //...Melt snow if air temperature is above minimum (tmelt(1))
        //    if (tave > OtherData.TMelt1)
        //    {
        //        //...Calculate the amount of snow to melt:
        //        double snowMelt = OtherData.TMelt2 * (tave - OtherData.TMelt1);  //1 is 4.0, 2 is -8

        //        PlugIn.ModelCore.UI.WriteLine("Line 126. Wacky Equation for snowMelt. snowmelt={0}.", snowMelt);
        //        if (snowMelt > snow)
        //            snowMelt = snow;
        //        snow = snow - snowMelt;

        //        //..Melted snow goes to snow pack and drains excess
        //        //  addToSoil rain-on-snow  and melted snow to snowpack liquid (liquidSnowpack):
        //        if (tave > 0.0 && snow > 0.0)
        //            liquidSnowpack = H2Oinputs;

        //        liquidSnowpack = liquidSnowpack + snowMelt;
        //        PlugIn.ModelCore.UI.WriteLine("Line 136. Melting snow. LiquidSnowpack={0}, snow={1}, snowMelt={2}.", liquidSnowpack, snow, snowMelt);

        //        //...Drain snowpack to 5% liquid content (weight/weight), excess to soil:
        //        if (liquidSnowpack > (0.05 * snow))
        //        {
        //            addToSoil = liquidSnowpack - 0.05 * snow;
        //            liquidSnowpack = liquidSnowpack - addToSoil;
        //        }
        //        PlugIn.ModelCore.UI.WriteLine("Line 144. Melting snow. LiquidSnowpack={0}, addtoSoil={1}.", liquidSnowpack, addToSoil);
        //    }

        //    //...Evaporate water from the snow pack (rewritten Pulliam 9/94 to
        //    //     evaporate from both snow aqnd liquidSnowpack in proportion)
        //    //...Coefficient 0.87 relates to heat of fusion for ice vs. liquid water
        //    //     wasn't modified as snow pack is at least 95% ice.
        //    if (snow > 0.0)
        //    {
        //        //...Calculate cm of snow that remaining pet energy can evaporate:
        //        double evaporatedSnow = remainingPET * 0.87;

        //        //...Calculate total snowpack water, ice + liquid:
        //        double totalSnowpack = snow + liquidSnowpack;

        //        //...Don't evaporate more snow than actually exists:
        //        if (evaporatedSnow > totalSnowpack)
        //            evaporatedSnow = totalSnowpack;

        //        //...Take evaporatedSnow from snow and liquidSnowpack in proportion:
        //        snow = snow - evaporatedSnow * (snow / totalSnowpack);
        //        liquidSnowpack = liquidSnowpack - evaporatedSnow * (liquidSnowpack / totalSnowpack);

        //        //...addToSoil evaporated snow to evaporation accumulator (totalEvaporated):
        //        totalEvaporated = evaporatedSnow;

        //        //...Decrement remaining pet by energy used to evaporate snow:
        //        remainingPET = remainingPET - evaporatedSnow / 0.87;

        //        if (remainingPET < 0.0)
        //            remainingPET = 0.0;
        //        PlugIn.ModelCore.UI.WriteLine("Line 174. Evaporating snow. TotalEvaporated={0:0.000}, remainingPET={1}, evaportedSnow={2:0.o}.", totalEvaporated, remainingPET, evaporatedSnow);
        //    }

        //    //...Calculate bare soil water loss and interception
        //    //     when air temperature is above freezing and no snow cover.
        //    //...Mofified 9/94 to allow interception when t < 0 but no snow
        //    //     cover, Pulliam
        //    if (snow <= 0.0)
        //    {
        //        PlugIn.ModelCore.UI.WriteLine("Line 183. There is no snow left on ground.");
        //        //...Calculate total canopy cover and litter, put cap on effects:
        //        double standingBiomass = liveBiomass + deadBiomass;

        //        if (standingBiomass > 800.0) standingBiomass = 800.0;
        //        if (litterBiomass > 400.0) litterBiomass = 400.0;

        //        //...canopy interception, fraction of  precip (canopyIntercept):
        //        double canopyIntercept = ((0.0003 * litterBiomass) + (0.0006 * standingBiomass)) * OtherData.WaterLossFactor1;

        //        //...Bare soil evaporation, fraction of precip (bareSoilEvap):
        //        bareSoilEvap = 0.5 * System.Math.Exp((-0.002 * litterBiomass) - (0.004 * standingBiomass)) * OtherData.WaterLossFactor2;
        //        PlugIn.ModelCore.UI.WriteLine("If there is no snow. Line 195. BareSoilEvap={0}, litterBiomass={1}, standingBiomass={2}, CanopyIntercept={3}.", bareSoilEvap, litterBiomass, standingBiomass, canopyIntercept);

        //        //...Calculate total surface evaporation losses, maximum
        //        //     allowable is 0.4 * pet. -rm 6/94
        //        evaporativeLoss = System.Math.Min(((bareSoilEvap + canopyIntercept) * H2Oinputs), (0.4 * remainingPET));
        //        totalEvaporated = totalEvaporated + evaporativeLoss;

        //        //...Calculate remaining water to addToSoil to soil and potential
        //        //     transpiration as remaining pet:
        //        addToSoil = H2Oinputs - evaporativeLoss;
        //        transpiration = remainingPET - evaporativeLoss;
        //        PlugIn.ModelCore.UI.WriteLine("Line 202. This is the amount added to soil when no snow. evaporativeloss={0:0.0}, totalevaporated={1:0.0}, addToSoil={2:0.0}, RemainingPET={3:0.0}, Transpiration={4:0.0}", evaporativeLoss, totalEvaporated, addToSoil, remainingPET, transpiration);

        //    }

        //    // **************************************************************************
        //    //...Determine potential transpiration water loss (transpiration, cm/mon) as a
        //    //     function of precipitation and live biomass.
        //    //...If temperature is less than 2C turn off transpiration. -rm 6/94
        //    if (tave < 2.0)
        //        potentialTrans = 0.0;
        //    else
        //        potentialTrans = remainingPET * 0.65 * (1.0 - System.Math.Exp(-0.020 * liveBiomass));
        //    PlugIn.ModelCore.UI.WriteLine("Line 220. Potential Transpiration. PotentialTranspiration={0:0.000}.", potentialTrans);

        //    if (potentialTrans < transpiration)
        //        transpiration = potentialTrans;

        //    if (transpiration < 0.0) transpiration = 0.01;

        //    // **************************************************************************
        //    //...Calculate the potential evaporation rate from the top soil layer
        //    //     (potentialEvapTop-cm/day).  This is not actually taken out until after
        //    //     transpiration losses
        //    double potentialEvapTop = remainingPET - transpiration - evaporativeLoss;

        //    PlugIn.ModelCore.UI.WriteLine("PotentialEvapTop={0:0.0}, remainingPET={1:0.0}, transpiration={2:0.0}, evaporativeLoss={3:0.0}.", potentialEvapTop, remainingPET, transpiration, evaporativeLoss);
        //    if (potentialEvapTop < 0.0) potentialEvapTop = 0.0;

        //    // **************************************************************************
        //    //...Transpire water from added water first, before passing water
        //    //     on to soil.  This is necessary for a monthly time step to
        //    //     give plants in wet climates adequate access to water for
        //    //     transpiration. -rm 6/94, Pulliam 9/94
        //    double transpireFromPrecipitation = System.Math.Min((transpiration - 0.01), addToSoil);

        //    transpiration = transpiration - transpireFromPrecipitation;
        //    addToSoil = addToSoil - transpireFromPrecipitation;

        //    //RMS: PlugIn.ModelCore.UI.WriteLine("Yr={0},Mo={1}, tran={2:0.0}, transpiration={3:0.0}, addToSoil={4:0.00}.", year, month, tran, transpiration, addToSoil);

        //    //...Add water to the soil
        //    //...Changed to add base flow and storm flow.  -rm 2/92
        //    //...addToSoil water to layer:

        //    soilWaterContent += addToSoil;
        //    // PlugIn.ModelCore.UI.WriteLine("Yr={0},Mo={1}, soilWaterContent={2:0.0}, addToSoil={3:0.0}.", year, month, soilWaterContent,addToSoil);

        //    //...Calculate field capacity of soil, drain soil, pass excess
        //    //     on to waterMovement:
        //    double waterFull = soilDepth * fieldCapacity;  //units of cm
        //    PlugIn.ModelCore.UI.WriteLine("Line 255. waterFull_WaterSoil={0:0.0}, soilWaterContent={1:0.000} addToSoil={2:0.000}, tran={3:0.000}.", waterFull, soilWaterContent, addToSoil, tran);
        //    double waterMovement = 0.0;

        //    if (soilWaterContent > waterFull)
        //    {

        //        waterMovement = (soilWaterContent - waterFull); // / soilWaterContent;
        //        soilWaterContent = waterFull;
        //        PlugIn.ModelCore.UI.WriteLine("Line 263. soilwatercontent={0:0.00} soilWaterContent > waterFull.", soilWaterContent);

        //        //...Compute storm flow.
        //        stormFlow = waterMovement * stormFlowFraction;
        //    }

        //    //...Compute base flow and stream flow for H2O. 
        //    //...Put water draining out bottom that doesn't go to stormflow
        //    //     into nlayer+1 holding tank:
        //    double drainedWater = addToSoil - stormFlow;

        //    //...Drain baseflow fraction from holding tank:
        //    baseFlow = drainedWater * baseFlowFraction;
        //    drainedWater = drainedWater - baseFlow;
        //    PlugIn.ModelCore.UI.WriteLine("Line 269. StormFlowStuff.stormFlow={0:0.000}, baseflow={1:0.000}, drainedwater={2:0.0000}.", stormFlow, baseFlow, drainedWater);

        //    //...Streamflow = stormflow + baseflow:
        //    double streamFlow = stormFlow + baseFlow;

        //    //PlugIn.ModelCore.UI.WriteLine("Yr={0},Mo={1}, soilWaterContent={2:0.0}, stormFlow={3:0.00}, baseFlow={4:0.00}.", year, month, soilWaterContent, stormFlow, baseFlow);

        //    //...Save soilWaterContent before transpiration for future use:
        //    double asimx = soilWaterContent;

        //    // **************************************************************************
        //    //...Calculate transpiration water loss from each layer
        //    //...This section was completely rewritten by Pulliam, though it
        //    //     should still do the same thing.  9/94

        //    //...Calculate available water in layer, soilWaterContent minus wilting point:
        //    //double availableWater = soilWaterContent - wiltingPoint * soilDepth;  ML thinks this is incorrect
        //    double waterEmpty = wiltingPoint * soilDepth;
        //    double availableWater = soilWaterContent - waterEmpty;
        //    PlugIn.ModelCore.UI.WriteLine("Line 285. BeforeTranspiration. soilwater content={0:0.00}, wiltingpoint={1:0.000}, availableWater={2:0.00000}.", soilWaterContent, wiltingPoint, availableWater);

        //    if (availableWater < 0.0)
        //        availableWater = 0.0;

        //    //...Calculate available water weighted by transpiration loss depth
        //    //      distribution factors:
        //    double availableWaterWeighted = availableWater * OtherData.TranspirationLossFactor;
        //    PlugIn.ModelCore.UI.WriteLine("Line 293. AfterTranspiration. availableWaterWeighted={0:0.0}, TranspLossFactor={1:0.0}.", availableWaterWeighted, OtherData.TranspirationLossFactor);

        //    //...Calculate the actual transpiration water loss(tran-cm/mon)
        //    //...Also rewritten by Pulliam 9/94, should do the same thing
        //    //...Update potential transpiration to be no greater than available water:
        //    transpiration = System.Math.Min(availableWater, transpiration);

        //    //...Transpire water from layer:

        //    if (availableWaterWeighted > 0.0)
        //    {
        //        //waterEmpty = wiltingPoint * soilDepth;
        //        //availableWater = soilWaterContent - wiltingPoint * soilDepth; Again ML thinks no depth
        //        availableWater = soilWaterContent - waterEmpty;
        //        //RMS: PlugIn.ModelCore.UI.WriteLine("Yr={0},Mo={1}, availableWater={2:0.0}, SWC={3:0.0}", year, month, availableWater, soilWaterContent);

        //        if (availableWater < 0.0) availableWater = 0.0;

        //        //...Calculate transpiration loss from layer, using weighted
        //        //     water availabilities:
        //        double transpirationLoss = (transpiration * availableWaterWeighted) / availableWaterWeighted;

        //        if (transpirationLoss > availableWater)
        //            transpirationLoss = availableWater;

        //        soilWaterContent -= transpirationLoss;
        //        availableWater -= transpirationLoss;
        //        //tran = tran + transpirationLoss;  // ????
        //        //relativeWaterContent = ((soilWaterContent / soilDepth) - wiltingPoint) / (fieldCapacity - wiltingPoint); ML thinks this is incorrect
        //        relativeWaterContent = (soilWaterContent) / fieldCapacity;  //This is wrong too, but a placeholder for now

        //        PlugIn.ModelCore.UI.WriteLine("Line 335. Transpiration={0:0.000}, TranspirationLoss={1:0.0}, availableWater={2:0.0}, soilwaterContent={3:0.00}, relativeWaterContent={4:0.00}, fieldcapacity={5:0.000}.", transpiration, transpirationLoss, availableWater, soilWaterContent, relativeWaterContent, fieldCapacity);
        //    }

        //    // **************************************************************************
        //    //...Evaporate water from the top layer
        //    //...Rewritten by Pulliam, should still do the same thing 9/94

        //    //...Minimum relative water content for top layer to evaporate:
        //    //double fwlos = 0.25;

        //    //...Fraction of water content between fwlos and field capacity:
        //    //double evmt = (relativeWaterContent - fwlos) / (1.0 - fwlos);
        //    ////PlugIn.ModelCore.UI.WriteLine("evmt={0:0.0}, relativeWaterContent={1:0.00}, fwlos={2}.", evmt,relativeWaterContent,fwlos);

        //    //if (evmt <= 0.01) evmt = 0.01;

        //    //...Evaporation loss from layer 1:
        //    //double evapLossTop = evmt * potentialEvapTop * bareSoilEvap * 0.10;

        //    //double topWater = soilWaterContent - wiltingPoint * soilDepth;
        //    //if (topWater < 0.0) topWater = 0.0;  //topWater = max evaporative loss from surface
        //    //if (evapLossTop > topWater) evapLossTop = topWater;

        //    ////...Update available water pools minus evaporation from top layer
        //    //availableWater -= evapLossTop;
        //    //soilWaterContent -= evapLossTop;

        //    PlugIn.ModelCore.UI.WriteLine("Line 362. evapLossTop={0:0.00000}, availablewater={1:0.00}, SWC={2:0.0000}", evapLossTop, availableWater, soilWaterContent);
        //    //totalEvaporated += evapLossTop;

        //    //...Recalculate relative Water Content to estimate mid-month water content
        //    double avhsm = (soilWaterContent + relativeWaterContent * asimx) / (1.0 + relativeWaterContent);
        //    relativeWaterContent = ((avhsm / soilDepth) - wiltingPoint) / (fieldCapacity - wiltingPoint);

        //    PlugIn.ModelCore.UI.WriteLine("Line 370. soilwaterContent={0:0.00}, relativeWaterContent={1:0.00}, avhsm={2:0.00}.", soilWaterContent, relativeWaterContent, avhsm);

        //    // Compute the ratio of precipitation to PET
        //    double ratioPrecipPET = 0.0;
        //    if (pet > 0.0) ratioPrecipPET = (availableWater + H2Oinputs) / pet;

        //    PlugIn.ModelCore.UI.WriteLine("Line 375. ratioPrecipPET={0:0.0}, totalAvailableH20={1:0.00}, H2Oinputs={2:0.00}, pet={3:0.00}.", ratioPrecipPET, availableWater, H2Oinputs, pet);

        //    SiteVars.WaterMovement[site] = waterMovement;
        //    SiteVars.AvailableWater[site] = availableWater;  //available to plants for growth
        //    SiteVars.SoilWaterContent[site] = soilWaterContent;
        //    SiteVars.SoilTemperature[site] = CalculateSoilTemp(tmin, tmax, liveBiomass, litterBiomass, month);
        //    SiteVars.DecayFactor[site] = CalculateDecayFactor((int)OtherData.WType, SiteVars.SoilTemperature[site], relativeWaterContent, ratioPrecipPET, month);
        //    SiteVars.AnaerobicEffect[site] = CalculateAnaerobicEffect(drain, ratioPrecipPET, pet, tave);

        //    //SoilWater.Leach(site, baseFlow, stormFlow);

        //    PlugIn.ModelCore.UI.WriteLine("Line 386. availH2O={0}, soilH2O={1}, wiltP={2}, soilCM={3}, waterMovement={4}, RatioPrecipPET={5}", availableWater, soilWaterContent, wiltingPoint, soilDepth, waterMovement, ratioPrecipPET);
        //    //PlugIn.ModelCore.UI.WriteLine("   yr={0}, mo={1}, DecayFactor={2:0.00}, Anaerobic={3:0.00}.", year, month, SiteVars.DecayFactor[site], SiteVars.AnaerobicEffect[site]);

        //    return;
        //}
    }
}

