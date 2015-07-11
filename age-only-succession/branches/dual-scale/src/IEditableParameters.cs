//  Copyright 2005 University of Wisconsin-Madison
//  Authors:  Jimm Domingo, FLEL
//  License:  Available at  
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Util;

using Landis.Ecoregions;
using Landis.Species;
using Landis.Succession;

namespace Landis.AgeOnly.Succession
{
    /// <summary>
    /// Editable set of parameters for age-only succession.
    /// </summary>
    public interface IEditableParameters
        : IEditable<IParameters>
    {
        /// <summary>
        /// Timestep (years)
        /// </summary>
        InputValue<int> Timestep
        {
            get;
            set;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Seeding algorithm
        /// </summary>
        InputValue<SeedingAlgorithms> SeedAlgorithm
        {
            get;
            set;
        }

        //---------------------------------------------------------------------

        void SetProbability(IEcoregion ecoregion,
                            ISpecies   species,
                            double     probability);
    }
}
