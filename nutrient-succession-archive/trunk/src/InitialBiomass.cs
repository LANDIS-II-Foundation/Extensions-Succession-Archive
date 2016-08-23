//  Copyright 2007-2010 University of Nevada, Portland State University
//  Authors:  Sarah Ganschow, Robert M. Scheller
//  License:  Available at
//  http://www.landis-ii.org/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Landis.Ecoregions;
using Landis.Landscape;
using Landis.Species;
using Landis.Cohorts;
using Landis.InitialCommunities;

using System.Collections.Generic;

namespace Landis.Biomass.NuCycling.Succession
{
    /// <summary>
    /// The initial live and dead biomass at a site.
    /// </summary>
    public class InitialBiomass
    {
        private SiteCohorts cohorts;
        private List<PoolD> litterPool;
        private PoolD woodyDebrisPool;
        private List<PoolD> deadFRootsPool;
        private Pool fineRootsPool;
        private Pool coarseRootsPool;
        private Charcoal charcoalPool;

        //---------------------------------------------------------------------

        /// <summary>
        /// The site's initial cohorts.
        /// </summary>
        public SiteCohorts InitialCohorts
        {
            get
            {
                return cohorts;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The site's initial dead non-woody pool.
        /// </summary>
        public List<PoolD> LitterPool
        {
            get
            {
                return litterPool;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The site's initial dead woody pool.
        /// </summary>
        public PoolD WoodyDebrisPool
        {
            get
            {
                return woodyDebrisPool;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The site's initial dead fine roots pool.
        /// </summary>
        public List<PoolD> DeadFRootsPool
        {
            get
            {
                return deadFRootsPool;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The site's initial fine roots pool.
        /// </summary>
        public Pool FineRootsPool
        {
            get
            {
                return fineRootsPool;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The site's initial coarse roots pool.
        /// </summary>
        public Pool CoarseRootsPool
        {
            get
            {
                return coarseRootsPool;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The site's initial charcoal pool.
        /// </summary>
        public Charcoal CharcoalPool
        {
            get
            {
                return charcoalPool;
            }
        }

        //---------------------------------------------------------------------

        private InitialBiomass(SiteCohorts cohorts,
                               List<PoolD> litterPool,
                               PoolD woodyDebrisPool,
                               List<PoolD> deadFRootsPool,
                               Pool fineRootsPool,
                               Pool coarseRootsPool,
                               Charcoal charcoalPool)
        {
            this.cohorts = cohorts;
            this.litterPool = litterPool;
            this.woodyDebrisPool = woodyDebrisPool;
            this.deadFRootsPool = deadFRootsPool;
            this.fineRootsPool = fineRootsPool;
            this.coarseRootsPool = coarseRootsPool;
            this.charcoalPool = charcoalPool;
        }

        //---------------------------------------------------------------------

        private static IDictionary<uint, InitialBiomass> initialSites;
        //  Initial site biomass for each unique pair of initial
        //  community and ecoregion; Key = 32-bit unsigned integer where
        //  high 16-bits is the map code of the initial community and the
        //  low 16-bits is the ecoregion's map code

        private static IDictionary<ushort, List<AgeCohort.ICohort>> sortedCohorts;
        //  Age cohorts for an initial community sorted from oldest to
        //  youngest.  Key = initial community's map code

        private static ushort successionTimestep;

        //---------------------------------------------------------------------

        private static uint ComputeKey(ushort initCommunityMapCode,
                               ushort ecoregionMapCode)
        {
            return (uint)((initCommunityMapCode << 16) | ecoregionMapCode);
        }

        //---------------------------------------------------------------------

        static InitialBiomass()
        {
            initialSites = new Dictionary<uint, InitialBiomass>();
            sortedCohorts = new Dictionary<ushort, List<AgeCohort.ICohort>>();
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
            successionTimestep = (ushort)timestep;
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
            IEcoregion ecoregion = Model.Core.Ecoregion[site];
            uint key = ComputeKey(initialCommunity.MapCode, ecoregion.MapCode);
            InitialBiomass initialBiomass;
            if (initialSites.TryGetValue(key, out initialBiomass))
                return initialBiomass;

            //  If we don't have a sorted list of age cohorts for the initial
            //  community, make the list
            List<AgeCohort.ICohort> sortedAgeCohorts;
            if (!sortedCohorts.TryGetValue(initialCommunity.MapCode, out sortedAgeCohorts))
            {
                sortedAgeCohorts = SortCohorts(initialCommunity.Cohorts);
                sortedCohorts[initialCommunity.MapCode] = sortedAgeCohorts;
            }

            SiteCohorts cohorts = MakeBiomassCohorts(sortedAgeCohorts, site);
            initialBiomass = new InitialBiomass(cohorts,
                                                SiteVars.Litter[site],
                                                SiteVars.WoodyDebris[site],
                                                SiteVars.DeadFineRoots[site],
                                                SiteVars.FineRoots[site],
                                                SiteVars.CoarseRoots[site],
                                                SiteVars.Charcoal[site]);
            initialSites[key] = initialBiomass;

            return initialBiomass;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Makes a list of age cohorts in an initial community sorted from
        ///   oldest to youngest.
        /// </summary>
        public static List<AgeCohort.ICohort> SortCohorts(AgeCohort.ISiteCohorts siteCohorts)
        {
            List<AgeCohort.ICohort> cohorts = new List<AgeCohort.ICohort>();
            foreach (AgeCohort.ISpeciesCohorts speciesCohorts in siteCohorts)
            {
                foreach (AgeCohort.ICohort cohort in speciesCohorts)
                    cohorts.Add(cohort);
            }
            cohorts.Sort(AgeCohort.Util.WhichIsOlderCohort);
            return cohorts;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// A method that computes the initial biomass for a new cohort at a
        ///   site based on the existing cohorts.
        /// </summary>
        public delegate float[] ComputeMethod(SiteCohorts siteCohorts,
                                             ActiveSite site, ISpecies species);

        //---------------------------------------------------------------------

        /// <summary>
        /// Makes the set of biomass cohorts at a site based on the age cohorts
        ///   at the site, using a specified method for computing a cohort's
        ///   initial biomass.
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
        public static SiteCohorts MakeBiomassCohorts(List<AgeCohort.ICohort> ageCohorts,
                                                     ActiveSite site,
                                                     ComputeMethod initialBiomassMethod)
        {
            // Fix to keep initial mineral N and P as paramterized for year 1.
            double minN = SiteVars.MineralSoil[site].ContentN;
            double minP = SiteVars.MineralSoil[site].ContentP;

            SiteCohorts biomassCohorts = new SiteCohorts();
            if (ageCohorts.Count == 0)
                return biomassCohorts;

            int indexNextAgeCohort = 0;
            //The index in the list of sorted age cohorts of the next
            //  cohort to be considered.

            //Loop through time from -N to 0 where N is the oldest cohort.
            //  So we're going from the time when the oldest cohort was "born"
            //  to the present time (= 0).  Because the age of any age cohort
            //  is a multiple of the succession timestep, we go from -N to 0
            //  by that timestep.  NOTE: the case where timestep = 1 requires
            //  special treatment because if we start at time = -N with a
            //  cohort with age = 1, then at time = 0, its age will N+1 not N.
            //  Therefore, when timestep = 1, the ending time is -1.
            int endTime = (successionTimestep == 1) ? -1 : 0;
            for (int time = -(ageCohorts[0].Age); time <= endTime; time += successionTimestep)
            {
                //Grow current biomass cohorts.
                NutrientSuccession.InitGrowCohorts(biomassCohorts, site, successionTimestep, true);

                //Add those cohorts that were born at the current year
                while (indexNextAgeCohort < ageCohorts.Count &&
                       ageCohorts[indexNextAgeCohort].Age == -time)
                {
                    float[] initialBiomass = initialBiomassMethod(biomassCohorts,
                                                                 site, ageCohorts[indexNextAgeCohort].Species);
                    float initialWoodBiomass = initialBiomass[0];
                    float initialLeafBiomass = initialBiomass[1];

                    biomassCohorts.AddNewCohort(ageCohorts[indexNextAgeCohort].Species,
                        initialWoodBiomass, initialLeafBiomass);
                    indexNextAgeCohort++;
                }
            }

            // Reset mineral nutrients
            SiteVars.MineralSoil[site].ContentN = minN;
            SiteVars.MineralSoil[site].ContentP = minP;

            return biomassCohorts;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Makes the set of biomass cohorts at a site based on the age cohorts
        ///   at the site, using the default method for computing a cohort's
        ///   initial biomass.
        /// </summary>
        /// <param name="ageCohorts">
        /// A sorted list of age cohorts, from oldest to youngest.
        /// </param>
        /// <param name="site">
        /// Site where cohorts are located.
        /// </param>
        public static SiteCohorts MakeBiomassCohorts(List<AgeCohort.ICohort> ageCohorts,
                                                     ActiveSite site)
        {
            return MakeBiomassCohorts(ageCohorts, site, CohortBiomass.InitialBiomass);
        }
    }
}
