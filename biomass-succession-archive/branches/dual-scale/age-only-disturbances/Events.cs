//  Copyright 2006 University of Wisconsin-Madison
//  Author: Jimm Domingo, FLEL

using Edu.Wisc.Forest.Flel.Util;
using Landis.Landscape;
using Landis.PlugIns;

namespace Landis.Biomass.Succession.AgeOnlyDisturbances
{
    /// <summary>
    /// The handlers for various type of events related to age-only
    /// disturbances.
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
            ushort nonWoody = cohort.ComputeNonWoodyBiomass(site);
            ushort woody = (ushort) (cohort.Biomass - nonWoody);

            ushort nonWoodyInput = ReduceInput(nonWoody,
                                               cohortReductions.NonWoody);
            ushort woodyInput = ReduceInput(woody,
                                            cohortReductions.Woody);

            Dead.Pools.AddBiomass(woodyInput, nonWoodyInput, cohort.Species, site);
        }

        //---------------------------------------------------------------------

        private static ushort ReduceInput(ushort     poolInput,
                                          Percentage reductionPercentage)
        {
            ushort reduction = (ushort) (poolInput * reductionPercentage);
            return (ushort) (poolInput - reduction);
        }

        //---------------------------------------------------------------------

        public static void SiteDisturbed(object               sender,
                                         DisturbanceEventArgs eventArgs)
        {
            PlugInType disturbanceType = eventArgs.DisturbanceType;
            PoolPercentages poolReductions = Module.Parameters.PoolReductions[disturbanceType];

            ActiveSite site = eventArgs.Site;
            Dead.Pools.Woody[site].ReduceBiomass(poolReductions.Woody);
            Dead.Pools.NonWoody[site].ReduceBiomass(poolReductions.NonWoody);
        }
    }
}
