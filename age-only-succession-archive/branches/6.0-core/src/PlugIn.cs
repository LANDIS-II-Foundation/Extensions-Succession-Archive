//  Copyright 2005,2007 University of Wisconsin-Madison
//  Authors:  James Domingo, UW-Madison, FLEL
//  License:  Available at
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Landis.Library.BaseCohorts;
using Landis.Ecoregions;
using Landis.Species;
using Landis.Library.InitialCommunities;
using Landis.PlugIns;
using System.Collections.Generic;
using Wisc.Flel.GeospatialModeling.Landscapes;
using age_only_successsion;

namespace Landis.Extension.Succession.AgeOnly
{
    public class PlugIn
        : Landis.Library.Succession.ExtensionBase
    {
        private IInputParameters parameters;

        //private static double[,] establishProbabilities;

        public static Species.AuxParm<Ecoregions.AuxParm<double>> establishProbabilities;
        //public static double[,] establishProbabilities;


        //---------------------------------------------------------------------

        public PlugIn()
            : base("Age-only Succession")
        {
            //establishProbabilities = new double[Model.Core.Ecoregions.Count, Model.Core.Species.Count];
        }

        //---------------------------------------------------------------------

        public override void LoadParameters(string        dataFile,
                                            PlugIns.ICore modelCore)
        {
            Model.Core = modelCore;
            InputParametersParser parser = new InputParametersParser();
            parameters = modelCore.Load<IInputParameters>(dataFile, parser);

        }
        //---------------------------------------------------------------------

        public override void Initialize(string        dataFile,
                                        PlugIns.ICore modelCore)
        {
            Timestep = parameters.Timestep;

            SiteVars.Initialize();

            Cohort.DeathEvent += CohortDied; 

            establishProbabilities = parameters.EstablishProbabilities;

            base.Initialize(modelCore,
                            //parameters.EstablishProbabilities,
                            parameters.SeedAlgorithm,
                            AddNewCohort);
            
            InitializeSites(parameters.InitialCommunities, parameters.InitialCommunitiesMap, modelCore);
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
            /*if (SiteVars.Cohorts[site].HasAge())
            {
                //UI.WriteLine("System Error");
                throw new System.InvalidOperationException("Incompatible extensions given in the scenario file:  Age data required for this extension to operate.");
            }*/
        }

        //---------------------------------------------------------------------

        protected override void AgeCohorts(ActiveSite site,
                                           ushort     years,
                                           int?       successionTimestep)
        {
            SiteVars.Cohorts[site].Grow(years, site, successionTimestep);
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

        /*
        /// <summary>
        /// Changes the table of establishment probabilities because of a
        /// change in climate.
        /// </summary>

        public static void ChangeEstablishProbabilities(double[,] estbProbabilities)
        {
            establishProbabilities = estbProbabilities;
        }

        //---------------------------------------------------------------------

        
        /// <summary>
        /// Gets the establishment probablity for a particular species at a
        /// site.
        /// </summary>
        
        public static double GetEstablishProbability(ISpecies   species,
                                                     ActiveSite site)
        {
            return establishProbabilities[Model.Core.Ecoregion[site].Index, species.Index];
        }
       
        
        //---------------------------------------------------------------------
        
        public static double setEstablishProbability(IEcoregion ecoregion,
                                                     ISpecies species,
                                                     double probability)
        {
            return establishProbabilities[ecoregion.Index, species.Index] = probability;
        }
        */ 
        
        //---------------------------------------------------------------------

        /// <summary>
        /// Determines if a species can establish on a site.
        /// </summary>
        public bool Establish(ISpecies species, ActiveSite site)
        {
            // double establishProbability = establishProbabilities[Model.Core.Ecoregion[site].Index, species.Index];

            Landis.Model.iui.WriteLine("ALOHAAAAAAAAAAAAAAA");

            double establishProbability = establishProbabilities[species][Model.Core.Ecoregion[site]];

            return Landis.Model.GenerateUniform() < establishProbability;
        }

        //---------------------------------------------------------------------
    }
}
