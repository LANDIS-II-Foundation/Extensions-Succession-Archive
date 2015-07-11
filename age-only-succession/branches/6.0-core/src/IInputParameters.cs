//  Copyright 2005 University of Wisconsin-Madison
//  Authors:  Jimm Domingo, FLEL
//  License:  Available at
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Landis.Library.Succession;
using Landis.Ecoregions;
using Landis.Species;
//using Landis.Util;
using Edu.Wisc.Forest.Flel.Util;

namespace Landis.Extension.Succession.AgeOnly
{
    /// <summary>
    /// The parameters for age-only succession.
    /// </summary>
    public interface IInputParameters
    {
        /// <summary>
        /// Timestep (years)
        /// </summary>
        int Timestep
        {
            get;set;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Seeding algorithm
        /// </summary>
        SeedingAlgorithms SeedAlgorithm
        {
            get;set;
        }

        //---------------------------------------------------------------------

        InputValue<string> InitialCommunities
        {
            get;
            set;
        }

        //---------------------------------------------------------------------

        InputValue<string> InitialCommunitiesMap
        {
            get;
            set;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The species establishment probabilities for all ecoregions.
        /// </summary>
        /// <remarks>
        /// Indexed by ecoregion and species index numbers, i.e.,
        /// [ecoregion.Index][species.Index]
        /// </remarks>
        Species.AuxParm<Ecoregions.AuxParm<double>> EstablishProbabilities
        {
            get;
        }

        //---------------------------------------------------------------------

        void SetProbability(IEcoregion ecoregion,
                            ISpecies   species,
                            double     probability);

    }
}
