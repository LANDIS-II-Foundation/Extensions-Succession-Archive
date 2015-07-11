using Edu.Wisc.Forest.Flel.Util;
using Landis.Biomass.Succession.ClimateChange;
using NUnit.Framework;
using System.Collections.Generic;

using IEcoregion = Landis.Ecoregions.IEcoregion;
using ISpecies = Landis.Species.ISpecies;

namespace Landis.Test.Biomass.Succession.ClimateChange
{
    public abstract class BiomassParametersParser_Tester
    {
        private InputLine inputLine;

        //---------------------------------------------------------------------

        protected InputLine InputLine
        {
            get {
                return inputLine;
            }

            set {
                inputLine = value;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Checks the biomass parameters with the values in a text file.
        /// </summary>
        /// <param name="parameters">
        /// The set of biomass parameters that were read from the text file,
        /// and are to be checked.
        /// </param>
        /// <param name="namesThatFollows">
        /// The names of the parameters or tables that may follow the set of
        /// biomass parameters in the text file.
        /// </param>
        /// <remarks>
        /// The property InputLine has been initialized to an input line in the
        /// text file.
        /// </remarks>
        protected void CheckBiomassParameters(IParameters     parameters,
                                              params string[] namesThatFollows)
        {
            inputLine.MatchName("MinRelativeBiomass");
            inputLine.GetNext();
            List<IEcoregion> ecoregions = ReadEcoregions();
            for (byte shadeClass = 1; shadeClass <= 5; shadeClass++) {
                StringReader currentLine = new StringReader(inputLine.ToString());
                Assert.AreEqual(shadeClass, ReadInputValue<byte>(currentLine));
                foreach (IEcoregion ecoregion in ecoregions)
                    //  TODO: Eventually allow equality testing for Percentage
                    Assert.AreEqual((double) ReadInputValue<Percentage>(currentLine),
                                    (double) parameters.MinRelativeBiomass[shadeClass][ecoregion]);
                inputLine.GetNext();
            }

            inputLine.MatchName("BiomassParameters");
            inputLine.GetNext();
            while (inputLine.VariableName != "EstablishProbabilities") {
                StringReader currentLine = new StringReader(inputLine.ToString());
                ISpecies species = ReadSpecies(currentLine);
                Assert.AreEqual(ReadInputValue<double>(currentLine),
                                parameters.LeafLongevity[species]);
                Assert.AreEqual(ReadInputValue<double>(currentLine),
                                parameters.WoodyDecayRate[species]);
                Assert.AreEqual(ReadInputValue<double>(currentLine),
                                parameters.MortCurveShapeParm[species]);
                inputLine.GetNext();
            }

            CheckParameterTable("EstablishProbabilities",
                                parameters.EstablishProbability,
                                "MaxANPP");

            CheckParameterTable("MaxANPP",
                                parameters.MaxANPP,
                                "LeafLitter:DecayRates");

            CheckParameterTable("LeafLitter:DecayRates",
                                parameters.LeafLitterDecayRate,
                                namesThatFollows);
        }

        //---------------------------------------------------------------------

        protected TValue ReadInputVar<TValue>(string name)
        {
            InputVar<TValue> var = new InputVar<TValue>(name);
            inputLine.ReadVar(var);
            inputLine.GetNext();
            return var.Value.Actual;
        }

        //---------------------------------------------------------------------

        protected TValue ReadInputValue<TValue>(StringReader currentLine)
        {
            InputVar<TValue> var = new InputVar<TValue>("(dummy)");
            var.ReadValue(currentLine);
            return var.Value.Actual;
        }

        //---------------------------------------------------------------------

        private List<IEcoregion> ReadEcoregions()
        {
            List<IEcoregion> ecoregions = new List<IEcoregion>();

            InputVar<string> ecoregionName = new InputVar<string>("Ecoregion");

            StringReader currentLine = new StringReader(inputLine.ToString());
            TextReader.SkipWhitespace(currentLine);
            while (currentLine.Peek() != -1) {
                ecoregionName.ReadValue(currentLine);
                IEcoregion ecoregion = Data.EcoregionDataset[ecoregionName.Value.Actual];
                Assert.IsNotNull(ecoregion);
                ecoregions.Add(ecoregion);
                TextReader.SkipWhitespace(currentLine);
            }
            inputLine.GetNext();
            return ecoregions;
        }

        //---------------------------------------------------------------------

        private ISpecies ReadSpecies(StringReader currentLine)
        {
            InputVar<string> speciesName = new InputVar<string>("Species");
            speciesName.ReadValue(currentLine);
            ISpecies species = Data.SpeciesDataset[speciesName.Value.Actual];
            Assert.IsNotNull(species);
            return species;
        }

        //---------------------------------------------------------------------

        private void CheckParameterTable<TParm>(string                                     tableName,
                                                Species.AuxParm<Ecoregions.AuxParm<TParm>> parmValues,
                                                params string[]                            namesThatFollow)
        {
            inputLine.MatchName(tableName);
            bool haveLine = inputLine.GetNext();

            List<string> namesAfterTable;
            if (namesThatFollow == null)
                namesAfterTable = new List<string>();
            else
                namesAfterTable = new List<string>(namesThatFollow);

            List<IEcoregion> ecoregions = ReadEcoregions();
            while (haveLine && ! namesAfterTable.Contains(inputLine.VariableName)) {
                StringReader currentLine = new StringReader(inputLine.ToString());
                ISpecies species = ReadSpecies(currentLine);
                foreach (IEcoregion ecoregion in ecoregions)
                    Assert.AreEqual(ReadInputValue<TParm>(currentLine),
                                    parmValues[species][ecoregion]);
                haveLine = inputLine.GetNext();
            }
        }
    }
}
