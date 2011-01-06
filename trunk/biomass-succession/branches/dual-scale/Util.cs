using Landis.Cohorts;
using Landis.Ecoregions;
using Wisc.Flel.GeospatialModeling.Landscapes.DualScale;
using Landis.Species;

namespace Landis.Biomass.Succession
{
    /// <summary>
    /// Utility methods.
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// Grows all cohorts at a site for a specified number of years.  The
        /// dead pools at the site also decompose for the given time period.
        /// </summary>
        public static void GrowCohorts(SiteCohorts cohorts,
                                       ActiveSite  site,
                                       int         years,
                                       bool        isSuccessionTimestep)
        {
            for (int y = 1; y <= years; ++y) {
                cohorts.Grow(site, (y == years && isSuccessionTimestep));
                Dead.Pools.Woody[site].Decompose();
                Dead.Pools.NonWoody[site].Decompose();
            }
    	}

        //---------------------------------------------------------------------

        /// <summary>
        /// Converts a table indexed by species and ecoregion into a
        /// 2-dimensional array.
        /// </summary>
        public static T[,] ToArray<T>(Species.AuxParm<Ecoregions.AuxParm<T>> table)
        {
            T[,] array = new T[Model.Core.Ecoregions.Count, Model.Core.Species.Count];
            foreach (ISpecies species in Model.Core.Species) {
                foreach (IEcoregion ecoregion in Model.Core.Ecoregions) {
                    array[ecoregion.Index, species.Index] = table[species][ecoregion];
                }
            }
            return array;
        }
    }
}
