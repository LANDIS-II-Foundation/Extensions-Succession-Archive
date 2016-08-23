//  Copyright 2005 University of Nevada, University of Wisconsin
//  Authors:  Sarah Ganschow, Robert M. Scheller, James B. Domingo
//  License:  Available at
//  http://www.landis-ii.org/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Util;
using Edu.Wisc.Forest.Flel.Grids;

using Landis.Ecoregions;
using Landis.Landscape;
using Landis.Species;
using Landis.AgeCohort;
using Landis.Util;
using Landis.PlugIns;

using System;

namespace Landis.Biomass.NuCycling.Succession
{
    /// <summary>
    /// Utility methods.
    /// </summary>
    public static class NutrientSuccession
    {
        /// <summary>
        /// Grows all cohorts (including productivity and mortality) at a site
        /// for a specified number of years. The dead pools at the site also
        /// decompose for the given time period and fires occur.
        /// </summary>
        public static void GrowCohorts(SiteCohorts cohorts,
                                       ActiveSite site,
                                       int years,
                                       bool isSuccessionTimestep)
        {
            IEcoregion ecoregion = Model.Core.Ecoregion[site];

            for (int y = 1; y <= years; ++y)
            {
                //Account for any preceding fires.
                if (SiteVars.FireSeverity != null)
                {
                    FireEffects.ComputeFireEffects(site);
                }

                //Decompose litter cohorts (each member of list is 1 annual cohort).
                foreach (PoolD litter in SiteVars.Litter[site])
                {
                    ForestFloor.DecomposeLitter(litter, site);
                }

                //Remove litter cohorts past the limit value.
                foreach (PoolD removeLitter in SiteVars.RemoveLitter[site])
                {
                    SiteVars.Litter[site].Remove(removeLitter);
                    SiteVars.Litter[site].TrimExcess();
                }
                SiteVars.RemoveLitter[site].Clear();
                SiteVars.RemoveLitter[site].TrimExcess();

                //Decompose dead fine roots cohorts (each member
                //  of list is 1 annual cohort).
                foreach (PoolD deadFRoots in SiteVars.DeadFineRoots[site])
                {
                    Roots.DecomposeDeadFineRoots(deadFRoots, site);
                }

                //Remove dead fine roots cohorts past the limit value.
                foreach (PoolD removeDeadFRoots in SiteVars.RemoveDeadFineRoots[site])
                {
                    SiteVars.DeadFineRoots[site].Remove(removeDeadFRoots);
                    SiteVars.DeadFineRoots[site].TrimExcess();
                }
                SiteVars.RemoveDeadFineRoots[site].Clear();
                SiteVars.RemoveDeadFineRoots[site].TrimExcess();

                //Decompose coarse woody debris (includes coarse roots).
                ForestFloor.DecomposeWood(site);

                //Decompose soil organic matter.
                SoilOrganicMatter.Decompose(ecoregion, site);

                //Weather charcoal to release N and P to mineral soil pools.
                Charcoal.Weathering(SiteVars.Charcoal[site], SiteVars.MineralSoil[site]);

                //Weather rock to release P to mineral pool.
                Rock.Weathering(ecoregion, SiteVars.Rock[site], SiteVars.MineralSoil[site]);

                //N and P deposition.
                MineralSoil.Deposition(ecoregion, SiteVars.MineralSoil[site]);

                //Grow all cohorts at once, including above-/belowground growth, mortality,
                //  and litterfall.  N and P are taken up with growth and N limits ANPP.
                cohorts.Grow(site, (y == years && isSuccessionTimestep), true);

                //Add yearly cohorts to litter and dead fine roots lists.
                ForestFloor.AddYearLitter(site);
                Roots.AddYearDeadFineRoots(site);

                //Leach excess mineral N.
                MineralSoil.Leaching(site);
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Grows all cohorts at a site to reach the specified initial communities.
        ///   Includes resets for soil organic matter and soil mineral contents to
        ///   user-specified initial values.
        /// </summary>
        public static void InitGrowCohorts(SiteCohorts cohorts,
                                           ActiveSite site,
                                           int years,
                                           bool isSuccessionTimestep)
        {
            for (int y = 1; y <= years; ++y)
            {
                //Used to reset soil minerals to user-specified parameters
                //  during run-up to initial communities.
                double minN = SiteVars.MineralSoil[site].ContentN;
                double minP = SiteVars.MineralSoil[site].ContentP;
                double somMass = SiteVars.SoilOrganicMatter[site].Mass;
                double somC = SiteVars.SoilOrganicMatter[site].ContentC;
                double somN = SiteVars.SoilOrganicMatter[site].ContentN;
                double somP = SiteVars.SoilOrganicMatter[site].ContentP;

                // Make sure mineral N and P are sufficient for initial growth
                //  (initial mineral N/P values represent a static point, not integrated
                //   over history when values may have been higher).
                SiteVars.MineralSoil[site].ContentN = 1000;
                SiteVars.MineralSoil[site].ContentP = 1000;

                SiteVars.PrevYearMortality[site] = SiteVars.CurrentYearMortality[site];
                SiteVars.CurrentYearMortality[site] = 0.0;

                //Grow cohorts.
                cohorts.Grow(site, (y == years && isSuccessionTimestep), true);

                //Add yearly cohorts to litter and dead fine roots lists.
                ForestFloor.AddYearLitter(site);
                Roots.AddYearDeadFineRoots(site);

                //Decompose coarse woody debris (includes coarse roots).
                ForestFloor.DecomposeWood(site);

                //Decompose litter cohorts (each member of list is 1
                //  annual cohort).
                foreach (PoolD litter in SiteVars.Litter[site])
                {
                    ForestFloor.DecomposeLitter(litter, site);
                }

                //Remove litter cohorts past the limit value.
                foreach (PoolD removeLitter in SiteVars.RemoveLitter[site])
                {
                    SiteVars.Litter[site].Remove(removeLitter);
                    SiteVars.Litter[site].TrimExcess();
                }
                SiteVars.RemoveLitter[site].Clear();
                SiteVars.RemoveLitter[site].TrimExcess();

                //Decompose dead fine roots cohorts (each member
                //  of list is 1 annual cohort).
                foreach(PoolD deadFRoots in SiteVars.DeadFineRoots[site])
                {
                    Roots.DecomposeDeadFineRoots(deadFRoots, site);
                }

                //Remove dead fine roots cohorts past the limit value.
                foreach (PoolD removeDeadFRoots in SiteVars.RemoveDeadFineRoots[site])
                {
                    SiteVars.DeadFineRoots[site].Remove(removeDeadFRoots);
                    SiteVars.DeadFineRoots[site].TrimExcess();
                }
                SiteVars.RemoveDeadFineRoots[site].Clear();
                SiteVars.RemoveDeadFineRoots[site].TrimExcess();

                //Reset soil organic and mineral values to user-specified parameters.
                SiteVars.MineralSoil[site].ContentN = minN;
                SiteVars.MineralSoil[site].ContentP = minP;
                SiteVars.SoilOrganicMatter[site].Mass = somMass;
                SiteVars.SoilOrganicMatter[site].ContentC = somC;
                SiteVars.SoilOrganicMatter[site].ContentN = somN;
                SiteVars.SoilOrganicMatter[site].ContentP = somP;

            }
        }

    }
}
