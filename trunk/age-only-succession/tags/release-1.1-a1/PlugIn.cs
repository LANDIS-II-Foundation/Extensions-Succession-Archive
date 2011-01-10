//  Copyright 2005 University of Wisconsin-Madison
//  Authors:  Jimm Domingo, FLEL
//  License:  Available at  
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Landis.AgeCohort;
using Landis.Ecoregions;
using Landis.InitialCommunities;
using Landis.Landscape;
using Landis.PlugIns;
using Landis.Species;
using System.Collections.Generic;

namespace Landis.AgeOnly.Succession
{
    public class PlugIn
        : Landis.Succession.PlugIn
    {
        private ISiteVar<SiteCohorts> cohorts;
        private LandscapeCohorts landscapeCohorts;
        private static readonly PlugInType fireDisturbanceType = new PlugInType("disturbance:fire");

        //---------------------------------------------------------------------

        public PlugIn()
            : base("Age-only Succession")
        {
        }

        //---------------------------------------------------------------------

        public override void Initialize(string        dataFile,
                                        PlugIns.ICore modelCore)
        {
            Model.Core = modelCore;
            ParametersParser parser = new ParametersParser();
            IParameters parameters = Landis.Data.Load<IParameters>(dataFile,
                                                                   parser);

            Timestep = parameters.Timestep;

            //  Cohorts must be created before the base class is initialized
            //  because the base class' reproduction module uses the core's
            //  SuccessionCohorts property in its Initialization method.
            cohorts = modelCore.Landscape.NewSiteVar<SiteCohorts>();
            landscapeCohorts = new LandscapeCohorts(cohorts);
            Cohorts = landscapeCohorts;

            Cohort.DeathEvent += CohortDied;

            base.Initialize(modelCore,
                            parameters.EstablishProbabilities,
                            parameters.SeedAlgorithm,
                            AddNewCohort);
        }

        //---------------------------------------------------------------------

        public void CohortDied(object         sender,
                               DeathEventArgs eventArgs)
        {
            PlugInType disturbanceType = eventArgs.DisturbanceType;
            if (disturbanceType != null) {
                ActiveSite site = eventArgs.Site;
                Landis.Succession.SiteVars.Disturbed[site] = true;
                if (disturbanceType == fireDisturbanceType)
                    Landis.Succession.Reproduction.CheckForPostFireRegen(eventArgs.Cohort, site);
                else
                    Landis.Succession.Reproduction.CheckForResprouting(eventArgs.Cohort, site);
            }
        }

        //---------------------------------------------------------------------

        public void AddNewCohort(ISpecies   species,
                                 ActiveSite site)
        {
            cohorts[site].AddNewCohort(species);
        }

        //---------------------------------------------------------------------

        protected override void InitializeSite(ActiveSite site,
                                               ICommunity initialCommunity)
        {
            cohorts[site] = new SiteCohorts(initialCommunity.Cohorts);
        }

        //---------------------------------------------------------------------

        protected override void AgeCohorts(ActiveSite site,
                                           ushort     years,
                                           int?       successionTimestep)
        {
            cohorts[site].Grow(years, site, successionTimestep);
        }

        //---------------------------------------------------------------------

        public override byte ComputeShade(ActiveSite site)
        {
            byte shade = 0;
            foreach (SpeciesCohorts speciesCohorts in cohorts[site]) {
                ISpecies species = speciesCohorts.Species;
                if (species.ShadeTolerance > shade)
                    shade = species.ShadeTolerance;
            }
            return shade;
        }
    }
}
