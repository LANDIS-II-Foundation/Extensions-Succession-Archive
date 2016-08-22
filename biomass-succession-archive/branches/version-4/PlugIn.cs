//  Copyright 2007 Conservation Biology Institute
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
        private double pctSun1;
        private double pctSun2;
        private double pctSun3;
        private double pctSun4;
        private double pctSun5;



        //---------------------------------------------------------------------

        public PlugIn()
            : base("Biomass Succession v3")
        {
        }

        //---------------------------------------------------------------------

        public override void Initialize(string dataFile,
                                        PlugIns.ICore modelCore)
        {
            Model.Core = modelCore;
            InputParametersParser parser = new InputParametersParser();
            IInputParameters parameters = Landis.Data.Load<IInputParameters>(dataFile, parser);

            Timestep = parameters.Timestep;
            CalibrateMode = parameters.CalibrateMode;
            CohortBiomass.SpinupMortalityFraction = parameters.SpinupMortalityFraction;

            sufficientLight = parameters.LightClassProbabilities;
            pctSun1 = parameters.PctSun1;
            pctSun2 = parameters.PctSun2;
            pctSun3 = parameters.PctSun3;
            pctSun4 = parameters.PctSun4;
            pctSun5 = parameters.PctSun5;

            SiteVars.Initialize();
            SpeciesData.Initialize(parameters);
            EcoregionData.Initialize(parameters);
            DynamicInputs.Initialize(parameters.DynamicInputFile, false);
            SpeciesData.ChangeDynamicParameters(0);  // Year 0
            Outputs.Initialize(parameters);


            //  Cohorts must be created before the base class is initialized
            //  because the base class' reproduction module uses the core's
            //  SuccessionCohorts property in its Initialization method.
            Biomass.Cohorts.Initialize(Timestep, new CohortBiomass());

            //cohorts = SiteVars.Cohorts; //Model.Core.Landscape.NewSiteVar<SiteCohorts>();
            landscapeCohorts = new LandscapeCohorts(SiteVars.Cohorts);
            Cohorts = landscapeCohorts;

            Reproduction.SufficientLight = SufficientLight;

            InitialBiomass.Initialize(Timestep);
            //LivingBiomass.Initialize(parameters, cohorts);

            base.Initialize(modelCore,
                            Util.ToArray<double>(SpeciesData.EstablishProbability),
                            parameters.SeedAlgorithm,
                            (Reproduction.Delegates.AddNewCohort)AddNewCohort);


            Cohort.DeathEvent += CohortDied;
            AgeOnlyDisturbances.Module.Initialize(parameters.AgeOnlyDisturbanceParms);

            //DynamicChange.Module.Initialize(parameters.DynamicChangeUpdates);
        }


        //---------------------------------------------------------------------

        protected override void InitializeSite(ActiveSite site,
                                               ICommunity initialCommunity)
        {
            InitialBiomass initialBiomass = InitialBiomass.Compute(site, initialCommunity);

            SiteVars.Cohorts[site] = initialBiomass.Cohorts.Clone();
            SiteVars.WoodyDebris[site] = initialBiomass.DeadWoodyPool.Clone();
            SiteVars.Litter[site] = initialBiomass.DeadNonWoodyPool.Clone();
        }

        //---------------------------------------------------------------------

        public override void Run()
        {
            if(Model.Core.CurrentTime == Timestep)
                Outputs.WriteLogFile(0);

            if(Model.Core.CurrentTime > 0 && SiteVars.CapacityReduction == null)
                SiteVars.CapacityReduction   = Model.Core.GetSiteVar<double>("Harvest.CapacityReduction");

            //SpeciesData.ChangeDynamicParameters(Model.Core.CurrentTime);

            //DynamicChange.Module.CheckForUpdate();
            base.Run();

            Outputs.WriteLogFile(Model.Core.CurrentTime);
            /*
            //  Write LAI map
            string path = MapNames.ReplaceTemplateVars("./biomass-succession/LAI-{timestep}.gis", Model.Core.CurrentTime);
            IOutputRaster<UShortPixel> map = Util.CreateMap(path);
            using (map) {
                UShortPixel pixel = new UShortPixel();
                foreach (Site site in Model.Core.Landscape.AllSites) {
                    if (site.IsActive) {
                            pixel.Band0 = (ushort) (SiteVars.LAI[site]);
                    }
                    else {
                        //  Inactive site
                        pixel.Band0 = 0;
                    }
                    map.WritePixel(pixel);
                }
            }
            */

            //  Write Percent Shade map
            string path2 = MapNames.ReplaceTemplateVars("./biomass-succession/Percent-Shade-{timestep}.gis", Model.Core.CurrentTime);
            IOutputRaster<UShortPixel> map2 = Util.CreateMap(path2);
            using (map2)
            {
                UShortPixel pixel = new UShortPixel();
                foreach (Site site in Model.Core.Landscape.AllSites)
                {
                    if (site.IsActive)
                    {
                        pixel.Band0 = (ushort)Math.Max(SiteVars.PercentShade[site] * 100.0, 0.0);
                    }
                    else
                    {
                        //  Inactive site
                        pixel.Band0 = 0;
                    }
                    map2.WritePixel(pixel);
                }
            }
        }


        //---------------------------------------------------------------------
        /// <summary>
        /// Determines if there is sufficient light at a site for a species to
        /// germinate/resprout.
        /// </summary>
        public bool SufficientLight(ISpecies species, ActiveSite site)
        {

            //UI.WriteLine("  Calculating Sufficient Light from Succession.");
            byte siteShade = Model.Core.GetSiteVar<byte>("Shade")[site];

            double lightProbability = 0.0;

            bool found = false;

            foreach (ISufficientLight lights in sufficientLight)
            {

                //UI.WriteLine("Sufficient Light:  ShadeClass={0}, Prob0={1}.", lights.ShadeClass, lights.ProbabilityLight0);
                if (lights.ShadeClass == species.ShadeTolerance)
                {
                    if (siteShade == 0) lightProbability = lights.ProbabilityLight0;
                    if (siteShade == 1) lightProbability = lights.ProbabilityLight1;
                    if (siteShade == 2) lightProbability = lights.ProbabilityLight2;
                    if (siteShade == 3) lightProbability = lights.ProbabilityLight3;
                    if (siteShade == 4) lightProbability = lights.ProbabilityLight4;
                    if (siteShade == 5) lightProbability = lights.ProbabilityLight5;
                    found = true;
                }
            }

            if (!found) UI.WriteLine("Could not find sufficient light data for {0}.", species.Name);

            return Landis.Util.Random.GenerateUniform() < lightProbability;
        }
        //---------------------------------------------------------------------
        // Original for v3
        /*
        public override byte ComputeShade(ActiveSite site)
        {
            // Use correlation from Scheller and Mladenoff (Figure 4b)
            // to assign a shade class depending on percent transmittance.

            double percentSun = Math.Max(1.0 - SiteVars.PercentShade[site], 0.0);
            percentSun = Math.Min(1.0, percentSun);

            percentSun = percentSun * 100.0;

            byte shadeClass = 0;

            if(percentSun < 60.0) shadeClass = 1;
            if(percentSun < 45.0) shadeClass = 2;
            if(percentSun < 30.0) shadeClass = 3;
            if(percentSun < 15.0) shadeClass = 4;
            if(percentSun < 5.0) shadeClass = 5;

            return shadeClass;

        }
         * */
        //---------------------------------------------------------------------
        // Revised 10/5/09 - BRM

        public override byte ComputeShade(ActiveSite site)
        {
            // Use correlation from Scheller and Mladenoff (Figure 4b)
            // to assign a shade class depending on percent transmittance.


            SiteVars.PercentShade[site] = 1.0 - SiteVars.LightTrans[site];

            double percentSun = Math.Max(1.0 - SiteVars.PercentShade[site], 0.0);
            percentSun = Math.Min(1.0, percentSun);

            percentSun = percentSun * 100.0;

            byte shadeClass = 0;

            if (percentSun < pctSun1) shadeClass = 1;
            if (percentSun < pctSun2) shadeClass = 2;
            if (percentSun < pctSun3) shadeClass = 3;
            if (percentSun < pctSun4) shadeClass = 4;
            if (percentSun < pctSun5) shadeClass = 5;

            return shadeClass;

        }
        //---------------------------------------------------------------------

        public void CohortDied(object sender,
                               DeathEventArgs eventArgs)
        {
            PlugInType disturbanceType = eventArgs.DisturbanceType;
            if (disturbanceType != null)
            {
                ActiveSite site = eventArgs.Site;
                Disturbed[site] = true;
                if (disturbanceType.IsMemberOf("disturbance:fire"))
                    Landis.Succession.Reproduction.CheckForPostFireRegen(eventArgs.Cohort, site);
                else
                    Landis.Succession.Reproduction.CheckForResprouting(eventArgs.Cohort, site);
            }
        }

        //---------------------------------------------------------------------

        public void AddNewCohort(ISpecies species,
                                 ActiveSite site)
        {
            SiteVars.Cohorts[site].AddNewCohort(species, CohortBiomass.InitialBiomass(species, SiteVars.Cohorts[site],
                                                                    site));
        }

        //---------------------------------------------------------------------

        protected override void AgeCohorts(ActiveSite site,
                                           ushort years,
                                           int? successionTimestep)
        {
            GrowCohorts(SiteVars.Cohorts[site], site, years, successionTimestep.HasValue);
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Grows all cohorts at a site for a specified number of years.  The
        /// dead pools at the site also decompose for the given time period.
        /// </summary>
        public static void GrowCohorts(SiteCohorts cohorts,
                                       ActiveSite site,
                                       int years,
                                       bool isSuccessionTimestep)
        {
            if (SiteVars.Cohorts[site] == null)
                return;

            for (int y = 1; y <= years; ++y)
            {

                SpeciesData.ChangeDynamicParameters(Model.Core.CurrentTime + y - 1);

                SiteVars.ResetAnnualValues(site);
                CohortBiomass.SubYear = y - 1;
                CohortBiomass.CanopyLightExtinction = 0.0;

                // SiteVars.LAI[site] = 0.0;
                SiteVars.PercentShade[site] = 0.0;
                SiteVars.LightTrans[site] = 1.0;

                SiteVars.Cohorts[site].Grow(site, (y == years && isSuccessionTimestep));
                SiteVars.WoodyDebris[site].Decompose();
                SiteVars.Litter[site].Decompose();
            }
        }
    }
}
