using Edu.Wisc.Forest.Flel.Util;

namespace Landis.Biomass.Succession.ClimateChange
{
    /// <summary>
    /// The biomass parameters affected by climate change.
    /// </summary>
    public class Parameters
        : IParameters
    {
        private Ecoregions.AuxParm<Percentage>[] minRelativeBiomass;
        private Species.AuxParm<double> leafLongevity;
        private Species.AuxParm<double> woodyDecayRate;
        private Species.AuxParm<double> mortCurveShapeParm;
        private Species.AuxParm<Ecoregions.AuxParm<double>> establishProbability;
        private Species.AuxParm<Ecoregions.AuxParm<int>> maxANPP;
        private Species.AuxParm<Ecoregions.AuxParm<double>> leafLitterDecayRate;

        //---------------------------------------------------------------------

        public Ecoregions.AuxParm<Percentage>[] MinRelativeBiomass
        {
            get {
                return minRelativeBiomass;
            }
        }

        //---------------------------------------------------------------------

        public Species.AuxParm<double> LeafLongevity
        {
            get {
                return leafLongevity;
            }
        }

        //---------------------------------------------------------------------

        public Species.AuxParm<double> WoodyDecayRate
        {
            get {
                return woodyDecayRate;
            }
        }

        //---------------------------------------------------------------------

        public Species.AuxParm<double> MortCurveShapeParm
        {
            get {
                return mortCurveShapeParm;
            }
        }

        //---------------------------------------------------------------------

        public Species.AuxParm<Ecoregions.AuxParm<double>> EstablishProbability
        {
            get {
                return establishProbability;
            }
        }

        //---------------------------------------------------------------------

        public Species.AuxParm<Ecoregions.AuxParm<int>> MaxANPP
        {
            get {
                return maxANPP;
            }
        }

        //---------------------------------------------------------------------

        public Species.AuxParm<Ecoregions.AuxParm<double>> LeafLitterDecayRate
        {
            get {
                return leafLitterDecayRate;
            }
        }

        //---------------------------------------------------------------------

        public Parameters(Ecoregions.AuxParm<Percentage>[]            minRelativeBiomass,
                          Species.AuxParm<double>                     leafLongevity,
                          Species.AuxParm<double>                     woodyDecayRate,
                          Species.AuxParm<double>                     mortCurveShapeParm,
                          Species.AuxParm<Ecoregions.AuxParm<double>> establishProbability,
                          Species.AuxParm<Ecoregions.AuxParm<int>>    maxANPP,
                          Species.AuxParm<Ecoregions.AuxParm<double>> leafLitterDecayRate)
        {
            this.minRelativeBiomass = minRelativeBiomass;
            this.leafLongevity = leafLongevity;
            this.woodyDecayRate = woodyDecayRate;
            this.mortCurveShapeParm = mortCurveShapeParm;
            this.establishProbability = establishProbability;
            this.maxANPP = maxANPP;
            this.leafLitterDecayRate = leafLitterDecayRate;
        }
    }
}
