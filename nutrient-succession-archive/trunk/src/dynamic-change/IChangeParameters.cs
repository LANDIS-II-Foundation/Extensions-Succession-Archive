using Edu.Wisc.Forest.Flel.Util;

namespace Landis.Biomass.NuCycling.Succession.DynamicChange
{
    /// <summary>
    /// The biomass parameters affected by climate change.
    /// </summary>
    public interface IInputParameters
    {
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
        /// Leaf Carbon content for each species.
        /// </summary>
        Species.AuxParm<double> LeafFractionC { get;}
        Species.AuxParm<double> LeafFractionN { get;}
        Species.AuxParm<double> LeafFractionP { get;}
        Species.AuxParm<double> WoodFractionC { get;}
        Species.AuxParm<double> WoodFractionN { get;}
        Species.AuxParm<double> WoodFractionP { get;}
        Species.AuxParm<double> FRootFractionC { get;}
        Species.AuxParm<double> FRootFractionN { get;}
        Species.AuxParm<double> FRootFractionP { get;}
        Species.AuxParm<double> LitterFractionC { get;}
        Species.AuxParm<double> LitterFractionN { get;}
        Species.AuxParm<double> LitterFractionP { get;}
        Species.AuxParm<double> LeafLignin { get;}
        Species.AuxParm<int> NTolerance { get;}

        Ecoregions.AuxParm<int> DepositionN { get;}
        Ecoregions.AuxParm<int> DepositionP { get;}
        Ecoregions.AuxParm<double> DecayRateSOM { get;}
        Ecoregions.AuxParm<int> InitialSOMMass { get;}
        Ecoregions.AuxParm<int> InitialSOMC { get;}
        Ecoregions.AuxParm<int> InitialSOMN { get;}
        Ecoregions.AuxParm<int> InitialSOMP { get;}
        Ecoregions.AuxParm<double> WeatheringP { get;}
        Ecoregions.AuxParm<int> InitialMineralN { get;}
        Ecoregions.AuxParm<int> InitialMineralP { get;}
        Ecoregions.AuxParm<int> AET { get;}

        //---------------------------------------------------------------------

        /// <summary>
        /// Definitions of wind severities.
        /// </summary>
        ISeverity[] FireSeverities
        {
            get;set;
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
        /// Species' maximum aboveground live biomass for all ecoregions.
        /// </summary>
        Species.AuxParm<Ecoregions.AuxParm<int>> MaxBiomass
        {
            get;
        }
    }
}
