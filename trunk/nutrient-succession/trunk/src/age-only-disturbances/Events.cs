//  Copyright 2010 Portland State University
//  Author: Robert Scheller

using Edu.Wisc.Forest.Flel.Util;
using Landis.Landscape;
using Landis.PlugIns;

namespace Landis.Biomass.NuCycling.Succession.AgeOnlyDisturbances
{
    /// <summary>
    /// The handlers for various type of events related to age-only
    ///   disturbances.
    /// </summary>
    public static class Events
    {
        public static void CohortDied(object         sender,
                                      DeathEventArgs eventArgs)
        {
            PlugInType disturbanceType = eventArgs.DisturbanceType;
            PoolPercentages cohortReductions = Module.Parameters.CohortReductions[disturbanceType];

            ICohort cohort = eventArgs.Cohort;
            ActiveSite site = eventArgs.Site;
            double nonWoody = (double) cohort.LeafBiomass;
            double woody = (double)cohort.WoodBiomass;

            double nonWoodyInput = ReduceInput(nonWoody,
                                               cohortReductions.NonWoody);
            double woodyInput = ReduceInput(woody,
                                            cohortReductions.Woody);

            ForestFloor.AddWoodyDebris(woodyInput, cohort.Species, SiteVars.WoodyDebris[site]);
            //Assuming that all disturbance happen during leaf on periods:
            ForestFloor.AddDisturbanceLitter(nonWoodyInput, cohort.Species, site, 
                SiteVars.LitterAdd[site]);

            double killedFineRoots = Roots.CalculateFineRoot(nonWoody, SpeciesData.LeafLongevity[cohort.Species]);
            Roots.KillFineRoots(killedFineRoots, cohort.Species, SiteVars.DeadFineRootsAdd[site]);
            Roots.ReduceFineRoots(killedFineRoots, cohort.Species, SiteVars.FineRoots[site]);
            double killedCoarseRoots = Roots.CalculateCoarseRoot(woody, SpeciesData.LeafLongevity[cohort.Species]);
            ForestFloor.AddWoodyDebris(killedCoarseRoots, cohort.Species, SiteVars.WoodyDebris[site]);
            Roots.ReduceCoarseRoots(killedCoarseRoots, cohort.Species, SiteVars.CoarseRoots[site]);

            //If the disturbance is due to fire, add portion of consumed biomass to
            //  charcoal (Preston and Schmidt 2006) and mineral soil pools 
            //  (Raison et al. 1985).
            if (disturbanceType.Name == "disturbance:fire")
            {
            SiteVars.Charcoal[site].ContentC += (((nonWoody - nonWoodyInput) * 
                SpeciesData.LeafFractionC[cohort.Species] + (woody - woodyInput) * 
                SpeciesData.WoodFractionC[cohort.Species]) * 0.08);
            SiteVars.Charcoal[site].ContentN += (((nonWoody - nonWoodyInput) *
                SpeciesData.LeafFractionN[cohort.Species] + (woody - woodyInput) *
                SpeciesData.WoodFractionN[cohort.Species]) * 0.08);
            SiteVars.Charcoal[site].ContentP += (((nonWoody - nonWoodyInput) *
                SpeciesData.LeafFractionP[cohort.Species] + (woody - woodyInput) *
                SpeciesData.WoodFractionP[cohort.Species]) * 0.08);
            SiteVars.MineralSoil[site].ContentN += (((nonWoody - nonWoodyInput) * 
                SpeciesData.LeafFractionN[cohort.Species] + (woody - woodyInput) * 
                SpeciesData.WoodFractionN[cohort.Species]) * 0.01);
            SiteVars.MineralSoil[site].ContentP += (((nonWoody - nonWoodyInput) * 
                SpeciesData.LeafFractionP[cohort.Species] + (woody - woodyInput) * 
                SpeciesData.WoodFractionP[cohort.Species]) * 0.42);
            }
        }

        //---------------------------------------------------------------------

        private static double ReduceInput(double poolInput,
                                          Percentage reductionPercentage)
        {
            double reduction = poolInput * reductionPercentage;
            return (poolInput - reduction);
        }

        //---------------------------------------------------------------------

        public static void SiteDisturbed(object               sender,
                                         DisturbanceEventArgs eventArgs)
        {
            PlugInType disturbanceType = eventArgs.DisturbanceType;
            PoolPercentages poolReductions = Module.Parameters.PoolReductions[disturbanceType];

            ActiveSite site = eventArgs.Site;
            SiteVars.WoodyDebris[site].ReducePercentage(poolReductions.Woody);
            foreach (PoolD litter in SiteVars.Litter[site])
            {
                litter.Mass -= (litter.Mass * poolReductions.NonWoody);
                litter.ContentC -= (litter.ContentC * poolReductions.NonWoody);
                litter.ContentN -= (litter.ContentN * poolReductions.NonWoody);
                litter.ContentP -= (litter.ContentP * poolReductions.NonWoody);
            }
        }
    }
}
