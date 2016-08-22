using Landis.Cohorts;
using Landis.Landscape;

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
    }
}
