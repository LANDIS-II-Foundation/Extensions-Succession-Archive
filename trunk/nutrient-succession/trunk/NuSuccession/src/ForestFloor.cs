//  Copyright 2007-2010 University of Nevada, Portland State University
//  Authors:  Sarah Ganschow, Robert M. Scheller
//  License:  Available at
//  http://www.landis-ii.org/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Util;
using Landis.Ecoregions;
using Landis.Landscape;
using Landis.Species;

using System;
using System.Collections.Generic;

namespace Landis.Biomass.NuCycling.Succession
{
    /// <summary>
    /// Deals with additions to and subtractions from litter and coarse
    ///   woody debris pools (i.e., the forest floor).
    /// </summary>
    public class ForestFloor
    {
        /// <summary>
        /// Adds biomass for a species to the woody debris pool at a site.
        /// </summary>
        public static void AddWoodyDebris(double woodBiomass,
                                          ISpecies species,
                                          PoolD woodyDebris)
        {
            woodyDebris.AddMass(woodBiomass, SpeciesData.WoodFractionC[species],
                SpeciesData.WoodFractionN[species], SpeciesData.WoodFractionP[species],
                SpeciesData.WoodyDebrisDecay[species]);
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Adds biomass for a species to the annual litter pool at a site.
        /// </summary>
        public static void AddLitter(double leafBiomass,
                                     ISpecies species,
                                     ActiveSite site,
                                     List<PoolD> litterAdd)
        {
            IEcoregion ecoregion = Model.Core.Ecoregion[site];
            double siteAET = (double)(EcoregionData.AET[ecoregion]);

            PoolD litterAddition = new PoolD();
            litterAddition.Mass = leafBiomass;
            litterAddition.InitialMass = leafBiomass;
            litterAddition.ContentC = SpeciesData.LitterFractionC[species] *
                litterAddition.Mass;
            litterAddition.ContentN = SpeciesData.LitterFractionN[species] *
                litterAddition.Mass;
            litterAddition.ContentP = SpeciesData.LitterFractionP[species] *
                litterAddition.Mass;

            //Calculate decomposition rate (Fan et al. 1998)
            double leafKReg = ((0.10015 * siteAET - 3.44618) - (0.01341 + 0.00147 * siteAET) *
                SpeciesData.LeafLignin[species]) / 100;
            double leafKStd = ((0.10015 * 551 - 3.44618) - (0.01341 + 0.00147 * 551) *
                SpeciesData.LeafLignin[species]) / 100;
            double leafKAdj = 8.35 * Math.Pow((SpeciesData.LeafLignin[species] /
                SpeciesData.LitterFractionN[species]), -0.784) * (leafKReg / leafKStd);
            litterAddition.DecayValue = leafKAdj;

            //Calculate mass loss limit (Berg et al. 1996). Average is 7% to
            //  soil organic matter (Verburg and Johnson 2001, other models).
            //  Use ceiling of 95% to ensure litter is removed.
            double limitValue = 0.01 * (91.241 - (1.744 *
                (SpeciesData.LitterFractionN[species] * 1000)));
            litterAddition.LimitValue = Math.Min(limitValue, 0.95);

            litterAdd.Add(litterAddition);
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Adds biomass for a species to the annual litter pool at a site due to
        ///   disturbance (meaning, N was not translocated).
        /// </summary>
        public static void AddDisturbanceLitter(double leafBiomass,
                                     ISpecies species,
                                     ActiveSite site,
                                     List<PoolD> litterAdd)
        {
            IEcoregion ecoregion = Model.Core.Ecoregion[site];
            double siteAET = (double)(EcoregionData.AET[ecoregion]);

            PoolD litterAddition = new PoolD();
            litterAddition.Mass = leafBiomass;
            litterAddition.InitialMass = leafBiomass;
            litterAddition.ContentC = SpeciesData.LeafFractionC[species] *
                litterAddition.Mass;
            litterAddition.ContentN = SpeciesData.LeafFractionN[species] *
                litterAddition.Mass;
            litterAddition.ContentP = SpeciesData.LeafFractionP[species] *
                litterAddition.Mass;

            //Calculate decomposition rate (Fan et al. 1998)
            double leafKReg = ((0.10015 * siteAET - 3.44618) - (0.01341 + 0.00147 * siteAET) *
                SpeciesData.LeafLignin[species]) / 100;
            double leafKStd = ((0.10015 * 551 - 3.44618) - (0.01341 + 0.00147 * 551) *
                SpeciesData.LeafLignin[species]) / 100;
            double leafKAdj = 8.35 * Math.Pow((SpeciesData.LeafLignin[species] /
                SpeciesData.LeafFractionN[species]), -0.784) * (leafKReg / leafKStd);
            litterAddition.DecayValue = leafKAdj;

            //Calculate mass loss limit (Berg et al. 1996). Average is 7% to
            //  soil organic matter (Verburg and Johnson 2001, other models).
            //  Use ceiling of 95% to ensure litter is removed.
            double limitValue = 0.01 * (91.241 - (1.744 *
                (SpeciesData.LeafFractionN[species] * 1000)));
            litterAddition.LimitValue = Math.Min(limitValue, 0.95);

            litterAdd.Add(litterAddition);
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Adds yearly addition to litter pools at a site.
        ///   Sums all species-cohort additions and adds as a single cohort.
        /// </summary>
        public static void AddYearLitter(ActiveSite site)
        {
            double addMass = 0.0;
            double addC = 0.0;
            double addN = 0.0;
            double addP = 0.0;
            double addDecay = 0.0;
            double addLimit = 0.0;

            foreach (PoolD litter in SiteVars.LitterAdd[site])
            {
                addMass += litter.Mass;
                addC += litter.ContentC;
                addN += litter.ContentN;
                addP += litter.ContentP;
                addDecay += (litter.DecayValue * litter.Mass);  //weighted avg
                addLimit += (litter.LimitValue * litter.Mass);  //weighted avg
            }

            PoolD LitterAddition = new PoolD();
            LitterAddition.Mass = addMass;
            LitterAddition.ContentC = addC;
            LitterAddition.ContentN = addN;
            LitterAddition.ContentP = addP;
            LitterAddition.DecayValue = addDecay / addMass;
            LitterAddition.LimitValue = addLimit / addMass;
            LitterAddition.InitialMass = addMass;
            SiteVars.Litter[site].Add(LitterAddition);

            SiteVars.LitterAdd[site].Clear();
            SiteVars.LitterAdd[site].TrimExcess();
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Decomposition for coarse woody debris (includes woody roots)
        ///   Not as detailed as litter decomposition because not a major source
        ///   of nutrients (avg less than 5% N, 10% P) (Laiho and Prescott 2004).
        ///
        /// Nutrient release modeled at same rate as mass loss because it accurately
        ///   reflects nutrient loss throughout decomposition (Laiho and Prescott 1999).
        /// </summary>
        public static void DecomposeWood(ActiveSite site)
        {
            SiteVars.WoodyDebris[site].Mass = SiteVars.WoodyDebris[site].Mass *
                Math.Exp(-1 * SiteVars.WoodyDebris[site].DecayValue);

            double decomposedC = SiteVars.WoodyDebris[site].ContentC - (SiteVars.WoodyDebris[site].ContentC *
                Math.Exp(-1 * SiteVars.WoodyDebris[site].DecayValue));
            SiteVars.WoodyDebris[site].ContentC -= decomposedC;

            double mineralizedN = SiteVars.WoodyDebris[site].ContentN - (SiteVars.WoodyDebris[site].ContentN *
                Math.Exp(-1 * SiteVars.WoodyDebris[site].DecayValue));
            SiteVars.WoodyDebris[site].ContentN -= mineralizedN;
            SiteVars.MineralSoil[site].ContentN += mineralizedN;

            double mineralizedP = SiteVars.WoodyDebris[site].ContentP - (SiteVars.WoodyDebris[site].ContentP *
                Math.Exp(-1 * SiteVars.WoodyDebris[site].DecayValue));
            SiteVars.WoodyDebris[site].ContentP -= mineralizedP;
            SiteVars.MineralSoil[site].ContentP += mineralizedP;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Decomposition for leaf litter in three phases (Berg model). Phase
        ///   transitions defined in if...else if...else if...else statements.
        /// </summary>
        public static void DecomposeLitter(PoolD litter,
                                           ActiveSite site)
        {
            double CNratio = litter.ContentC / litter.ContentN;
            double massLoss = (litter.InitialMass - litter.Mass) / litter.InitialMass;

            if (litter.Mass < 1.0)
            {
                SiteVars.RemoveLitter[site].Add(litter);
                return;
            }

            //Limit to mass loss determines cut-off betweeen phases 2 and 3.
            //Phase 3.
            else if (massLoss > litter.LimitValue)
            {
                ForestFloor.DecompPhase3(litter, site);

                return;
            }

            //Critical C:N < 40 used as cut-off between phases 1 and 2.
            //  (Upper bound in Prescott et al. 2000 and empirical finding
            //  in Janssen 1996).
            //Phase 1.
            else if (CNratio > 40)
            {
                ForestFloor.DecompPhase1(litter, site);

                return;
            }

            //Phase 2.
            else
            {
                ForestFloor.DecompPhase2(litter, site);

                return;
            }

        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Phase 1 of decomposition: Rapid decomposition with net nitrogen
        ///   immobilization and either no change or mineralization in
        ///   phosphorus.
        /// </summary>
        public static void DecompPhase1(PoolD litter,
                                        ActiveSite site)
        {
            litter.Mass = litter.Mass * Math.Exp(-1 * litter.DecayValue);
            litter.ContentC = litter.ContentC * Math.Exp(-1 * litter.DecayValue);

            //Net immobilization of nitrogen (Jannsen 1996 with application
            //  described in Noij et al. 1996).
            //Microbe CUE and C:N ratios set to defaults for conifers.
            double microbeCUE = 0.5;
            double microbeCN = 20;

            double mineralizedN = litter.ContentC * (litter.DecayValue /
                (litter.ContentC / litter.ContentN)) - microbeCUE *
                litter.ContentC * (litter.DecayValue / microbeCN);
            litter.ContentN -= mineralizedN;
            SiteVars.MineralSoil[site].ContentN += mineralizedN;

            //Critical C:P < 900 used as cut-off of net P mineralization
            //  (Prescott et al. 2000 upperbound). If C:P > 900, conserve
            //  P; else, mineralization occurs at decomposition rate.
            double CPratio = litter.ContentC / litter.ContentP;
            if (CPratio < 900)
            {
                double mineralizedP = litter.ContentP - (litter.ContentP *
                    Math.Exp(-1 * litter.DecayValue));
                litter.ContentP -= mineralizedP;
                SiteVars.MineralSoil[site].ContentP += mineralizedP;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Phase 2 of decomposition: Nitrogen and phosphorus mineralized
        ///   proportionally to decomposition rate (Berg and Staaf 1981,
        ///   Moore et al. 2006).
        /// </summary>
        public static void DecompPhase2(PoolD litter,
                                        ActiveSite site)
        {
            litter.Mass = litter.Mass * Math.Exp(-1 * litter.DecayValue);
            litter.ContentC = litter.ContentC * Math.Exp(-1 * litter.DecayValue);

            //Net N mineralization proportional to mass loss.
            double mineralizedN = litter.ContentN - (litter.ContentN *
                Math.Exp(-1 * litter.DecayValue));
            litter.ContentN -= mineralizedN;
            SiteVars.MineralSoil[site].ContentN += mineralizedN;

            //Net P mineralization proportional to mass loss.
            double mineralizedP = litter.ContentP - (litter.ContentP *
                Math.Exp(-1 * litter.DecayValue));
            litter.ContentP -= mineralizedP;
            SiteVars.MineralSoil[site].ContentP += mineralizedP;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Phase 3 of decomposition: Matter is transferred to soil organic
        ///   matter. If C:P > 450, additional P is released prior to
        ///   humification (Moore et al. 2006).
        /// </summary>
        public static void DecompPhase3(PoolD litter,
                                        ActiveSite site)
        {
            double CPratio = litter.ContentC / litter.ContentP;

            if (CPratio > 450)
            {
                double mineralizedP = litter.ContentC / 450;
                litter.ContentP -= mineralizedP;
                SiteVars.MineralSoil[site].ContentP += mineralizedP;
            }

            SiteVars.SoilOrganicMatter[site].Mass += litter.Mass;
            SiteVars.SoilOrganicMatter[site].ContentC += litter.ContentC;
            SiteVars.SoilOrganicMatter[site].ContentN += litter.ContentN;
            SiteVars.SoilOrganicMatter[site].ContentP += litter.ContentP;

            SiteVars.RemoveLitter[site].Add(litter);
        }
    }
}
