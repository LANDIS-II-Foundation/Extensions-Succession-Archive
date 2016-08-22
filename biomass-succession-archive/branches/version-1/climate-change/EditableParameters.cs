using Edu.Wisc.Forest.Flel.Util; 

using Landis.Ecoregions;
using Landis.Species;
using Landis.Succession;

using System.Diagnostics;

namespace Landis.Biomass.Succession.ClimateChange
{
    /// <summary>
    /// Editable set of biomass parameters that are affected by climate change.
    /// </summary>
    public class EditableParameters
        : IEditable<IParameters>
    {
        private Ecoregions.IDataset ecoregionDataset;
        private Species.IDataset speciesDataset;

        private Ecoregions.AuxParm<InputValue<Percentage>>[] minRelativeBiomass;
        private Species.AuxParm<InputValue<double>> leafLongevity;
        private Species.AuxParm<InputValue<double>> woodyDecayRate;
        private Species.AuxParm<InputValue<double>> mortCurveShapeParm;
        private Species.AuxParm<InputValue<double>> leafLignin;
        private Ecoregions.AuxParm<InputValue<int>> aet;
        private ListOfEditable<IEditableSufficientLight, ISufficientLight> sufficientLight;
        private Species.AuxParm<Ecoregions.AuxParm<InputValue<double>>> establishProbability;
        private Species.AuxParm<Ecoregions.AuxParm<InputValue<int>>> maxANPP;
        private Species.AuxParm<Ecoregions.AuxParm<InputValue<int>>> maxBiomass;

        //---------------------------------------------------------------------

        public virtual bool IsComplete
        {
            get {
                return true;
            }
        }

        //---------------------------------------------------------------------

        public EditableParameters(Ecoregions.IDataset ecoregionDataset,
                                  Species.IDataset    speciesDataset)
        {
            this.ecoregionDataset = ecoregionDataset;
            this.speciesDataset = speciesDataset;

            minRelativeBiomass = new Ecoregions.AuxParm<InputValue<Percentage>>[6];
            for (byte shadeClass = 1; shadeClass <= 5; shadeClass++) {
                minRelativeBiomass[shadeClass] = new Ecoregions.AuxParm<InputValue<Percentage>>(ecoregionDataset);
            }

            leafLongevity = new Species.AuxParm<InputValue<double>>(speciesDataset);
            woodyDecayRate = new Species.AuxParm<InputValue<double>>(speciesDataset);
            mortCurveShapeParm = new Species.AuxParm<InputValue<double>>(speciesDataset);
            leafLignin = new Species.AuxParm<InputValue<double>>(speciesDataset);
            aet = new Ecoregions.AuxParm<InputValue<int>>(ecoregionDataset);
            sufficientLight = new ListOfEditable<IEditableSufficientLight, ISufficientLight>();
            establishProbability = CreateSpeciesEcoregionParm<double>();
            maxANPP = CreateSpeciesEcoregionParm<int>();
            maxBiomass = CreateSpeciesEcoregionParm<int>();
        }

        //---------------------------------------------------------------------

        private Species.AuxParm<Ecoregions.AuxParm<InputValue<T>>> CreateSpeciesEcoregionParm<T>()
        {
            Species.AuxParm<Ecoregions.AuxParm<InputValue<T>>> newParm;
            newParm = new Species.AuxParm<Ecoregions.AuxParm<InputValue<T>>>(speciesDataset);
            foreach (ISpecies species in speciesDataset) {
                newParm[species] = new Ecoregions.AuxParm<InputValue<T>>(ecoregionDataset);
            }
            return newParm;
        }

        //---------------------------------------------------------------------

        public void SetMinRelativeBiomass(byte                   shadeClass,
                                          IEcoregion             ecoregion,
                                          InputValue<Percentage> newValue)
        {
            Debug.Assert(1 <= shadeClass && shadeClass <= 5);
            Debug.Assert(ecoregion != null);
            if (newValue != null) {
                if (newValue.Actual < 0.0 || newValue.Actual > 1.0)
                    throw new InputValueException(newValue.String,
                                                  "{0} is not between 0% and 100%", newValue.String);
            }
            minRelativeBiomass[shadeClass][ecoregion] = newValue;
        }

        //---------------------------------------------------------------------

        public void SetLeafLongevity(ISpecies           species,
                                     InputValue<double> newValue)
        {
            Debug.Assert(species != null);
            leafLongevity[species] = CheckBiomassParm(newValue, 1.0, 10.0);
        }

        //---------------------------------------------------------------------

        public void SetWoodyDecayRate(ISpecies           species,
                                     InputValue<double> newValue)
        {
            Debug.Assert(species != null);
            woodyDecayRate[species] = CheckBiomassParm(newValue, 0.0, 1.0);
        }

        //---------------------------------------------------------------------

        public void SetMortCurveShapeParm(ISpecies           species,
                                          InputValue<double> newValue)
        {
            Debug.Assert(species != null);
            mortCurveShapeParm[species] = CheckBiomassParm(newValue, 5.0, 25.0);
        }

        //---------------------------------------------------------------------

        public void SetLeafLignin(ISpecies           species,
                                          InputValue<double> newValue)
        {
            Debug.Assert(species != null);
            leafLignin[species] = CheckBiomassParm(newValue, 0.0, 0.4);
        }
        //---------------------------------------------------------------------

        public void SetAET(IEcoregion           ecoregion,
                                          InputValue<int> newValue)
        {
            Debug.Assert(ecoregion != null);
            aet[ecoregion] = CheckBiomassParm(newValue, 0, 10000);  //FIXME:  FIND GOOD MAXIMUM
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// Definitions of sufficient light.
        /// </summary>
        public ListOfEditable<IEditableSufficientLight, ISufficientLight> LightClassProbabilities
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

        private InputValue<double> CheckBiomassParm(InputValue<double> newValue,
                                                    double             minValue,
                                                    double             maxValue)
        {
            if (newValue != null) {
                if (newValue.Actual < minValue || newValue.Actual > maxValue)
                    throw new InputValueException(newValue.String,
                                                  "{0} is not between {1:0.0} and {2:0.0}",
                                                  newValue.String, minValue, maxValue);
            }
            return newValue;
        }
        //---------------------------------------------------------------------

        private InputValue<int> CheckBiomassParm(InputValue<int> newValue,
                                                    int             minValue,
                                                    int             maxValue)
        {
            if (newValue != null) {
                if (newValue.Actual < minValue || newValue.Actual > maxValue)
                    throw new InputValueException(newValue.String,
                                                  "{0} is not between {1:0.0} and {2:0.0}",
                                                  newValue.String, minValue, maxValue);
            }
            return newValue;
        }

        //---------------------------------------------------------------------

        public void SetEstablishProbability(ISpecies           species,
                                            IEcoregion         ecoregion,
                                            InputValue<double> newValue)
        {
            Debug.Assert(species != null);
            Debug.Assert(ecoregion != null);
            establishProbability[species][ecoregion] = CheckBiomassParm(newValue, 0.0, 1.0);
        }

        //---------------------------------------------------------------------

        public void SetMaxANPP(ISpecies        species,
                                     IEcoregion      ecoregion,
                                     InputValue<int> newValue)
        {
            Debug.Assert(species != null);
            Debug.Assert(ecoregion != null);
            if (newValue != null) {
                if (newValue.Actual < 0 || newValue.Actual > 100000)
                    throw new InputValueException(newValue.String,
                                                  "{0} is not between 0 and 100,000",
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
                if (newValue.Actual < 0 || newValue.Actual > 1000000)
                    throw new InputValueException(newValue.String,
                                                  "{0} is not between 0 and 1,000,000",
                                                  newValue.String);
            }
            maxBiomass[species][ecoregion] = newValue;
        }

        //---------------------------------------------------------------------

        public IParameters GetComplete()
        {
            if (this.IsComplete) {
                Ecoregions.AuxParm<Percentage>[] maxRelBiomass = new Ecoregions.AuxParm<Percentage>[6];
                for (byte shadeClass = 1; shadeClass <= 5; shadeClass++) {
                    maxRelBiomass[shadeClass] = ConvertToActualValues(minRelativeBiomass[shadeClass]);
                }

                return new Parameters(maxRelBiomass,
                                      ConvertToActualValues(leafLongevity),
                                      ConvertToActualValues(woodyDecayRate),
                                      ConvertToActualValues(mortCurveShapeParm),
                                      ConvertToActualValues(leafLignin),
                                      ConvertToActualValues(aet),
                                      sufficientLight.GetComplete(),
                                      ConvertToActualValues(establishProbability),
                                      ConvertToActualValues(maxANPP),
                                      ConvertToActualValues(maxBiomass));
            }
            else
                return null;
        }

        //---------------------------------------------------------------------

        private Ecoregions.AuxParm<T> ConvertToActualValues<T>(Ecoregions.AuxParm<InputValue<T>> inputValues)
        {
            Ecoregions.AuxParm<T> actualValues = new Ecoregions.AuxParm<T>(ecoregionDataset);
            foreach (IEcoregion ecoregion in ecoregionDataset)
                if (inputValues[ecoregion] != null)
                    actualValues[ecoregion] = inputValues[ecoregion].Actual;
            return actualValues;
        }

        //---------------------------------------------------------------------

        private Species.AuxParm<T> ConvertToActualValues<T>(Species.AuxParm<InputValue<T>> inputValues)
        {
            Species.AuxParm<T> actualValues = new Species.AuxParm<T>(speciesDataset);
            foreach (ISpecies species in speciesDataset)
                if (inputValues[species] != null)
                    actualValues[species] = inputValues[species].Actual;
            return actualValues;
        }

        //---------------------------------------------------------------------

        private Species.AuxParm<Ecoregions.AuxParm<T>> ConvertToActualValues<T>(Species.AuxParm<Ecoregions.AuxParm<InputValue<T>>> inputValues)
        {
            Species.AuxParm<Ecoregions.AuxParm<T>> actualValues = new Species.AuxParm<Ecoregions.AuxParm<T>>(speciesDataset);
            foreach (ISpecies species in speciesDataset)
                actualValues[species] = ConvertToActualValues(inputValues[species]);
            return actualValues;
        }
    }
}
