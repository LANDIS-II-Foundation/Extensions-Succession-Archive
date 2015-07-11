using Edu.Wisc.Forest.Flel.Util;

using Landis.Ecoregions;
using Landis.Species;

using System.Collections.Generic;

namespace Landis.Extension.Succession.Century.Dynamic
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


            //----------------------------------------------------------
            // First, read table of additional parameters for ecoregions
            //ReadName("DynamicEcoregionParameters");
            //const string EstablishProbabilities = "EstablishProbabilities";


            //-------------------------------------------
            //  Tables of [species, ecoregion] parameters
            //
            //      EstablishProbabilities table
            //      MaxANPP table

            const string MaxANPP = "MonthlyMaxNPP";

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
    }
}
