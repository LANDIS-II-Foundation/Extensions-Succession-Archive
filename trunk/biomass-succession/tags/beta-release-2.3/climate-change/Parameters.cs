using Edu.Wisc.Forest.Flel.Util;
using System.Collections.Generic;

using Landis.Ecoregions;
using Landis.Species;

using System.Diagnostics;



namespace Landis.Biomass.Succession.ClimateChange
{
    /// <summary>
    /// The biomass parameters affected by climate change.
    /// </summary>
    public class Parameters
        : IParameters
    {
        private Ecoregions.IDataset ecoregionDataset;
        private Species.IDataset speciesDataset;


        private Ecoregions.AuxParm<Percentage>[] minRelativeBiomass;
        private Species.AuxParm<double> leafLongevity;
        private Species.AuxParm<double> woodyDecayRate;
        private Species.AuxParm<double> mortCurveShapeParm;
        private Species.AuxParm<double> growthCurveShapeParm;
        private Species.AuxParm<double> leafLignin;
        private Species.AuxParm<double> maxLAI;
        private Ecoregions.AuxParm<int> aet;
        private List<ISufficientLight> sufficientLight;
        private Species.AuxParm<Ecoregions.AuxParm<double>> establishProbability;
        private Species.AuxParm<Ecoregions.AuxParm<int>> maxANPP;
        private Species.AuxParm<Ecoregions.AuxParm<int>> maxBiomass;

        //---------------------------------------------------------------------

        public Ecoregions.AuxParm<Percentage>[] MinRelativeBiomass
        {
            get {
                return minRelativeBiomass;
            }
        }

        //---------------------------------------------------------------------

        public Species.AuxParm<double> LeafLongevity
        {
            get {
                return leafLongevity;
            }
        }

        //---------------------------------------------------------------------

        public Species.AuxParm<double> WoodyDecayRate
        {
            get {
                return woodyDecayRate;
            }
        }

        //---------------------------------------------------------------------

        public Species.AuxParm<double> MortCurveShapeParm
        {
            get {
                return mortCurveShapeParm;
            }
        }

        //---------------------------------------------------------------------

        public Species.AuxParm<double> GrowthCurveShapeParm
        {
            get {
                return growthCurveShapeParm;
            }
        }
        //---------------------------------------------------------------------

        public Species.AuxParm<double> LeafLignin
        {
            get {
                return leafLignin;
            }
        }
        //---------------------------------------------------------------------

        public Species.AuxParm<double> MaxLAI
        {
            get
            {
                return maxLAI;
            }
        }
        //---------------------------------------------------------------------

        public Ecoregions.AuxParm<int> AET
        {
            get {
                return aet;
            }
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// Definitions of sufficient light probabilities.
        /// </summary>
        public List<ISufficientLight> LightClassProbabilities
        {
            get {
                return sufficientLight;
            }
            set {
            	sufficientLight = value;
            }
        }
        //---------------------------------------------------------------------

        public Species.AuxParm<Ecoregions.AuxParm<double>> EstablishProbability
        {
            get {
                return establishProbability;
            }
        }

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

        public void SetGrowthCurveShapeParm(ISpecies           species,
                                          InputValue<double> newValue)
        {
            Debug.Assert(species != null);
            growthCurveShapeParm[species] = CheckBiomassParm(newValue, 0.0, 1.0);
        }
        //---------------------------------------------------------------------

        public void SetLeafLignin(ISpecies           species,
                                          InputValue<double> newValue)
        {
            Debug.Assert(species != null);
            leafLignin[species] = CheckBiomassParm(newValue, 0.0, 0.4);
        }
        //---------------------------------------------------------------------

        public void SetMaxLAI(ISpecies species,
                                          InputValue<double> newValue)
        {
            Debug.Assert(species != null);
            maxLAI[species] = CheckBiomassParm(newValue, 1.0, 20.0);
        }
        //---------------------------------------------------------------------

        public void SetAET(IEcoregion           ecoregion,
                                          InputValue<int> newValue)
        {
            Debug.Assert(ecoregion != null);
            aet[ecoregion] = CheckBiomassParm(newValue, 0, 10000);  //FIXME:  FIND GOOD MAXIMUM
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


        public Parameters(Ecoregions.IDataset ecoregionDataset,
                                  Species.IDataset    speciesDataset)
        {
            this.ecoregionDataset = ecoregionDataset;
            this.speciesDataset = speciesDataset;

            minRelativeBiomass = new Ecoregions.AuxParm<Percentage>[6];
            for (byte shadeClass = 1; shadeClass <= 5; shadeClass++) {
                minRelativeBiomass[shadeClass] = new Ecoregions.AuxParm<Percentage>(ecoregionDataset);
            }

            leafLongevity       = new Species.AuxParm<double>(speciesDataset);
            woodyDecayRate      = new Species.AuxParm<double>(speciesDataset);
            mortCurveShapeParm  = new Species.AuxParm<double>(speciesDataset);
            growthCurveShapeParm  = new Species.AuxParm<double>(speciesDataset);
            leafLignin          = new Species.AuxParm<double>(speciesDataset);
            maxLAI              = new Species.AuxParm<double>(speciesDataset);
            aet = new Ecoregions.AuxParm<int>(ecoregionDataset);
            sufficientLight     = new List<ISufficientLight>();
            establishProbability = CreateSpeciesEcoregionParm<double>();
            maxANPP             = CreateSpeciesEcoregionParm<int>();
            maxBiomass          = CreateSpeciesEcoregionParm<int>();
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

    }
}
