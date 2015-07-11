//  Copyright 2005-2010 Portland State University
//  Authors:  Robert M. Scheller
//  License:  Available at
//  http://www.landis-ii.org/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Util;

using Landis.InitialCommunities;
using Landis.Landscape;
using Landis.PlugIns;
using Landis.Species;
using Landis.Ecoregions;
using Landis.Succession;
using Landis.RasterIO;
using Landis.AgeCohort;

using System;
using System.Collections.Generic;

namespace Landis.Biomass.Succession
{
    public class PlugIn
        : Landis.Succession.PlugIn
    {
        //private ISiteVar<SiteCohorts> cohorts;
        private LandscapeCohorts landscapeCohorts;
        private List<ISufficientLight> sufficientLight;
        public static bool CalibrateMode;

        //---------------------------------------------------------------------

        public PlugIn()
            : base("Biomass Succession v2")
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
            sufficientLight = parameters.LightClassProbabilities;

            SiteVars.Initialize();
            SpeciesData.Initialize(parameters);
            EcoregionData.Initialize(parameters);
            CalibrateMode = parameters.CalibrateMode;
            CohortBiomass.SpinupMortalityFraction = parameters.SpinupMortalityFraction;

            //  Cohorts must be created before the base class is initialized
            //  because the base class' reproduction module uses the core's
            //  SuccessionCohorts property in its Initialization method.
            Biomass.Cohorts.Initialize(Timestep, new CohortBiomass());

            //cohorts = SiteVars.Cohorts;
            landscapeCohorts = new LandscapeCohorts(SiteVars.Cohorts);
            Cohorts = landscapeCohorts;

            Reproduction.SufficientLight = SufficientLight;

            InitialBiomass.Initialize(Timestep);

            base.Initialize(modelCore,
                            Util.ToArray<double>(parameters.EstablishProbability),
                            parameters.SeedAlgorithm,
                            (Reproduction.Delegates.AddNewCohort) AddNewCohort);


            Cohort.DeathEvent += CohortDied;
            AgeOnlyDisturbances.Module.Initialize(parameters.AgeOnlyDisturbanceParms);

            ClimateChange.Module.Initialize(parameters.ClimateChangeUpdates);
        }
        //---------------------------------------------------------------------

        protected override void InitializeSite(ActiveSite site,
                                               ICommunity initialCommunity)
        {
            InitialBiomass initialBiomass = InitialBiomass.Compute(site, initialCommunity);

            SiteVars.Cohorts[site]      = initialBiomass.Cohorts.Clone();
            SiteVars.WoodyDebris[site]  = initialBiomass.DeadWoodyPool.Clone();
            SiteVars.Litter[site]       = initialBiomass.DeadNonWoodyPool.Clone();
        }

        //---------------------------------------------------------------------

        public override void Run()
        {
            if(Model.Core.CurrentTime > 0 && SiteVars.CapacityReduction == null)
            	SiteVars.CapacityReduction   = Model.Core.GetSiteVar<double>("Harvest.CapacityReduction");

			ClimateChange.Module.CheckForUpdate();

            base.Run();
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Grows all cohorts at a site for a specified number of years.  The
        /// dead pools at the site also decompose for the given time period.
        /// </summary>
        public static void GrowCohorts(//SiteCohorts cohorts,
                                       ActiveSite  site,
                                       int         years,
                                       bool        isSuccessionTimestep)
        {

            //SiteCohorts siteCohorts = SiteVars.Cohorts[site];
            for (int y = 1; y <= years; ++y) {

                // Calculate competition for each cohort
                //CohortBiomass.CalculateCompetition(site);

                CohortBiomass.SubYear = y;

                SiteVars.Cohorts[site].Grow(site, (y == years && isSuccessionTimestep));
                SiteVars.WoodyDebris[site].Decompose();
                SiteVars.Litter[site].Decompose();
            }
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
                    Landis.Succession.Reproduction.CheckForPostFireRegen(eventArgs.Cohort, site);
                else
                    Landis.Succession.Reproduction.CheckForResprouting(eventArgs.Cohort, site);
            }
        }

        //---------------------------------------------------------------------

        public void AddNewCohort(ISpecies   species,
                                 ActiveSite site)
        {
            SiteVars.Cohorts[site].AddNewCohort(species,
                                       CohortBiomass.InitialBiomass(species, SiteVars.Cohorts[site],
                                                                    site));
        }



        //---------------------------------------------------------------------

        protected override void AgeCohorts(ActiveSite site,
                                           ushort     years,
                                           int?       successionTimestep)
        {
            GrowCohorts(site, years, successionTimestep.HasValue);
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Determines if there is sufficient light at a site for a species to
        /// germinate/resprout.
        /// </summary>
        public bool SufficientLight(ISpecies   species, ActiveSite site)
        {

            //UI.WriteLine("  Calculating Sufficient Light from Succession.");
            byte siteShade = Model.Core.GetSiteVar<byte>("Shade")[site];

            double lightProbability = 0.0;

            bool found = false;

            foreach(ISufficientLight lights in sufficientLight)
            {

                //UI.WriteLine("Sufficient Light:  ShadeClass={0}, Prob0={1}.", lights.ShadeClass, lights.ProbabilityLight0);
                if (lights.ShadeClass == species.ShadeTolerance)
                {
                    if (siteShade == 0)  lightProbability = lights.ProbabilityLight0;
                    if (siteShade == 1)  lightProbability = lights.ProbabilityLight1;
                    if (siteShade == 2)  lightProbability = lights.ProbabilityLight2;
                    if (siteShade == 3)  lightProbability = lights.ProbabilityLight3;
                    if (siteShade == 4)  lightProbability = lights.ProbabilityLight4;
                    if (siteShade == 5)  lightProbability = lights.ProbabilityLight5;
                    found = true;
                }
            }

            if(!found) UI.WriteLine("Could not find sufficient light data for {0}.", species.Name);

            return Landis.Util.Random.GenerateUniform() < lightProbability;
        }
        //---------------------------------------------------------------------

        public override byte ComputeShade(ActiveSite site)
        {
            //return LivingBiomass.ComputeShade(site);
            IEcoregion ecoregion = Model.Core.Ecoregion[site];
            double B_ACT = 0.0;

            if(SiteVars.Cohorts[site] != null)
                B_ACT = (double) Biomass.Cohorts.ComputeNonYoungBiomass(SiteVars.Cohorts[site]);
            //(double) SiteVars.ComputeNonYoungBiomass(site);

            int lastMortality = SiteVars.Cohorts[site].PrevYearMortality;
            B_ACT = Math.Min(EcoregionData.B_MAX[ecoregion] - lastMortality, B_ACT);

            //ActualSiteBiomass(cohorts[site], site, out ecoregion);

            //  Relative living biomass (ratio of actual to maximum site
            //  biomass).
            double B_AM = B_ACT / EcoregionData.B_MAX[ecoregion];

            for (byte shade = 5; shade >= 1; shade--)
            {
                if(EcoregionData.MinRelativeBiomass[shade][ecoregion] == null)
                {
                    string mesg = string.Format("Minimum relative biomass has not been defined for ecoregion {0}", ecoregion.Name);
                    throw new System.ApplicationException(mesg);
                }
                if (B_AM >= EcoregionData.MinRelativeBiomass[shade][ecoregion])
                    return shade;
            }
            return 0;

        }
        //---------------------------------------------------------------------

    }
}
