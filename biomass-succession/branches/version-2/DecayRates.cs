using Landis.Ecoregions;
using Landis.Species;

namespace Landis.Biomass.Succession
{
    /// <summary>
    /// Decay rates for dead pools.
    /// </summary>
    public static class DecayRates
    {
        /// <summary>
        /// Woody decay rate for each species.
        /// </summary>
        public static Species.AuxParm<double> Woody;

        //---------------------------------------------------------------------

        /// <summary>
        /// Leaf-litter decay rate for each species in each ecoregion.
        /// </summary>
        //public static Species.AuxParm<Ecoregions.AuxParm<double>> LeafLitter;
    }
}
