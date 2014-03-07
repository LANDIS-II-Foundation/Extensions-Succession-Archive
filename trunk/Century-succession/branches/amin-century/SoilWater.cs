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

            //...Description of variables
            //   deadBiomass        the average monthly standing dead biomass(gm/m..2)
            //   soilDepth          depth of the ith soil layer(cm)
            //   fieldCapacity      the field capacity of the ith soil layer(fraction)
            //   litterBiomass      the average monthly litter biomass(gm/m..2)
            //   liveBiomass        the average monthly live plant biomass(gm/m..2)
            //   waterMovement      the index for water movement(0-no flow,1-satruated flow)
            //   soilWaterContent   the soil water content of the ith soil layer(cm h2o)
            //   asnow              the snow pack water contint(cm-h2o)
            //   avh2o (1)          NA water available to plants for growth
            //   avh2o (2)          NA water available to plants for survival
            //                      (available water in the whole soil profile)
            //   availableWater     available water in current soil layer
            //   wiltingPoint       the wilting point of the  ith soil layer(fraction)
            //   transpLossFactor   the weight factor for transpiration water loss 
            //   totalEvaporated               the water evaporated from the  soil and vegetation(cm/mon)
            //   evaporatedSnow             snow evaporated
            //   inputs             rain + irrigation
            //   H2Oinputs            inputs which are water (not converted to snow)
            //   nlayer             NA number of soil layers with water available for plant survival
            //   nlaypg             NA number of soil layers with water available for plant growth
            //   remainingPET       remaining pet, updated after each incremental h2o loss
            //   potentialEvapTop               the potential evaporation rate from the top  soil layer (cm/day)
            //   rain               the total monthly rainfall (cm/month)
            //   relativeWaterContent        the relative water content of the ith soil layer(0-1)
            //   liquidSnowpack               the liquid water in the snow pack
            //   tav                average monthly air temperature (2m-        //)
            //   tran               transpriation water loss(cm/mon)
            //   transpirationLoss  transpiration water loss

            //...Initialize Local Variables
            double addToSoil = 0.0;
            double bareSoilEvap = 0.0;
            baseFlow = 0.0;
            double totalEvaporated = 0.0;
            double evaporativeLoss = 0.0;
            double potentialTrans = 0.0;
            double relativeWaterContent = 0.0;
            double snow = 0.0;
            double liquidSnowpack = 0.0;
            stormFlow = 0.0;
            double tran = 0.0;
            double transpiration = 0.01;

            //...Calculate external inputs
            IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];

            double litterBiomass = (SiteVars.SurfaceStructural[site].Carbon + SiteVars.SurfaceMetabolic[site].Carbon) * 2.0;
            double deadBiomass = SiteVars.SurfaceDeadWood[site].Carbon * 2.0;
            double soilWaterContent = SiteVars.SoilWaterContent[site];

            H2Oinputs = EcoregionData.AnnualWeather[ecoregion].MonthlyPrecip[month]; //rain + irract;
            tave = EcoregionData.AnnualWeather[ecoregion].MonthlyTemp[month];
            tmax = EcoregionData.AnnualWeather[ecoregion].MonthlyMaxTemp[month];
            tmin = EcoregionData.AnnualWeather[ecoregion].MonthlyMinTemp[month];
            pet = EcoregionData.AnnualWeather[ecoregion].MonthlyPET[month];

            //PlugIn.ModelCore.UI.WriteLine(" tave={0}, tmax={1}, tmin={2}, pet={3}, ppt={4}.", tave, tmax, tmin, pet, H2Oinputs);
            //double soilTemp         = tave;    

            double wiltingPoint = EcoregionData.WiltingPoint[ecoregion];
            double soilDepth = EcoregionData.SoilDepth[ecoregion];
            double fieldCapacity = EcoregionData.FieldCapacity[ecoregion];
            double stormFlowFraction = EcoregionData.StormFlowFraction[ecoregion];
            double baseFlowFraction = EcoregionData.BaseFlowFraction[ecoregion];
            double drain = EcoregionData.Drain[ecoregion];

            deadBiomass = 0.0;

            //...Throughout, uses remainingPET as remaining energy for pet after
            //     each melting and evaporation step.  Initially calculated
            //     pet is not modified.  Pulliam 9/94
            double remainingPET = pet;

            //...Determine the snow pack, melt snow, and evaporate from the snow pack
            //...When mean monthly air temperature is below freezing,
            //     precipitation is in the form of snow.
            if (tave < 0.0)
            {
                snow = H2Oinputs; //snow + inputs;
                H2Oinputs = 0.0;
            }

            //...Melt snow if air temperature is above minimum (tmelt(1))
            if (tave > OtherData.TMelt1)
            {
                //...Calculate the amount of snow to melt:
                double snowMelt = OtherData.TMelt2 * (tave - OtherData.TMelt1);

                if (snowMelt > snow)
                    snowMelt = snow;
                snow = snow - snowMelt;

                //..Melted snow goes to snow pack and drains excess
                //  addToSoil rain-on-snow  and melted snow to snowpack liquid (liquidSnowpack):
                if (tave > 0.0 && snow > 0.0)
                    liquidSnowpack = H2Oinputs;

                liquidSnowpack = liquidSnowpack + snowMelt;

                //...Drain snowpack to 5% liquid content (weight/weight), excess to soil:
                if (liquidSnowpack > (0.05 * snow))
                {
                    addToSoil = liquidSnowpack - 0.05 * snow;
                    liquidSnowpack = liquidSnowpack - addToSoil;
                }
            }

            //...Evaporate water from the snow pack (rewritten Pulliam 9/94 to
            //     evaporate from both snow aqnd liquidSnowpack in proportion)
            //...Coefficient 0.87 relates to heat of fusion for ice vs. liquid water
            //     wasn't modified as snow pack is at least 95% ice.
            if (snow > 0)
            {
                //...Calculate cm of snow that remaining pet energy can evaporate:
                double evaporatedSnow = remainingPET * 0.87;

                //...Calculate total snowpack water, ice + liquid:
                double totalSnowpack = snow + liquidSnowpack;

                //...Don't evaporate more snow than actually exists:
                if (evaporatedSnow > totalSnowpack)
                    evaporatedSnow = totalSnowpack;

                //...Take evaporatedSnow from snow and liquidSnowpack in proportion:
                snow = snow - evaporatedSnow * (snow / totalSnowpack);
                liquidSnowpack = liquidSnowpack - evaporatedSnow * (liquidSnowpack / totalSnowpack);

                //...addToSoil evaporated snow to evaporation accumulator (totalEvaporated):
                totalEvaporated = evaporatedSnow;

                //...Decrement remaining pet by energy used to evaporate snow:
                remainingPET = remainingPET - evaporatedSnow / 0.87;

                if (remainingPET < 0.0)
                    remainingPET = 0.0;
            }

            //...Calculate bare soil water loss and interception
            //     when air temperature is above freezing and no snow cover.
            //...Mofified 9/94 to allow interception when t < 0 but no snow
            //     cover, Pulliam
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
                //PlugIn.ModelCore.UI.WriteLine("bareSoilEvap={0}, litterBiomass={1}, standingBiomass={2}.", bareSoilEvap, litterBiomass, standingBiomass);

                //...Calculate total surface evaporation losses, maximum
                //     allowable is 0.4 * pet. -rm 6/94
                evaporativeLoss = System.Math.Min(((bareSoilEvap + canopyIntercept) * H2Oinputs), (0.4 * remainingPET));
                totalEvaporated = totalEvaporated + evaporativeLoss;

                //...Calculate remaining water to addToSoil to soil and potential
                //     transpiration as remaining pet:
                addToSoil = H2Oinputs - evaporativeLoss;
                transpiration = remainingPET - evaporativeLoss;
                //RMS: PlugIn.ModelCore.UI.WriteLine("Yr={0},Mo={1}, evaporativeLoss={2:0.0}, addToSoil={3:0.0}, remainingPET={4:0.0}", year, month, evaporativeLoss, addToSoil, remainingPET);

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

            // **************************************************************************
            //...Calculate the potential evaporation rate from the top soil layer
            //     (potentialEvapTop-cm/day).  This is not actually taken out until after
            //     transpiration losses
            double potentialEvapTop = remainingPET - transpiration - evaporativeLoss;

            //RMS: PlugIn.ModelCore.UI.WriteLine("Yr={0},Mo={1}, PotentialEvapTop={2:0.0}, remainingPET={3:0.0}, transpiration={4:0.0}, evaporativeLoss={5:0.0}.", year, month, potentialEvapTop, remainingPET, transpiration, evaporativeLoss);
            if (potentialEvapTop < 0.0) potentialEvapTop = 0.0;

            // **************************************************************************
            //...Transpire water from added water first, before passing water
            //     on to soil.  This is necessary for a monthly time step to
            //     give plants in wet climates adequate access to water for
            //     transpiration. -rm 6/94, Pulliam 9/94
            tran = System.Math.Min((transpiration - 0.01), addToSoil);
            transpiration = transpiration - tran;
            //RMS: PlugIn.ModelCore.UI.WriteLine("Yr={0},Mo={1}, tran={2:0.0}, transpiration={3:0.0}, addToSoil={4:0.00}.", year, month, tran, transpiration, addToSoil);
            addToSoil = addToSoil - tran;

            //...Add water to the soil
            //...Changed to add base flow and storm flow.  -rm 2/92
            //...addToSoil water to layer:

            soilWaterContent += addToSoil;
            // PlugIn.ModelCore.UI.WriteLine("Yr={0},Mo={1}, soilWaterContent={2:0.0}, addToSoil={3:0.0}.", year, month, soilWaterContent,addToSoil);

            //...Calculate field capacity of soil, drain soil, pass excess
            //     on to waterMovement:
            double waterFull = soilDepth * fieldCapacity;  //units of cm
            double waterMovement = 0.0;

            if (soilWaterContent > waterFull)
            {
                //RMS: PlugIn.ModelCore.UI.WriteLine("Yr={0},Mo={1}, soilWaterContent > waterFull", year, month); 
                waterMovement = (soilWaterContent - waterFull); // / soilWaterContent;
                soilWaterContent = waterFull;

                //...Compute storm flow.
                stormFlow = waterMovement * stormFlowFraction;
            }

            //...Compute base flow and stream flow for H2O. 
            //...Put water draining out bottom that doesn't go to stormflow
            //     into nlayer+1 holding tank:
            double drainedWater = addToSoil - stormFlow;

            //...Drain baseflow fraction from holding tank:
            baseFlow = drainedWater * baseFlowFraction;
            drainedWater = drainedWater - baseFlow;

            //...Streamflow = stormflow + baseflow:
            double streamFlow = stormFlow + baseFlow;

            //PlugIn.ModelCore.UI.WriteLine("Yr={0},Mo={1}, soilWaterContent={2:0.0}, stormFlow={3:0.00}, baseFlow={4:0.00}.", year, month, soilWaterContent, stormFlow, baseFlow);

            //...Save soilWaterContent before transpiration for future use:
            double asimx = soilWaterContent;

            // **************************************************************************
            //...Calculate transpiration water loss from each layer
            //...This section was completely rewritten by Pulliam, though it
            //     should still do the same thing.  9/94

            //...Calculate available water in layer, soilWaterContent minus wilting point:
            double availableWater = soilWaterContent - wiltingPoint * soilDepth;
            //RMS: PlugIn.ModelCore.UI.WriteLine("Yr={0},Mo={1}, availableWater={2:0.0}, soilWaterContent={3:0.0}.", year, month, availableWater, soilWaterContent);

            if (availableWater < 0.0)
                availableWater = 0.0;

            //...Calculate available water weighted by transpiration loss depth
            //      distribution factors:
            double availableWaterWeighted = availableWater * OtherData.TranspirationLossFactor;

            //...Calculate the actual transpiration water loss(tran-cm/mon)
            //...Also rewritten by Pulliam 9/94, should do the same thing
            //...Update potential transpiration to be no greater than available water:
            transpiration = System.Math.Min(availableWater, transpiration);

            //...Transpire water from layer:

            if (availableWaterWeighted > 0.0)
            {
                //...Calculate available water in layer j:
                //for( j = 0; j < nlayer; j++)
                availableWater = soilWaterContent - wiltingPoint * soilDepth;
                //RMS: PlugIn.ModelCore.UI.WriteLine("Yr={0},Mo={1}, availableWater={2:0.0}, SWC={3:0.0}", year, month, availableWater, soilWaterContent);

                if (availableWater < 0.0) availableWater = 0.0;

                //...Calculate transpiration loss from layer, using weighted
                //     water availabilities:
                double transpirationLoss = (transpiration * availableWaterWeighted) / availableWaterWeighted;

                if (transpirationLoss > availableWater)
                    transpirationLoss = availableWater;

                soilWaterContent -= transpirationLoss;
                availableWater -= transpirationLoss;
                //tran = tran + transpirationLoss;  // ????
                relativeWaterContent = ((soilWaterContent / soilDepth) - wiltingPoint) / (fieldCapacity - wiltingPoint);

                //RMS: PlugIn.ModelCore.UI.WriteLine("Yr={0},Mo={1}, TranspirationLoss={2:0.0}, availableWater={3:0.0}, soilwaterContent={4:0.00}, relativeWaterContent={5:0.00}.", year, month, transpirationLoss, availableWater, soilWaterContent, relativeWaterContent);
            }

            // **************************************************************************
            //...Evaporate water from the top layer
            //...Rewritten by Pulliam, should still do the same thing 9/94

            //...Minimum relative water content for top layer to evaporate:
            double fwlos = 0.25;

            //...Fraction of water content between fwlos and field capacity:
            double evmt = (relativeWaterContent - fwlos) / (1.0 - fwlos);
            //PlugIn.ModelCore.UI.WriteLine("evmt={0:0.0}, relativeWaterContent={1:0.00}, fwlos={2}.", evmt,relativeWaterContent,fwlos);

            if (evmt <= 0.01) evmt = 0.01;

            //...Evaporation loss from layer 1:
            double evapLossTop = evmt * potentialEvapTop * bareSoilEvap * 0.10;

            double topWater = soilWaterContent - wiltingPoint * soilDepth;
            if (topWater < 0.0) topWater = 0.0;  //topWater = max evaporative loss from surface
            if (evapLossTop > topWater) evapLossTop = topWater;

            //...Update available water pools minus evaporation from top layer
            availableWater -= evapLossTop;
            soilWaterContent -= evapLossTop;

            //RMS: PlugIn.ModelCore.UI.WriteLine("Yr={0},Mo={1}, evapLossTop={2:0.00}, SWC={3:0.00}", year, month, evapLossTop, soilWaterContent);
            //totalEvaporated += evapLossTop;

            //...Recalculate relative Water Content to estimate mid-month water content
            double avhsm = (soilWaterContent + relativeWaterContent * asimx) / (1.0 + relativeWaterContent);
            relativeWaterContent = ((avhsm / soilDepth) - wiltingPoint) / (fieldCapacity - wiltingPoint);

            //RMS: PlugIn.ModelCore.UI.WriteLine("Yr={0},Mo={1}, soilwaterContent={2:0.00}, relativeWaterContent={3:0.00}.", year, month,soilWaterContent, relativeWaterContent);

            // Compute the ratio of precipitation to PET
            double ratioPrecipPET = 0.0;
            if (pet > 0.0) ratioPrecipPET = (availableWater + H2Oinputs) / pet;

            //PlugIn.ModelCore.UI.WriteLine("ratioPrecipPET={0:0.0}, totalAvailableH20={1:0.00}, H2Oinputs={2:0.00}, pet={3:0.00}.", ratioPrecipPET, availableWater, H2Oinputs, pet);

            SiteVars.WaterMovement[site] = waterMovement;
            SiteVars.AvailableWater[site] = availableWater;  //available to plants for growth
            SiteVars.SoilWaterContent[site] = soilWaterContent;
            SiteVars.SoilTemperature[site] = CalculateSoilTemp(tmin, tmax, liveBiomass, litterBiomass, month);
            SiteVars.DecayFactor[site] = CalculateDecayFactor((int)OtherData.WType, SiteVars.SoilTemperature[site], relativeWaterContent, ratioPrecipPET, month);
            SiteVars.AnaerobicEffect[site] = CalculateAnaerobicEffect(drain, ratioPrecipPET, pet, tave);

            //SoilWater.Leach(site, baseFlow, stormFlow);

            //PlugIn.ModelCore.UI.WriteLine("availH2O={0}, soilH2O={1}, wiltP={2}, soilCM={3}, waterMovement={4}", availableWater, soilWaterContent, wiltingPoint, soilDepth, waterMovement);
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

