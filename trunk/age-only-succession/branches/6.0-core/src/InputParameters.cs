//  Copyright 2005 University of Wisconsin-Madison
//  Authors:  Jimm Domingo, FLEL
//  License:  Available at
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Landis.Library.Succession;
using Landis.Ecoregions;
using Landis.Species;
using Edu.Wisc.Forest.Flel.Util;

namespace Landis.Extension.Succession.AgeOnly
{
    /// <summary>
    /// The parameters for age-only succession.
    /// </summary>
    public class InputParameters
        : IInputParameters
    {

        private Ecoregions.IDataset ecoregionDataset;
        private Species.IDataset speciesDataset;

        private int timestep;
        private SeedingAlgorithms seedAlg;
        private Species.AuxParm<Ecoregions.AuxParm<double>> establishProbs;
        private InputValue<string> initCommunities;
        private InputValue<string> communitiesMap;

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

        public Species.AuxParm<Ecoregions.AuxParm<double>> EstablishProbabilities
        {
            get {
                return establishProbs;
            }
        }

        //---------------------------------------------------------------------

        public InputParameters()
        {
            //establishProbs = new double[Model.Core.Ecoregions.Count, Model.Core.Species.Count];

            ecoregionDataset = Model.Core.Ecoregions;
            speciesDataset = Model.Core.Species;

            establishProbs = CreateSpeciesEcoregionParm<double>();
        }

        //---------------------------------------------------------------------

        public Species.AuxParm<Ecoregions.AuxParm<T>> CreateSpeciesEcoregionParm<T>()
        {
            Species.AuxParm<Ecoregions.AuxParm<T>> newParm;
            newParm = new Species.AuxParm<Ecoregions.AuxParm<T>>(speciesDataset);
            foreach (ISpecies species in speciesDataset)
            {
                newParm[species] = new Ecoregions.AuxParm<T>(ecoregionDataset);
            }
            return newParm;
        }


        //---------------------------------------------------------------------

        public void SetProbability(IEcoregion ecoregion,
                                   ISpecies   species,
                                   double     probability)
        {
            //establishProbs[ecoregion.Index, species.Index] = probability;
            establishProbs[species][ecoregion] = probability;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Path to the file with the initial communities' definitions.
        /// </summary>
        public InputValue<string> InitialCommunities
        {
            get
            {
                return initCommunities;
            }

            set
            {
                if (value != null)
                {
                    ValidatePath(value.Actual);
                }
                initCommunities = value;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Path to the raster file showing where the initial communities are.
        /// </summary>
        public InputValue<string> InitialCommunitiesMap
        {
            get
            {
                return communitiesMap;
            }

            set
            {
                if (value != null)
                {
                    ValidatePath(value.Actual);
                }
                communitiesMap = value;
            }
        }

        //---------------------------------------------------------------------

        private void ValidatePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new InputValueException();
            if (path.Trim(null).Length == 0)
                throw new InputValueException(path,
                                              "\"{0}\" is not a valid path.",
                                              path);
        }

        //---------------------------------------------------------------------

/*        public InputParameters(int               timestep,
                          SeedingAlgorithms seedAlgorithm,
                          double[,]         establishProbabilities)
        {
            this.timestep = timestep;
            this.seedAlg = seedAlgorithm;
            this.establishProbs = establishProbabilities;
        } */
    }
}
