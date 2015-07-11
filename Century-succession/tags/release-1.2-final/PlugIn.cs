//  Copyright 2007 Conservation Biology Institute
//  Authors:  Robert M. Scheller
//  License:  Available at
//  http://www.landis-ii.org/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Util;
using Edu.Wisc.Forest.Flel.Grids;

using Landis.InitialCommunities;
using Landis.Landscape;
using Landis.PlugIns;
using Landis.Species;
using Landis.Ecoregions;
using Landis.Succession;
using Landis.RasterIO;
using Landis.Util;
using Landis.Biomass;
using Landis.Library.Climate;

using System;
using System.Collections.Generic;
//using System.Threading;

namespace Landis.Extension.Succession.Century
{
    public class PlugIn
        : Landis.Succession.PlugIn
    {
        private LandscapeCohorts landscapeCohorts;
        private List<ISufficientLight> sufficientLight;
        //private uint? prevSiteDataIndex;
        public static string SoilCarbonMapNames = null;
        public static int SoilCarbonMapFrequency;
        public static string SoilNitrogenMapNames = null;
        public static int SoilNitrogenMapFrequency;
        public static string ANPPMapNames = null;
        public static int ANPPMapFrequency;
        public static string ANEEMapNames = null;
        public static int ANEEMapFrequency;

        //---------------------------------------------------------------------

        public PlugIn()
            : base("Century Succession")
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

            Timestep              = parameters.Timestep;
            sufficientLight       = parameters.LightClassProbabilities;
            CohortBiomass.SpinupMortalityFraction = parameters.SpinupMortalityFraction;

            SiteVars.Initialize();

            //  Initialize climate.  A list of ecoregion indices is passed so that
            //  the climate library can operate independently of the LANDIS-II core.
            List<int> ecoregionIndices = new List<int>();
            foreach(IEcoregion ecoregion in Model.Core.Ecoregions)
            {
                ecoregionIndices.Add(ecoregion.Index);
                UI.WriteLine("    Century:  preparing climate data:  {0} = ecoregion index {1}", ecoregion.Name, ecoregion.Index);
            }
            Climate.Initialize(parameters.ClimateFile, ecoregionIndices, false);

            EcoregionData.Initialize(parameters);
            SpeciesData.Initialize(parameters);
            EcoregionData.ChangeParameters(parameters);

            OtherData.Initialize(parameters);
            FunctionalType.Initialize(parameters);
            Outputs.Initialize(parameters);
            Outputs.InitializeMonthly(parameters);

            //  Cohorts must be created before the base class is initialized
            //  because the base class' reproduction module uses the core's
            //  SuccessionCohorts property in its Initialization method.
            Biomass.Cohorts.Initialize(Timestep, new CohortBiomass());

            //cohorts = Model.Core.Landscape.NewSiteVar<SiteCohorts>();
            landscapeCohorts = new LandscapeCohorts(SiteVars.SiteCohorts); //cohorts);
            Cohorts = landscapeCohorts;

            Reproduction.SufficientLight = SufficientLight;

            InitialBiomass.Initialize(Timestep);

            base.Initialize(modelCore,
                            Util.ToArray<double>(SpeciesData.EstablishProbability),
                            parameters.SeedAlgorithm,
                            (Reproduction.Delegates.AddNewCohort) AddNewCohort);

            Cohort.DeathEvent += CohortDied;
            AgeOnlyDisturbances.Module.Initialize(parameters.AgeOnlyDisturbanceParms);

            Dynamic.Module.Initialize(parameters.DynamicUpdates);
            //EcoregionData.Initialize(parameters);
            FireEffects.Initialize(parameters);


        }

        //---------------------------------------------------------------------

        public override void Run()
        {
            if(Model.Core.CurrentTime == Timestep)
                Outputs.WriteLogFile(0);

            if(Model.Core.CurrentTime > 0)
                SiteVars.InitializeDisturbances();


            Dynamic.Module.CheckForUpdate();
            EcoregionData.GenerateNewClimate(Model.Core.CurrentTime, Timestep);

            // Update Pest only once.
            SpeciesData.EstablishProbability = Establishment.GenerateNewEstablishProbabilities(Timestep);
            Reproduction.ChangeEstablishProbabilities(Util.ToArray<double>(SpeciesData.EstablishProbability));


            base.RunReproductionFirst();

            // Write monthly log file:
            // Output must reflect the order of operation:
            int[] months = new int[12]{6, 7, 8, 9, 10, 11, 0, 1, 2, 3, 4, 5};

            if(OtherData.CalibrateMode)
            months = new int[12]{0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11};

            for (int i = 0; i < 12; i++)
            {
                int month = months[i];
                Outputs.WriteMonthlyLogFile(month);
            }
            Outputs.WriteLogFile(Model.Core.CurrentTime);

            if(Model.Core.CurrentTime > 0)
            {
                string path = MapNames.ReplaceTemplateVars("century/TotalC-{timestep}.gis", Model.Core.CurrentTime);
                IOutputRaster<UShortPixel> map = Model.Core.CreateRaster<UShortPixel>(path, Model.Core.Landscape.Dimensions, Model.Core.LandscapeMapMetadata);
                using (map)
                {
                    UShortPixel pixel = new UShortPixel();
                    foreach (Site site in Model.Core.Landscape.AllSites)
                    {
                        if (site.IsActive)
                        {
                            pixel.Band0 = (ushort)((SiteVars.SOM1surface[site].Carbon +
                                                    SiteVars.SOM1soil[site].Carbon +
                                                    SiteVars.SOM2[site].Carbon +
                                                    SiteVars.SOM3[site].Carbon +
                                                    SiteVars.CohortLeafC[site] +
                                                    SiteVars.CohortWoodC[site] +
                                                    SiteVars.SurfaceDeadWood[site].Carbon +
                                                    SiteVars.SurfaceStructural[site].Carbon +
                                                    SiteVars.SurfaceMetabolic[site].Carbon +
                                                    SiteVars.SoilDeadWood[site].Carbon +
                                                    SiteVars.SoilStructural[site].Carbon +
                                                    SiteVars.SoilMetabolic[site].Carbon) / 100.0);
                        }
                        else
                        {
                            //  Inactive site
                            pixel.Band0 = 0;
                        }
                        map.WritePixel(pixel);
                    }
                }
            }

            
            if(SoilCarbonMapNames != null && (Model.Core.CurrentTime % SoilCarbonMapFrequency) == 0)
            {
                string path = MapNames.ReplaceTemplateVars(SoilCarbonMapNames, Model.Core.CurrentTime);
                IOutputRaster<UShortPixel> map = Model.Core.CreateRaster<UShortPixel>(path, Model.Core.Landscape.Dimensions, Model.Core.LandscapeMapMetadata);
                using (map) {
                    UShortPixel pixel = new UShortPixel();
                    foreach (Site site in Model.Core.Landscape.AllSites) {
                        if (site.IsActive) {
                            pixel.Band0 = (ushort) ((SiteVars.SOM1surface[site].Carbon + SiteVars.SOM1soil[site].Carbon + SiteVars.SOM2[site].Carbon + SiteVars.SOM3[site].Carbon) / 100.0);
                        }
                        else {
                            //  Inactive site
                            pixel.Band0 = 0;
                        }
                        map.WritePixel(pixel);
                    }
                }
            }


            if(SoilNitrogenMapNames != null && (Model.Core.CurrentTime % SoilNitrogenMapFrequency) == 0)
            {
                string path = MapNames.ReplaceTemplateVars(SoilNitrogenMapNames, Model.Core.CurrentTime);
                IOutputRaster<UShortPixel> map2 = Model.Core.CreateRaster<UShortPixel>(path, Model.Core.Landscape.Dimensions, Model.Core.LandscapeMapMetadata);
                using (map2) {
                    UShortPixel pixel = new UShortPixel();
                    foreach (Site site in Model.Core.Landscape.AllSites) {
                        if (site.IsActive) {
                            pixel.Band0 = (ushort) (SiteVars.MineralN[site]);
                        }
                        else {
                            //  Inactive site
                            pixel.Band0 = 0;
                        }
                        map2.WritePixel(pixel);
                    }
                }
            }

            if(ANPPMapNames != null && (Model.Core.CurrentTime % ANPPMapFrequency) == 0)
            {
                string path = MapNames.ReplaceTemplateVars(ANPPMapNames, Model.Core.CurrentTime);
                IOutputRaster<UShortPixel> map3 = Model.Core.CreateRaster<UShortPixel>(path, Model.Core.Landscape.Dimensions, Model.Core.LandscapeMapMetadata);
                using (map3) {
                    UShortPixel pixel = new UShortPixel();
                    foreach (Site site in Model.Core.Landscape.AllSites) {
                        if (site.IsActive) {
                            pixel.Band0 = (ushort) SiteVars.AGNPPcarbon[site];
                        }
                        else {
                            //  Inactive site
                            pixel.Band0 = 0;
                        }
                        map3.WritePixel(pixel);
                    }
                }
            }
            if(ANEEMapNames != null && (Model.Core.CurrentTime % ANEEMapFrequency) == 0)
            {

                string path = MapNames.ReplaceTemplateVars(ANEEMapNames, Model.Core.CurrentTime);
                IOutputRaster<UShortPixel> map4 = Model.Core.CreateRaster<UShortPixel>(path, Model.Core.Landscape.Dimensions, Model.Core.LandscapeMapMetadata);
                using (map4) {
                    UShortPixel pixel = new UShortPixel();
                    foreach (Site site in Model.Core.Landscape.AllSites) {
                        //UI.WriteLine("  ANEE = {0:0.00}, Transformed = {1:0}.", SiteVars.AnnualNEE[site], (ushort) ((SiteVars.AnnualNEE[site] * -0.1) + 1000));
                        if (site.IsActive) {
                            pixel.Band0 = (ushort) (SiteVars.AnnualNEE[site] + 1000) ;
                        }
                        else {
                            //  Inactive site
                            pixel.Band0 = 0;
                        }
                        map4.WritePixel(pixel);
                    }
                }
            }
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Determines if there is sufficient light at a site for a species to
        /// germinate/resprout.
        /// Also accounts for SITE level N limitations.  N limits could not
        /// be accommodated in the Establishment Probability as that is an ecoregion x spp property.
        /// Therefore, would better be described as "SiteLevelDeterminantReproduction".
        /// </summary>
        public bool SufficientLight(ISpecies   species,
                                           ActiveSite site)
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

            if(!found)
                UI.WriteLine("A Sufficient Light value was not found for {0}.", species.Name);

            // This is neccesary to account for Nitrogen limitation:
            // Update species establishment probabilities
            //double NlimitedEstablishment = SpeciesData.NLimits[species];

            //if(NlimitedEstablishment < lightProbability)
            //    UI.WriteLine("Establishment limited by NITROGEN.  Spp={0}, Nlimit={1:0.00}, Llimit={2:0.00}.", species.Name, NlimitedEstablishment, lightProbability);

            //lightProbability = Math.Min(lightProbability, NlimitedEstablishment);


            return Landis.Util.Random.GenerateUniform() < lightProbability;

        }

        //---------------------------------------------------------------------

        public override byte ComputeShade(ActiveSite site)
        {
            //return LivingBiomass.ComputeShade(site);
            IEcoregion ecoregion = Model.Core.Ecoregion[site];
            double B_MAX = (double) EcoregionData.B_MAX[ecoregion];

            double oldBiomass = (double) Biomass.Cohorts.ComputeNonYoungBiomass(SiteVars.SiteCohorts[site]);

            int lastMortality = SiteVars.SiteCohorts[site].PrevYearMortality;
            double B_ACT = Math.Min(B_MAX - lastMortality, oldBiomass);

            //  Relative living biomass (ratio of actual to maximum site
            //  biomass).
            double B_AM = B_ACT / B_MAX;

            for (byte shade = 5; shade >= 1; shade--)
            {
                if(EcoregionData.ShadeBiomass[shade][ecoregion] <= 0)
                {
                    string mesg = string.Format("Minimum relative biomass has not been defined for ecoregion {0}", ecoregion.Name);
                    throw new System.ApplicationException(mesg);
                }
                if (B_AM >= EcoregionData.ShadeBiomass[shade][ecoregion])
                {
                    //UI.WriteLine("Shade Calculation:  lastMort={0:0.0}, B_MAX={1}, oldB={2}, B_ACT={3}, shade={4}.", lastMortality, B_MAX, oldBiomass, B_ACT, shade);
                    return shade;
                }
            }

            //UI.WriteLine("Shade Calculation:  lastMort={0:0.0}, B_MAX={1}, oldB={2}, B_ACT={3}, shade=0.", lastMortality, B_MAX,oldBiomass,B_ACT);

            return 0;
        }
        //---------------------------------------------------------------------

        protected override void InitializeSite(ActiveSite site,
                                               ICommunity initialCommunity)
        {
            //SpeciesData.CalculateNGrowthLimits(site);

            InitialBiomass initialBiomass = InitialBiomass.Compute(site, initialCommunity);
            SiteVars.SiteCohorts[site] = initialBiomass.Cohorts.Clone();

            SiteVars.SurfaceDeadWood[site]       = initialBiomass.SurfaceDeadWood.Clone();
            SiteVars.SurfaceStructural[site]     = initialBiomass.SurfaceStructural.Clone();
            SiteVars.SurfaceMetabolic[site]      = initialBiomass.SurfaceMetabolic.Clone();

            SiteVars.SoilDeadWood[site]          = initialBiomass.SoilDeadWood.Clone();
            SiteVars.SoilStructural[site]        = initialBiomass.SoilStructural.Clone();
            SiteVars.SoilMetabolic[site]         = initialBiomass.SoilMetabolic.Clone();

            SiteVars.SOM1surface[site]           = initialBiomass.SOM1surface.Clone();
            SiteVars.SOM1soil[site]              = initialBiomass.SOM1soil.Clone();
            SiteVars.SOM2[site]                  = initialBiomass.SOM2.Clone();
            SiteVars.SOM3[site]                  = initialBiomass.SOM3.Clone();

            SiteVars.MineralN[site]              = initialBiomass.MineralN;
            SiteVars.CohortLeafC[site]           = initialBiomass.CohortLeafC;
            SiteVars.CohortLeafN[site]           = initialBiomass.CohortLeafN;
            SiteVars.CohortWoodC[site]           = initialBiomass.CohortWoodC;
            SiteVars.CohortWoodN[site]           = initialBiomass.CohortWoodN;
        }


        //---------------------------------------------------------------------

        public void CohortDied(object         sender,
                               DeathEventArgs eventArgs)
        {

            //UI.WriteLine("Cohort Died! :-(");

            PlugInType disturbanceType = eventArgs.DisturbanceType;
            ActiveSite site = eventArgs.Site;

            ICohort cohort = eventArgs.Cohort;
            double foliar = (double) cohort.LeafBiomass;

            double wood = (double) cohort.WoodBiomass;

            //UI.WriteLine("Cohort Died: species={0}, age={1}, biomass={2}, foliage={3}.", cohort.Species.Name, cohort.Age, cohort.Biomass, foliar);

            if (disturbanceType == null) {
                //UI.WriteLine("NO EVENT: Cohort Died: species={0}, age={1}, disturbance={2}.", cohort.Species.Name, cohort.Age, eventArgs.DisturbanceType);

                ForestFloor.AddWoodLitter(wood, cohort.Species, eventArgs.Site);
                ForestFloor.AddFoliageLitter(foliar, cohort.Species, eventArgs.Site);

                Roots.AddCoarseRootLitter(wood, cohort.Species, eventArgs.Site);
                Roots.AddFineRootLitter(foliar, cohort.Species, eventArgs.Site);
            }

            if (disturbanceType != null) {
                //UI.WriteLine("DISTURBANCE EVENT: Cohort Died: species={0}, age={1}, disturbance={2}.", cohort.Species.Name, cohort.Age, eventArgs.DisturbanceType);

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
            float[] initialBiomass = CohortBiomass.InitialBiomass(SiteVars.SiteCohorts[site], site, species);
            SiteVars.SiteCohorts[site].AddNewCohort(species, initialBiomass[0], initialBiomass[1]);
        }
        //---------------------------------------------------------------------

        protected override void AgeCohorts(ActiveSite site,
                                           ushort     years,
                                           int?       successionTimestep)
        {
            Century.Run(site.Location, years, successionTimestep.HasValue);

        }


    }

}
