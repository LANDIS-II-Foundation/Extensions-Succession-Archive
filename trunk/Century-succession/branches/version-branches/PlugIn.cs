//  Copyright 2007-2010 Portland State University, University of Wisconsin-Madison
//  Author: Robert Scheller, Ben Sulman, Fugui Wang

using Landis.Core;
using Landis.SpatialModeling;
using Edu.Wisc.Forest.Flel.Util;

using Landis.Library.InitialCommunities;
using Landis.Library.Succession;
using Landis.Library.LeafBiomassCohorts;
using Landis.Library.Climate;

using System;
using System.Collections.Generic;
//using System.Threading;

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
        public static int SuccessionTimeStep;
        public static double ProbEstablishAdjust;

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
            CohortBiomass.SpinupMortalityFraction = parameters.SpinupMortalityFraction;

            //  Initialize climate.
            Climate.Initialize(parameters.ClimateFile, false, modelCore);

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
            EcoregionData.Initialize(parameters);
            FireEffects.Initialize(parameters);
            InitializeSites(parameters.InitialCommunities, parameters.InitialCommunitiesMap, modelCore);
            if (parameters.CalibrateMode)
                Outputs.CreateCalibrateLogFile();

        }

        //---------------------------------------------------------------------

        public override void Run()
        {

            //if(PlugIn.ModelCore.CurrentTime == Timestep)
            //    Outputs.WriteLogFile(0);

            if(PlugIn.ModelCore.CurrentTime > 0)
                SiteVars.InitializeDisturbances();


            Dynamic.Module.CheckForUpdate();
            EcoregionData.GenerateNewClimate(PlugIn.ModelCore.CurrentTime, Timestep);

            // Update Pest only once.
            SpeciesData.EstablishProbability = Establishment.GenerateNewEstablishProbabilities(Timestep);

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
            Outputs.WriteLogFile(PlugIn.ModelCore.CurrentTime);

            if(SoilCarbonMapNames != null && (PlugIn.ModelCore.CurrentTime % SoilCarbonMapFrequency) == 0)
            {
                string path = MapNames.ReplaceTemplateVars(SoilCarbonMapNames, PlugIn.ModelCore.CurrentTime);
                using (IOutputRaster<IntPixel> outputRaster = modelCore.CreateRaster<IntPixel>(path, modelCore.Landscape.Dimensions))
                {
                    IntPixel pixel = outputRaster.BufferPixel;
                    foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                    {
                        if (site.IsActive) {
                            pixel.MapCode.Value = (int) ((SiteVars.SOM1surface[site].Carbon + SiteVars.SOM1soil[site].Carbon + SiteVars.SOM2[site].Carbon + SiteVars.SOM3[site].Carbon));
                        }
                        else {
                            //  Inactive site
                            pixel.MapCode.Value = 0;
                        }
                        outputRaster.WriteBufferPixel();
                    }
                }
            }


            if(SoilNitrogenMapNames != null && (PlugIn.ModelCore.CurrentTime % SoilNitrogenMapFrequency) == 0)
            {
                string path2 = MapNames.ReplaceTemplateVars(SoilNitrogenMapNames, PlugIn.ModelCore.CurrentTime);
                using (IOutputRaster<ShortPixel> outputRaster = modelCore.CreateRaster<ShortPixel>(path2, modelCore.Landscape.Dimensions))
                {
                    ShortPixel pixel = outputRaster.BufferPixel;
                    foreach (Site site in PlugIn.ModelCore.Landscape.AllSites) {
                        if (site.IsActive) {
                            pixel.MapCode.Value = (short) (SiteVars.MineralN[site]);
                        }
                        else {
                            //  Inactive site
                            pixel.MapCode.Value = 0;
                        }
                        outputRaster.WriteBufferPixel();
                    }
                }
            }

            if(ANPPMapNames != null && (PlugIn.ModelCore.CurrentTime % ANPPMapFrequency) == 0)
            {
                string path3 = MapNames.ReplaceTemplateVars(ANPPMapNames, PlugIn.ModelCore.CurrentTime);
                using (IOutputRaster<ShortPixel> outputRaster = modelCore.CreateRaster<ShortPixel>(path3, modelCore.Landscape.Dimensions))
                {
                    ShortPixel pixel = outputRaster.BufferPixel;
                    foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                    {
                        if (site.IsActive) {
                            pixel.MapCode.Value = (short) SiteVars.AGNPPcarbon[site];
                        }
                        else {
                            //  Inactive site
                            pixel.MapCode.Value = 0;
                        }
                        outputRaster.WriteBufferPixel();
                    }
                }
            }
            if(ANEEMapNames != null && (PlugIn.ModelCore.CurrentTime % ANEEMapFrequency) == 0)
            {

                string path4 = MapNames.ReplaceTemplateVars(ANEEMapNames, PlugIn.ModelCore.CurrentTime);
                using (IOutputRaster<ShortPixel> outputRaster = modelCore.CreateRaster<ShortPixel>(path4, modelCore.Landscape.Dimensions))
                {
                    ShortPixel pixel = outputRaster.BufferPixel;
                    foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                    {
                        if (site.IsActive) {
                            pixel.MapCode.Value = (short)(SiteVars.AnnualNEE[site]+1000);
                        }
                        else {
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
                //PlugIn.ModelCore.Log.WriteLine("Shade Calculation:  lastMort={0:0.0}, B_MAX={1}, oldB={2}, B_ACT={3}, shade={4}.", lastMortality, B_MAX,oldBiomass,B_ACT,shade);
                if (B_AM >= EcoregionData.ShadeBiomass[shade][ecoregion])
                {
                    finalShade = shade;
                    break;
                }
            }

            //PlugIn.ModelCore.Log.WriteLine("Yr={0},      Shade Calculation:  B_MAX={1}, B_ACT={2}, Shade={3}.", PlugIn.ModelCore.CurrentTime, B_MAX, B_ACT, finalShade);

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
            
            SiteVars.SurfaceDeadBranch[site] = initialBiomass.SurfaceDeadBranch.Clone();
            
           


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
            

            SiteVars.CohortBranchC[site] = initialBiomass.CohortBranchC;
            SiteVars.CohortBranchN[site] = initialBiomass.CohortBranchN;
        }


        //---------------------------------------------------------------------

        public void CohortDied(object         sender,
                               DeathEventArgs eventArgs)
        {

            //PlugIn.ModelCore.Log.WriteLine("Cohort Died! :-(");

            ExtensionType disturbanceType = eventArgs.DisturbanceType;
            ActiveSite site = eventArgs.Site;

            ICohort cohort = eventArgs.Cohort;
            double foliar = (double) cohort.LeafBiomass;
            double wood = (double) cohort.WoodBiomass;
            double branch = (double)cohort.BranchBiomass;

            //PlugIn.ModelCore.Log.WriteLine("Cohort Died: species={0}, age={1}, biomass={2}, foliage={3}.", cohort.Species.Name, cohort.Age, cohort.Biomass, foliar);

            if (disturbanceType == null) {
                //PlugIn.ModelCore.Log.WriteLine("NO EVENT: Cohort Died: species={0}, age={1}, disturbance={2}.", cohort.Species.Name, cohort.Age, eventArgs.DisturbanceType);

                ForestFloor.AddWoodLitter(wood, cohort.Species, eventArgs.Site);
                ForestFloor.AddBranchLitter(branch, cohort.Species, eventArgs.Site); 
                ForestFloor.AddFoliageLitter(foliar, cohort.Species, eventArgs.Site);
                Roots.AddCoarseRootLitter(wood, cohort.Species, eventArgs.Site);
                Roots.AddFineRootLitter(foliar, cohort.Species, eventArgs.Site);
            }

            if (disturbanceType != null) {
                //PlugIn.ModelCore.Log.WriteLine("DISTURBANCE EVENT: Cohort Died: species={0}, age={1}, disturbance={2}.", cohort.Species.Name, cohort.Age, eventArgs.DisturbanceType);

                Disturbed[site] = true;
                if (disturbanceType.IsMemberOf("disturbance:fire"))
                    Landis.Library.Succession.Reproduction.CheckForPostFireRegen(eventArgs.Cohort, site);
                else
                    Landis.Library.Succession.Reproduction.CheckForResprouting(eventArgs.Cohort, site);
            }
        }

        //---------------------------------------------------------------------

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

            //PlugIn.ModelCore.Log.WriteLine("  Calculating Sufficient Light from Succession.");
            byte siteShade = PlugIn.ModelCore.GetSiteVar<byte>("Shade")[site];

            double lightProbability = 0.0;
            bool found = false;

            foreach (ISufficientLight lights in sufficientLight)
            {

                //PlugIn.ModelCore.Log.WriteLine("Sufficient Light:  ShadeClass={0}, Prob0={1}.", lights.ShadeClass, lights.ProbabilityLight0);
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
            //wang
            SiteVars.Cohorts[site].AddNewCohort(species, 1, initialBiomass[0], initialBiomass[1], initialBiomass[2]);
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
        /// Determines if there is a mature cohort at a site.  
        /// This is a Delegate method to base succession.
        /// </summary>
        public bool MaturePresent(ISpecies species, ActiveSite site)
        {
            return SiteVars.Cohorts[site].IsMaturePresent(species);
        }

    }

}
