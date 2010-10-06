//  Copyright 2007-2010 University of Nevada, Portland State University
//  Authors:  Sarah Ganschow, Robert M. Scheller
//  License:  Available at
//  http://www.landis-ii.org/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Util;

using Landis.Landscape;
using Landis.Species;
using Landis.InitialCommunities;
using Landis.Succession;
using Landis.PlugIns;
using Landis.RasterIO;

using System.Collections.Generic;


namespace Landis.Biomass.NuCycling.Succession
{
    public class PlugIn
        : Landis.Succession.PlugIn
    {
        private List<ISufficientLight> sufficientLight;
        public static string TotalCarbonMapNames = "output/total-carbon-{timestep}.gis";

        public PlugIn()
            : base("Nutrient Cycling Succession")
        {
        }

        //---------------------------------------------------------------------

        public override void Initialize(string dataFile,
                                PlugIns.ICore modelCore)
        {
            Model.Core = modelCore;
            InputParametersParser parser = new InputParametersParser(Model.Core.Ecoregions,
                                                           Model.Core.Species,
                                                           Model.Core.StartTime,
                                                           Model.Core.EndTime);
            IInputParameters parameters = Landis.Data.Load<IInputParameters>(dataFile, parser);

            Timestep = parameters.Timestep;
            sufficientLight = parameters.LightClassProbabilities;

            SiteVars.Initialize();
            FireEffects.Initialize(parameters);

            SpeciesData.Initialize(parameters);
            EcoregionData.Initialize(parameters);

            SoilOrganicMatter.Initialize(parameters);

            MineralSoil.Initialize(parameters);
            Rock.Initialize(parameters);
            Outputs.Initialize(parameters);

            //  Cohorts must be created before the base class is initialized
            //  because the base class' reproduction module uses the core's
            //  SuccessionCohorts property in its Initialization method.
            CohortBiomass.Calculator = new CohortBiomass();
            Biomass.Cohorts.Initialize(Timestep, CohortBiomass.Calculator);

            Cohorts = new LandscapeCohorts(SiteVars.Cohorts);
            Reproduction.SufficientLight = SufficientLight;

            InitialBiomass.Initialize(Timestep);

            base.Initialize(modelCore,
                            SpeciesData.ToArray<double>(parameters.EstablishProbability),
                            parameters.SeedAlgorithm,
                            (Reproduction.Delegates.AddNewCohort)AddNewCohort);

            Cohort.DeathEvent += CohortDied;

            AgeOnlyDisturbances.Module.Initialize(parameters.AgeOnlyDisturbanceParms);
            DynamicChange.Module.Initialize(parameters.DynamicChangeUpdates);
        }

        //---------------------------------------------------------------------

        public override void Run()
        {
            DynamicChange.Module.CheckForUpdate();

            base.Run();

            Outputs.WriteLogFile(Model.Core.CurrentTime);

            if ((Model.Core.CurrentTime % Timestep) == 0)
            {
                string path = MapNames.ReplaceTemplateVars(TotalCarbonMapNames, Model.Core.CurrentTime);
                IOutputRaster<UShortPixel> map = Model.Core.CreateRaster<UShortPixel>(path, Model.Core.Landscape.Dimensions, Model.Core.LandscapeMapMetadata);
                using (map)
                {
                    UShortPixel pixel = new UShortPixel();
                    foreach (Site site in Model.Core.Landscape.AllSites)
                    {
                        if (site.IsActive)
                        {
                            pixel.Band0 = (ushort) (SiteVars.ComputeTotalC((ActiveSite) site, (int) SiteVars.ComputeTotalBiomass((ActiveSite) site)) / 1000.0);
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

        }
        //---------------------------------------------------------------------

        public void CohortDied(object sender,
                               DeathEventArgs eventArgs)
        {
            PlugInType disturbanceType = eventArgs.DisturbanceType;
            ActiveSite site = eventArgs.Site;

            ICohort cohort = eventArgs.Cohort;
            double foliar = (double) cohort.LeafBiomass;

            double wood = (double) cohort.WoodBiomass;

            if (disturbanceType == null) {
                //UI.WriteLine("NO EVENT: Cohort Died: species={0}, age={1}, disturbance={2}.", cohort.Species.Name, cohort.Age, eventArgs.DisturbanceType);

                ForestFloor.AddWoodyDebris(wood, cohort.Species, SiteVars.WoodyDebris[site]);
                ForestFloor.AddDisturbanceLitter(foliar, cohort.Species, site, SiteVars.Litter[site]);

                double killedFineRoots = Roots.CalculateFineRoot(foliar, SpeciesData.LeafLongevity[cohort.Species]);
                Roots.KillFineRoots(killedFineRoots, cohort.Species, SiteVars.DeadFineRootsAdd[site]);
                Roots.ReduceFineRoots(killedFineRoots, cohort.Species, SiteVars.FineRoots[site]);
                double killedCoarseRoots = Roots.CalculateCoarseRoot(wood, SpeciesData.LeafLongevity[cohort.Species]);
                ForestFloor.AddWoodyDebris(killedCoarseRoots, cohort.Species, SiteVars.WoodyDebris[site]);
                Roots.ReduceCoarseRoots(killedCoarseRoots, cohort.Species, SiteVars.CoarseRoots[site]);
            }
            if (disturbanceType != null)
            {
                //ActiveSite site = eventArgs.Site;
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
            float[] initialBiomass = CohortBiomass.InitialBiomass(SiteVars.Cohorts[site], site, species);
            float initialWoodMass = initialBiomass[0];
            float initialLeafMass = initialBiomass[1];

            SiteVars.Cohorts[site].AddNewCohort(species, initialWoodMass, initialLeafMass);
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Determines if there is sufficient light at a site for a species to
        /// germinate/resprout.
        /// </summary>
        public bool SufficientLight(ISpecies species,
                                           ActiveSite site)
        {
            //UI.WriteLine("  Calculating Sufficient Light from Succession.");
            byte siteShade = Model.Core.GetSiteVar<byte>("Shade")[site];

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
                UI.WriteLine("A Sufficient Light value was not found for {0}.", species.Name);

            return Landis.Util.Random.GenerateUniform() < lightProbability;
        }

        //---------------------------------------------------------------------

        protected override void InitializeSite(ActiveSite site,
                                               ICommunity initialCommunity)
        {
            InitialBiomass initialBiomass = InitialBiomass.Compute(site, initialCommunity);

            //Clone initial cohorts
            SiteVars.Cohorts[site] = initialBiomass.InitialCohorts.Clone();

            //Clone initial coarse roots to all sites
            SiteVars.CoarseRoots[site].Mass = initialBiomass.CoarseRootsPool.Mass;
            SiteVars.CoarseRoots[site].ContentC = initialBiomass.CoarseRootsPool.ContentC;
            SiteVars.CoarseRoots[site].ContentN = initialBiomass.CoarseRootsPool.ContentN;
            SiteVars.CoarseRoots[site].ContentP = initialBiomass.CoarseRootsPool.ContentP;

            //Clone initial fine roots to all sites
            SiteVars.FineRoots[site].Mass = initialBiomass.FineRootsPool.Mass;
            SiteVars.FineRoots[site].ContentC = initialBiomass.FineRootsPool.ContentC;
            SiteVars.FineRoots[site].ContentN = initialBiomass.FineRootsPool.ContentN;
            SiteVars.FineRoots[site].ContentP = initialBiomass.FineRootsPool.ContentP;

            //Clone initial woody debris to all sites
            SiteVars.WoodyDebris[site].Mass = initialBiomass.WoodyDebrisPool.Mass;
            SiteVars.WoodyDebris[site].ContentC = initialBiomass.WoodyDebrisPool.ContentC;
            SiteVars.WoodyDebris[site].ContentN = initialBiomass.WoodyDebrisPool.ContentN;
            SiteVars.WoodyDebris[site].ContentP = initialBiomass.WoodyDebrisPool.ContentP;
            SiteVars.WoodyDebris[site].DecayValue = initialBiomass.WoodyDebrisPool.DecayValue;

            //Clone initial litter to all sites
            if (SiteVars.Litter[site].Count == 0)
            {
                foreach (PoolD litter in initialBiomass.LitterPool)
                {
                    PoolD litterAdd = new PoolD();
                    litterAdd.Mass = litter.Mass;
                    litterAdd.ContentC = litter.ContentC;
                    litterAdd.ContentN = litter.ContentN;
                    litterAdd.ContentP = litter.ContentP;
                    litterAdd.DecayValue = litter.DecayValue;
                    litterAdd.InitialMass = litter.InitialMass;
                    litterAdd.LimitValue = litter.LimitValue;
                    SiteVars.Litter[site].Add(litterAdd);
                }
            }

            //Clone initial dead fine roots to all sites
            if (SiteVars.DeadFineRoots[site].Count == 0)
            {
                foreach (PoolD litter in initialBiomass.DeadFRootsPool)
                {
                    PoolD litterAdd = new PoolD();
                    litterAdd.Mass = litter.Mass;
                    litterAdd.ContentC = litter.ContentC;
                    litterAdd.ContentN = litter.ContentN;
                    litterAdd.ContentP = litter.ContentP;
                    litterAdd.DecayValue = litter.DecayValue;
                    litterAdd.InitialMass = litter.InitialMass;
                    litterAdd.LimitValue = litter.LimitValue;
                    SiteVars.DeadFineRoots[site].Add(litterAdd);
                }
            }

            //Clone initial charcoal to all sites
            SiteVars.Charcoal[site].ContentC = initialBiomass.CharcoalPool.ContentC;
            SiteVars.Charcoal[site].ContentN = initialBiomass.CharcoalPool.ContentN;
            SiteVars.Charcoal[site].ContentP = initialBiomass.CharcoalPool.ContentP;
        }


        //---------------------------------------------------------------------

        protected override void AgeCohorts(ActiveSite site,
                                           ushort years,
                                           int? successionTimestep)
        {
            NutrientSuccession.GrowCohorts(SiteVars.Cohorts[site], site, years, successionTimestep.HasValue);
            SiteVars.TotalWoodBiomass[site] = SiteVars.ComputeWoodBiomass((ActiveSite)site);

        }

        //---------------------------------------------------------------------

        public override byte ComputeShade(ActiveSite site)
        {
            return CohortBiomass.ComputeShade(site);
        }
    }
}
