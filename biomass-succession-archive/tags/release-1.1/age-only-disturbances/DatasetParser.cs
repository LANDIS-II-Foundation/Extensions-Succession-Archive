//  Copyright 2006 University of Wisconsin-Madison
//  Author: Jimm Domingo, FLEL

using Edu.Wisc.Forest.Flel.Util;
using Landis.PlugIns;
using Landis.Util;
using System.Collections.Generic;

namespace Landis.Biomass.Succession.AgeOnlyDisturbances
{
    /// <summary>
    /// A parser that reads a dataset of biomass parameters for age-only
    /// disturbances from text input.
    /// </summary>
    public class DatasetParser
        : Landis.TextParser<IParameterDataset>
    {
        private Dictionary<string, int> lineNums;

        //---------------------------------------------------------------------

        public override string LandisDataValue
        {
            get {
                return "Age-only Disturbances - Biomass Parameters";
            }
        }

        //---------------------------------------------------------------------

        public DatasetParser()
        {
            lineNums = new Dictionary<string, int>();
        }

        //---------------------------------------------------------------------

        protected override IParameterDataset Parse()
        {
            ReadLandisDataVar();

            EditableParameterDataset dataset = new EditableParameterDataset();
            const string DeadPoolReductions = "DeadPoolReductions";
            ParseTable(dataset.CohortReductions,
                       "CohortBiomassReductions",
                       DeadPoolReductions);
            ParseTable(dataset.PoolReductions,
                       DeadPoolReductions,
                       null);

            return dataset.GetComplete();
        }

        //---------------------------------------------------------------------

        private void ParseTable(EditablePercentageTable table,
                                string                  tableName,
                                string                  nextTableName)
        {
            ReadName(tableName);

            InputVar<string> disturbance = new InputVar<string>("Disturbance");
            InputVar<Percentage> woodyPercentage = new InputVar<Percentage>("Woody");
            InputVar<Percentage> nonWoodyPercentage = new InputVar<Percentage>("Non-Woody");
            string lastColumn = "the " + nonWoodyPercentage.Name + " column";

            const string defaultName = "(default)";
            bool defaultFound = false;
            lineNums.Clear();
            while (! AtEndOfInput && CurrentName != nextTableName) {
                StringReader currentLine = new StringReader(CurrentLine);

                ReadValue(disturbance, currentLine);
                int lineNum;
                if (lineNums.TryGetValue(disturbance.Value.Actual, out lineNum))
                    throw new InputValueException(disturbance.Value.Actual,
                                                  "The value \"{0}\" was previously used on line {1}",
                                                  disturbance.Value.Actual,
                                                  lineNum);
                lineNums[disturbance.Value.String] = LineNumber;

                EditablePoolPercentages percentages;
                if (disturbance.Value.Actual == defaultName) {
                    defaultFound = true;
                    percentages = table.Default;
                }
                else {
                    PlugInType disturbanceType = new PlugInType("disturbance:" + disturbance.Value.Actual);
                    percentages = table[disturbanceType];
                }

                ReadValue(woodyPercentage, currentLine);
                percentages.Woody = woodyPercentage.Value;

                ReadValue(nonWoodyPercentage, currentLine);
                percentages.NonWoody = nonWoodyPercentage.Value;

                CheckNoDataAfter(lastColumn, currentLine);
                GetNextLine();
            }

            if (! defaultFound)
                throw NewParseException("Missing the \"{0}\" row in the percentage table",
                                        defaultName);
        }
    }
}
