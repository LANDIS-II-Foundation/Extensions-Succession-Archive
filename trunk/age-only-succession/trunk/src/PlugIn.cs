//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.Library.AgeOnlyCohorts;
using Landis.Core;
using Landis.Library.InitialCommunities;
using System.Collections.Generic;
// using Wisc.Flel.GeospatialModeling.Landscapes;
using Landis.SpatialModeling;
using Landis.SpatialModeling.CoreServices;

namespace Landis.Extension.Succession.AgeOnly
{
    public class PlugIn
        : Landis.Library.Succession.ExtensionBase
    {
        public static readonly string ExtensionName = "Age-only Succession";

        private IInputParameters parameters;

        public static Species.AuxParm<Ecoregions.AuxParm<double>> establishProbabilities;
        
        private static ICore modelCore;


        //---------------------------------------------------------------------

        public PlugIn()
            : base(ExtensionName)
        {
        }

        //---------------------------------------------------------------------

        public override void LoadParameters(string        dataFile,
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

            Timestep = parameters.Timestep;

            SiteVars.Initialize();

            Cohort.DeathEvent += CohortDied; 

            establishProbabilities = parameters.EstablishProbabilities;

            base.Initialize(modelCore,
                            parameters.SeedAlgorithm,
                            AddNewCohort);
            
            InitializeSites(parameters.InitialCommunities, parameters.InitialCommunitiesMap, modelCore);
        }

        //---------------------------------------------------------------------

        public void CohortDied(object         sender,
                               DeathEventArgs eventArgs)
        {
            ExtensionType disturbanceType = eventArgs.DisturbanceType;
            if (disturbanceType != null) {
                ActiveSite site = eventArgs.Site;
                Disturbed[site] = true;
                if (disturbanceType.IsMemberOf("disturbance:fire"))
                    Landis.Library.Succession.Reproduction.CheckForPostFireRegen(eventArgs.Cohort, site);//EST
                else
                    Landis.Library.Succession.Reproduction.CheckForResprouting(eventArgs.Cohort, site);//EST
            }
        }

        //---------------------------------------------------------------------

        public void AddNewCohort(ISpecies   species,
                                 ActiveSite site)
        {
            SiteVars.Cohorts[site].AddNewCohort(species);
        }

        //---------------------------------------------------------------------

        protected override void InitializeSite(ActiveSite site,
                                               ICommunity initialCommunity)
        {
            SiteVars.Cohorts[site] = new SiteCohorts(initialCommunity.Cohorts);
        }

        //---------------------------------------------------------------------

        protected override void AgeCohorts(ActiveSite site,
                                           ushort     years,
                                           int?       successionTimestep)
        {
            SiteVars.Cohorts[site].Grow(years, site, successionTimestep, modelCore);
            //string msg = "";
            //foreach (ISpeciesCohorts sppcohort in SiteVars.Cohorts[site])
            //    msg += sppcohort.Species.Name + " ";

            //modelCore.Log.WriteLine("   Ageing cohorts:  {0}.", msg);
        }

        //---------------------------------------------------------------------

        public override byte ComputeShade(ActiveSite site)
        {
            byte shade = 0;
            foreach (SpeciesCohorts speciesCohorts in SiteVars.Cohorts[site]) {
                ISpecies species = speciesCohorts.Species;
                if (species.ShadeTolerance > shade)
                    shade = species.ShadeTolerance;
            }
            return shade;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Determines if a species can establish on a site.
        /// </summary>
        public bool Establish(ISpecies species, ActiveSite site)
        {
            double establishProbability = establishProbabilities[species][modelCore.Ecoregion[site]];

            return modelCore.GenerateUniform() < establishProbability;
        }

        //---------------------------------------------------------------------
    }
}
