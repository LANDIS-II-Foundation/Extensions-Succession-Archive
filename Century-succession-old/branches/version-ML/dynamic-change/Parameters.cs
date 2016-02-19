//  Copyright 2007-2010 Portland State University, University of Wisconsin-Madison
//  Author: Robert Scheller, Ben Sulman

using Landis.Core;
using Edu.Wisc.Forest.Flel.Util;

using System.Diagnostics;


namespace Landis.Extension.Succession.Century.Dynamic
{
    /// <summary>
    /// The biomass parameters affected by climate change.
    /// </summary>
    public class Parameters
        : IParameters
    {
        public IEcoregionDataset ecoregionDataset;
        public ISpeciesDataset speciesDataset;

        private Species.AuxParm<Ecoregions.AuxParm<int>> maxANPP;
        private Species.AuxParm<Ecoregions.AuxParm<int>> maxBiomass;

        //---------------------------------------------------------------------

        public Species.AuxParm<Ecoregions.AuxParm<int>> MaxANPP
        {
            get {
                return maxANPP;
            }
        }

        //---------------------------------------------------------------------

        public Species.AuxParm<Ecoregions.AuxParm<int>> MaxBiomass
        {
            get {
                return maxBiomass;
            }
        }
        //---------------------------------------------------------------------

        public Parameters(
                          Species.AuxParm<Ecoregions.AuxParm<int>>    maxANPP,
                          Species.AuxParm<Ecoregions.AuxParm<int>>    maxBiomass)
        {
            this.maxANPP = maxANPP;
            this.maxBiomass = maxBiomass;
        }
        //---------------------------------------------------------------------

        public Parameters(IEcoregionDataset ecoregionDataset,
                                  ISpeciesDataset    speciesDataset)
        {
            this.ecoregionDataset = ecoregionDataset;
            this.speciesDataset = speciesDataset;
            maxANPP                 = CreateSpeciesEcoregionParm<int>();
            maxBiomass              = CreateSpeciesEcoregionParm<int>();
        }

        //---------------------------------------------------------------------

        private Species.AuxParm<Ecoregions.AuxParm<T>> CreateSpeciesEcoregionParm<T>()
        {
            Species.AuxParm<Ecoregions.AuxParm<T>> newParm;
            newParm = new Species.AuxParm<Ecoregions.AuxParm<T>>(speciesDataset);
            foreach (ISpecies species in speciesDataset) {
                newParm[species] = new Ecoregions.AuxParm<T>(ecoregionDataset);
            }
            return newParm;
        }

        //---------------------------------------------------------------------
/*
        private double CheckBiomassParm(InputValue<double> newValue,
                                                    double             minValue,
                                                    double             maxValue)
        {
            if (newValue != null) {
                if (newValue.Actual < minValue || newValue.Actual > maxValue)
                    throw new InputValueException(newValue.String,
                                                  "{0} is not between {1:0.0} and {2:0.0}",
                                                  newValue.String, minValue, maxValue);
            }
            return newValue.Actual;
        }
        //---------------------------------------------------------------------

        private int CheckBiomassParm(InputValue<int> newValue,
                                                    int             minValue,
                                                    int             maxValue)
        {
            if (newValue != null) {
                if (newValue.Actual < minValue || newValue.Actual > maxValue)
                    throw new InputValueException(newValue.String,
                                                  "{0} is not between {1:0.0} and {2:0.0}",
                                                  newValue.String, minValue, maxValue);
            }
            return newValue.Actual;
        }*/

        //---------------------------------------------------------------------

        public void SetMaxANPP(ISpecies        species,
                                     IEcoregion      ecoregion,
                                     InputValue<int> newValue)
        {
            Debug.Assert(species != null);
            Debug.Assert(ecoregion != null);
            if (newValue != null) {
                if (newValue.Actual < 0 || newValue.Actual > 10000)
                    throw new InputValueException(newValue.String,
                                                  "{0} is not between 0 and 10,000",
                                                  newValue.String);
            }
            maxANPP[species][ecoregion] = newValue;
        }

        //---------------------------------------------------------------------

        public void SetMaxBiomass(ISpecies        species,
                                     IEcoregion      ecoregion,
                                     InputValue<int> newValue)
        {
            Debug.Assert(species != null);
            Debug.Assert(ecoregion != null);
            if (newValue != null) {
                if (newValue.Actual < 0 || newValue.Actual > 5000000)
                    throw new InputValueException(newValue.String,
                                                  "{0} is not between 0 and 5,000,000",
                                                  newValue.String);
            }
            maxBiomass[species][ecoregion] = newValue;
        }
/*        //---------------------------------------------------------------------

        public IParameters //GetComplete()
        {
            if (this.IsComplete) {

                return new Parameters(
                                      //ConvertToActualValues(establishProbability),
                                      //ConvertToActualValues(maxANPP),
                                      //ConvertToActualValues(maxBiomass))
                                      maxANPP, maxBiomass);
            }
            else
                return null;
        }
        //---------------------------------------------------------------------

        public virtual bool IsComplete
        {
            get {
                return true;
            }
        }
*/

    }
}
