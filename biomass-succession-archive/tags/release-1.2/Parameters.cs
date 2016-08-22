using Edu.Wisc.Forest.Flel.Util;
using Landis.Succession;
using System.Collections.Generic;

namespace Landis.Biomass.Succession
{
    /// <summary>
    /// The parameters for biomass succession.
    /// </summary>
    public class Parameters
        : ClimateChange.Parameters, IParameters
    {
        private int timestep;
        private SeedingAlgorithms seedAlg;
        private string ageOnlyDisturbanceParms;
        private List<ClimateChange.ParametersUpdate> climateChangeUpdates;

        //---------------------------------------------------------------------

        public int Timestep
        {
            get {
                return timestep;
            }
        }

        //---------------------------------------------------------------------

        public SeedingAlgorithms SeedAlgorithm
        {
            get {
                return seedAlg;
            }
        }

        //---------------------------------------------------------------------

        public string AgeOnlyDisturbanceParms
        {
            get {
                return ageOnlyDisturbanceParms;
            }
        }

        //---------------------------------------------------------------------

        public List<ClimateChange.ParametersUpdate> ClimateChangeUpdates
        {
            get {
                return climateChangeUpdates;
            }
        }

        //---------------------------------------------------------------------

        public Parameters(int                                         timestep,
                          SeedingAlgorithms                           seedAlgorithm,
                          Ecoregions.AuxParm<Percentage>[]            minRelativeBiomass,
                          Species.AuxParm<double>                     leafLongevity,
                          Species.AuxParm<double>                     woodyDecayRate,
                          Species.AuxParm<double>                     mortCurveShapeParm,
                          Species.AuxParm<Ecoregions.AuxParm<double>> establishProbability,
                          Species.AuxParm<Ecoregions.AuxParm<int>>    maxANPP,
                          Species.AuxParm<Ecoregions.AuxParm<double>> leafLitterDecayRate,
                          string                                      ageOnlyDisturbanceParms,
                          List<ClimateChange.ParametersUpdate>        climateChangeUpdates)
            : base(minRelativeBiomass,
                   leafLongevity,
                   woodyDecayRate,
                   mortCurveShapeParm,
                   establishProbability,
                   maxANPP,
                   leafLitterDecayRate)
        {
            this.timestep = timestep;
            this.seedAlg = seedAlgorithm;
            this.ageOnlyDisturbanceParms = ageOnlyDisturbanceParms;
            this.climateChangeUpdates = climateChangeUpdates;
        }
    }
}
