//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.Library.Succession;
using Landis.Core;
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
        string DynamicInputFile { get; set; }

        //---------------------------------------------------------------------

        /// <summary>
        /// The species establishment probabilities for all ecoregions.
        /// </summary>
        /// <remarks>
        /// Indexed by ecoregion and species index numbers, i.e.,
        /// [ecoregion.Index][species.Index]
        /// </remarks>
        //Species.AuxParm<Ecoregions.AuxParm<double>> EstablishProbabilities
        //{
        //    get;
        //}

        //---------------------------------------------------------------------

        //void SetProbability(IEcoregion ecoregion,
        //                    ISpecies   species,
        //                    double     probability);

    }
}
