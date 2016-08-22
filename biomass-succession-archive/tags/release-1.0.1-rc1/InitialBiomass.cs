using Landis.Cohorts;
using Landis.Ecoregions;
using Landis.InitialCommunities;
using Landis.Landscape;
using System.Collections.Generic;

namespace Landis.Biomass.Succession
{
    /// <summary>
    /// The initial live and dead biomass at a site.
    /// </summary>
    public class InitialBiomass
    {
        private SiteCohorts cohorts;
        private Dead.Pool deadWoodyPool;
        private Dead.Pool deadNonWoodyPool;

        //---------------------------------------------------------------------

        /// <summary>
        /// The site's initial cohorts.
        /// </summary>
        public SiteCohorts Cohorts
        {
            get {
                return cohorts;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The site's initial dead woody pool.
        /// </summary>
        public Dead.Pool DeadWoodyPool
        {
            get {
                return deadWoodyPool;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The site's initial dead non-woody pool.
        /// </summary>
        public Dead.Pool DeadNonWoodyPool
        {
            get {
                return deadNonWoodyPool;
            }
        }

        //---------------------------------------------------------------------

        private InitialBiomass(SiteCohorts cohorts,
                               Dead.Pool   deadWoodyPool,
                               Dead.Pool   deadNonWoodyPool)
        {
            this.cohorts = cohorts;
            this.deadWoodyPool = deadWoodyPool;
            this.deadNonWoodyPool = deadNonWoodyPool;
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
            return (uint) ((initCommunityMapCode << 16) | ecoregionMapCode);
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
            IEcoregion ecoregion = Model.Core.Ecoregion[site];
            uint key = ComputeKey(initialCommunity.MapCode, ecoregion.MapCode);
            InitialBiomass initialBiomass;
            if (initialSites.TryGetValue(key, out initialBiomass))
                return initialBiomass;

            //  If we don't have a sorted list of age cohorts for the initial
            //  community, make the list
            List<AgeCohort.ICohort> sortedAgeCohorts;
            if (! sortedCohorts.TryGetValue(initialCommunity.MapCode, out sortedAgeCohorts)) {
                sortedAgeCohorts = SortCohorts(initialCommunity.Cohorts);
                sortedCohorts[initialCommunity.MapCode] = sortedAgeCohorts;
            }

            SiteCohorts cohorts = MakeBiomassCohorts(sortedAgeCohorts, site);
            initialBiomass = new InitialBiomass(cohorts,
                                                Dead.Pools.Woody[site],
                                                Dead.Pools.NonWoody[site]);
            initialSites[key] = initialBiomass;
            return initialBiomass;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Makes a list of age cohorts in an initial community sorted from
        /// oldest to youngest.
        /// </summary>
        public static List<AgeCohort.ICohort> SortCohorts(AgeCohort.ISiteCohorts siteCohorts)
        {
            List<AgeCohort.ICohort> cohorts = new List<AgeCohort.ICohort>();
            foreach (AgeCohort.ISpeciesCohorts speciesCohorts in siteCohorts) {
                foreach (AgeCohort.ICohort cohort in speciesCohorts)
                    cohorts.Add(cohort);
            }
            cohorts.Sort(AgeCohort.Util.WhichIsOlderCohort);
            return cohorts;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// A method that computes the initial biomass for a new cohort at a
        /// site based on the existing cohorts.
        /// </summary>
        public delegate ushort ComputeMethod(SiteCohorts siteCohorts,
                                             ActiveSite  site);

        //---------------------------------------------------------------------

        /// <summary>
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
        public static SiteCohorts MakeBiomassCohorts(List<AgeCohort.ICohort> ageCohorts,
                                                     ActiveSite              site,
                                                     ComputeMethod           initialBiomassMethod)
        {
            SiteCohorts biomassCohorts = new SiteCohorts();
            if (ageCohorts.Count == 0)
                return biomassCohorts;

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
            int endTime = (successionTimestep == 1) ? -1 : 0;
            for (int time = -(ageCohorts[0].Age); time <= endTime; time += successionTimestep) {
                //  Grow current biomass cohorts.
                Util.GrowCohorts(biomassCohorts, site, successionTimestep, true);

                //  Add those cohorts that were born at the current year
                while (indexNextAgeCohort < ageCohorts.Count &&
                       ageCohorts[indexNextAgeCohort].Age == -time) {
                    ushort initialBiomass = initialBiomassMethod(biomassCohorts,
                                                                 site);
                    biomassCohorts.AddNewCohort(ageCohorts[indexNextAgeCohort].Species,
                                                initialBiomass);
                    indexNextAgeCohort++;
                }
            }

            return biomassCohorts;
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
        public static SiteCohorts MakeBiomassCohorts(List<AgeCohort.ICohort> ageCohorts,
                                                     ActiveSite              site)
        {
            return MakeBiomassCohorts(ageCohorts, site,
                                      CohortBiomass.InitialBiomass);
        }
    }
}
