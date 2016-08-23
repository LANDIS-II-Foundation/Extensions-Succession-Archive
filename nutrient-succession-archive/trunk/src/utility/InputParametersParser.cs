//  Copyright 2005 University of Nevada, University of Wisconsin
//  Authors:  Sarah Ganschow, Robert M. Scheller, James B. Domingo
//  License:  Available at
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Util;

using Landis.Ecoregions;
using Landis.Species;
using Landis.Succession;

using System.Collections.Generic;

namespace Landis.Biomass.NuCycling.Succession
{
    /// <summary>
    /// A parser that reads biomass succession parameters from text input.
    /// </summary>
    public class InputParametersParser
        : DynamicChange.BiomassParametersParser<IInputParameters>
    {
        public static class Names
        {
            public const string Timestep = "Timestep";
            public const string SeedingAlgorithm = "SeedingAlgorithm";
            public const string AgeOnlyDisturbanceParms = "AgeOnlyDisturbances:BiomassParameters";
            public const string ClimateChange = "ClimateChange";
        }

        //---------------------------------------------------------------------

        private Ecoregions.IDataset ecoregionDataset;
        private Species.IDataset speciesDataset;

        //---------------------------------------------------------------------

        public override string LandisDataValue
        {
            get
            {
                return "Nutrient Cycling Succession";
            }
        }

        //---------------------------------------------------------------------

        static InputParametersParser()
        {
            SeedingAlgorithmsUtil.RegisterForInputValues();
        }

        //---------------------------------------------------------------------

        public InputParametersParser(Ecoregions.IDataset ecoregionDataset,
                                Species.IDataset speciesDataset,
                                int startYear,
                                int endYear)
            : base(ecoregionDataset,
                   speciesDataset)
        {
            this.ecoregionDataset = ecoregionDataset;
            this.speciesDataset = speciesDataset;

            DynamicChange.InputValidation.Initialize(startYear, endYear);
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

            //----------------------------------------------------------
            //  Read table of sufficient light probabilities.
            //  Shade classes are in increasing order.
            //  Sufficient light values are not temporally dynamic.
            ReadName("SufficientLightTable");

            InputVar<byte> sc = new InputVar<byte>("Shade Class");
            InputVar<double> pl0 = new InputVar<double>("Probability of Germination - Light Level 0");
            InputVar<double> pl1 = new InputVar<double>("Probability of Germination - Light Level 1");
            InputVar<double> pl2 = new InputVar<double>("Probability of Germination - Light Level 2");
            InputVar<double> pl3 = new InputVar<double>("Probability of Germination - Light Level 3");
            InputVar<double> pl4 = new InputVar<double>("Probability of Germination - Light Level 4");
            InputVar<double> pl5 = new InputVar<double>("Probability of Germination - Light Level 5");

            int previousNumber = 0;

            while (!AtEndOfInput && previousNumber != 6 && CurrentName != "MinRelativeBiomass")
            {
                StringReader currentLine = new StringReader(CurrentLine);

                ISufficientLight suffLight = new SufficientLight();

                parameters.LightClassProbabilities.Add(suffLight);

                ReadValue(sc, currentLine);
                suffLight.ShadeClass = sc.Value;

                //  Check that the current shade class is 1 more than
                //  the previous number (numbers are must be in increasing order).
                if (sc.Value.Actual != (byte) previousNumber + 1)
                    throw new InputValueException(sc.Value.String,
                                                  "Expected the shade number {0}",
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

            // ------- These parameters can vary over time --------------------

            ParseBiomassParameters(parameters, Names.AgeOnlyDisturbanceParms,
                                               Names.ClimateChange);

            //  AgeOnlyDisturbances:BiomassParameters (optional)
            string lastParameter = null;
            if (!AtEndOfInput && CurrentName == Names.AgeOnlyDisturbanceParms)
            {
                InputVar<string> ageOnlyDisturbanceParms = new InputVar<string>(Names.AgeOnlyDisturbanceParms);
                ReadVar(ageOnlyDisturbanceParms);
                parameters.AgeOnlyDisturbanceParms = ageOnlyDisturbanceParms.Value;

                lastParameter = "the " + Names.AgeOnlyDisturbanceParms + " parameter";
            }

            //  Climate Change table (optional)
            if (ReadOptionalName(Names.ClimateChange))
            {
                ReadDynamicChangeTable(parameters.DynamicChangeUpdates);
            }
            else if (lastParameter != null)
                CheckNoDataAfter(lastParameter);

            return parameters; //.GetComplete();
        }

        //---------------------------------------------------------------------

        protected void ReadDynamicChangeTable(List<DynamicChange.ParametersUpdate> parameterUpdates)
        {
            int? prevYear = null;
            int prevYearLineNum = 0;
            InputVar<int> year = new InputVar<int>("Year", DynamicChange.InputValidation.ReadYear);
            InputVar<string> file = new InputVar<string>("Parameter File");
            while (!AtEndOfInput)
            {
                StringReader currentLine = new StringReader(CurrentLine);

                ReadValue(year, currentLine);
                if (prevYear.HasValue)
                {
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
        }
    }
}
