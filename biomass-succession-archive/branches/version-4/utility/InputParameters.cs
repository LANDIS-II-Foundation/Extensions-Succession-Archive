using Edu.Wisc.Forest.Flel.Util;

using Landis.Ecoregions;
using Landis.Species;
using Landis.Succession;

using System.Collections.Generic;
using System.Diagnostics;

namespace Landis.Biomass.Succession
{
    /// <summary>
    /// The parameters for biomass succession.
    /// </summary>
    public interface IInputParameters
    {
        int Timestep {get; set;}
        SeedingAlgorithms SeedAlgorithm {get; set;}
        bool CalibrateMode {get; set;}
        double SpinupMortalityFraction {get; set;}
        List<ISufficientLight> LightClassProbabilities {get; set;}

        Species.AuxParm<double> LeafLongevity {get; }
        Species.AuxParm<double> WoodyDecayRate {get; }
        Species.AuxParm<double> MortCurveShapeParm {get; }
        Species.AuxParm<double> GrowthCurveShapeParm { get; }
        Species.AuxParm<double> LeafLignin { get; }
        //Species.AuxParm<double> BTOLAI {get;}
        //Species.AuxParm<int> KLAI {get;}
        Species.AuxParm<double> MAXLAI {get;}
        Species.AuxParm<double> LightExtinctionCoeff {get;}
        Species.AuxParm<double> PctBioMaxLAI { get;}

        Ecoregions.AuxParm<int> AET {get;}

        double PctSun1 { get; set;}
        double PctSun2 { get; set;}
        double PctSun3 { get; set;}
        double PctSun4 { get; set;}
        double PctSun5 { get; set;}

        string DynamicInputFile {get;set;}

        string AgeOnlyDisturbanceParms{get; set;}
        //List<DynamicChange.ParametersUpdate> DynamicChangeUpdates {get; }
    }

    /// <summary>
    /// The parameters for biomass succession.
    /// </summary>
    public class InputParameters
        : IInputParameters //DynamicChange.Parameters, IParameters
    {
        private int timestep;
        private SeedingAlgorithms seedAlg;
        private bool calibrateMode;
        private double spinupMortalityFraction;
        private double pctSun1;
        private double pctSun2;
        private double pctSun3;
        private double pctSun4;
        private double pctSun5;

        private List<ISufficientLight> sufficientLight;

       //private Species.IDataset speciesDataset;
        private Species.AuxParm<double> leafLongevity;
        private Species.AuxParm<double> woodyDecayRate;
        private Species.AuxParm<double> mortCurveShapeParm;
        private Species.AuxParm<double> growthCurveShapeParm;
        private Species.AuxParm<double> leafLignin;
        //private Species.AuxParm<double> btoLAI;
        //private Species.AuxParm<int> kLAI;
        private Species.AuxParm<double> maxLAI;
        private Species.AuxParm<double> lightExtinctionCoeff;
        private Species.AuxParm<double> pctBioMaxLAI;
        private Ecoregions.AuxParm<int> aet;


        private string dynamicInputFile;
        private string ageOnlyDisturbanceParms;

        //private List<DynamicChange.ParametersUpdate> dynamicChangeUpdates;

        //---------------------------------------------------------------------
        /// <summary>
        /// Timestep (years)
        /// </summary>

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

        /// <summary>
        /// Seeding algorithm
        /// </summary>
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
        public bool CalibrateMode
        {
            get {
                return calibrateMode;
            }
            set {
                calibrateMode = value;
            }
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Background mortality rates applied during the biomass spin-up phase.
        /// This represents background disturbance before year 1.
        /// </summary>

        public double SpinupMortalityFraction
        {
            get {
                return spinupMortalityFraction;
            }
            set {
                if (value < 0.0 || value > 0.5)
                        throw new InputValueException(value.ToString(), "SpinupMortalityFraction must be > 0.0 and < 0.5");
                spinupMortalityFraction = value;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Min Pct Sun Shade Class 1
        /// </summary>
        public double PctSun1
        {
            get
            {
                return pctSun1;
            }
            set
            {
                pctSun1 = value;
            }
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Min Pct Sun Shade Class 2
        /// </summary>
        public double PctSun2
        {
            get
            {
                return pctSun2;
            }
            set
            {
                pctSun2 = value;
            }
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Min Pct Sun Shade Class 3
        /// </summary>
        public double PctSun3
        {
            get
            {
                return pctSun3;
            }
            set
            {
                pctSun3 = value;
            }
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Min Pct Sun Shade Class 4
        /// </summary>
        public double PctSun4
        {
            get
            {
                return pctSun4;
            }
            set
            {
                pctSun4 = value;
            }
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Min Pct Sun Shade Class 5
        /// </summary>
        public double PctSun5
        {
            get
            {
                return pctSun5;
            }
            set
            {
                pctSun5 = value;
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
            set
            {
                Debug.Assert(sufficientLight.Count != 0);
                sufficientLight = value;
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
            get
            {
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
        /* No longer used
        public Species.AuxParm<int> KLAI
        {
            get {
                return kLAI;
            }
        }
        */
        //---------------------------------------------------------------------
        /* No longer used
        public Species.AuxParm<double> BTOLAI
        {
            get {
                return btoLAI;
            }
        }
        */
        //---------------------------------------------------------------------

        public Species.AuxParm<double> MAXLAI
        {
            get {
                return maxLAI;
            }
        }
        //---------------------------------------------------------------------

        public Species.AuxParm<double> LightExtinctionCoeff
        {
            get {
                return lightExtinctionCoeff;
            }
        }
        //---------------------------------------------------------------------

        public Species.AuxParm<double> PctBioMaxLAI
        {
            get
            {
                return pctBioMaxLAI;
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
        /// Input file for the dynamic inputs
        /// </summary>
        public string DynamicInputFile
        {
            get
            {
                return dynamicInputFile;
            }
            set
            {
                dynamicInputFile = value;
            }
        }
        //---------------------------------------------------------------------

        public void SetLeafLongevity(ISpecies           species,
                                     InputValue<double> newValue)
        {
            Debug.Assert(species != null);
            leafLongevity[species] = Util.CheckBiomassParm(newValue, 1.0, 10.0);
        }

        //---------------------------------------------------------------------

        public void SetWoodyDecayRate(ISpecies           species,
                                     InputValue<double> newValue)
        {
            Debug.Assert(species != null);
            woodyDecayRate[species] = Util.CheckBiomassParm(newValue, 0.0, 1.0);
        }

        //---------------------------------------------------------------------

        public void SetMortCurveShapeParm(ISpecies           species,
                                          InputValue<double> newValue)
        {
            Debug.Assert(species != null);
            mortCurveShapeParm[species] = Util.CheckBiomassParm(newValue, 5.0, 25.0);
        }

        //---------------------------------------------------------------------

        public void SetGrowthCurveShapeParm(ISpecies species,
                                          InputValue<double> newValue)
        {
            Debug.Assert(species != null);
            growthCurveShapeParm[species] = Util.CheckBiomassParm(newValue, 0.0, 1.0);
        }
        //---------------------------------------------------------------------

        public void SetLeafLignin(ISpecies           species,
                                          InputValue<double> newValue)
        {
            Debug.Assert(species != null);
            leafLignin[species] = Util.CheckBiomassParm(newValue, 0.0, 0.4);
        }
        //---------------------------------------------------------------------

        public void SetMAXLAI(ISpecies species, InputValue<double> newValue)
        {
            Debug.Assert(species != null);
            maxLAI[species] = Util.CheckBiomassParm(newValue, 0.0, 30.0);
        }
        //---------------------------------------------------------------------

        public void SetLightExtinctionCoeff(ISpecies species, InputValue<double> newValue)
        {
            Debug.Assert(species != null);
            lightExtinctionCoeff[species] = Util.CheckBiomassParm(newValue, 0.0, 1.0);
        }
        //---------------------------------------------------------------------


        public void SetPctBioMaxLAI(ISpecies species, InputValue<double> newValue)
        {
            Debug.Assert(species != null);
            pctBioMaxLAI[species] = Util.CheckBiomassParm(newValue, 0.0, 100.0);
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Path to the optional file with the biomass parameters for age-only
        /// disturbances.
        /// </summary>
        public string AgeOnlyDisturbanceParms
        {
            get {
                return ageOnlyDisturbanceParms;
            }
            set {
                string path = value;
                if (path.Trim(null).Length == 0)
                    throw new InputValueException(path, "\"{0}\" is not a valid path.", path);
                ageOnlyDisturbanceParms = value;
            }
        }

        //---------------------------------------------------------------------

        public void SetAET(IEcoregion           ecoregion,
                                          InputValue<int> newValue)
        {
            Debug.Assert(ecoregion != null);
            aet[ecoregion] = Util.CheckBiomassParm(newValue, 0, 10000);  //FIXME:  FIND GOOD MAXIMUM
        }
        //---------------------------------------------------------------------

/*        public List<DynamicChange.ParametersUpdate> DynamicChangeUpdates
        {
            get {
                return dynamicChangeUpdates;
            }
        }*/
        //---------------------------------------------------------------------

        public InputParameters(Ecoregions.IDataset ecoregionDataset,
                                  Species.IDataset    speciesDataset)
       //     : base(ecoregionDataset,
       //            speciesDataset)
        {
            sufficientLight = new List<ISufficientLight>();
            pctSun1 = new double();
            pctSun2 = new double();
            pctSun3 = new double();
            pctSun4 = new double();
            pctSun5 = new double();
            leafLongevity       = new Species.AuxParm<double>(speciesDataset);
            woodyDecayRate      = new Species.AuxParm<double>(speciesDataset);
            mortCurveShapeParm  = new Species.AuxParm<double>(speciesDataset);
            growthCurveShapeParm = new Species.AuxParm<double>(speciesDataset);
            leafLignin = new Species.AuxParm<double>(speciesDataset);
            maxLAI              = new Species.AuxParm<double>(speciesDataset);
            lightExtinctionCoeff= new Species.AuxParm<double>(speciesDataset);
            pctBioMaxLAI        = new Species.AuxParm<double>(speciesDataset);
            aet                 = new Ecoregions.AuxParm<int>(ecoregionDataset);
            //this.dynamicChangeUpdates = new List<DynamicChange.ParametersUpdate>();
        }

    }
}
