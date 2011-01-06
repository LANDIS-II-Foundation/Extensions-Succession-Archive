using Landis.Cohorts;
using Landis.Ecoregions;
using Landis.Landscape;
using Landis.Species;

using System.Text;
using System.Reflection;

using log4net;

namespace Landis.Biomass.Succession
{
    /// <summary>
    /// Utility methods.
    /// </summary>
    public static class Util
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly bool isDebugEnabled = log.IsDebugEnabled;

        //---------------------------------------------------------------------

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

                if (isDebugEnabled) {
                    log.DebugFormat("site {0}: biomass = {1}", site.Location, cohorts.TotalBiomass);
                    int computedTotal = 0;
                    foreach (ISpeciesCohorts speciesCohorts in cohorts) {
                        int speciesBiomass;
                        log.DebugFormat("  {0}", ToString(speciesCohorts, out speciesBiomass));
                        computedTotal += speciesBiomass;
                    }
                    log.DebugFormat("  computed biomass = {0}{1}", computedTotal,
                                    computedTotal != cohorts.TotalBiomass ? " <- DIFFERENT" : "");
                }
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Converts all the cohorts for a species into a list in string form.
        /// </summary>
        public static string ToString(ISpeciesCohorts speciesCohorts,
                                      out int         speciesBiomass)
        {
            speciesBiomass = 0;
            StringBuilder result = new StringBuilder();
            result.AppendFormat("{0}: ", speciesCohorts.Species.Name);
            foreach (ICohort cohort in speciesCohorts) {
                result.AppendFormat(" {0} yrs ({1})", cohort.Age, cohort.Biomass);
                speciesBiomass += cohort.Biomass;
            }    
            return result.ToString();    
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
