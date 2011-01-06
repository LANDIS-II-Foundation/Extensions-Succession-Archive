using Landis.Cohorts;
using Landis.Ecoregions;
using Landis.InitialCommunities;
using Landis.Landscape;
using System.Collections.Generic;

namespace Landis.Biomass.Succession
{
    /// <summary>
    /// Initial communities of biomass cohorts.
    /// </summary>
    public static class InitialCommunities
    {
        private static IDictionary<uint, SiteCohorts> communityCohorts;
            //  Biomass cohorts for each pair of (initial community, ecoregion);
            //  Key = 32-bit unsigned integer where high 16-bits is the map
            //  code of the initial community and the low 16-bits is the
            //  ecoregion's map code

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

        static InitialCommunities()
        {
            communityCohorts = new Dictionary<uint, SiteCohorts>();
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
        /// Gets the set of biomass cohorts for the initial community of age
        /// cohorts in a particular ecoregion.
        /// </summary>
        public static SiteCohorts GetCohorts(ICommunity initialCommunity,
                                             ActiveSite site)
        {
            IEcoregion ecoregion = Model.SiteVars.Ecoregion[site];
            uint key = ComputeKey(initialCommunity.MapCode, ecoregion.MapCode);
            SiteCohorts cohorts;
            if (communityCohorts.TryGetValue(key, out cohorts))
                return cohorts;

            //  If we don't have a sorted list of age cohorts for the initial
            //  community, make the list
            List<AgeCohort.ICohort> sortedAgeCohorts;
            if (! sortedCohorts.TryGetValue(initialCommunity.MapCode, out sortedAgeCohorts)) {
                sortedAgeCohorts = SortCohorts(initialCommunity.Cohorts);
                sortedCohorts[initialCommunity.MapCode] = sortedAgeCohorts;
            }

            cohorts = MakeBiomassCohorts(sortedAgeCohorts, site);
            communityCohorts[key] = cohorts;
            return cohorts;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Makes a list of age cohorts in an initial community sorted from
        /// oldest to youngest.
        /// </summary>
        public static List<AgeCohort.ICohort> SortCohorts(ISiteCohorts<AgeCohort.ICohort> siteCohorts)
        {
            List<AgeCohort.ICohort> cohorts = new List<AgeCohort.ICohort>();
            foreach (ISpeciesCohorts<AgeCohort.ICohort> speciesCohorts in siteCohorts) {
                foreach (AgeCohort.ICohort cohort in speciesCohorts)
                    cohorts.Add(cohort);
            }
            cohorts.Sort(WhichIsOlderCohort);
            return cohorts;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Compares the ages of two cohorts to determine which is older.
        /// </summary>
        /// <returns>
        /// <list type="">
        ///   <item>
        ///     A negative value if x is older than y.
        ///   </item>
        ///   <item>
        ///     0 if x and y are the same age.
        ///   </item>
        ///   <item>
        ///     A positive value if x is younger than y.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This method matches the signature for the System.Comparison
        /// delegate so it can be used to sort an arrary or list from oldest
        /// to youngest.  Sort methods require that the delegate return a
        /// negative value if x comes before y in the sort order, 0 if they are
        /// equivalent, and a positive value is x comes after y in the sort
        /// order.
        /// </remarks>
        public static int WhichIsOlderCohort(AgeCohort.ICohort x,
                                             AgeCohort.ICohort y)
        {
            return y.Age - x.Age;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Makes the set of biomass cohorts at a site based on the age cohorts
        /// at the site.
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
            //  by that timestep.
            for (int time = -(ageCohorts[0].Age); time <= 0; time += successionTimestep) {
                //  Grow current biomass cohorts.
                biomassCohorts.Grow(successionTimestep, site, true);

                //  Add those cohorts that were born at the current year
                while (indexNextAgeCohort < ageCohorts.Count &&
                       ageCohorts[indexNextAgeCohort].Age == -time) {
                    ushort initialBiomass = CohortBiomass.InitialBiomass(biomassCohorts,
                                                                         site);
                    biomassCohorts.AddNewCohort(ageCohorts[indexNextAgeCohort].Species,
                                                initialBiomass);
                    indexNextAgeCohort++;
                }
            }

            return biomassCohorts;
        }
    }
}
