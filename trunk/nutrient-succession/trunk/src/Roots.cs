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
    /// Fine and coarse roots.
    /// </summary>
    public class Roots
    {
        /// <summary>
        /// Adds some biomass for a species to the coarse roots pools at a site.
        /// </summary>
        public static void AddLiveCoarseRoots(double abovegroundWoodyBiomass,
                                              //double leafLongevity,
                                              ISpecies species,
                                              ActiveSite site,
                                              Pool coarseRoots)
        {
            double leafLongevity = SpeciesData.LeafLongevity[species];
            double coarseRootBiomass = CalculateCoarseRoot(abovegroundWoodyBiomass, leafLongevity);
            double inputPercentC = SpeciesData.WoodFractionC[species];
            double inputPercentN = SpeciesData.WoodFractionN[species];
            double inputPercentP = SpeciesData.WoodFractionP[species];

            coarseRoots.AddMass(coarseRootBiomass, inputPercentC, inputPercentN, inputPercentP);
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Adds some biomass for a species to the fine roots pools at a site.
        /// </summary>
        public static void AddLiveFineRoots(double abovegroundLeafBiomass,
                                            //double leafLongevity,
                                            ISpecies species,
                                            ActiveSite site,
                                            Pool fineRoots)
        {
            double leafLongevity = SpeciesData.LeafLongevity[species];
            double fineRootBiomass = CalculateFineRoot(abovegroundLeafBiomass, leafLongevity);
            double inputPercentC = SpeciesData.FRootFractionC[species];
            double inputPercentN = SpeciesData.FRootFractionN[species];
            double inputPercentP = SpeciesData.FRootFractionP[species];

            fineRoots.AddMass(fineRootBiomass, inputPercentC,
                inputPercentN, inputPercentP);
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Kills fine roots and add the biomass to the Dead Fine Roots pool.
        /// </summary>
        public static void KillFineRoots(double litterMass,
                                         ISpecies species,
                                         List<PoolD> deadFRootsAdd)
        {
            PoolD deadFRootsAddition = new PoolD();
            deadFRootsAddition.Mass = litterMass;   //litterMass is result of CalculateFineRoots
            deadFRootsAddition.ContentC = SpeciesData.FRootFractionC[species] *
                deadFRootsAddition.Mass;
            deadFRootsAddition.ContentN = SpeciesData.FRootFractionN[species] *
                deadFRootsAddition.Mass;
            deadFRootsAddition.ContentP = SpeciesData.FRootFractionP[species] *
                deadFRootsAddition.Mass;

            //Calculate decomposition rate (Silver and Miya 2001, Fig. 3,
            //  with substituted (-) for (+) to match figure).
            deadFRootsAddition.DecayValue = Math.Exp(3.92 - (1.12 *
                (Math.Log(SpeciesData.FRootFractionC[species] /
                SpeciesData.FRootFractionN[species]))));

            //Calculate mass loss limit (Berg et al. 1996). Lower limit
            //  than for litter (Verburg and Johnson 2001 and others).
            //  Use ceiling of 0.95 to ensure litter is removed.
            double limitValue = 0.01 * (91.241 - (1.744 *
                (SpeciesData.FRootFractionN[species] * 1000)));
            deadFRootsAddition.LimitValue = Math.Min(limitValue, 0.95);

            deadFRootsAdd.Add(deadFRootsAddition);
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Adds yearly biomass addition to dead fine roots at a site.
        ///   Sums all species-cohort additions and adds as a single cohort.
        /// </summary>
        public static void AddYearDeadFineRoots(ActiveSite site)
        {
            double addMass = 0.0;
            double addC = 0.0;
            double addN = 0.0;
            double addP = 0.0;
            double addDecay = 0.0;
            double addLimit = 0.0;

            foreach (PoolD litter in SiteVars.DeadFineRootsAdd[site])
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

            SiteVars.DeadFineRoots[site].Add(LitterAddition);
            SiteVars.DeadFineRootsAdd[site].Clear();
            SiteVars.DeadFineRootsAdd[site].TrimExcess();
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Decomposition of fine roots. Follows Berg's 3-part model (generally
        ///   higher limit values, but same dynamics as ForestFloor.DecomposeLitter)
        ///   (McClaugherty et al. 1984).
        /// </summary>
        public static void DecomposeDeadFineRoots(PoolD litter,
                                                  ActiveSite site)
        {
            double CNratio = litter.ContentC / litter.ContentN;
            double massLoss = (litter.InitialMass - litter.Mass) / litter.InitialMass;

            if (litter.Mass < 1.0)
            {
                SiteVars.RemoveDeadFineRoots[site].Add(litter);
                return;
            }

            //Limit to mass loss determines cut-off betweeen phases 2 and 3.
            //Phase 3.
            else if (massLoss > litter.LimitValue)
            {
                Roots.DecompPhase3(litter, site);

                return;
            }

            //Critical C:N < 40 used as cut-off between phases 1 and 2.
            //  (Upper bound in Prescott et al. 2000 and empirical finding
            //  in Janssen 1996).
            //Phase 1.
            else if (CNratio > 40)
            {
                Roots.DecompPhase1(litter, site);

                return;
            }

            //Phase 2.
            else
            {
                Roots.DecompPhase2(litter, site);

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
            //Microbe CUE and C:N ratios set to default values.
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

            SiteVars.RemoveDeadFineRoots[site].Add(litter);
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Calculate coarse roots based on aboveground woody and leaf biomass.
        ///   Coarse root:Aboveground Woody fom White et al. 2000/Niklas &
        ///   Enquist 2002. Using averages of all species.
        /// </summary>
        public static double CalculateCoarseRoot(double woodyMass,
                                                 double leafLongevity)
        {
            double coarseRootMultiplier = 0.22;
            if (leafLongevity > 1.0) coarseRootMultiplier = 0.29;

            return (woodyMass * coarseRootMultiplier);
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Calculate fine roots based on aboveground woody and leaf biomass.
        ///   Fine root:Leaf fom White et al. 2000, using averages for
        ///   evergreens and deciduous trees.
        /// </summary>
        public static double CalculateFineRoot(double leafMass,
                                               double leafLongevity)
        {
            double fineRootMultiplier = 1.2;
            if (leafLongevity > 1.0) fineRootMultiplier = 1.4;

            return (leafMass * fineRootMultiplier);
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Reduces the fine root's biomass and nutrients by a specified amount.
        /// </summary>
        public static void ReduceFineRoots(double amount,
                                 ISpecies species,
                                 Pool fineRoots)
        {
            fineRoots.Mass -= amount;
            fineRoots.ContentC -= (amount * SpeciesData.FRootFractionC[species]);
            fineRoots.ContentN -= (amount * SpeciesData.FRootFractionN[species]);
            fineRoots.ContentP -= (amount * SpeciesData.FRootFractionP[species]);
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Reduces the coarse root's biomass and nutrients by a specified amount.
        /// </summary>
        public static void ReduceCoarseRoots(double amount,
                                             ISpecies species,
                                             Pool coarseRoots)
        {
            coarseRoots.Mass -= amount;
            coarseRoots.ContentC -= (amount * SpeciesData.WoodFractionC[species]);
            coarseRoots.ContentN -= (amount * SpeciesData.WoodFractionN[species]);
            coarseRoots.ContentP -= (amount * SpeciesData.WoodFractionP[species]);
        }
    }
}
