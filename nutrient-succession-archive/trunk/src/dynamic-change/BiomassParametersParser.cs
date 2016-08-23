using Edu.Wisc.Forest.Flel.Util;

using Landis.Ecoregions;
using Landis.Species;

using System.Collections.Generic;

namespace Landis.Biomass.NuCycling.Succession.DynamicChange
{
    /// <summary>
    /// Base class for parsers that read biomass parameters affected by
    ///   climate change from text input.
    /// </summary>
    public abstract class BiomassParametersParser<TParseResult>
        : Landis.TextParser<TParseResult>
    {
        private Ecoregions.IDataset ecoregionDataset;
        private Species.IDataset speciesDataset;
        private Dictionary<string, int> speciesLineNums;
        private InputVar<string> speciesName;

        private delegate void SetParmMethod<TParm>(ISpecies species,
                                                   IEcoregion ecoregion,
                                                   InputValue<TParm> newValue);

        //---------------------------------------------------------------------

        static BiomassParametersParser()
        {
            //  FIXME: Need to add RegisterForInputValues method to
            //    Percentage class, but for now, we'll trigger it by creating
            //    a local variable of that type.
            Percentage dummy = new Percentage();
        }

        //---------------------------------------------------------------------

        public BiomassParametersParser(Ecoregions.IDataset ecoregionDataset,
                                       Species.IDataset speciesDataset)
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
        ///   the input.
        /// </param>
        /// <param name="namesThatFollows">
        /// The names of the parameters or tables that may follow the biomass
        ///   parameters in the text input.
        /// </param>
        protected void ParseBiomassParameters(InputParameters parameters,
                                              params string[] namesThatFollows)
        {
            //--------------------------
            //  MinRelativeBiomass table

            ReadName("MinRelativeBiomass");
            List<IEcoregion> ecoregions = ReadEcoregions();
            string lastEcoregion = ecoregions[ecoregions.Count - 1].Name;

            InputVar<byte> shadeClassVar = new InputVar<byte>("Shade Class");
            for (byte shadeClass = 1; shadeClass <= 5; shadeClass++)
            {
                if (AtEndOfInput)
                    throw NewParseException("Expected a line with shade class {0}", shadeClass);

                StringReader currentLine = new StringReader(CurrentLine);
                ReadValue(shadeClassVar, currentLine);
                if (shadeClassVar.Value.Actual != shadeClass)
                    throw new InputValueException(shadeClassVar.Value.String,
                                                  "Expected the shade class {0}", shadeClass);

                foreach (IEcoregion ecoregion in ecoregions)
                {
                    InputVar<Percentage> MinRelativeBiomass = new InputVar<Percentage>("Ecoregion " + ecoregion.Name);
                    ReadValue(MinRelativeBiomass, currentLine);
                    parameters.SetMinRelativeBiomass(shadeClass, ecoregion, MinRelativeBiomass.Value);
                }

                CheckNoDataAfter("the Ecoregion " + lastEcoregion + " column",
                                 currentLine);
                GetNextLine();
            }

            //-------------------------
            //  BiomassParameters table

            ReadName("BiomassParameters");
            const string EcoregionParameters = "EcoregionParameters";

            speciesLineNums.Clear();  //  If parser re-used (i.e., for testing purposes)

            InputVar<double> leafLongevity = new InputVar<double>("Leaf Longevity");
            InputVar<double> woodyDecayRate = new InputVar<double>("Woody Decay Rate");
            InputVar<double> mortCurveShapeParm = new InputVar<double>("Mortality Curve Shape Parameter");
            InputVar<double> leafLignin = new InputVar<double>("Leaf Percent Lignin");
            InputVar<int> ntolerance = new InputVar<int>("Nitrogen Tolerance");
            InputVar<double> leafFractionC = new InputVar<double>("Carbon Leaf Fraction");
            InputVar<double> leafFractionN = new InputVar<double>("Nitrogen Leaf Fraction");
            InputVar<double> leafFractionP = new InputVar<double>("Phosphorus Leaf Fraction");
            InputVar<double> litterFractionC = new InputVar<double>("Carbon Litter Fraction");
            InputVar<double> litterFractionN = new InputVar<double>("Nitrogen Litter Fraction");
            InputVar<double> litterFractionP = new InputVar<double>("Phosphorus Litter Fraction");
            InputVar<double> woodFractionC = new InputVar<double>("Carbon Wood Fraction");
            InputVar<double> woodFractionN = new InputVar<double>("Nitrogen Wood Fraction");
            InputVar<double> woodFractionP = new InputVar<double>("Phosphorus Wood Fraction");
            InputVar<double> fRootFractionC = new InputVar<double>("Carbon Fine Root Fraction");
            InputVar<double> fRootFractionN = new InputVar<double>("Nitrogen Fine Root Fraction");
            InputVar<double> fRootFractionP = new InputVar<double>("Phosphorus Fine Root Fraction");
            string lastColumn = "the " + fRootFractionP.Name + " column";

            while (!AtEndOfInput && CurrentName != EcoregionParameters)
            {
                StringReader currentLine = new StringReader(CurrentLine);
                ISpecies species = ReadSpecies(currentLine);

                ReadValue(leafLongevity, currentLine);
                parameters.SetLeafLongevity(species, leafLongevity.Value);

                ReadValue(woodyDecayRate, currentLine);
                parameters.SetWoodyDecayRate(species, woodyDecayRate.Value);

                ReadValue(mortCurveShapeParm, currentLine);
                parameters.SetMortCurveShapeParm(species, mortCurveShapeParm.Value);

                ReadValue(leafLignin, currentLine);
                parameters.SetLeafLignin(species, leafLignin.Value);

                ReadValue(ntolerance, currentLine);
                parameters.SetNitrogenTolerance(species, ntolerance.Value);

                ReadValue(leafFractionC, currentLine);
                parameters.SetLeafFractionC(species, leafFractionC.Value);

                ReadValue(leafFractionN, currentLine);
                parameters.SetLeafFractionN(species, leafFractionN.Value);

                ReadValue(leafFractionP, currentLine);
                parameters.SetLeafFractionP(species, leafFractionP.Value);

                ReadValue(litterFractionC, currentLine);
                parameters.SetLitterFractionC(species, litterFractionC.Value);

                ReadValue(litterFractionN, currentLine);
                parameters.SetLitterFractionN(species, litterFractionN.Value);

                ReadValue(litterFractionP, currentLine);
                parameters.SetLitterFractionP(species, litterFractionP.Value);

                ReadValue(woodFractionC, currentLine);
                parameters.SetWoodFractionC(species, woodFractionC.Value);

                ReadValue(woodFractionN, currentLine);
                parameters.SetWoodFractionN(species, woodFractionN.Value);

                ReadValue(woodFractionP, currentLine);
                parameters.SetWoodFractionP(species, woodFractionP.Value);

                ReadValue(fRootFractionC, currentLine);
                parameters.SetFRootFractionC(species, fRootFractionC.Value);

                ReadValue(fRootFractionN, currentLine);
                parameters.SetFRootFractionN(species, fRootFractionN.Value);

                ReadValue(fRootFractionP, currentLine);
                parameters.SetFRootFractionP(species, fRootFractionP.Value);

                CheckNoDataAfter(lastColumn, currentLine);
                GetNextLine();
            }
            //----------------------------------------------------------
            // First, read table of additional parameters for ecoregions
            ReadName("EcoregionParameters");
            const string FireSeverities = "FireSeverityTable";

            InputVar<string> ecoregionName = new InputVar<string>("Ecoregion");
            InputVar<int> depositionN = new InputVar<int>("Nitrogen Deposition");
            InputVar<int> depositionP = new InputVar<int>("Phosphorus Deposition");
            InputVar<double> decayRateSOM = new InputVar<double>("Decay Rate of Soil Organic Matter");
            InputVar<int> initialSOMMass = new InputVar<int>("Initial Soil Organic Matter Mass");
            InputVar<int> initialSOMC = new InputVar<int>("Initial Soil Organic Matter C");
            InputVar<int> initialSOMN = new InputVar<int>("Initial Soil Organic Matter N");
            InputVar<int> initialSOMP = new InputVar<int>("Initial Soil Organic Matter P");
            InputVar<double> weatheringP = new InputVar<double>("Weathering Rate of Phosphorus from Mineral Soil");
            InputVar<int> initMinN = new InputVar<int>("Initial Mineral N");
            InputVar<int> initMinP = new InputVar<int>("Initial Mineral P");
            InputVar<int> aet = new InputVar<int>("Actual Evapotranspiration");
            lastColumn = "the " + aet.Name + " column";

            Dictionary<string, int> lineNumbers = new Dictionary<string, int>();

            while (!AtEndOfInput && (CurrentName != FireSeverities))
            {
                StringReader currentLine = new StringReader(CurrentLine);

                ReadValue(ecoregionName, currentLine);

                IEcoregion ecoregion = GetEcoregion(ecoregionName.Value,
                                                    lineNumbers);

                ReadValue(depositionN, currentLine);
                parameters.SetDepositionN(ecoregion, depositionN.Value);

                ReadValue(depositionP, currentLine);
                parameters.SetDepositionP(ecoregion, depositionP.Value);

                ReadValue(decayRateSOM, currentLine);
                parameters.SetDecayRateSOM(ecoregion, decayRateSOM.Value);

                ReadValue(initialSOMMass, currentLine);
                parameters.SetInitialSOMMass(ecoregion, initialSOMMass.Value);

                ReadValue(initialSOMC, currentLine);
                parameters.SetInitialSOMC(ecoregion, initialSOMC.Value);

                ReadValue(initialSOMN, currentLine);
                parameters.SetInitialSOMN(ecoregion, initialSOMN.Value);

                ReadValue(initialSOMP, currentLine);
                parameters.SetInitialSOMP(ecoregion, initialSOMP.Value);

                ReadValue(weatheringP, currentLine);
                parameters.SetWeatheringP(ecoregion, weatheringP.Value);

                ReadValue(initMinN, currentLine);
                parameters.SetInitialMineralN(ecoregion, initMinN.Value);

                ReadValue(initMinP, currentLine);
                parameters.SetInitialMineralP(ecoregion, initMinP.Value);

                ReadValue(aet, currentLine);
                parameters.SetAET(ecoregion, aet.Value);

                CheckNoDataAfter(lastColumn, currentLine);
                GetNextLine();
            }

            //Read table of fire severities.
            //  Severities are in decreasing order.
            ReadName(FireSeverities);
            const string EstablishProbabilities = "EstablishProbabilities";

            InputVar<byte> number = new InputVar<byte>("Severity Number");
            InputVar<double> lr = new InputVar<double>("Litter Reduction");
            InputVar<double> wdr = new InputVar<double>("Woody Debris Reduction");

            byte previousNumber = 6;

            while (!AtEndOfInput && CurrentName != EstablishProbabilities
                                  && previousNumber != 1)
            {
                StringReader currentLine = new StringReader(CurrentLine);

                ISeverity severity = new Severity();


                ReadValue(number, currentLine);
                //severity.Number = number.Value;

                //Check that the current severity's number is 1 less than
                //  the previous number (numbers are must be in decreasing
                //  order).
                if (number.Value.Actual != previousNumber - 1)
                    throw new InputValueException(number.Value.String,
                                                  "Expected the severity number {0}",
                                                  previousNumber - 1);
                previousNumber = number.Value.Actual;

                ReadValue(lr, currentLine);
                severity.LitterReduction = lr.Value;

                ReadValue(wdr, currentLine);
                severity.WoodyDebrisReduction = wdr.Value;

                //SetFireSeverities
                parameters.FireSeverities[number.Value - 1] = severity;

                CheckNoDataAfter("the " + wdr.Name + " column",
                                 currentLine);
                GetNextLine();
            }
            //if (parameters.FireSeverities.Count == 0)
            //    throw NewParseException("No severities defined.");
            if (previousNumber != 1)
                throw NewParseException("Expected fire severity {0}", previousNumber - 1);

            //-------------------------------------------
            //Tables of [species, ecoregion] parameters
            //
            //    EstablishProbabilities table
            //    MaxANPP table
            //    MaxBiomass table
            //    LeafLitter:DecayRates

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
            while (currentLine.Peek() != -1)
            {
                ReadValue(ecoregionName, currentLine);
                IEcoregion ecoregion = ecoregionDataset[ecoregionName.Value.Actual];
                if (ecoregion == null)
                    throw new InputValueException(ecoregionName.Value.String,
                                                  "{0} is not an ecoregion name.",
                                                  ecoregionName.Value.String);
                if (!ecoregion.Active)
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
        private void ReadSpeciesEcoregionParm<TParm>(string tableName,
                                                     SetParmMethod<TParm> setParmMethod,
                                                     params string[] namesThatFollow)
        {
            ReadName(tableName);
            List<IEcoregion> ecoregions = ReadEcoregions();
            string lastEcoregion = ecoregions[ecoregions.Count - 1].Name;

            List<string> namesAfterTable;
            if (namesThatFollow == null)
                namesAfterTable = new List<string>();
            else
                namesAfterTable = new List<string>(namesThatFollow);

            speciesLineNums.Clear();
            while (!AtEndOfInput && !namesAfterTable.Contains(CurrentName))
            {
                StringReader currentLine = new StringReader(CurrentLine);
                ISpecies species = ReadSpecies(currentLine);

                foreach (IEcoregion ecoregion in ecoregions)
                {
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

        private IEcoregion GetEcoregion(InputValue<string> ecoregionName,
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
