//  Copyright 2005 University of Wisconsin-Madison
//  Authors:  Jimm Domingo, FLEL
//  License:  Available at  
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Landis.Succession;
using Landis.Util;

namespace Landis.AgeOnly.Succession
{
    /// <summary>
    /// The parameters for age-only succession.
    /// </summary>
    public class Parameters
        : IParameters
    {
        private int timestep;
        private SeedingAlgorithms seedAlg;
        private double[,] establishProbs;

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

        public double[,] EstablishProbabilities
        {
            get {
                return establishProbs;
            }
        }

        //---------------------------------------------------------------------

        public Parameters(int               timestep,
                          SeedingAlgorithms seedAlgorithm,
                          double[,]         establishProbabilities)
        {
            this.timestep = timestep;
            this.seedAlg = seedAlgorithm;
            this.establishProbs = establishProbabilities;
        }
    }
}
