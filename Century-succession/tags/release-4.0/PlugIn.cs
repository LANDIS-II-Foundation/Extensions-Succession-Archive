//  Copyright 2007-2010 Portland State University, University of Wisconsin-Madison
//  Author: Robert Scheller, Ben Sulman

using Landis.Core;
using Landis.SpatialModeling;
using Edu.Wisc.Forest.Flel.Util;

using Landis.Library.InitialCommunities;
using Landis.Library.Succession;
using Landis.Library.LeafBiomassCohorts;
using Landis.Library.Climate;
using Landis.Library.Metadata;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Landis.Extension.Succession.Century
{
    public class PlugIn
        : Landis.Library.Succession.ExtensionBase
    {
        public static readonly string ExtensionName = "Century Succession";
        private static ICore modelCore;
        private IInputParameters parameters;


        private List<ISufficientLight> sufficientLight;
        public static string SoilCarbonMapNames = null;
        public static int SoilCarbonMapFrequency;
        public static string SoilNitrogenMapNames = null;
        public static int SoilNitrogenMapFrequency;
        public static string ANPPMapNames = null;
        public static int ANPPMapFrequency;
        public static string ANEEMapNames = null;
        public static int ANEEMapFrequency;
        public static string TotalCMapNames = null;
        public static int TotalCMapFrequency;
        public static int SuccessionTimeStep;
        public static double ProbEstablishAdjust;

        public static int FutureClimateBaseYear;

        //---------------------------------------------------------------------

        public PlugIn()
            : base(ExtensionName)
        {
        }

        //---------------------------------------------------------------------

        public override void LoadParameters(string dataFile,
                                            ICore mCore)
        {
            modelCore = mCore;
            SiteVars.Initialize();
            InputParametersParser parser = new InputParametersParser();
            parameters = Landis.Data.Load<IInputParameters>(dataFile, parser);

        }

        //---------------------------------------------------------------------

        public static ICore ModelCore
        {
            get
            {
                return modelCore;
            }
        }


        //---------------------------------------------------------------------

        public override void Initialize()
        {
            PlugIn.ModelCore.UI.WriteLine("Initializing {0} ...", ExtensionName);
            Timestep              = parameters.Timestep;
            SuccessionTimeStep    = Timestep;
            sufficientLight       = parameters.LightClassProbabilities;
            ProbEstablishAdjust = parameters.ProbEstablishAdjustment;
            MetadataHandler.InitializeMetadata(Timestep, modelCore, SoilCarbonMapNames, SoilNitrogenMapNames, ANPPMapNames, ANEEMapNames, TotalCMapNames);
            CohortBiomass.SpinupMortalityFraction = parameters.SpinupMortalityFraction;
            
            //Initialize climate.
            Climate.Initialize(parameters.ClimateConfigFile, false, modelCore);
            FutureClimateBaseYear = Climate.Future_MonthlyData.Keys.Min();
            
            EcoregionData.Initialize(parameters);
            SpeciesData.Initialize(parameters);
            EcoregionData.ChangeParameters(parameters);

            OtherData.Initialize(parameters);
            FunctionalType.Initialize(parameters);

            //  Cohorts must be created before the base class is initialized
            //  because the base class' reproduction module uses the core's
            //  SuccessionCohorts property in its Initialization method.
            Library.LeafBiomassCohorts.Cohorts.Initialize(Timestep, new CohortBiomass());

            // Initialize Reproduction routines:
            Reproduction.SufficientResources = SufficientLight;
            Reproduction.Establish = Establish;
            Reproduction.AddNewCohort = AddNewCohort;
            Reproduction.MaturePresent = MaturePresent;
            base.Initialize(modelCore, parameters.SeedAlgorithm);

            InitialBiomass.Initialize(Timestep);

            Cohort.DeathEvent += CohortDied;
            AgeOnlyDisturbances.Module.Initialize(parameters.AgeOnlyDisturbanceParms);

            Dynamic.Module.Initialize(parameters.DynamicUpdates);
            //EcoregionData.Initialize(parameters);
            FireEffects.Initialize(parameters);
            InitializeSites(parameters.InitialCommunities, parameters.InitialCommunitiesMap, modelCore); //the spinup is heppend within this fucntion
            if (parameters.CalibrateMode)
                Outputs.CreateCalibrateLogFile();

            Outputs.WritePrimaryLogFile(0);
            
        }

        //---------------------------------------------------------------------

        public override void Run()
        {
            
            if (PlugIn.ModelCore.CurrentTime > 0)
                    SiteVars.InitializeDisturbances();

            // Update Pest only once.
            SpeciesData.EstablishProbability = Establishment.GenerateNewEstablishProbabilities(Timestep);
            EcoregionData.AnnualNDeposition = new Ecoregions.AuxParm<double>(PlugIn.ModelCore.Ecoregions);

            base.RunReproductionFirst();

            // Write monthly log file:
            // Output must reflect the order of operation:
            int[] months = new int[12] { 6, 7, 8, 9, 10, 11, 0, 1, 2, 3, 4, 5 };

            if (OtherData.CalibrateMode)
                months = new int[12] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };

            for (int i = 0; i < 12; i++)
            {
                int month = months[i];
                Outputs.WriteMonthlyLogFile(month);
            }
            Outputs.WritePrimaryLogFile(PlugIn.ModelCore.CurrentTime);

            if (SoilCarbonMapNames != null)// && (PlugIn.ModelCore.CurrentTime % SoilCarbonMapFrequency) == 0)
            {
                string path = MapNames.ReplaceTemplateVars(SoilCarbonMapNames, PlugIn.ModelCore.CurrentTime);
                using (IOutputRaster<IntPixel> outputRaster = modelCore.CreateRaster<IntPixel>(path, modelCore.Landscape.Dimensions))
                {
                    IntPixel pixel = outputRaster.BufferPixel;
                    foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                    {
                        if (site.IsActive)
                        {
                            pixel.MapCode.Value = (int)((SiteVars.SOM1surface[site].Carbon + SiteVars.SOM1soil[site].Carbon + SiteVars.SOM2[site].Carbon + SiteVars.SOM3[site].Carbon));
                        }
                        else
                        {
                            //  Inactive site
                            pixel.MapCode.Value = 0;
                        }
                        outputRaster.WriteBufferPixel();
                    }
                }
            }

            if (SoilNitrogenMapNames != null)// && (PlugIn.ModelCore.CurrentTime % SoilNitrogenMapFrequency) == 0)
            {
                string path2 = MapNames.ReplaceTemplateVars(SoilNitrogenMapNames, PlugIn.ModelCore.CurrentTime);
                using (IOutputRaster<ShortPixel> outputRaster = modelCore.CreateRaster<ShortPixel>(path2, modelCore.Landscape.Dimensions))
                {
                    ShortPixel pixel = outputRaster.BufferPixel;
                    foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                    {
                        if (site.IsActive)
                        {
                            pixel.MapCode.Value = (short)(SiteVars.MineralN[site]);
                        }
                        else
                        {
                            //  Inactive site
                            pixel.MapCode.Value = 0;
                        }
                        outputRaster.WriteBufferPixel();
                    }
                }
            }

            if (ANPPMapNames != null)// && (PlugIn.ModelCore.CurrentTime % ANPPMapFrequency) == 0)
            {
                string path3 = MapNames.ReplaceTemplateVars(ANPPMapNames, PlugIn.ModelCore.CurrentTime);
                using (IOutputRaster<ShortPixel> outputRaster = modelCore.CreateRaster<ShortPixel>(path3, modelCore.Landscape.Dimensions))
                {
                    ShortPixel pixel = outputRaster.BufferPixel;
                    foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                    {
                        if (site.IsActive)
                        {
                            pixel.MapCode.Value = (short)SiteVars.AGNPPcarbon[site];
                        }
                        else
                        {
                            //  Inactive site
                            pixel.MapCode.Value = 0;
                        }
                        outputRaster.WriteBufferPixel();
                    }
                }
            }
            if (ANEEMapNames != null)// && (PlugIn.ModelCore.CurrentTime % ANEEMapFrequency) == 0)
            {

                string path4 = MapNames.ReplaceTemplateVars(ANEEMapNames, PlugIn.ModelCore.CurrentTime);
                using (IOutputRaster<ShortPixel> outputRaster = modelCore.CreateRaster<ShortPixel>(path4, modelCore.Landscape.Dimensions))
                {
                    ShortPixel pixel = outputRaster.BufferPixel;
                    foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                    {
                        if (site.IsActive)
                        {
                            pixel.MapCode.Value = (short)(SiteVars.AnnualNEE[site] + 1000);
                        }
                        else
                        {
                            //  Inactive site
                            pixel.MapCode.Value = 0;
                        }
                        outputRaster.WriteBufferPixel();
                    }
                }
            }
            if (TotalCMapNames != null)// && (PlugIn.ModelCore.CurrentTime % TotalCMapFrequency) == 0)
            {

                string path5 = MapNames.ReplaceTemplateVars(TotalCMapNames, PlugIn.ModelCore.CurrentTime);
                using (IOutputRaster<IntPixel> outputRaster = modelCore.CreateRaster<IntPixel>(path5, modelCore.Landscape.Dimensions))
                {
                    IntPixel pixel = outputRaster.BufferPixel;
                    foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                    {
                        if (site.IsActive)
                        {
                            pixel.MapCode.Value = (int)(Outputs.GetOrganicCarbon(site) +
                                SiteVars.CohortLeafC[site] +
                                SiteVars.CohortFRootC[site] +
                                SiteVars.CohortWoodC[site] +
                                SiteVars.CohortCRootC[site] +
                                SiteVars.SurfaceDeadWood[site].Carbon +
                                SiteVars.SoilDeadWood[site].Carbon);
                        }
                        else
                        {
                            //  Inactive site
                            pixel.MapCode.Value = 0;
                        }
                        outputRaster.WriteBufferPixel();
                    }
                }
            }


        }


        //---------------------------------------------------------------------

        public override byte ComputeShade(ActiveSite site)
        {
            IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];

            byte finalShade = 0;

            if (!ecoregion.Active)
                return 0;

            double B_MAX = (double) EcoregionData.B_MAX[ecoregion];

            double oldBiomass = (double) Library.LeafBiomassCohorts.Cohorts.ComputeNonYoungBiomass(SiteVars.Cohorts[site]);

            int lastMortality = SiteVars.PrevYearMortality[site];
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
                //PlugIn.ModelCore.UI.WriteLine("Shade Calculation:  lastMort={0:0.0}, B_MAX={1}, oldB={2}, B_ACT={3}, shade={4}.", lastMortality, B_MAX,oldBiomass,B_ACT,shade);
                if (B_AM >= EcoregionData.ShadeBiomass[shade][ecoregion])
                {
                    finalShade = shade;
                    break;
                }
            }

            //PlugIn.ModelCore.UI.WriteLine("Yr={0},      Shade Calculation:  B_MAX={1}, B_ACT={2}, Shade={3}.", PlugIn.ModelCore.CurrentTime, B_MAX, B_ACT, finalShade);

            return finalShade;
        }
        //---------------------------------------------------------------------

        protected override void InitializeSite(ActiveSite site,
                                               ICommunity initialCommunity)
        {

            InitialBiomass initialBiomass = InitialBiomass.Compute(site, initialCommunity);
            SiteVars.Cohorts[site] = InitialBiomass.Clone(initialBiomass.Cohorts);
            //IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];

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
            SiteVars.CohortFRootC[site]           = initialBiomass.CohortFRootC;
            SiteVars.CohortLeafN[site]           = initialBiomass.CohortLeafN;
            SiteVars.CohortFRootN[site]            = initialBiomass.CohortFRootN;
            SiteVars.CohortWoodC[site]           = initialBiomass.CohortWoodC;
            SiteVars.CohortCRootC[site]          = initialBiomass.CohortCRootC;
            SiteVars.CohortWoodN[site]           = initialBiomass.CohortWoodN;
            SiteVars.CohortCRootN[site]          = initialBiomass.CohortCRootN;

            //// Override the spin-up soil C and N values with the contemporary data
            //// provided in the intialization file.
            //SiteVars.SOM1surface[site].Carbon       = parameters.InitialSOM1surfC[ecoregion];
            //SiteVars.SOM1surface[site].Nitrogen     = parameters.InitialSOM1surfN[ecoregion];
            //SiteVars.SOM1soil[site].Carbon          = parameters.InitialSOM1soilC[ecoregion];
            //SiteVars.SOM1soil[site].Nitrogen        = parameters.InitialSOM1soilN[ecoregion];
            //SiteVars.SOM2[site].Carbon              = parameters.InitialSOM2C[ecoregion];
            //SiteVars.SOM2[site].Nitrogen            = parameters.InitialSOM2N[ecoregion];
            //SiteVars.SOM3[site].Carbon              = parameters.InitialSOM3C[ecoregion];
            //SiteVars.SOM3[site].Nitrogen            = parameters.InitialSOM3N[ecoregion];
            //SiteVars.MineralN[site]                 = parameters.InitialMineralN[ecoregion];
        }


        //---------------------------------------------------------------------

        public void CohortDied(object         sender,
                               DeathEventArgs eventArgs)
        {

            //PlugIn.ModelCore.UI.WriteLine("Cohort Died! :-(");

            ExtensionType disturbanceType = eventArgs.DisturbanceType;
            ActiveSite site = eventArgs.Site;

            ICohort cohort = eventArgs.Cohort;
            double foliar = (double) cohort.LeafBiomass;

            double wood = (double) cohort.WoodBiomass;

            //PlugIn.ModelCore.UI.WriteLine("Cohort Died: species={0}, age={1}, biomass={2}, foliage={3}.", cohort.Species.Name, cohort.Age, cohort.Biomass, foliar);

            if (disturbanceType == null) {
                //PlugIn.ModelCore.UI.WriteLine("NO EVENT: Cohort Died: species={0}, age={1}, disturbance={2}.", cohort.Species.Name, cohort.Age, eventArgs.DisturbanceType);

                ForestFloor.AddWoodLitter(wood, cohort.Species, eventArgs.Site);
                ForestFloor.AddFoliageLitter(foliar, cohort.Species, eventArgs.Site);

                Roots.AddCoarseRootLitter(wood, cohort, cohort.Species, eventArgs.Site);
                Roots.AddFineRootLitter(foliar, cohort,  cohort.Species, eventArgs.Site);
            }

            if (disturbanceType != null) {
                //PlugIn.ModelCore.UI.WriteLine("DISTURBANCE EVENT: Cohort Died: species={0}, age={1}, disturbance={2}.", cohort.Species.Name, cohort.Age, eventArgs.DisturbanceType);

                Disturbed[site] = true;
                if (disturbanceType.IsMemberOf("disturbance:fire"))
                    Landis.Library.Succession.Reproduction.CheckForPostFireRegen(eventArgs.Cohort, site);
                else
                    Landis.Library.Succession.Reproduction.CheckForResprouting(eventArgs.Cohort, site);
            }
        }

        //---------------------------------------------------------------------
        //Grows the cohorts for future climate
        protected override void AgeCohorts(ActiveSite site,
                                           ushort     years,
                                           int?       successionTimestep)
        {
            Century.Run(site, years, successionTimestep.HasValue);

        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Determines if there is sufficient light at a site for a species to
        /// germinate/resprout.
        /// This is a Delegate method to base succession.
        /// </summary>
        public bool SufficientLight(ISpecies species, ActiveSite site)
        {

            //PlugIn.ModelCore.UI.WriteLine("  Calculating Sufficient Light from Succession.");
            byte siteShade = PlugIn.ModelCore.GetSiteVar<byte>("Shade")[site];

            double lightProbability = 0.0;
            bool found = false;

            foreach (ISufficientLight lights in sufficientLight)
            {

                //PlugIn.ModelCore.UI.WriteLine("Sufficient Light:  ShadeClass={0}, Prob0={1}.", lights.ShadeClass, lights.ProbabilityLight0);
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

            if (!found)
                PlugIn.ModelCore.UI.WriteLine("A Sufficient Light value was not found for {0}.", species.Name);

            return modelCore.GenerateUniform() < lightProbability;

        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Add a new cohort to a site.
        /// This is a Delegate method to base succession.
        /// </summary>

        public void AddNewCohort(ISpecies species, ActiveSite site)
        {
            float[] initialBiomass = CohortBiomass.InitialBiomass(species, SiteVars.Cohorts[site], site);
            SiteVars.Cohorts[site].AddNewCohort(species, 1, initialBiomass[0], initialBiomass[1]);
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// Determines if a species can establish on a site.
        /// This is a Delegate method to base succession.
        /// </summary>
        public bool Establish(ISpecies species, ActiveSite site)
        {
            IEcoregion ecoregion = modelCore.Ecoregion[site];
            double establishProbability = SpeciesData.EstablishProbability[species][ecoregion];

            return modelCore.GenerateUniform() < establishProbability;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Determines if a species can establish on a site.
        /// This is a Delegate method to base succession.
        /// </summary>
        public bool PlantingEstablish(ISpecies species, ActiveSite site)
        {
            IEcoregion ecoregion = modelCore.Ecoregion[site];
            double establishProbability = SpeciesData.EstablishProbability[species][ecoregion];

            return establishProbability > 0.0;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Determines if there is a mature cohort at a site.
        /// This is a Delegate method to base succession.
        /// </summary>
        public bool MaturePresent(ISpecies species, ActiveSite site)
        {
            return SiteVars.Cohorts[site].IsMaturePresent(species);
        }

    }

}
