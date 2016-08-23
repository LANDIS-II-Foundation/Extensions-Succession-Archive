using Edu.Wisc.Forest.Flel.Util;
using Landis.Ecoregions;
using Landis.Species;
using System.Diagnostics;

namespace Landis.Biomass.NuCycling.Succession.DynamicChange
{
    /// <summary>
    /// The biomass parameters affected by climate change.
    /// </summary>
    public class InputParameters
        : IInputParameters
    {
        private Ecoregions.IDataset ecoregionDataset;
        private Species.IDataset speciesDataset;

        private Ecoregions.AuxParm<Percentage>[] minRelativeBiomass;
        private Species.AuxParm<double> leafLongevity;
        private Species.AuxParm<double> woodyDecayRate;
        private Species.AuxParm<double> mortCurveShapeParm;
        private Species.AuxParm<double> leafFractionC;
        private Species.AuxParm<double> leafFractionN;
        private Species.AuxParm<double> leafFractionP;
        private Species.AuxParm<double> woodFractionC;
        private Species.AuxParm<double> woodFractionN;
        private Species.AuxParm<double> woodFractionP;
        private Species.AuxParm<double> fRootFractionC;
        private Species.AuxParm<double> fRootFractionN;
        private Species.AuxParm<double> fRootFractionP;
        private Species.AuxParm<double> litterFractionC;
        private Species.AuxParm<double> litterFractionN;
        private Species.AuxParm<double> litterFractionP;
        private Species.AuxParm<double> leafLignin;
        private Species.AuxParm<int> nitrogenTolerance;
        private Ecoregions.AuxParm<int> depositionN;
        private Ecoregions.AuxParm<int> depositionP;
        private Ecoregions.AuxParm<double> decayRateSOM;
        private Ecoregions.AuxParm<int> initialSOMMass;
        private Ecoregions.AuxParm<int> initialSOMC;
        private Ecoregions.AuxParm<int> initialSOMN;
        private Ecoregions.AuxParm<int> initialSOMP;
        private Ecoregions.AuxParm<double> weatheringP;
        private Ecoregions.AuxParm<int> initialMineralN;
        private Ecoregions.AuxParm<int> initialMineralP;
        private Ecoregions.AuxParm<int> aet;
        private ISeverity[] severities;

        private Species.AuxParm<Ecoregions.AuxParm<double>> establishProbability;
        private Species.AuxParm<Ecoregions.AuxParm<int>> maxANPP;
        private Species.AuxParm<Ecoregions.AuxParm<int>> maxBiomass;

        //---------------------------------------------------------------------

        public Ecoregions.AuxParm<Percentage>[] MinRelativeBiomass
        {
            get
            {
                return minRelativeBiomass;
            }
            set
            {
                minRelativeBiomass = value;
            }
        }

        //---------------------------------------------------------------------

        public Species.AuxParm<double> LeafLongevity
        {
            get
            {
                return leafLongevity;
            }
        }

        //---------------------------------------------------------------------

        public Species.AuxParm<double> WoodyDecayRate
        {
            get
            {
                return woodyDecayRate;
            }
        }

        //---------------------------------------------------------------------

        public Species.AuxParm<double> MortCurveShapeParm
        {
            get
            {
                return mortCurveShapeParm;
            }
        }

        //---------------------------------------------------------------------

        public Species.AuxParm<double> LeafFractionC
        {
            get
            {
                return leafFractionC;
            }
        }
        //---------------------------------------------------------------------

        public Species.AuxParm<double> LeafFractionN
        {
            get
            {
                return leafFractionN;
            }
        }
        //---------------------------------------------------------------------

        public Species.AuxParm<double> LeafFractionP
        {
            get
            {
                return leafFractionP;
            }
        }
        //---------------------------------------------------------------------

        public Species.AuxParm<double> WoodFractionC
        {
            get
            {
                return woodFractionC;
            }
        }
        //---------------------------------------------------------------------

        public Species.AuxParm<double> WoodFractionN
        {
            get
            {
                return woodFractionN;
            }
        }
        //---------------------------------------------------------------------

        public Species.AuxParm<double> WoodFractionP
        {
            get
            {
                return woodFractionP;
            }
        }
        //---------------------------------------------------------------------

        public Species.AuxParm<double> FRootFractionC
        {
            get
            {
                return fRootFractionC;
            }
        }
        //---------------------------------------------------------------------

        public Species.AuxParm<double> FRootFractionN
        {
            get
            {
                return fRootFractionN;
            }
        }
        //---------------------------------------------------------------------

        public Species.AuxParm<double> FRootFractionP
        {
            get
            {
                return fRootFractionP;
            }
        }
        //---------------------------------------------------------------------

        public Species.AuxParm<double> LitterFractionC
        {
            get
            {
                return litterFractionC;
            }
        }
        //---------------------------------------------------------------------

        public Species.AuxParm<double> LitterFractionN
        {
            get
            {
                return litterFractionN;
            }
        }
        //---------------------------------------------------------------------

        public Species.AuxParm<double> LitterFractionP
        {
            get
            {
                return litterFractionP;
            }
        }
        //---------------------------------------------------------------------

        public Species.AuxParm<double> LeafLignin
        {
            get
            {
                return leafLignin;
            }
        }
        //---------------------------------------------------------------------

        public Species.AuxParm<int> NTolerance
        {
            get
            {
                return nitrogenTolerance;
            }
        }
        //---------------------------------------------------------------------

        public Ecoregions.AuxParm<int> DepositionN
        {
            get
            {
                return depositionN;
            }
        }
        //---------------------------------------------------------------------

        public Ecoregions.AuxParm<int> DepositionP
        {
            get
            {
                return depositionP;
            }
        }
        //---------------------------------------------------------------------

        public Ecoregions.AuxParm<double> DecayRateSOM
        {
            get
            {
                return decayRateSOM;
            }
        }
        //---------------------------------------------------------------------

        public Ecoregions.AuxParm<int> InitialSOMMass
        {
            get
            {
                return initialSOMMass;
            }
        }

        //---------------------------------------------------------------------

        public Ecoregions.AuxParm<int> InitialSOMC
        {
            get
            {
                return initialSOMC;
            }
        }
        //---------------------------------------------------------------------

        public Ecoregions.AuxParm<int> InitialSOMN
        {
            get
            {
                return initialSOMN;
            }
        }

        //---------------------------------------------------------------------

        public Ecoregions.AuxParm<int> InitialSOMP
        {
            get
            {
                return initialSOMP;
            }
        }
        //---------------------------------------------------------------------

        public Ecoregions.AuxParm<double> WeatheringP
        {
            get
            {
                return weatheringP;
            }
        }
        //---------------------------------------------------------------------

        public Ecoregions.AuxParm<int> InitialMineralN
        {
            get
            {
                return initialMineralN;
            }
        }
        //---------------------------------------------------------------------

        public Ecoregions.AuxParm<int> InitialMineralP
        {
            get
            {
                return initialMineralP;
            }
        }
        //---------------------------------------------------------------------

        public Ecoregions.AuxParm<int> AET
        {
            get
            {
                return aet;
            }
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// Definitions of fire severities.
        /// </summary>
        public ISeverity[] FireSeverities
        {
            get
            {
                return severities;
            }
            set
            {
                severities = value;
            }

        }
        //---------------------------------------------------------------------

        public Species.AuxParm<Ecoregions.AuxParm<double>> EstablishProbability
        {
            get
            {
                return establishProbability;
            }
        }

        //---------------------------------------------------------------------

        public Species.AuxParm<Ecoregions.AuxParm<int>> MaxANPP
        {
            get
            {
                return maxANPP;
            }
        }

        //---------------------------------------------------------------------

        public Species.AuxParm<Ecoregions.AuxParm<int>> MaxBiomass
        {
            get
            {
                return maxBiomass;
            }
        }

        //---------------------------------------------------------------------

        public void SetMinRelativeBiomass(byte shadeClass,
                                          IEcoregion ecoregion,
                                          InputValue<Percentage> newValue)
        {
            Debug.Assert(1 <= shadeClass && shadeClass <= 5);
            Debug.Assert(ecoregion != null);
            if (newValue != null)
            {
                if (newValue.Actual < 0.0 || newValue.Actual > 1.0)
                    throw new InputValueException(newValue.String,
                                                  "{0} is not between 0% and 100%", newValue.String);
            }
            MinRelativeBiomass[shadeClass][ecoregion] = newValue;
        }

        //---------------------------------------------------------------------

        public void SetLeafLongevity(ISpecies species,
                                     InputValue<double> newValue)
        {
            Debug.Assert(species != null);
            leafLongevity[species] = CheckBiomassParm(newValue, 1.0, 10.0);
        }

        //---------------------------------------------------------------------

        public void SetWoodyDecayRate(ISpecies species,
                                     InputValue<double> newValue)
        {
            Debug.Assert(species != null);
            WoodyDecayRate[species] = CheckBiomassParm(newValue, 0.0, 1.0);
        }

        //---------------------------------------------------------------------

        public void SetMortCurveShapeParm(ISpecies species,
                                          InputValue<double> newValue)
        {
            Debug.Assert(species != null);
            mortCurveShapeParm[species] = CheckBiomassParm(newValue, 5.0, 25.0);
        }

        //---------------------------------------------------------------------

        public void SetLeafFractionC(ISpecies species,
                                          InputValue<double> newValue)
        {
            Debug.Assert(species != null);
            leafFractionC[species] = CheckBiomassParm(newValue, 0.0, 1.0);
        }
        //---------------------------------------------------------------------

        public void SetLeafFractionN(ISpecies species,
                                          InputValue<double> newValue)
        {
            Debug.Assert(species != null);
            leafFractionN[species] = CheckBiomassParm(newValue, 0.0, 1.0);
        }
        //---------------------------------------------------------------------

        public void SetLeafFractionP(ISpecies species,
                                          InputValue<double> newValue)
        {
            Debug.Assert(species != null);
            leafFractionP[species] = CheckBiomassParm(newValue, 0.0, 1.0);
        }
        //---------------------------------------------------------------------

        public void SetWoodFractionC(ISpecies species,
                                          InputValue<double> newValue)
        {
            Debug.Assert(species != null);
            woodFractionC[species] = CheckBiomassParm(newValue, 0.0, 1.0);
        }
        //---------------------------------------------------------------------

        public void SetWoodFractionN(ISpecies species,
                                          InputValue<double> newValue)
        {
            Debug.Assert(species != null);
            woodFractionN[species] = CheckBiomassParm(newValue, 0.0, 1.0);
        }
        //---------------------------------------------------------------------

        public void SetWoodFractionP(ISpecies species,
                                          InputValue<double> newValue)
        {
            Debug.Assert(species != null);
            woodFractionP[species] = CheckBiomassParm(newValue, 0.0, 1.0);
        }
        //---------------------------------------------------------------------

        public void SetFRootFractionC(ISpecies species,
                                          InputValue<double> newValue)
        {
            Debug.Assert(species != null);
            fRootFractionC[species] = CheckBiomassParm(newValue, 0.0, 1.0);
        }
        //---------------------------------------------------------------------

        public void SetFRootFractionN(ISpecies species,
                                          InputValue<double> newValue)
        {
            Debug.Assert(species != null);
            fRootFractionN[species] = CheckBiomassParm(newValue, 0.0, 1.0);
        }
        //---------------------------------------------------------------------

        public void SetFRootFractionP(ISpecies species,
                                          InputValue<double> newValue)
        {
            Debug.Assert(species != null);
            fRootFractionP[species] = CheckBiomassParm(newValue, 0.0, 1.0);
        }
        //---------------------------------------------------------------------

        public void SetLitterFractionC(ISpecies species,
                                          InputValue<double> newValue)
        {
            Debug.Assert(species != null);
            litterFractionC[species] = CheckBiomassParm(newValue, 0.0, 1.0);
        }
        //---------------------------------------------------------------------

        public void SetLitterFractionN(ISpecies species,
                                          InputValue<double> newValue)
        {
            Debug.Assert(species != null);
            litterFractionN[species] = CheckBiomassParm(newValue, 0.0, 1.0);
        }
        //---------------------------------------------------------------------

        public void SetLitterFractionP(ISpecies species,
                                          InputValue<double> newValue)
        {
            Debug.Assert(species != null);
            litterFractionP[species] = CheckBiomassParm(newValue, 0.0, 1.0);
        }
        //---------------------------------------------------------------------

        public void SetLeafLignin(ISpecies species,
                                          InputValue<double> newValue)
        {
            Debug.Assert(species != null);
            leafLignin[species] = CheckBiomassParm(newValue, 0.0, 1.0);
        }
        //---------------------------------------------------------------------

        public void SetNitrogenTolerance(ISpecies species,
                                          InputValue<int> newValue)
        {
            Debug.Assert(species != null);
            nitrogenTolerance[species] = CheckBiomassParm(newValue, 0, 6);
        }
        //---------------------------------------------------------------------

        public void SetDepositionN(IEcoregion ecoregion,
                                          InputValue<int> newValue)
        {
            Debug.Assert(ecoregion != null);
            depositionN[ecoregion] = CheckBiomassParm(newValue, 0, 30);  //FIXME:  FIND GOOD MAXIMUM
        }
        //---------------------------------------------------------------------

        public void SetDepositionP(IEcoregion ecoregion,
                                          InputValue<int> newValue)
        {
            Debug.Assert(ecoregion != null);
            depositionP[ecoregion] = CheckBiomassParm(newValue, 0, 30);  //FIXME:  FIND GOOD MAXIMUM
        }
        //---------------------------------------------------------------------

        public void SetDecayRateSOM(IEcoregion ecoregion,
                                          InputValue<double> newValue)
        {
            Debug.Assert(ecoregion != null);
            decayRateSOM[ecoregion] = CheckBiomassParm(newValue, 0.0, 1.0);  //FIXME:  FIND GOOD MAXIMUM
        }

        //---------------------------------------------------------------------

        public void SetInitialSOMMass(IEcoregion ecoregion,
                                          InputValue<int> newValue)
        {
            Debug.Assert(ecoregion != null);
            initialSOMMass[ecoregion] = CheckBiomassParm(newValue, 0, 800000);  //FIXME:  FIND GOOD MAXIMUM
        }
        //---------------------------------------------------------------------

        public void SetInitialSOMC(IEcoregion ecoregion,
                                          InputValue<int> newValue)
        {
            Debug.Assert(ecoregion != null);
            initialSOMC[ecoregion] = CheckBiomassParm(newValue, 0, 600000);  //FIXME:  FIND GOOD MAXIMUM
        }

        //---------------------------------------------------------------------

        public void SetInitialSOMN(IEcoregion ecoregion,
                                          InputValue<int> newValue)
        {
            Debug.Assert(ecoregion != null);
            initialSOMN[ecoregion] = CheckBiomassParm(newValue, 0, 600000);  //FIXME:  FIND GOOD MAXIMUM
        }

        //---------------------------------------------------------------------

        public void SetInitialSOMP(IEcoregion ecoregion,
                                          InputValue<int> newValue)
        {
            Debug.Assert(ecoregion != null);
            initialSOMP[ecoregion] = CheckBiomassParm(newValue, 0, 600000);  //FIXME:  FIND GOOD MAXIMUM
        }

        //---------------------------------------------------------------------

        public void SetWeatheringP(IEcoregion ecoregion,
                                          InputValue<double> newValue)
        {
            Debug.Assert(ecoregion != null);
            weatheringP[ecoregion] = CheckBiomassParm(newValue, 0.0, 1.0);  //FIXME:  FIND GOOD MAXIMUM
        }
        //---------------------------------------------------------------------

        public void SetInitialMineralN(IEcoregion ecoregion,
                                          InputValue<int> newValue)
        {
            Debug.Assert(ecoregion != null);
            initialMineralN[ecoregion] = CheckBiomassParm(newValue, 0, 10000);  //FIXME:  FIND GOOD MAXIMUM
        }
        //---------------------------------------------------------------------

        public void SetInitialMineralP(IEcoregion ecoregion,
                                          InputValue<int> newValue)
        {
            Debug.Assert(ecoregion != null);
            initialMineralP[ecoregion] = CheckBiomassParm(newValue, 0, 10000);  //FIXME:  FIND GOOD MAXIMUM
        }
        //---------------------------------------------------------------------

        public void SetAET(IEcoregion ecoregion,
                                          InputValue<int> newValue)
        {
            Debug.Assert(ecoregion != null);
            aet[ecoregion] = CheckBiomassParm(newValue, 0, 10000);  //FIXME:  FIND GOOD MAXIMUM
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// Definitions of fire severities.
        /// </summary>
        /*public ListOfEditable<IEditableSeverity, ISeverity> FireSeverities
        {
            get
            {
                return severities;
            }
            set
            {
                Debug.Assert(severities.Count != 0);
                severities = value;
            }
        }*/

        //---------------------------------------------------------------------

        private InputValue<double> CheckBiomassParm(InputValue<double> newValue,
                                                    double minValue,
                                                    double maxValue)
        {
            if (newValue != null)
            {
                if (newValue.Actual < minValue || newValue.Actual > maxValue)
                    throw new InputValueException(newValue.String,
                                                  "{0} is not between {1:0.0} and {2:0.0}",
                                                  newValue.String, minValue, maxValue);
            }
            return newValue;
        }

        //---------------------------------------------------------------------

        private InputValue<int> CheckBiomassParm(InputValue<int> newValue,
                                                    int minValue,
                                                    int maxValue)
        {
            if (newValue != null)
            {
                if (newValue.Actual < minValue || newValue.Actual > maxValue)
                    throw new InputValueException(newValue.String,
                                                  "{0} is not between {1:0.0} and {2:0.0}",
                                                  newValue.String, minValue, maxValue);
            }
            return newValue;
        }
        //---------------------------------------------------------------------

        public void SetEstablishProbability(ISpecies species,
                                            IEcoregion ecoregion,
                                            InputValue<double> newValue)
        {
            Debug.Assert(species != null);
            Debug.Assert(ecoregion != null);
            establishProbability[species][ecoregion] = CheckBiomassParm(newValue, 0.0, 1.0);
        }

        //---------------------------------------------------------------------

        public void SetMaxANPP(ISpecies species,
                                     IEcoregion ecoregion,
                                     InputValue<int> newValue)
        {
            Debug.Assert(species != null);
            Debug.Assert(ecoregion != null);
            if (newValue != null)
            {
                if (newValue.Actual < 0 || newValue.Actual > 10000)
                    throw new InputValueException(newValue.String,
                                                  "{0} is not between 0 and 10,000",
                                                  newValue.String);
            }
            MaxANPP[species][ecoregion] = newValue;
        }

        //---------------------------------------------------------------------

        public void SetMaxBiomass(ISpecies species,
                                     IEcoregion ecoregion,
                                     InputValue<int> newValue)
        {
            Debug.Assert(species != null);
            Debug.Assert(ecoregion != null);
            if (newValue != null)
            {
                if (newValue.Actual < 0 || newValue.Actual > 700000)
                    throw new InputValueException(newValue.String,
                                                  "{0} is not between 0 and 700,000",
                                                  newValue.String);
            }
            MaxBiomass[species][ecoregion] = newValue;
        }
        //---------------------------------------------------------------------

        public InputParameters(Ecoregions.IDataset ecoregionDataset,
                                  Species.IDataset speciesDataset)
        {
            this.ecoregionDataset = ecoregionDataset;
            this.speciesDataset = speciesDataset;

            MinRelativeBiomass = new Ecoregions.AuxParm<Percentage>[6];
            for (byte shadeClass = 1; shadeClass <= 5; shadeClass++)
            {
                MinRelativeBiomass[shadeClass] = new Ecoregions.AuxParm<Percentage>(ecoregionDataset);
            }

            leafLongevity       = new Species.AuxParm<double>(speciesDataset);
            woodyDecayRate      = new Species.AuxParm<double>(speciesDataset);
            mortCurveShapeParm  = new Species.AuxParm<double>(speciesDataset);
            leafFractionC       = new Species.AuxParm<double>(speciesDataset);
            leafFractionN       = new Species.AuxParm<double>(speciesDataset);
            leafFractionP       = new Species.AuxParm<double>(speciesDataset);
            woodFractionC       = new Species.AuxParm<double>(speciesDataset);
            woodFractionN       = new Species.AuxParm<double>(speciesDataset);
            woodFractionP       = new Species.AuxParm<double>(speciesDataset);
            fRootFractionC      = new Species.AuxParm<double>(speciesDataset);
            fRootFractionN      = new Species.AuxParm<double>(speciesDataset);
            fRootFractionP      = new Species.AuxParm<double>(speciesDataset);
            litterFractionC     = new Species.AuxParm<double>(speciesDataset);
            litterFractionN     = new Species.AuxParm<double>(speciesDataset);
            litterFractionP     = new Species.AuxParm<double>(speciesDataset);
            leafLignin          = new Species.AuxParm<double>(speciesDataset);
            nitrogenTolerance   = new Species.AuxParm<int>(speciesDataset);

            depositionN         = new Ecoregions.AuxParm<int>(ecoregionDataset);
            depositionP         = new Ecoregions.AuxParm<int>(ecoregionDataset);
            decayRateSOM        = new Ecoregions.AuxParm<double>(ecoregionDataset);
            initialSOMMass      = new Ecoregions.AuxParm<int>(ecoregionDataset);
            initialSOMC         = new Ecoregions.AuxParm<int>(ecoregionDataset);
            initialSOMN         = new Ecoregions.AuxParm<int>(ecoregionDataset);
            initialSOMP         = new Ecoregions.AuxParm<int>(ecoregionDataset);
            weatheringP         = new Ecoregions.AuxParm<double>(ecoregionDataset);
            initialMineralN     = new Ecoregions.AuxParm<int>(ecoregionDataset);
            initialMineralP     = new Ecoregions.AuxParm<int>(ecoregionDataset);
            aet                 = new Ecoregions.AuxParm<int>(ecoregionDataset);
            severities          = new ISeverity[5];

            establishProbability    = CreateSpeciesEcoregionParm<double>();
            maxANPP                 = CreateSpeciesEcoregionParm<int>();
            maxBiomass              = CreateSpeciesEcoregionParm<int>();
        }
        //---------------------------------------------------------------------

        private Species.AuxParm<Ecoregions.AuxParm<T>> CreateSpeciesEcoregionParm<T>()
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

/*        public Parameters(Ecoregions.AuxParm<Percentage>[] minRelativeBiomass,
                          Species.AuxParm<double> leafLongevity,
                          Species.AuxParm<double> woodyDecayRate,
                          Species.AuxParm<double> mortCurveShapeParm,
                          Species.AuxParm<double> leafFractionC,
                          Species.AuxParm<double> leafFractionN,
                          Species.AuxParm<double> leafFractionP,
                          Species.AuxParm<double> woodFractionC,
                          Species.AuxParm<double> woodFractionN,
                          Species.AuxParm<double> woodFractionP,
                          Species.AuxParm<double> fRootFractionC,
                          Species.AuxParm<double> fRootFractionN,
                          Species.AuxParm<double> fRootFractionP,
                          Species.AuxParm<double> litterFractionC,
                          Species.AuxParm<double> litterFractionN,
                          Species.AuxParm<double> litterFractionP,
                          Species.AuxParm<double> leafLignin,
                          Species.AuxParm<int> nitrogenTolerance,
                          Ecoregions.AuxParm<int> depositionN,
                          Ecoregions.AuxParm<int> depositionP,
                          Ecoregions.AuxParm<double> decayRateSOM,
                          Ecoregions.AuxParm<int> initialSOMMass,
                          Ecoregions.AuxParm<int> initialSOMC,
                          Ecoregions.AuxParm<int> initialSOMN,
                          Ecoregions.AuxParm<int> initialSOMP,
                          Ecoregions.AuxParm<double> weatheringP,
                          Ecoregions.AuxParm<int> initialMineralN,
                          Ecoregions.AuxParm<int> initialMineralP,
                          Ecoregions.AuxParm<int> aet,
                          //ISeverity[] severities,
                          Species.AuxParm<Ecoregions.AuxParm<double>> establishProbability,
                          Species.AuxParm<Ecoregions.AuxParm<int>> maxANPP,
                          Species.AuxParm<Ecoregions.AuxParm<int>> maxBiomass)
        {
            this.minRelativeBiomass = minRelativeBiomass;
            this.leafLongevity = leafLongevity;
            this.woodyDecayRate = woodyDecayRate;
            this.mortCurveShapeParm = mortCurveShapeParm;
            this.leafFractionC = leafFractionC;
            this.leafFractionN = leafFractionN;
            this.leafFractionP = leafFractionP;
            this.woodFractionC = woodFractionC;
            this.woodFractionN = woodFractionN;
            this.woodFractionP = woodFractionP;
            this.fRootFractionC = fRootFractionC;
            this.fRootFractionN = fRootFractionN;
            this.fRootFractionP = fRootFractionP;
            this.litterFractionC = litterFractionC;
            this.litterFractionN = litterFractionN;
            this.litterFractionP = litterFractionP;
            this.leafLignin = leafLignin;
            this.nitrogenTolerance = nitrogenTolerance;
            this.depositionN = depositionN;
            this.depositionP = depositionP;
            this.decayRateSOM = decayRateSOM;
            this.initialSOMMass = initialSOMMass;
            this.initialSOMC = initialSOMC;
            this.initialSOMN = initialSOMN;
            this.initialSOMP = initialSOMP;
            this.weatheringP = weatheringP;
            this.initialMineralN = initialMineralN;
            this.initialMineralP = initialMineralP;
            this.aet = aet;
            //this.severities = severities;
            this.establishProbability = establishProbability;
            this.maxANPP = maxANPP;
            this.maxBiomass = maxBiomass;
        }*/
    }
}
