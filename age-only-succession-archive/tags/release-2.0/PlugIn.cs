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
using Landis.Succession;
using System.Collections.Generic;

namespace Landis.AgeOnly.Succession
{
    public class PlugIn
        : Landis.Succession.PlugIn
    {
        private ISiteVar<SiteCohorts> cohorts;
        private LandscapeCohorts landscapeCohorts;

        //---------------------------------------------------------------------

        public PlugIn()
            : base("Age-only Succession")
        {
        }

        //---------------------------------------------------------------------
/*
        public override void LoadParameters(string        dataFile,
                                        PlugIns.ICore modelCore)
        {
            Model.Core = modelCore;
            ParametersParser parser = new ParametersParser();
            IParameters parameters = Landis.Data.Load<IParameters>(dataFile,
                                                                   parser);
            Timestep = parameters.Timestep;
        }*/

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
                            (Reproduction.Delegates.AddNewCohort) AddNewCohort);
        }

        //---------------------------------------------------------------------

        public void CohortDied(object         sender,
                               DeathEventArgs eventArgs)
        {
            PlugInType disturbanceType = eventArgs.DisturbanceType;
            if (disturbanceType != null) {
                ActiveSite site = eventArgs.Site;
                Disturbed[site] = true;
                if (disturbanceType.IsMemberOf("disturbance:fire"))
                    Reproduction.CheckForPostFireRegen(eventArgs.Cohort, site);
                else
                    Reproduction.CheckForResprouting(eventArgs.Cohort, site);
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
            foreach (SpeciesCohorts speciesCohorts in cohorts[site])
            {
                foreach (ICohort cohort in speciesCohorts)
                {
                    //ISpecies species = speciesCohorts.Species;
                    if (cohort.Species.ShadeTolerance > shade && cohort.Age >= cohort.Species.Maturity)
                        shade = cohort.Species.ShadeTolerance;
                }
            }
            return shade;
        }
    }
}
