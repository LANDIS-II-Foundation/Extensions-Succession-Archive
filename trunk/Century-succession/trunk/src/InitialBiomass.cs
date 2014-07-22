//  Copyright 2007-2010 Portland State University, University of Wisconsin-Madison
//  Author: Robert Scheller, Ben Sulman

using Landis.Core;
using Landis.SpatialModeling;
using Edu.Wisc.Forest.Flel.Util;
using Landis.Library.InitialCommunities;
using System.Collections.Generic;
using Landis.Library.LeafBiomassCohorts;
using Landis.Library.Climate;
using System;
//using Landis.Cohorts;


namespace Landis.Extension.Succession.Century
{
    /// <summary>
    /// The initial live and dead biomass at a site.
    /// </summary>
    public class InitialBiomass
    {
        private ISiteCohorts cohorts;
        
        private Layer surfaceDeadWood;
        private Layer surfaceStructural;
        private Layer surfaceMetabolic;
        
        private Layer soilDeadWood;
        private Layer soilStructural;
        private Layer soilMetabolic;
        
        private Layer som1surface;
        private Layer som1soil;
        private Layer som2;
        private Layer som3;
        
        private double mineralN;
        private double cohortLeafC;
        private double cohortFRootC;
        private double cohortLeafN;
        private double cohortFRootN;
        private double cohortWoodC;
        private double cohortCRootC;
        private double cohortWoodN;
        private double cohortCRootN;
        
        //---------------------------------------------------------------------

        /// <summary>
        /// The site's initial cohorts.
        /// </summary>
        public ISiteCohorts Cohorts
        {
            get {
                return cohorts;
            }
        }
        
        public Layer SurfaceDeadWood 
        { get { return surfaceDeadWood; } 
        } 
        public Layer SoilDeadWood 
        { get { return soilDeadWood; } 
        } 
        public Layer SurfaceStructural 
        { get { return surfaceStructural ; } 
        } 
        public Layer SoilStructural 
        { get { return soilStructural ; } 
        } 
        public Layer SurfaceMetabolic 
        { get { return surfaceMetabolic ; } 
        } 
        public Layer SoilMetabolic 
        { get { return soilMetabolic ; } 
        } 
        public Layer SOM1surface 
        { get { return som1surface ; } 
        } 
        public Layer SOM1soil 
        { get { return som1soil ; } 
        } 
        public Layer SOM2 
        { get { return som2; } 
        } 
        public Layer SOM3 
        { get { return som3; } 
        } 
        
        public double MineralN { get { return mineralN; } } 
        public double CohortLeafC { get { return cohortLeafC; } }
        public double CohortFRootC { get { return cohortFRootC; } } 
        public double CohortLeafN { get { return cohortLeafN; } }
        public double CohortFRootN { get { return cohortFRootN; } } 
        public double CohortWoodC { get { return cohortWoodC; } }
        public double CohortCRootC { get { return cohortCRootC; } } 
        public double CohortWoodN { get { return cohortWoodN; } }
        public double CohortCRootN { get { return cohortCRootN; } }
        
        //---------------------------------------------------------------------

        private InitialBiomass(ISiteCohorts cohorts,
                
                Layer surfaceDeadWood,
                Layer surfaceStructural,
                Layer surfaceMetabolic,
                
                Layer soilDeadWood,
                Layer soilStructural,
                Layer soilMetabolic,
                
                Layer som1surface,
                Layer som1soil,
                Layer som2,
                Layer som3,
                
                double mineralN,
                double cohortLeafC,
                double cohortFRootC,
                double cohortLeafN,
                double cohortFRootN,
                double cohortWoodC,
                double cohortCRootC,
                double cohortWoodN,
                double cohortCRootN

                )
        {
            this.cohorts = cohorts;
            
            this.surfaceDeadWood = surfaceDeadWood;
            this.surfaceStructural = surfaceStructural;
            this.surfaceMetabolic = surfaceMetabolic;
            
            this.soilDeadWood = soilDeadWood;
            this.soilStructural = soilStructural;
            this.soilMetabolic = soilMetabolic;
            
            this.som1surface = som1surface;
            this.som1soil = som1soil;
            this.som2 = som2;
            this.som3 = som3;
            
            this.mineralN = mineralN;
            this.cohortLeafC = cohortLeafC;
            this.cohortFRootC = cohortFRootC;
            this.cohortLeafN = cohortLeafN;
            this.cohortFRootN = cohortFRootN;
            this.cohortWoodC = cohortWoodC;
            this.cohortCRootC = cohortCRootC;
            this.cohortWoodN = cohortWoodN;
            this.cohortCRootN = cohortCRootN;
        }

        //---------------------------------------------------------------------
        public static ISiteCohorts Clone(ISiteCohorts site_cohorts)
        {
            ISiteCohorts clone = new SiteCohorts();
            foreach (ISpeciesCohorts speciesCohorts in site_cohorts)
                foreach (ICohort cohort in speciesCohorts)
                    clone.AddNewCohort(cohort.Species, cohort.Age, cohort.WoodBiomass, cohort.LeafBiomass);  
            return clone;
        }
        //---------------------------------------------------------------------

        private static IDictionary<uint, InitialBiomass> initialSites;
            //  Initial site biomass for each unique pair of initial
            //  community and ecoregion; Key = 32-bit unsigned integer where
            //  high 16-bits is the map code of the initial community and the
            //  low 16-bits is the ecoregion's map code

        private static IDictionary<uint, List<Landis.Library.AgeOnlyCohorts.ICohort>> sortedCohorts;
            //  Age cohorts for an initial community sorted from oldest to
            //  youngest.  Key = initial community's map code

        private static ushort successionTimestep;

        //---------------------------------------------------------------------

        private static uint ComputeKey(uint initCommunityMapCode,
                                       uint ecoregionMapCode)
        {
            return (uint) ((initCommunityMapCode << 16) | ecoregionMapCode);
        }

        //---------------------------------------------------------------------

        static InitialBiomass()
        {
            initialSites = new Dictionary<uint, InitialBiomass>();
            sortedCohorts = new Dictionary<uint, List<Landis.Library.AgeOnlyCohorts.ICohort>>();
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Initializes this class.
        /// </summary>
        /// <param name="timestep">
        /// The plug-in's timestep.  It is used for growing biomass cohorts.
        /// </param>
        public static void Initialize(int timestep)
        {
            successionTimestep = (ushort) timestep;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Computes the initial biomass at a site.
        /// </summary>
        /// <param name="site">
        /// The selected site.
        /// </param>
        /// <param name="initialCommunity">
        /// The initial community of age cohorts at the site.
        /// </param>
        public static InitialBiomass Compute(ActiveSite site,
                                             ICommunity initialCommunity)
        {
            IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];

            if (!ecoregion.Active)
            {
                string mesg = string.Format("Initial community {0} is located on a non-active ecoregion {1}", initialCommunity.MapCode, ecoregion.Name);
                throw new System.ApplicationException(mesg);
            }

            uint key = ComputeKey(initialCommunity.MapCode, ecoregion.MapCode);
            InitialBiomass initialBiomass;
            if (initialSites.TryGetValue(key, out initialBiomass))
                return initialBiomass;

            //  If we don't have a sorted list of age cohorts for the initial
            //  community, make the list
            List<Landis.Library.AgeOnlyCohorts.ICohort> sortedAgeCohorts;
            if (! sortedCohorts.TryGetValue(initialCommunity.MapCode, out sortedAgeCohorts)) {
                sortedAgeCohorts = SortCohorts(initialCommunity.Cohorts);
                sortedCohorts[initialCommunity.MapCode] = sortedAgeCohorts;
            }
            
            ISiteCohorts cohorts = MakeBiomassCohorts(sortedAgeCohorts, site);
            initialBiomass = new InitialBiomass(
                        cohorts,
                        
                        SiteVars.SurfaceDeadWood[site],
                        SiteVars.SurfaceStructural[site],
                        SiteVars.SurfaceMetabolic[site],
                        
                        SiteVars.SoilDeadWood[site],
                        SiteVars.SoilStructural[site],
                        SiteVars.SoilMetabolic[site],
                        
                        SiteVars.SOM1surface[site],
                        SiteVars.SOM1soil[site],
                        SiteVars.SOM2[site],
                        SiteVars.SOM3[site],
                        
                        SiteVars.MineralN[site],
                        SiteVars.CohortLeafC[site],
                        SiteVars.CohortFRootC[site],
                        SiteVars.CohortLeafN[site],
                        SiteVars.CohortFRootN[site],
                        SiteVars.CohortWoodC[site],
                         SiteVars.CohortCRootC[site],
                        SiteVars.CohortWoodN[site],
                        SiteVars.CohortCRootN[site]
                        );

            initialSites[key] = initialBiomass;
            return initialBiomass;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Makes a list of age cohorts in an initial community sorted from
        /// oldest to youngest.
        /// </summary>
        public static List<Landis.Library.AgeOnlyCohorts.ICohort> SortCohorts(List<Landis.Library.AgeOnlyCohorts.ISpeciesCohorts> sppCohorts)
        {
            List<Landis.Library.AgeOnlyCohorts.ICohort> cohorts = new List<Landis.Library.AgeOnlyCohorts.ICohort>();
            foreach (Landis.Library.AgeOnlyCohorts.ISpeciesCohorts speciesCohorts in sppCohorts)
            {
                foreach (Landis.Library.AgeOnlyCohorts.ICohort cohort in speciesCohorts)
                {
                    cohorts.Add(cohort);
                    //PlugIn.ModelCore.UI.WriteLine("ADDED:  {0} {1}.", cohort.Species.Name, cohort.Age);
                }
            }
            cohorts.Sort(Landis.Library.AgeOnlyCohorts.Util.WhichIsOlderCohort);
            return cohorts;
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// A method that computes the initial biomass for a new cohort at a
        /// site based on the existing cohorts.
        /// </summary>
        public delegate float[] ComputeMethod(ISpecies species,
                                             ISiteCohorts siteCohorts,
                                             ActiveSite site);

        //---------------------------------------------------------------------

        /// <summary>
        /// Grows the cohorts during spin-up
        /// Makes the set of biomass cohorts at a site based on the age cohorts
        /// at the site, using a specified method for computing a cohort's
        /// initial biomass.
        /// </summary>
        /// <param name="ageCohorts">
        /// A sorted list of age cohorts, from oldest to youngest.
        /// </param>
        /// <param name="site">
        /// Site where cohorts are located.
        /// </param>
        /// <param name="initialBiomassMethod">
        /// The method for computing the initial biomass for a new cohort.
        /// </param>
        public static ISiteCohorts MakeBiomassCohorts(List<Landis.Library.AgeOnlyCohorts.ICohort> ageCohorts,   
                                                     ActiveSite site,
                                                     ComputeMethod initialBiomassMethod)
        {
            IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];
            //if (Climate.Spinup_AllData.Count >= ageCohorts[0].Age)
            //try
            //{    
                //PlugIn.ModelCore.UI.WriteLine("Making Biomass Cohorts using spin-up climate data");

                SiteVars.Cohorts[site] = new Library.LeafBiomassCohorts.SiteCohorts(); 
                if (ageCohorts.Count == 0)
                  return SiteVars.Cohorts[site];

                int indexNextAgeCohort = 0;
                //  The index in the list of sorted age cohorts of the next
                //  cohort to be considered

                //  Loop through time from -N to 0 where N is the oldest cohort.
                //  So we're going from the time when the oldest cohort was "born"
                //  to the present time (= 0).  Because the age of any age cohort
                //  is a multiple of the succession timestep, we go from -N to 0
                //  by that timestep.  NOTE: the case where timestep = 1 requires
                //  special treatment because if we start at time = -N with a
                //  cohort with age = 1, then at time = 0, its age will N+1 not N.
                //  Therefore, when timestep = 1, the ending time is -1.
                
                //int endTime = (successionTimestep == 1) ? -1 : -1;
                //PlugIn.ModelCore.UI.WriteLine("  Ageing initial cohorts.  Oldest cohorts={0} yrs, succession timestep={1}, endTime={2}.", ageCohorts[0].Age, successionTimestep, endTime);
                for (int time = -(ageCohorts[0].Age); time <= -1; time += successionTimestep)
                {
                     //PlugIn.ModelCore.UI.WriteLine("  Ageing initial cohorts.  Oldest cohorts={0} yrs, succession timestep={1}.", ageCohorts[0].Age, successionTimestep); 
                    EcoregionData.SetSingleAnnualClimate(ecoregion, time + ageCohorts[0].Age, Climate.Phase.SpinUp_Climate); //the spinup climate array is sorted from oldest to newest years

                     //  Add those cohorts that were born at the current year
                     while (indexNextAgeCohort < ageCohorts.Count && ageCohorts[indexNextAgeCohort].Age == -time)
                        {
                            ISpecies species = ageCohorts[indexNextAgeCohort].Species;

                            float[] initialBiomass = initialBiomassMethod(species, SiteVars.Cohorts[site], site);

                            SiteVars.Cohorts[site].AddNewCohort(ageCohorts[indexNextAgeCohort].Species, 1,
                                                        initialBiomass[0], initialBiomass[1]);
                            indexNextAgeCohort++;
                        }

                        Century.Run(site, successionTimestep, true);
                
                
                }
            //}
            //catch (ClimateDataOutOfRangeException ex)
            //{
            //    throw ex;   //do nothing
            //}
            return SiteVars.Cohorts[site];
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Makes the set of biomass cohorts at a site based on the age cohorts
        /// at the site, using the default method for computing a cohort's
        /// initial biomass.
        /// </summary>
        /// <param name="ageCohorts">
        /// A sorted list of age cohorts, from oldest to youngest.
        /// </param>
        /// <param name="site">
        /// Site where cohorts are located.
        /// </param>
        public static ISiteCohorts MakeBiomassCohorts(List<Landis.Library.AgeOnlyCohorts.ICohort> ageCohorts,
                                                     ActiveSite site)
        {
            return MakeBiomassCohorts(ageCohorts, site, CohortBiomass.InitialBiomass);
        }
    }
}
