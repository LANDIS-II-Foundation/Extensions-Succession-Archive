using Edu.Wisc.Forest.Flel.Util;
using Landis.Succession;

namespace Landis.Biomass.Succession
{
    /// <summary>
    /// The parameters for biomass succession.
    /// </summary>
    public interface IParameters
    {
        /// <summary>
        /// Timestep (years)
        /// </summary>
        int Timestep
        {
            get;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Seeding algorithm
        /// </summary>
        SeedingAlgorithms SeedAlgorithm
        {
            get;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The maximum relative biomass for each shade class.
        /// </summary>
        Ecoregions.AuxParm<Percentage>[] MinRelativeBiomass
        {
            get;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Leaf longevity for each species.
        /// </summary>
        Species.AuxParm<double> LeafLongevity
        {
            get;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Bole decay rate for each species.
        /// </summary>
        Species.AuxParm<double> WoodyDecayRate
        {
            get;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Shape parameter for the mortality curve for each species.
        /// </summary>
        Species.AuxParm<double> MortCurveShapeParm
        {
            get;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Species' establishment probabilities for all ecoregions.
        /// </summary>
        Species.AuxParm<Ecoregions.AuxParm<double>> EstablishProbability
        {
            get;
        }

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
        /// Species' decay rates of their leaf litter for all ecoregions.
        /// </summary>
        Species.AuxParm<Ecoregions.AuxParm<double>> LeafLitterDecayRate
        {
            get;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Path to the optional file with the biomass parameters for age-only
        /// disturbances.
        /// </summary>
        string AgeOnlyDisturbanceParms
        {
            get;
        }
    }
}
