//  Copyright 2005 University of Wisconsin-Madison
//  Authors:  Jimm Domingo, FLEL
//  License:  Available at  
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Landis.Succession;

namespace Landis.AgeOnly.Succession
{
    /// <summary>
    /// The parameters for age-only succession.
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
        /// The species establishment probabilities for all ecoregions.
        /// </summary>
        /// <remarks>
        /// Indexed by ecoregion and species index numbers, i.e.,
        /// [ecoregion.Index][species.Index]
        /// </remarks>
        double[,] EstablishProbabilities
        {
            get;
        }
    }
}
