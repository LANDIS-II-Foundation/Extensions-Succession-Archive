//  Copyright 2009 Conservation Biology Institute
//  Authors:  Robert M. Scheller
//  License:  Available at  
//  http://www.landis-ii.org/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Util;
using Landis.Landscape;
using Landis.PlugIns;
using Landis.Biomass;  

namespace Landis.Extension.Succession.Century.AgeOnlyDisturbances
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
            float foliar = cohort.LeafBiomass; 
            float wood = cohort.WoodBiomass; 
            

            float foliarInput = ReduceInput(foliar, cohortReductions.Foliar, site);
            float woodInput   = ReduceInput(wood, cohortReductions.Wood, site);

            //UI.WriteLine("EVENT: Cohort Died: species={0}, age={1}, disturbance={2}.", cohort.Species.Name, cohort.Age, eventArgs.DisturbanceType);
            //UI.WriteLine("       Cohort Reductions:  Foliar={0:0.00}.  Wood={1:0.00}.", cohortReductions.Foliar, cohortReductions.Wood);
            //UI.WriteLine("       InputB/TotalB:  Foliar={0:0.00}/{1:0.00}, Wood={2:0.0}/{3:0.0}.", foliarInput, foliar, woodInput, wood);
            
            ForestFloor.AddWoodLitter(woodInput, cohort.Species, site);
            ForestFloor.AddFoliageLitter(foliarInput, cohort.Species, site);
            
            Roots.AddCoarseRootLitter(Roots.CalculateCoarseRoot(wood), cohort.Species, site);  // All of cohorts roots are killed.
            Roots.AddFineRootLitter(Roots.CalculateFineRoot(foliar), cohort.Species, site);
            
        }

        //---------------------------------------------------------------------

        private static float ReduceInput(float     poolInput,
                                          Percentage reductionPercentage,
                                          ActiveSite site)
        {
            float reduction = (poolInput * (float) reductionPercentage);
            
            SiteVars.SourceSink[site].Carbon        += (double) reduction * 0.47;
            //SiteVars.FireEfflux[site]               += (double) reduction * 0.47;
            
            return (poolInput - reduction);
        }

        //---------------------------------------------------------------------

        public static void SiteDisturbed(object               sender,
                                         DisturbanceEventArgs eventArgs)
        {

            PlugInType disturbanceType = eventArgs.DisturbanceType;
            
            if(disturbanceType.ToString() == "disturbance:fire")
                return;

            PoolPercentages poolReductions = Module.Parameters.PoolReductions[disturbanceType];

            ActiveSite site = eventArgs.Site;
            SiteVars.SurfaceDeadWood[site].ReduceMass(poolReductions.Wood);
            SiteVars.SurfaceStructural[site].ReduceMass(poolReductions.Foliar);
            SiteVars.SurfaceMetabolic[site].ReduceMass(poolReductions.Foliar);
        }
    }
}
