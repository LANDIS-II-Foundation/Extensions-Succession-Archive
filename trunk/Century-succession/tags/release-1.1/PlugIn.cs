//  Copyright 2007-2010 Portland State University, University of Wisconsin-Madison
//  Author: Robert Scheller, Ben Sulman

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
            InputParametersParser parser = new InputParametersParser();
            parameters = modelCore.Load<IInputParameters>(dataFile, parser);

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

        public override void Initialize(string dataFile)
        {
            Timestep              = parameters.Timestep;
            sufficientLight       = parameters.LightClassProbabilities;
            CohortBiomass.SpinupMortalityFraction = parameters.SpinupMortalityFraction;
            
            SiteVars.Initialize();

            //  Initialize climate.  A list of ecoregion indices is passed so that
            //  the climate library can operate independently of the LANDIS-II core.
            List<int> ecoregionIndices = new List<int>();
            foreach(IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions)
            {
                ecoregionIndices.Add(ecoregion.Index);
                PlugIn.ModelCore.Log.WriteLine("    Century:  preparing climate data:  {0} = ecoregion index {1}", ecoregion.Name, ecoregion.Index);
            }
            Climate.Initialize(parameters.ClimateFile, ecoregionIndices, false, modelCore);

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
            
            //cohorts = PlugIn.ModelCore.Landscape.NewSiteVar<SiteCohorts>();
            //landscapeCohorts = new LandscapeCohorts(SiteVars.SiteCohorts); //cohorts);
            //Cohorts = landscapeCohorts;
            
            Reproduction.SufficientResources = SufficientLight; 

            InitialBiomass.Initialize(Timestep);
            
            base.Initialize(modelCore,
                            //Util.ToArray<double>(SpeciesData.EstablishProbability),
                            parameters.SeedAlgorithm,
                            (Reproduction.Delegates.AddNewCohort) AddNewCohort);

            Cohort.DeathEvent += CohortDied;
            AgeOnlyDisturbances.Module.Initialize(parameters.AgeOnlyDisturbanceParms);

            Dynamic.Module.Initialize(parameters.DynamicUpdates);
            EcoregionData.Initialize(parameters);
            FireEffects.Initialize(parameters);
            

        }

        //---------------------------------------------------------------------

        public override void Run()
        {
            if(PlugIn.ModelCore.CurrentTime == Timestep)
                Outputs.WriteLogFile(0);                

            if(PlugIn.ModelCore.CurrentTime > 0)
                SiteVars.InitializeDisturbances();

            
            Dynamic.Module.CheckForUpdate();
            EcoregionData.GenerateNewClimate(PlugIn.ModelCore.CurrentTime, Timestep);
            
            // Update Pest only once.  
            SpeciesData.EstablishProbability = Establishment.GenerateNewEstablishProbabilities(Timestep);  
            //Reproduction.ChangeEstablishProbabilities(Util.ToArray<double>(SpeciesData.EstablishProbability));


            //base.RunReproductionFirst();
            
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
                using (IOutputRaster<UShortPixel> outputRaster = modelCore.CreateRaster<UShortPixel>(path, modelCore.Landscape.Dimensions))
                {
                    UShortPixel pixel = outputRaster.BufferPixel;
                    foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                    {
                        if (site.IsActive) {
                            pixel.MapCode.Value = (ushort) ((SiteVars.SOM1surface[site].Carbon + SiteVars.SOM1soil[site].Carbon + SiteVars.SOM2[site].Carbon + SiteVars.SOM3[site].Carbon) / 100.0);
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
                using (IOutputRaster<UShortPixel> outputRaster = modelCore.CreateRaster<UShortPixel>(path2, modelCore.Landscape.Dimensions))
                {
                    UShortPixel pixel = outputRaster.BufferPixel;
                    foreach (Site site in PlugIn.ModelCore.Landscape.AllSites) {
                        if (site.IsActive) {
                            pixel.MapCode.Value = (ushort) (SiteVars.MineralN[site]);
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
                using (IOutputRaster<UShortPixel> outputRaster = modelCore.CreateRaster<UShortPixel>(path3, modelCore.Landscape.Dimensions))
                {
                    UShortPixel pixel = outputRaster.BufferPixel;
                    foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                    {
                        if (site.IsActive) {
                            pixel.MapCode.Value = (ushort) SiteVars.AGNPPcarbon[site];
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
                using (IOutputRaster<UShortPixel> outputRaster = modelCore.CreateRaster<UShortPixel>(path4, modelCore.Landscape.Dimensions))
                {
                    UShortPixel pixel = outputRaster.BufferPixel;
                    foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                    {
                        //PlugIn.ModelCore.Log.WriteLine("  ANEE = {0:0.00}, Transformed = {1:0}.", SiteVars.AnnualNEE[site], (ushort) ((SiteVars.AnnualNEE[site] * -0.1) + 1000));
                        if (site.IsActive) {
                            pixel.MapCode.Value = (ushort)(SiteVars.AnnualNEE[site] + 1000);
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
            
            //PlugIn.ModelCore.Log.WriteLine("  Calculating Sufficient Light from Succession.");
            byte siteShade = PlugIn.ModelCore.GetSiteVar<byte>("Shade")[site];
            
            double lightProbability = 0.0;
            bool found = false;
            
            foreach(ISufficientLight lights in sufficientLight)
            {
            
                //PlugIn.ModelCore.Log.WriteLine("Sufficient Light:  ShadeClass={0}, Prob0={1}.", lights.ShadeClass, lights.ProbabilityLight0);
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
                PlugIn.ModelCore.Log.WriteLine("A Sufficient Light value was not found for {0}.", species.Name);
                
            // This is neccesary to account for Nitrogen limitation:
            // Update species establishment probabilities
            double NlimitedEstablishment = SpeciesData.NLimits[species];
            
            //if(NlimitedEstablishment < lightProbability)
            //    PlugIn.ModelCore.Log.WriteLine("Establishment limited by NITROGEN.  Spp={0}, Nlimit={1:0.00}, Llimit={2:0.00}.", species.Name, NlimitedEstablishment, lightProbability);
            
            lightProbability = Math.Min(lightProbability, NlimitedEstablishment);

            
            return modelCore.GenerateUniform() < lightProbability;
            
        }

        //---------------------------------------------------------------------

        public override byte ComputeShade(ActiveSite site)
        {
            //return LivingBiomass.ComputeShade(site);
            IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];
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
                    return shade;
                }
            }
            
            //PlugIn.ModelCore.Log.WriteLine("Shade Calculation:  lastMort={0:0.0}, B_MAX={1}, oldB={2}, B_ACT={3}, shade=0.", lastMortality, B_MAX,oldBiomass,B_ACT);
            
            return 0;
        }
        //---------------------------------------------------------------------

        protected override void InitializeSite(ActiveSite site,
                                               ICommunity initialCommunity)
        {
            //SpeciesData.CalculateNGrowthLimits(site);

            InitialBiomass initialBiomass = InitialBiomass.Compute(site, initialCommunity);
            SiteVars.Cohorts[site] = initialBiomass.Cohorts.Clone();
            
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

            //PlugIn.ModelCore.Log.WriteLine("Cohort Died! :-(");

            ExtensionType disturbanceType = eventArgs.DisturbanceType;
            ActiveSite site = eventArgs.Site;
            
            ICohort cohort = eventArgs.Cohort;
            double foliar = (double) cohort.LeafBiomass; 
            
            double wood = (double) cohort.WoodBiomass; 

            //PlugIn.ModelCore.Log.WriteLine("Cohort Died: species={0}, age={1}, biomass={2}, foliage={3}.", cohort.Species.Name, cohort.Age, cohort.Biomass, foliar);
            
            if (disturbanceType == null) {
                //PlugIn.ModelCore.Log.WriteLine("NO EVENT: Cohort Died: species={0}, age={1}, disturbance={2}.", cohort.Species.Name, cohort.Age, eventArgs.DisturbanceType);

                ForestFloor.AddWoodLitter(wood, cohort.Species, eventArgs.Site);
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

        public void AddNewCohort(ISpecies   species,
                                 ActiveSite site)
        {
            float[] initialBiomass = CohortBiomass.InitialBiomass(SiteVars.Cohorts[site], site, species);
            SiteVars.Cohorts[site].AddNewCohort(species, initialBiomass[0], initialBiomass[1]);
        }
        //---------------------------------------------------------------------

        protected override void AgeCohorts(ActiveSite site,
                                           ushort     years,
                                           int?       successionTimestep)
        {
            Century.Run(SiteVars.Cohorts[site], site.Location, years, successionTimestep.HasValue);
            
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// Determines if a species can establish on a site.
        /// </summary>
        public bool Establish(ISpecies species, ActiveSite site)
        {
            double establishProbability = SpeciesData.EstablishProbability[species][ModelCore.Ecoregion[site]];

            return modelCore.GenerateUniform() < establishProbability;
        }

        //---------------------------------------------------------------------
       

    }
    
}
