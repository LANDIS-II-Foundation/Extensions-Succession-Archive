//  Copyright 2005-2010 Portland State University
//  Authors:  Robert M. Scheller
//  License:  Available at
//  http://www.landis-ii.org/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

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
        private bool calibrateMode;
        private double spinupMortalityFraction;
        private List<ClimateChange.ParametersUpdate> climateChangeUpdates;

        //---------------------------------------------------------------------

        public int Timestep
        {
            get {
                return timestep;
            }
            set {
                if (value < 0)
                        throw new InputValueException(value.ToString(), "Timestep must be > or = 0");
                timestep = value;
            }
        }

        //---------------------------------------------------------------------

        public SeedingAlgorithms SeedAlgorithm
        {
            get {
                return seedAlg;
            }
            set {
                seedAlg = value;
            }
        }
        //---------------------------------------------------------------------
        public bool CalibrateMode
        {
            get {
                return calibrateMode;
            }
            set {
                calibrateMode = value;
            }
        }

        //---------------------------------------------------------------------

        public double SpinupMortalityFraction
        {
            get {
                return spinupMortalityFraction;
            }
            set {
                if (value < 0.0 || value > 0.5)
                        throw new InputValueException(value.ToString(), "SpinupMortalityFraction must be > 0.0 and < 0.5");
                spinupMortalityFraction = value;
            }
        }
        //---------------------------------------------------------------------

        public string AgeOnlyDisturbanceParms
        {
            get {
                return ageOnlyDisturbanceParms;
            }
            set {
                string path = value;
               if (path.Trim(null).Length == 0)
                    throw new InputValueException(path,
                                                      "\"{0}\" is not a valid path.",
                                                      path);
                ageOnlyDisturbanceParms = value;
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

        public Parameters(Ecoregions.IDataset ecoregionDataset,
                                  Species.IDataset    speciesDataset)
            : base(ecoregionDataset,
                   speciesDataset)
        {
            this.climateChangeUpdates = new List<ClimateChange.ParametersUpdate>();
        }
    }
}
