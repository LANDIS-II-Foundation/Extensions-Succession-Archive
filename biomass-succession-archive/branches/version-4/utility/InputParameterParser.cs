using Edu.Wisc.Forest.Flel.Util;

using Landis.Ecoregions;
using Landis.Species;
using Landis.Succession;

using System.Collections.Generic;

namespace Landis.Biomass.Succession
{
    /// <summary>
    /// A parser that reads biomass succession parameters from text input.
    /// </summary>
    public class InputParametersParser
        : Landis.TextParser<IInputParameters>
    {
        public static class Names
        {
            public const string Timestep = "Timestep";
            public const string SeedingAlgorithm = "SeedingAlgorithm";
            public const string AgeOnlyDisturbanceParms = "AgeOnlyDisturbances:BiomassParameters";
            public const string DynamicInputFile = "DynamicInputFile";
            public const string CalibrateMode = "CalibrateMode";
        }

        //---------------------------------------------------------------------

        private Ecoregions.IDataset ecoregionDataset;
        private Species.IDataset speciesDataset;
        private Dictionary<string, int> speciesLineNums;
        private InputVar<string> speciesName;

        //---------------------------------------------------------------------

        public override string LandisDataValue
        {
            get {
                return "Biomass Succession v3";
            }
        }

        //---------------------------------------------------------------------

        static InputParametersParser()
        {
            SeedingAlgorithmsUtil.RegisterForInputValues();
        }

        //---------------------------------------------------------------------

        public InputParametersParser()
        {
            this.ecoregionDataset = Model.Core.Ecoregions;
            this.speciesDataset = Model.Core.Species;
            this.speciesLineNums = new Dictionary<string, int>();
            this.speciesName = new InputVar<string>("Species");

            //DynamicChange.InputValidation.Initialize(startYear, endYear);
        }

        //---------------------------------------------------------------------

        protected override IInputParameters Parse()
        {
            ReadLandisDataVar();

            InputParameters parameters = new InputParameters(ecoregionDataset, speciesDataset);

            InputVar<int> timestep = new InputVar<int>(Names.Timestep);
            ReadVar(timestep);
            parameters.Timestep = timestep.Value;

            InputVar<SeedingAlgorithms> seedAlg = new InputVar<SeedingAlgorithms>(Names.SeedingAlgorithm);
            ReadVar(seedAlg);
            parameters.SeedAlgorithm = seedAlg.Value;

            InputVar<bool> calimode = new InputVar<bool>(Names.CalibrateMode);
            if(ReadOptionalVar(calimode))
                parameters.CalibrateMode = calimode.Value;
            else
                parameters.CalibrateMode = false;

            InputVar<double> spinMort = new InputVar<double>("SpinupMortalityFraction");
            if(ReadOptionalVar(spinMort))
                parameters.SpinupMortalityFraction = spinMort.Value;
            else
                parameters.SpinupMortalityFraction = 0.0;

            //----------------------------------------------------------
            // ShadeClassTable
            // Read table of min percent sun by shade class
            // Shade classes are in increasing order

            const string PercentSun = "ShadeClassTable";

            ReadName(PercentSun);

            InputVar<double> pctSun1 = new InputVar<double>("Class1");
            InputVar<double> pctSun2 = new InputVar<double>("Class2");
            InputVar<double> pctSun3 = new InputVar<double>("Class3");
            InputVar<double> pctSun4 = new InputVar<double>("Class4");
            InputVar<double> pctSun5 = new InputVar<double>("Class5");

            ReadVar(pctSun1);
            parameters.PctSun1 = pctSun1.Value;
            ReadVar(pctSun2);
            parameters.PctSun2 = pctSun2.Value;
            ReadVar(pctSun3);
            parameters.PctSun3 = pctSun3.Value;
            ReadVar(pctSun4);
            parameters.PctSun4 = pctSun4.Value;
            ReadVar(pctSun5);
            parameters.PctSun5 = pctSun5.Value;

            //----------------------------------------------------------
            //  Read table of sufficient light probabilities.
            //  Shade classes are in increasing order.

            const string SufficientLight = "SufficientLightTable";
            ReadName(SufficientLight);
            const string SpeciesParameters = "SpeciesParameters";


            InputVar<byte> sc = new InputVar<byte>("Shade Class");
            InputVar<double> pl0 = new InputVar<double>("Probability of Germination - Light Level 0");
            InputVar<double> pl1 = new InputVar<double>("Probability of Germination - Light Level 1");
            InputVar<double> pl2 = new InputVar<double>("Probability of Germination - Light Level 2");
            InputVar<double> pl3 = new InputVar<double>("Probability of Germination - Light Level 3");
            InputVar<double> pl4 = new InputVar<double>("Probability of Germination - Light Level 4");
            InputVar<double> pl5 = new InputVar<double>("Probability of Germination - Light Level 5");

            int previousNumber = 0;


            while (! AtEndOfInput && CurrentName != SpeciesParameters
                                  && previousNumber != 6) {
                StringReader currentLine = new StringReader(CurrentLine);

                ISufficientLight suffLight = new SufficientLight();

                parameters.LightClassProbabilities.Add(suffLight);

                ReadValue(sc, currentLine);
                suffLight.ShadeClass = sc.Value;

                //  Check that the current shade class is 1 more than
                //  the previous number (numbers are must be in increasing order).
                if (sc.Value.Actual != (byte) previousNumber + 1)
                    throw new InputValueException(sc.Value.String,
                                                  "Expected the severity number {0}",
                                                  previousNumber + 1);
                previousNumber = (int) sc.Value.Actual;

                ReadValue(pl0, currentLine);
                suffLight.ProbabilityLight0 = pl0.Value;

                ReadValue(pl1, currentLine);
                suffLight.ProbabilityLight1 = pl1.Value;

                ReadValue(pl2, currentLine);
                suffLight.ProbabilityLight2 = pl2.Value;

                ReadValue(pl3, currentLine);
                suffLight.ProbabilityLight3 = pl3.Value;

                ReadValue(pl4, currentLine);
                suffLight.ProbabilityLight4 = pl4.Value;

                ReadValue(pl5, currentLine);
                suffLight.ProbabilityLight5 = pl5.Value;


                CheckNoDataAfter("the " + pl5.Name + " column",
                                 currentLine);
                GetNextLine();
            }
            if (parameters.LightClassProbabilities.Count == 0)
                throw NewParseException("No sufficient light probabilities defined.");
            if (previousNumber != 5)
                throw NewParseException("Expected shade class {0}", previousNumber + 1);

            //-------------------------
            //  SpeciesParameters table

            ReadName("SpeciesParameters");
            const string EcoregionParameters = "EcoregionParameters";

            speciesLineNums.Clear();  //  If parser re-used (i.e., for testing purposes)

            InputVar<double> leafLongevity = new InputVar<double>("Leaf Longevity");
            InputVar<double> woodyDecayRate = new InputVar<double>("Woody Decay Rate");
            InputVar<double> mortCurveShapeParm = new InputVar<double>("Mortality Curve Shape Parameter");
            InputVar<double> growthCurveShapeParm = new InputVar<double>("Mortality Curve Shape Parameter");
            InputVar<double> leafLignin = new InputVar<double>("Leaf Percent Lignin");
            InputVar<double> maxlai = new InputVar<double>("Maximum LAI");
            InputVar<double> lec = new InputVar<double>("Light extinction coefficient");
            InputVar<double> pctBio = new InputVar<double>("Pct Biomass Max LAI");
            //string lastColumn = "the " + mortCurveShapeParm.Name + " column";

            while (! AtEndOfInput && CurrentName != EcoregionParameters) {
                StringReader currentLine = new StringReader(CurrentLine);
                ISpecies species = ReadSpecies(currentLine);

                ReadValue(leafLongevity, currentLine);
                parameters.SetLeafLongevity(species, leafLongevity.Value);

                ReadValue(woodyDecayRate, currentLine);
                parameters.SetWoodyDecayRate(species, woodyDecayRate.Value);

                ReadValue(mortCurveShapeParm, currentLine);
                parameters.SetMortCurveShapeParm(species, mortCurveShapeParm.Value);

                ReadValue(growthCurveShapeParm, currentLine);
                parameters.SetGrowthCurveShapeParm(species, growthCurveShapeParm.Value);

                ReadValue(leafLignin, currentLine);
                parameters.SetLeafLignin(species, leafLignin.Value);

                //ReadValue(btolai, currentLine);
                //parameters.SetBTOLAI(species, btolai.Value);

                //ReadValue(klai, currentLine);
                //parameters.SetKLAI(species, klai.Value);

                ReadValue(maxlai, currentLine);
                parameters.SetMAXLAI(species, maxlai.Value);

                ReadValue(lec, currentLine);
                parameters.SetLightExtinctionCoeff(species, lec.Value);

                ReadValue(pctBio, currentLine);
                parameters.SetPctBioMaxLAI(species, pctBio.Value);

                CheckNoDataAfter(lec.Name, currentLine);
                GetNextLine();
            }

            ReadName("EcoregionParameters");

            InputVar<string> ecoregionName = new InputVar<string>("Ecoregion Name");
            InputVar<int> aet = new InputVar<int>("Actual Evapotranspiration");
            Dictionary <string, int> lineNumbers = new Dictionary<string, int>();

            string lastColumn = "the " + aet.Name + " column";

            while (! AtEndOfInput && CurrentName != Names.DynamicInputFile) {
                StringReader currentLine = new StringReader(CurrentLine);

                ReadValue(ecoregionName, currentLine);

                IEcoregion ecoregion = GetEcoregion(ecoregionName.Value,
                                                    lineNumbers);

                ReadValue(aet, currentLine);
                parameters.SetAET(ecoregion, aet.Value);

                CheckNoDataAfter(lastColumn, currentLine);
                GetNextLine();
            }


            InputVar<string> dynInputFile = new InputVar<string>(Names.DynamicInputFile);
            ReadVar(dynInputFile);
            parameters.DynamicInputFile = dynInputFile.Value;


            //ParseBiomassParameters(parameters, Names.AgeOnlyDisturbanceParms,
            //                                   Names.DynamicChange);

            //  AgeOnlyDisturbances:SpeciesParameters (optional)
            string lastParameter = null;
            if (! AtEndOfInput && CurrentName == Names.AgeOnlyDisturbanceParms) {
                InputVar<string> ageOnlyDisturbanceParms = new InputVar<string>(Names.AgeOnlyDisturbanceParms);
                ReadVar(ageOnlyDisturbanceParms);
                parameters.AgeOnlyDisturbanceParms = ageOnlyDisturbanceParms.Value;

                lastParameter = "the " + Names.AgeOnlyDisturbanceParms + " parameter";
            }

            //  Climate Change table (optional)
            //if (ReadOptionalName(Names.DynamicChange)) {
            //    ReadDynamicChangeTable(parameters.DynamicChangeUpdates);
            //}
            //else
            if (lastParameter != null)
                CheckNoDataAfter(lastParameter);

            return parameters;
        }

        //---------------------------------------------------------------------

/*        protected void ReadDynamicChangeTable(List<DynamicChange.ParametersUpdate> parameterUpdates)
        {
            int? prevYear = null;
            int prevYearLineNum = 0;
            InputVar<int> year = new InputVar<int>("Year", DynamicChange.InputValidation.ReadYear);
            InputVar<string> file = new InputVar<string>("Parameter File");
            while (! AtEndOfInput) {
                StringReader currentLine = new StringReader(CurrentLine);

                ReadValue(year, currentLine);
                if (prevYear.HasValue) {
                    if (year.Value.Actual < prevYear.Value)
                        throw new InputValueException(year.Value.String,
                                                      "Year {0} is before year {1} which was on line {2}",
                                                      year.Value.Actual, prevYear.Value, prevYearLineNum);
                    if (year.Value.Actual == prevYear.Value)
                        throw new InputValueException(year.Value.String,
                                                      "Year {0} was already used on line {1}",
                                                      year.Value.Actual, prevYearLineNum);
                }
                prevYear = year.Value.Actual;
                prevYearLineNum = LineNumber;

                ReadValue(file, currentLine);
                DynamicChange.InputValidation.CheckPath(file.Value);

                CheckNoDataAfter("the " + file + " column", currentLine);
                parameterUpdates.Add(new DynamicChange.ParametersUpdate(year.Value.Actual,
                                                                        file.Value.Actual));
                GetNextLine();
            }
        } */
        //---------------------------------------------------------------------

        /// <summary>
        /// Reads a species name from the current line, and verifies the name.
        /// </summary>
        private ISpecies ReadSpecies(StringReader currentLine)
        {
            ReadValue(speciesName, currentLine);
            Species.ISpecies species = speciesDataset[speciesName.Value.Actual];
            if (species == null)
                throw new InputValueException(speciesName.Value.String,
                                              "{0} is not a species name.",
                                              speciesName.Value.String);
            int lineNumber;
            if (speciesLineNums.TryGetValue(species.Name, out lineNumber))
                throw new InputValueException(speciesName.Value.String,
                                              "The species {0} was previously used on line {1}",
                                              speciesName.Value.String, lineNumber);
            else
                speciesLineNums[species.Name] = LineNumber;
            return species;
        }
        //---------------------------------------------------------------------

        private IEcoregion GetEcoregion(InputValue<string>      ecoregionName,
                                        Dictionary<string, int> lineNumbers)
        {
            IEcoregion ecoregion = ecoregionDataset[ecoregionName.Actual];
            if (ecoregion == null)
                throw new InputValueException(ecoregionName.String,
                                              "{0} is not an ecoregion name.",
                                              ecoregionName.String);
            int lineNumber;
            if (lineNumbers.TryGetValue(ecoregion.Name, out lineNumber))
                throw new InputValueException(ecoregionName.String,
                                              "The ecoregion {0} was previously used on line {1}",
                                              ecoregionName.String, lineNumber);
            else
                lineNumbers[ecoregion.Name] = LineNumber;

            return ecoregion;
        }
    }
}
