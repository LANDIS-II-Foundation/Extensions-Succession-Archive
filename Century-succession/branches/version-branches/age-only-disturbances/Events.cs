//  Copyright 2007-2010 Portland State University, University of Wisconsin-Madison
//  Author: Robert Scheller, Ben Sulman, Fugui Wang

using Landis.Core;
using Landis.SpatialModeling;
using Edu.Wisc.Forest.Flel.Util;
using System.Collections.Generic;

using Landis.Library.LeafBiomassCohorts;  

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
            ExtensionType disturbanceType = eventArgs.DisturbanceType;
            
            PoolPercentages cohortReductions = Module.Parameters.CohortReductions[disturbanceType];

            ICohort cohort = eventArgs.Cohort;
            ActiveSite site = eventArgs.Site;
            float foliar = cohort.LeafBiomass; 
            float wood = cohort.WoodBiomass; 
            //wang
            float branch = cohort.BranchBiomass;

            float foliarInput = ReduceInput(foliar, cohortReductions.Foliar, site);
            float woodInput   = ReduceInput(wood, cohortReductions.Wood, site);

           
            //wang
            float branchInput = ReduceInput(branch, cohortReductions.Branch, site);
            
            
            PlugIn.ModelCore.UI.WriteLine("EVENT: Cohort Died: species={0}, age={1}, disturbance={2}.", cohort.Species.Name, cohort.Age, eventArgs.DisturbanceType);
            PlugIn.ModelCore.UI.WriteLine(" Cohort Reductions:  branch_reduction%={0:0.00}.  branchdead={1:0.0000}. branchlive={2:0.0000}.", (float)cohortReductions.Branch, branchInput*0.47, branch*0.47);
           
           
           

            /*
            if ((float)cohortReductions.Branch == 1.0 | (float)cohortReductions.Branch == 0.0)
            ForestFloor.AddBranchLitter(branchInput, cohort.Species, site);
            else
           */

            ForestFloor.AddBranchRLitter(branchInput, cohort.Species, site);
                       
            //PlugIn.ModelCore.Log.WriteLine("       InputB/TotalB:  Foliar={0:0.00}/{1:0.00}, Wood={2:0.0}/{3:0.0}.", foliarInput, foliar, woodInput, wood);
            
            ForestFloor.AddWoodLitter(woodInput, cohort.Species, site);
            ForestFloor.AddFoliageLitter(foliarInput, cohort.Species, site);
            
            Roots.AddCoarseRootLitter(wood, cohort.Species, site);  // All of cohorts roots are killed.
            Roots.AddFineRootLitter(foliar, cohort.Species, site);
            
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

            ExtensionType disturbanceType = eventArgs.DisturbanceType;
            
            if(disturbanceType.ToString() == "disturbance:fire")
                return;

            PoolPercentages poolReductions = Module.Parameters.PoolReductions[disturbanceType];

            ActiveSite site = eventArgs.Site;
            SiteVars.SurfaceDeadWood[site].ReduceMass(poolReductions.Wood);
            SiteVars.SurfaceDeadBranch[site].ReduceMass(poolReductions.Branch);//wang
           
            SiteVars.SurfaceStructural[site].ReduceMass(poolReductions.Foliar);
            SiteVars.SurfaceMetabolic[site].ReduceMass(poolReductions.Foliar);
        }
    }
}
