using Edu.Wisc.Forest.Flel.Util;

using Landis.Ecoregions;
using Landis.Species;

using System.Collections.Generic;

namespace Landis.Biomass.Succession.ClimateChange
{
    /// <summary>
    /// Base class for parsers that read biomass parameters affected by
    /// climate change from text input.
    /// </summary>
    public abstract class BiomassParametersParser<TParseResult>
        : Landis.TextParser<TParseResult>
    {
        private Ecoregions.IDataset ecoregionDataset;
        private Species.IDataset speciesDataset;
        private Dictionary<string, int> speciesLineNums;
        private InputVar<string> speciesName;

        private delegate void SetParmMethod<TParm>(ISpecies          species,
                                                   IEcoregion        ecoregion,
                                                   InputValue<TParm> newValue);

        //---------------------------------------------------------------------

        static BiomassParametersParser()
        {
            //    FIXME: Need to add RegisterForInputValues method to
            //  Percentage class, but for now, we'll trigger it by creating
            //  a local variable of that type.
            Percentage dummy = new Percentage();
        }

        //---------------------------------------------------------------------

        public BiomassParametersParser(Ecoregions.IDataset ecoregionDataset,
                                       Species.IDataset    speciesDataset)
        {
            this.ecoregionDataset = ecoregionDataset;
            this.speciesDataset = speciesDataset;
            this.speciesLineNums = new Dictionary<string, int>();
            this.speciesName = new InputVar<string>("Species");
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Parses the portion of the text input with the biomass parameters.
        /// </summary>
        /// <param name="parameters">
        /// Editable set of parameters that are assigned the values parsed from
        /// the input.
        /// </param>
        /// <param name="namesThatFollows">
        /// The names of the parameters or tables that may follow the biomass
        /// parameters in the text input.
        /// </param>
        protected void ParseBiomassParameters(Parameters parameters,
                                              params string[]    namesThatFollows)
        {
            //--------------------------
            //  MinRelativeBiomass table

            ReadName("MinRelativeBiomass");

            const string SufficientLight = "SufficientLightTable";

            List<IEcoregion> ecoregions = ReadEcoregions();
            string lastEcoregion = ecoregions[ecoregions.Count-1].Name;

            InputVar<byte> shadeClassVar = new InputVar<byte>("Shade Class");
            for (byte shadeClass = 1; shadeClass <= 5; shadeClass++) {
                if (AtEndOfInput)
                    throw NewParseException("Expected a line with shade class {0}", shadeClass);

                StringReader currentLine = new StringReader(CurrentLine);
                ReadValue(shadeClassVar, currentLine);
                if (shadeClassVar.Value.Actual != shadeClass)
                    throw new InputValueException(shadeClassVar.Value.String,
                                                  "Expected the shade class {0}", shadeClass);

                foreach (IEcoregion ecoregion in ecoregions) {
                    InputVar<Percentage> MinRelativeBiomass = new InputVar<Percentage>("Ecoregion " + ecoregion.Name);
                    ReadValue(MinRelativeBiomass, currentLine);
                    parameters.SetMinRelativeBiomass(shadeClass, ecoregion, MinRelativeBiomass.Value);
                }

                CheckNoDataAfter("the Ecoregion " + lastEcoregion + " column",
                                 currentLine);
                GetNextLine();
            }

            //----------------------------------------------------------
            //  Read table of sufficient light probabilities.
            //  Shade classes are in increasing order.
            ReadName(SufficientLight);
            const string BiomassParameters = "BiomassParameters";


            InputVar<byte> sc = new InputVar<byte>("Shade Class");
            InputVar<double> pl0 = new InputVar<double>("Probability of Germination - Light Level 0");
            InputVar<double> pl1 = new InputVar<double>("Probability of Germination - Light Level 1");
            InputVar<double> pl2 = new InputVar<double>("Probability of Germination - Light Level 2");
            InputVar<double> pl3 = new InputVar<double>("Probability of Germination - Light Level 3");
            InputVar<double> pl4 = new InputVar<double>("Probability of Germination - Light Level 4");
            InputVar<double> pl5 = new InputVar<double>("Probability of Germination - Light Level 5");

            int previousNumber = 0;

            ISufficientLight[] sufficientLight = new ISufficientLight[6];


            while (! AtEndOfInput && CurrentName != BiomassParameters
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
            //  BiomassParameters table

            ReadName("BiomassParameters");
            const string EcoregionParameters = "EcoregionParameters";

            speciesLineNums.Clear();  //  If parser re-used (i.e., for testing purposes)

            InputVar<double> leafLongevity = new InputVar<double>("Leaf Longevity");
            InputVar<double> woodyDecayRate = new InputVar<double>("Woody Decay Rate");
            InputVar<double> mortCurveShapeParm = new InputVar<double>("Mortality Curve Shape Parameter");
            InputVar<double> growthCurveShapeParm = new InputVar<double>("Growth Curve Shape Parameter");
            InputVar<double> leafLignin = new InputVar<double>("Leaf Percent Lignin");
            InputVar<double> maxLAI = new InputVar<double>("Maximum Leaf Area Index");
            string lastColumn = "the " + mortCurveShapeParm.Name + " column";

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

                ReadValue(maxLAI, currentLine);
                parameters.SetMaxLAI(species, maxLAI.Value);

                CheckNoDataAfter(lastColumn, currentLine);
                GetNextLine();
            }

            //----------------------------------------------------------
            // First, read table of additional parameters for ecoregions
            ReadName("EcoregionParameters");
            const string EstablishProbabilities = "EstablishProbabilities";

            InputVar<string> ecoregionName = new InputVar<string>("Ecoregion");
            InputVar<int> aet = new InputVar<int>("Actual Evapotranspiration");
            Dictionary <string, int> lineNumbers = new Dictionary<string, int>();

            while (! AtEndOfInput && CurrentName != EstablishProbabilities) {
                StringReader currentLine = new StringReader(CurrentLine);

                ReadValue(ecoregionName, currentLine);

                IEcoregion ecoregion = GetEcoregion(ecoregionName.Value,
                                                    lineNumbers);

                ReadValue(aet, currentLine);
                parameters.SetAET(ecoregion, aet.Value);

                CheckNoDataAfter(lastColumn, currentLine);
                GetNextLine();
            }


            //-------------------------------------------
            //  Tables of [species, ecoregion] parameters
            //
            //      EstablishProbabilities table
            //      MaxANPP table
            //      LeafLitter:DecayRates

            const string MaxANPP = "MaxANPP";
            ReadSpeciesEcoregionParm<double>(EstablishProbabilities,
                                             parameters.SetEstablishProbability,
                                             MaxANPP);

            const string MaxBiomass = "MaxBiomass";
            ReadSpeciesEcoregionParm<int>(MaxANPP,
                                          parameters.SetMaxANPP,
                                          MaxBiomass);


            ReadSpeciesEcoregionParm<int>(MaxBiomass,
                                          parameters.SetMaxBiomass,
                                          namesThatFollows);
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Reads ecoregion names as column headings
        /// </summary>
        private List<IEcoregion> ReadEcoregions()
        {
            if (AtEndOfInput)
                throw NewParseException("Expected a line with the names of 1 or more active ecoregions.");

            InputVar<string> ecoregionName = new InputVar<string>("Ecoregion");
            List<IEcoregion> ecoregions = new List<IEcoregion>();
            StringReader currentLine = new StringReader(CurrentLine);
            TextReader.SkipWhitespace(currentLine);
            while (currentLine.Peek() != -1) {
                ReadValue(ecoregionName, currentLine);
                IEcoregion ecoregion = ecoregionDataset[ecoregionName.Value.Actual];
                if (ecoregion == null)
                    throw new InputValueException(ecoregionName.Value.String,
                                                  "{0} is not an ecoregion name.",
                                                  ecoregionName.Value.String);
                if (! ecoregion.Active)
                    throw new InputValueException(ecoregionName.Value.String,
                                                  "{0} is not an active ecoregion",
                                                  ecoregionName.Value.String);
                if (ecoregions.Contains(ecoregion))
                    throw new InputValueException(ecoregionName.Value.String,
                                                  "The ecoregion {0} appears more than once.",
                                                  ecoregionName.Value.String);
                ecoregions.Add(ecoregion);
                TextReader.SkipWhitespace(currentLine);
            }
            GetNextLine();

            return ecoregions;
        }

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

        /// <summary>
        /// Reads a table for a species parameter that varies by ecoregion.
        /// </summary>
        private void ReadSpeciesEcoregionParm<TParm>(string               tableName,
                                                     SetParmMethod<TParm> setParmMethod,
                                                     params string[]      namesThatFollow)
        {
            ReadName(tableName);
            List<IEcoregion> ecoregions = ReadEcoregions();
            string lastEcoregion = ecoregions[ecoregions.Count-1].Name;

            List<string> namesAfterTable;
            if (namesThatFollow == null)
                namesAfterTable = new List<string>();
            else
                namesAfterTable = new List<string>(namesThatFollow);

            speciesLineNums.Clear();
            while (! AtEndOfInput && ! namesAfterTable.Contains(CurrentName)) {
                StringReader currentLine = new StringReader(CurrentLine);
                ISpecies species = ReadSpecies(currentLine);

                foreach (IEcoregion ecoregion in ecoregions) {
                    InputVar<TParm> parameter = new InputVar<TParm>("Ecoregion " + ecoregion.Name);
                    ReadValue(parameter, currentLine);
                    setParmMethod(species, ecoregion, parameter.Value);
                }

                CheckNoDataAfter("the Ecoregion " + lastEcoregion + " column",
                                 currentLine);
                GetNextLine();
            }
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
