using Edu.Wisc.Forest.Flel.Util;

using Landis.InitialCommunities;
using Landis.Landscape;
using Landis.PlugIns;
using Landis.Species;

using System;
using System.Collections.Generic;

namespace Landis.Biomass.Succession
{
    public class PlugIn
        : Landis.Succession.PlugIn
    {
        private static readonly PlugInType fireDisturbanceType = new PlugInType("disturbance:fire");

        private ISiteVar<SiteCohorts> cohorts;
        private LandscapeCohorts landscapeCohorts;

        //---------------------------------------------------------------------

        public PlugIn()
            : base("Biomass Succession")
        {
        }

        //---------------------------------------------------------------------

        public override void Initialize(string        dataFile,
                                        PlugIns.ICore modelCore)
        {
            Model.Core = modelCore;
            ParametersParser parser = new ParametersParser(Model.Core.Ecoregions,
                                                           Model.Core.Species,
                                                           Model.Core.StartTime,
                                                           Model.Core.EndTime);
            IParameters parameters = Landis.Data.Load<IParameters>(dataFile, parser);

            Timestep = parameters.Timestep;

            //  Cohorts must be created before the base class is initialized
            //  because the base class' reproduction module uses the core's
            //  SuccessionCohorts property in its Initialization method.
            Biomass.Cohorts.Initialize(Timestep,
                                       new CohortBiomass());
            cohorts = Model.Core.Landscape.NewSiteVar<SiteCohorts>();
            landscapeCohorts = new LandscapeCohorts(cohorts);
            Cohorts = landscapeCohorts;

            InitialBiomass.Initialize(Timestep);
            Dead.Pools.Initialize(modelCore,
                                  parameters.WoodyDecayRate,
                                  parameters.LeafLitterDecayRate);

            base.Initialize(modelCore,
                            Util.ToArray<double>(parameters.EstablishProbability),
                            parameters.SeedAlgorithm,
                            AddNewCohort);

            LivingBiomass.Initialize(parameters, cohorts);

            Cohort.DeathEvent += CohortDied;
            AgeOnlyDisturbances.Module.Initialize(parameters.AgeOnlyDisturbanceParms);

            ClimateChange.Module.Initialize(parameters.ClimateChangeUpdates);
        }

        //---------------------------------------------------------------------
    
        public void CohortDied(object         sender,
                               DeathEventArgs eventArgs)
        {
            PlugInType disturbanceType = eventArgs.DisturbanceType;
            if (disturbanceType != null) {
                ActiveSite site = eventArgs.Site;
                Disturbed[site] = true;
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
            cohorts[site].AddNewCohort(species,
                                       CohortBiomass.InitialBiomass(cohorts[site],
                                                                    site));
        }

        //---------------------------------------------------------------------

        protected override void InitializeSite(ActiveSite site,
                                               ICommunity initialCommunity)
        {
            InitialBiomass initialBiomass = InitialBiomass.Compute(site, initialCommunity);
            cohorts[site] = initialBiomass.Cohorts.Clone();
            Dead.Pools.Woody[site] = initialBiomass.DeadWoodyPool;
            Dead.Pools.NonWoody[site] = initialBiomass.DeadNonWoodyPool;
        }

        //---------------------------------------------------------------------

        public override void Run()
        {
            ClimateChange.Module.CheckForUpdate();
            base.Run();
        }

        //---------------------------------------------------------------------

        protected override void AgeCohorts(ActiveSite site,
                                           ushort     years,
                                           int?       successionTimestep)
        {
            Util.GrowCohorts(cohorts[site], site, years, successionTimestep.HasValue);
        }

        //---------------------------------------------------------------------

        public override byte ComputeShade(ActiveSite site)
        {
            return LivingBiomass.ComputeShade(site);
        }
    }
}
