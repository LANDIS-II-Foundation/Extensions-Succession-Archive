//  Copyright 2005 University of Nevada, University of Wisconsin
//  Authors:  Sarah Ganschow, Robert M. Scheller, James B. Domingo
//  License:  Available at
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Util;
using Landis.Succession;

using System.Collections.Generic;
using System.Diagnostics;


namespace Landis.Biomass.NuCycling.Succession
{
    /// <summary>
    /// The parameters for biomass succession.
    /// </summary>
    public class InputParameters
        : DynamicChange.InputParameters, IInputParameters
    {
        private int timestep;
        private SeedingAlgorithms seedAlg;
        private string ageOnlyDisturbanceParms;
        private List<ISufficientLight> sufficientLight;
        private List<DynamicChange.ParametersUpdate> climateChangeUpdates;

        //---------------------------------------------------------------------

        public int Timestep
        {
            get
            {
                return timestep;
            }
            set
            {
                if (value < 0)
                        throw new InputValueException(value.ToString(), "Timestep must be > or = 0");
                timestep = value;
            }
        }

        //---------------------------------------------------------------------

        public SeedingAlgorithms SeedAlgorithm
        {
            get
            {
                return seedAlg;
            }
            set
            {
                seedAlg = value;
            }
        }

        //---------------------------------------------------------------------

        public string AgeOnlyDisturbanceParms
        {
            get
            {
                return ageOnlyDisturbanceParms;
            }
            set
            {
                string path = value;
                    if (path.Trim(null).Length == 0)
                        throw new InputValueException(path, "\"{0}\" is not a valid path.", path);
                ageOnlyDisturbanceParms = value;
            }
        }

        //---------------------------------------------------------------------

        public List<DynamicChange.ParametersUpdate> DynamicChangeUpdates
        {
            get
            {
                return climateChangeUpdates;
            }
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Definitions of sufficient light probabilities.
        /// </summary>
        public List<ISufficientLight> LightClassProbabilities
        {
            get
            {
                return sufficientLight;
            }
            set
            {
                Debug.Assert(sufficientLight.Count != 0);
                sufficientLight = value;
            }
        }

        //---------------------------------------------------------------------

        public InputParameters(Ecoregions.IDataset ecoregionDataset,
                                  Species.IDataset speciesDataset)
            : base(ecoregionDataset,
                   speciesDataset)
        {
            sufficientLight = new List<ISufficientLight>();
            this.climateChangeUpdates = new List<DynamicChange.ParametersUpdate>();
        }
        //---------------------------------------------------------------------

        /*public Parameters(int timestep,
                          SeedingAlgorithms seedAlgorithm,
                          ISufficientLight[] sufficientLight,
                          Ecoregions.AuxParm<Percentage>[] minRelativeBiomass,
                          Species.AuxParm<double> leafLongevity,
                          Species.AuxParm<double> woodyDecayRate,
                          Species.AuxParm<double> mortCurveShapeParm,
                          Species.AuxParm<double> leafFractionC,
                          Species.AuxParm<double> leafFractionN,
                          Species.AuxParm<double> leafFractionP,
                          Species.AuxParm<double> woodFractionC,
                          Species.AuxParm<double> woodFractionN,
                          Species.AuxParm<double> woodFractionP,
                          Species.AuxParm<double> fRootFractionC,
                          Species.AuxParm<double> fRootFractionN,
                          Species.AuxParm<double> fRootFractionP,
                          Species.AuxParm<double> litterFractionC,
                          Species.AuxParm<double> litterFractionN,
                          Species.AuxParm<double> litterFractionP,
                          Species.AuxParm<double> leafLignin,
                          Species.AuxParm<int> nitrogenTolerance,
                          Ecoregions.AuxParm<int> depositionN,
                          Ecoregions.AuxParm<int> depositionP,
                          Ecoregions.AuxParm<double> decayRateSOM,
                          Ecoregions.AuxParm<int> initialSOMMass,
                          Ecoregions.AuxParm<int> initialSOMC,
                          Ecoregions.AuxParm<int> initialSOMN,
                          Ecoregions.AuxParm<int> initialSOMP,
                          Ecoregions.AuxParm<double> weatheringP,
                          Ecoregions.AuxParm<int> initialMineralN,
                          Ecoregions.AuxParm<int> initialMineralP,
                          Ecoregions.AuxParm<int> aet,
                          //ISeverity[] severities,
                          Species.AuxParm<Ecoregions.AuxParm<double>> establishProbability,
                          Species.AuxParm<Ecoregions.AuxParm<int>> maxANPP,
                          Species.AuxParm<Ecoregions.AuxParm<int>> maxBiomass,
                          string ageOnlyDisturbanceParms,
                          List<ClimateChange.ParametersUpdate> climateChangeUpdates)
            : base(minRelativeBiomass,
                   leafLongevity,
                   woodyDecayRate,
                   mortCurveShapeParm,
                   leafFractionC,
                   leafFractionN,
                   leafFractionP,
                   woodFractionC,
                   woodFractionN,
                   woodFractionP,
                   fRootFractionC,
                   fRootFractionN,
                   fRootFractionP,
                   litterFractionC,
                   litterFractionN,
                   litterFractionP,
                   leafLignin,
                   nitrogenTolerance,
                   depositionN,
                   depositionP,
                   decayRateSOM,
                   initialSOMMass,
                   initialSOMC,
                   initialSOMN,
                   initialSOMP,
                   weatheringP,
                   initialMineralN,
                   initialMineralP,
                   aet,
                   //severities,
                   establishProbability,
                   maxANPP,
                   maxBiomass)
        {
            this.timestep = timestep;
            this.seedAlg = seedAlgorithm;
            this.sufficientLight = sufficientLight;
            this.ageOnlyDisturbanceParms = ageOnlyDisturbanceParms;
            this.climateChangeUpdates = climateChangeUpdates;
        }*/
    }
}
