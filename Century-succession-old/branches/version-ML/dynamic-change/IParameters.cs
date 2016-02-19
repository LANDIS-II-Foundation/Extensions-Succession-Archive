using Edu.Wisc.Forest.Flel.Util;

namespace Landis.Extension.Succession.Century.Dynamic
{
    /// <summary>
    /// The biomass parameters affected by climate change.
    /// </summary>
    public interface IParameters
    {
        
        //---------------------------------------------------------------------
/*
        /// <summary>
        /// Species' establishment probabilities for all ecoregions.
        /// </summary>
        Species.AuxParm<Ecoregions.AuxParm<double>> EstablishProbability
        {
            get;
        }
*/
        //---------------------------------------------------------------------

        /// <summary>
        /// Species' maximum growth rates for all ecoregions.
        /// </summary>
        Species.AuxParm<Ecoregions.AuxParm<int>> MaxANPP
        {
            get;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Species' maximum aboveground live biomass for all ecoregions.
        /// </summary>
        Species.AuxParm<Ecoregions.AuxParm<int>> MaxBiomass
        {
            get;
        }
    }
}
