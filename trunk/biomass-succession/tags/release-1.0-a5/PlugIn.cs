using Landis.Cohorts;
using Landis.Ecoregions;
using Landis.InitialCommunities;
using Landis.Landscape;
using Landis.Species;

using System;
using System.Collections.Generic;

namespace Landis.Biomass.Succession
{
    public class PlugIn
        : Landis.Succession.PlugIn
    {
        private ISiteVar<Biomass.SiteCohorts> cohorts;
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
            ParametersParser parser = new ParametersParser(Model.Core.Ecoregions, Model.Core.Species);
            IParameters parameters = Landis.Data.Load<IParameters>(dataFile, parser);

            Timestep = parameters.Timestep;

            //  Cohorts must be created before the base class is initialized
            //  because the base class' reproduction module uses the core's
            //  SuccessionCohorts property in its Initialization method.
            Biomass.Cohorts.Initialize(Timestep,
                                       new CohortBiomass());
            cohorts = Model.Core.Landscape.NewSiteVar<Biomass.SiteCohorts>();
            landscapeCohorts = new LandscapeCohorts(cohorts);
            Cohorts = landscapeCohorts;

            InitialBiomass.Initialize(Timestep);
            Dead.Pools.Initialize(null, // TODO: parameters.DeadPoolInitFile,
                                  modelCore,
                                  parameters.WoodyDecayRate,
                                  parameters.LeafLitterDecayRate);

            double[,] estabProbabilities = new double[Model.Core.Ecoregions.Count, Model.Core.Species.Count];
            foreach (ISpecies species in Model.Core.Species) {
                foreach (IEcoregion ecoregion in Model.Core.Ecoregions) {
                    estabProbabilities[ecoregion.Index, species.Index] = parameters.EstablishProbability[species][ecoregion];
                }
            }
            base.Initialize(modelCore,
                            estabProbabilities,
                            parameters.SeedAlgorithm,
                            AddNewCohort);

            LivingBiomass.Initialize(parameters, cohorts);

            Cohort.DiedEvent += new CohortDiedEventHandler(this.CohortDied);
        }

        //---------------------------------------------------------------------
    
        public void CohortDied(ICohort    cohort,
                               ActiveSite site)
        {
            Log.Info("Cohort died at site {0}: {1} {2:#,##0}({3:#,##0})",
                     site.Location, cohort.Species.Name, cohort.Age,
                     cohort.Biomass);
            Landis.Succession.SiteVars.Disturbed[site] = true;
            if (Model.Core.CurrentPlugInType.IsMemberOf("disturbance:fire"))
                Landis.Succession.Reproduction.CheckForPostFireRegen(cohort, site);
            else
                Landis.Succession.Reproduction.CheckForResprouting(cohort, site);
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
