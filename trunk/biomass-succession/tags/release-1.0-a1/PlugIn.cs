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
        : Landis.Succession.BaseComponent, PlugIns.ISuccession,
                                           Landis.Cohorts.ISuccession<Biomass.ICohort>,
                                           Landis.Cohorts.ISuccession<AgeCohort.ICohort>
    {
        private ISiteVar<Biomass.SiteCohorts> cohorts;
        private LandscapeCohorts landscapeCohorts;

        //---------------------------------------------------------------------

        public string Name
        {
            get {
                return "Biomass Succession";
            }
        }

        //---------------------------------------------------------------------

        public ILandscapeCohorts<Biomass.ICohort> Cohorts
        {
            get {
                return landscapeCohorts;
            }
        }

        //---------------------------------------------------------------------

        ILandscapeCohorts<AgeCohort.ICohort> Landis.Cohorts.ISuccession<AgeCohort.ICohort>.Cohorts
        {
            get {
                return landscapeCohorts;
            }
        }

        //---------------------------------------------------------------------

        public PlugIn()
        {
        }

        //---------------------------------------------------------------------

        public void Initialize(string dataFile,
                               int    startTime)
        {
            ParametersParser parser = new ParametersParser(Model.Ecoregions, Model.Species);
            IParameters parameters = Landis.Data.Load<IParameters>(dataFile, parser);

            //  Cohorts must be created before the base class is initialized
            //  because the base class' reproduction module uses
            //  Model.GetSuccession<...>().Cohorts in its Initialization
            //  method.
            Biomass.Cohorts.Initialize(parameters.Timestep,
                                       CohortDied, // TODO: pass method from DeadBiomass lib
                                       new CohortBiomass());
            cohorts = Model.Landscape.NewSiteVar<Biomass.SiteCohorts>();
            landscapeCohorts = new LandscapeCohorts(cohorts);

            InitialCommunities.Initialize(parameters.Timestep);

            double[,] estabProbabilities = new double[Model.Ecoregions.Count, Model.Species.Count];
            foreach (ISpecies species in Model.Species) {
                foreach (IEcoregion ecoregion in Model.Ecoregions) {
                    estabProbabilities[ecoregion.Index, species.Index] = parameters.EstablishProbability[species][ecoregion];
                }
            }
            base.Initialize(parameters.Timestep,
                            estabProbabilities,
                            startTime,
                            parameters.SeedAlgorithm,
                            AddNewCohort);

            LivingBiomass.Initialize(parameters, cohorts);
        }

        //---------------------------------------------------------------------
    
        public void CohortDied(ICohort    cohort,
                               ActiveSite site)
        {
            Log.Info("Cohort died at site {0}: {1} {2:#,##0}({3:#,##0})",
                     site.Location, cohort.Species.Name, cohort.Age,
                     cohort.Biomass);
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
            cohorts[site] = InitialCommunities.GetCohorts(initialCommunity, site);
        }

        //---------------------------------------------------------------------

        protected override void AgeCohorts(ActiveSite site,
                                           ushort     years,
                                           int?       successionTimestep)
        {
            cohorts[site].Grow(years, site, successionTimestep.HasValue);
            // TODO: Invoke decay method for dead biomass pools at the site
        }

        //---------------------------------------------------------------------

        public override byte ComputeShade(ActiveSite site)
        {
            return LivingBiomass.ComputeShade(site);
        }

        //---------------------------------------------------------------------

        public void CheckForResprouting(Biomass.ICohort cohort,
                                        ActiveSite      site)
        {
            Landis.Succession.Reproduction.CheckForResprouting(cohort, site);
        }

        //---------------------------------------------------------------------

        public void CheckForPostFireRegen(Biomass.ICohort cohort,
                                          ActiveSite      site)
        {
            Landis.Succession.Reproduction.CheckForPostFireRegen(cohort, site);
        }

        //---------------------------------------------------------------------

        public void CheckForResprouting(AgeCohort.ICohort cohort,
                                        ActiveSite        site)
        {
            Landis.Succession.Reproduction.CheckForResprouting(cohort, site);
        }

        //---------------------------------------------------------------------

        public void CheckForPostFireRegen(AgeCohort.ICohort cohort,
                                          ActiveSite        site)
        {
            Landis.Succession.Reproduction.CheckForPostFireRegen(cohort, site);
        }
    }
}
