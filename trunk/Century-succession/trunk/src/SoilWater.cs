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

            //PlugIn.ModelCore.UI.WriteLine("year = {0}, month = {1}", year, month);
            //Originally from h2olos.f of CENTURY model
            //Water Submodel for Century - written by Bill Parton
            //     Updated from Fortran 4 - rm 2/92
            //     Rewritten by Bill Pulliam - 9/94
            // Rewritten by Melissa Lucash and Rob Scheller- 11/2014
        

            //...Initialize Local Variables
            double addToSoil = 0.0;
            double bareSoilEvap = 0.0;
            baseFlow = 0.0;
            double totalEvaporated = 0.0;  
            double potentialTrans = 0.0;
            double relativeWaterContent = 0.0;
            double snow = 0.0;
            double liquidSnowpack = 0.0;
            stormFlow = 0.0;
            double transpiration = 0.01;
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

            H2Oinputs = EcoregionData.AnnualWeather[ecoregion].MonthlyPrecip[month]; //rain + irract in cm;
            tave = EcoregionData.AnnualWeather[ecoregion].MonthlyTemp[month];
            tmax = EcoregionData.AnnualWeather[ecoregion].MonthlyMaxTemp[month];
            tmin = EcoregionData.AnnualWeather[ecoregion].MonthlyMinTemp[month];
            pet = EcoregionData.AnnualWeather[ecoregion].MonthlyPET[month];

            PlugIn.ModelCore.UI.WriteLine("Line 91. Really just the inputs. Year={0}, month={1}, AvgMonthlyTemp={2}, tmax={3}, tmin={4}, pet={5:0.0000}, ppt={6}, soilWaterContent={7:0.0000}.", Century.Year, Century.Month, tave, tmax, tmin, pet, H2Oinputs, soilWaterContent);
            double wiltingPoint = EcoregionData.WiltingPoint[ecoregion];
            double soilDepth = EcoregionData.SoilDepth[ecoregion];
            double fieldCapacity = EcoregionData.FieldCapacity[ecoregion];
            double stormFlowFraction = EcoregionData.StormFlowFraction[ecoregion];
            double baseFlowFraction = EcoregionData.BaseFlowFraction[ecoregion];
            double drain = EcoregionData.Drain[ecoregion];

            //PlugIn.ModelCore.UI.WriteLine("Really just more inputs. wiltingpoint={0}, soildepth={1}, fieldcapacity={2}, stormflow={3}, baseflow={4}, drainage={5}.", wiltingPoint, soilDepth, fieldCapacity, stormFlowFraction, baseFlowFraction, drain);

            deadBiomass = 0.0;  //ML: Why is this set to zero here?  
                                   
            //...Determine the snow pack, melt snow, and evaporate from the snow pack
            
            //...Calculating snow pack first. Occurs when mean monthly air temperature is below freezing,
            //     precipitation is in the form of snow.
            //if (tave < 0.0)
            if (tave <= 2.0) // ML: We could use Avg Tmin here instead.  I shifted it to 2oC because it can (and often does) snow at 2oC.  
            {
                snow = H2Oinputs; //snow + inputs;
                H2Oinputs = 0.0;  //ML: Why does this get set to zero here?
                PlugIn.ModelCore.UI.WriteLine("Let it snow!! snow={0}, waterinputs={1}.", snow, H2Oinputs);
            }

            //...Then melt snow if air temperature is above minimum (tmelt(1))
            //if (tave > OtherData.TMelt1)  ///ML: This used to be -8, which seemed very low.  And all the snow melted by Feb in MN.
            if (tave > 0.0)
            {
                //...Calculate the amount of snow to melt:
                //double snowMelt = OtherData.TMelt2 * (tave - OtherData.TMelt1);  //1 is 4.0, 2 is -8  ML: this gave incredibly high values that were not biologically meaningful
                double snowMeltFraction = (tave * 0.05) + 0.024;//This equation assumes a linear increase in the fraction of snow that melts as a function of air temp

               if (snowMeltFraction > 1.0)
                    snowMeltFraction = 0.0;

                snow = snow * snowMeltFraction;                         

                //..Melted snow goes to snow pack and drains excess
                //  addToSoil rain-on-snow  and melted snow to snowpack liquid (liquidSnowpack):
                if (tave > 0.0 && snow > 0.0)
                    liquidSnowpack = H2Oinputs;

                liquidSnowpack = liquidSnowpack + snow;
                //PlugIn.ModelCore.UI.WriteLine("Line 136. Melting snow. LiquidSnowpack={0}, snow={1}, snowMelt={2}.", liquidSnowpack, snow, snowMelt);

                //...Drain snowpack to 5% liquid content (weight/weight), excess to soil:
                if (liquidSnowpack > (0.05 * snow))
                {
                    addToSoil = liquidSnowpack - 0.05 * snow;
                    liquidSnowpack = liquidSnowpack - addToSoil;
                }
                soilWaterContent += addToSoil;
                availableWaterMax = soilWaterContent;
                PlugIn.ModelCore.UI.WriteLine("Line 120. Melting snow. snowMeltFraction={0}, snow={1}, LiquidSnowpack={2}, availableWaterMax={3}.", snowMeltFraction, snow, liquidSnowpack, addToSoil, availableWaterMax);
            }

            //...Evaporate water from the snow pack (rewritten Pulliam 9/94 to
            //     evaporate from both snow aqnd liquidSnowpack in proportion)
            //...Coefficient 0.87 relates to heat of fusion for ice vs. liquid water
            //     wasn't modified as snow pack is at least 95% ice.
           
            if (snow > 0.0)
            {
                //...Calculate cm of snow that remaining pet energy can evaporate:
                double evaporatedSnow = pet * 0.87;

                //...Calculate total snowpack water, ice + liquid:
                double totalSnowpack = snow + liquidSnowpack;

                //...Don't evaporate more snow than actually exists:
                if (evaporatedSnow > totalSnowpack)
                    evaporatedSnow = totalSnowpack;

                //...Take evaporatedSnow from snow and liquidSnowpack in proportion:
                snow = snow - evaporatedSnow * (snow / totalSnowpack);
                liquidSnowpack = liquidSnowpack - evaporatedSnow * (liquidSnowpack / totalSnowpack);

                //...Decrement remaining pet by energy used to evaporate snow:
                remainingPET = pet - evaporatedSnow / 0.87;

                if (remainingPET < 0.0) 
                    remainingPET = 0.0;

                //Add evaporated snow to accumulator of evaporation and substract it from the soil water content
                totalEvaporated = evaporatedSnow;
                soilWaterContent -= evaporatedSnow;

                PlugIn.ModelCore.UI.WriteLine("Line 152. Evaporating snow. TotalEvaporated={0:0.000}, remainingPET={1}, totalSnowpack={2:0.00}, soilWaterContent={3:0.00}.", totalEvaporated, remainingPET, totalSnowpack, soilWaterContent);
            }

            //Allow excess water to run off during storm events (stormflow)
            double waterFull = soilDepth * fieldCapacity;  //units of cm
            
            double waterMovement = 0.0;            

            if (soilWaterContent > waterFull)
            {

                waterMovement = (soilWaterContent - waterFull); // / soilWaterContent;
                soilWaterContent = waterFull;
                
                //...Compute storm flow.
                stormFlow = waterMovement * stormFlowFraction;

                PlugIn.ModelCore.UI.WriteLine("Line 171. soilwatercontent={0:0.00}, waterfull={0:0.00}, watermovement={0:0.00}, stormflow={0:0.00} soilWaterContent > waterFull.", soilWaterContent, waterFull, waterMovement, stormFlow);
                
            }                                                 
            
            //...Calculate bare soil water loss and interception  when air temperature is above freezing and no snow cover.
            //...Mofified 9/94 to allow interception when t < 0 but no snow cover, Pulliam
            if (snow <= 0.0)
            {
                //...Calculate total canopy cover and litter, put cap on effects:
                double standingBiomass = liveBiomass + deadBiomass;

                if (standingBiomass > 800.0) standingBiomass = 800.0;
                if (litterBiomass > 400.0) litterBiomass = 400.0;

                //...canopy interception, fraction of  precip (canopyIntercept):
                double canopyIntercept = ((0.0003 * litterBiomass) + (0.0006 * standingBiomass)) * OtherData.WaterLossFactor1;

                //...Bare soil evaporation, fraction of precip (bareSoilEvap):
                bareSoilEvap = 0.5 * System.Math.Exp((-0.002 * litterBiomass) - (0.004 * standingBiomass)) * OtherData.WaterLossFactor2;
                PlugIn.ModelCore.UI.WriteLine("If there is no snow. Line 191. BareSoilEvap={0}, litterBiomass={1}, standingBiomass={2}, CanopyIntercept={3}.", bareSoilEvap, litterBiomass, standingBiomass, canopyIntercept);

                //...Calculate total surface evaporation losses, maximum
                //     allowable is 0.4 * pet. -rm 6/94
                double soilEvaporation= System.Math.Min(((bareSoilEvap + canopyIntercept) * H2Oinputs), (0.4 * remainingPET));
                totalEvaporated += soilEvaporation;

                //...Calculate remaining water to addToSoil to soil and subtract soil evaporation from soil water content
                addToSoil = H2Oinputs - totalEvaporated;
                soilWaterContent -= soilEvaporation;
                
                PlugIn.ModelCore.UI.WriteLine("Line 202. This is the amount added to soil when no snow. bareSoilEvap={0:0.0}, soilevaporated={1:0.0}, TotalEvap={2:0.0}, addToSoil={3:0.0}", bareSoilEvap, soilEvaporation, totalEvaporated, addToSoil);

            }

            // **************************************************************************
            //...Determine potential transpiration water loss (transpiration, cm/mon) as a
            //     function of precipitation and live biomass.
            //...If temperature is less than 2C turn off transpiration. -rm 6/94
            if (tave < 2.0)
                potentialTrans = 0.0;
            else
                potentialTrans = remainingPET * 0.65 * (1.0 - System.Math.Exp(-0.020 * liveBiomass));
           
            if (potentialTrans < transpiration)
                transpiration = potentialTrans;

            if (transpiration < 0.0) transpiration = 0.01;
            PlugIn.ModelCore.UI.WriteLine("Line 218. Potential Transpiration. PotentialTranspiration={0:0.000}.", potentialTrans);


            // Calculate actual evapotranspiration
            double waterEmpty = wiltingPoint * soilDepth;

            if (soilWaterContent > waterFull)
                actualET = transpiration;
            else
            {
                actualET = transpiration * ((soilWaterContent - waterEmpty) / (waterFull - waterEmpty));
            }

            if (actualET < 0.0)
                actualET = 0.0;

            //Subtract transpiration from soil water
            soilWaterContent -= actualET;

            PlugIn.ModelCore.UI.WriteLine("Line 237. Calculating AET. actualET={0:0.000}, waterFull={1:0.000}, waterEmpty={2:0.000}, pet={3:0.000}", actualET, waterFull, waterEmpty, pet);

              //...Drain baseflow fraction from holding tank:
            baseFlow = soilWaterContent * baseFlowFraction;
            double streamFlow = stormFlow + baseFlow;

            //Subtract transpiration from soil water
            soilWaterContent -= streamFlow;

            PlugIn.ModelCore.UI.WriteLine("Line 246. StormFlowStuff.stormFlow={0:0.000}, baseflow={1:0.000}, streamflow={2:0.0000}, soilwaterContent={3:0.0000}.", stormFlow, baseFlow, streamFlow, soilWaterContent);

            //Calculate the amount of available water after all the evapotranspiration and leaching has taken place (minimum available water)           
            availableWaterMin = soilWaterContent;

            //Calculate the final amount of available water, which is the average of the max and min          
            availableWater = (availableWaterMax - availableWaterMin)/ 2;
            PlugIn.ModelCore.UI.WriteLine("Line 253. availablewater={0:0.000}, availableWaterMax={0:0.000}, availableWaterMin={0:0.000}.", availableWater, availableWaterMax, availableWaterMin);
            
            //// Compute the ratio of precipitation to PET
            double ratioPrecipPET = 0.0;
            if (pet > 0.0) ratioPrecipPET = (availableWater + H2Oinputs) / pet;

            //PlugIn.ModelCore.UI.WriteLine("Line 375. ratioPrecipPET={0:0.0}, totalAvailableH20={1:0.00}, H2Oinputs={2:0.00}, pet={3:0.00}.", ratioPrecipPET, availableWater, H2Oinputs, pet);

            SiteVars.WaterMovement[site] = waterMovement;
            SiteVars.AvailableWater[site] = availableWater;  //available to plants for growth
            SiteVars.SoilWaterContent[site] = soilWaterContent;
            SiteVars.SoilTemperature[site] = CalculateSoilTemp(tmin, tmax, liveBiomass, litterBiomass, month);
            SiteVars.DecayFactor[site] = CalculateDecayFactor((int)OtherData.WType, SiteVars.SoilTemperature[site], relativeWaterContent, ratioPrecipPET, month);
            SiteVars.AnaerobicEffect[site] = CalculateAnaerobicEffect(drain, ratioPrecipPET, pet, tave);

            //SoilWater.Leach(site, baseFlow, stormFlow);

            PlugIn.ModelCore.UI.WriteLine("Line 386. availH2O={0}, soilH2O={1}, wiltP={2}, soilCM={3}, waterMovement={4}, RatioPrecipPET={5}", availableWater, soilWaterContent, wiltingPoint, soilDepth, waterMovement, ratioPrecipPET);
            //PlugIn.ModelCore.UI.WriteLine("   yr={0}, mo={1}, DecayFactor={2:0.00}, Anaerobic={3:0.00}.", year, month, SiteVars.DecayFactor[site], SiteVars.AnaerobicEffect[site]);

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

            double aneref1 = OtherData.RatioPrecipPETMaximum;
            double aneref2 = OtherData.RatioPrecipPETMinimum;
            double aneref3 = OtherData.AnerobicEffectMinimum;

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
                }

                if (anerob < aneref3)
                    anerob = aneref3;
            }

            //PlugIn.ModelCore.UI.WriteLine("Anaerobic Effect = {1}.", anerob);
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
                amtNLeached = textureEffect * SiteVars.MineralN[site] * OtherData.NfracLeachWater * OtherData.NO3frac;

                //PlugIn.ModelCore.UI.WriteLine("amtNLeach={0:0.0}, textureEffect={1:0.0}, waterMove={2:0.0}, MineralN={3.00}", amtNLeached, textureEffect, waterMove, SiteVars.MineralN[site]);      
            }        
            

            double totalNleached = (baseFlow * amtNLeached) + (stormFlow * amtNLeached);
            
            SiteVars.MineralN[site] -= totalNleached;
            //PlugIn.ModelCore.UI.WriteLine("AfterLeach.totalNLeach={0:0.0}, MineralN={1:0.00}", totalNleached, SiteVars.MineralN[site]);         

            SiteVars.Stream[site].Nitrogen += totalNleached;

            return;
        }
    }
}

