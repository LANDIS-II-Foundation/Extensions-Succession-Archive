//  Copyright 2009 Conservation Biology Institute
//  Authors:  Robert M. Scheller
//  License:  Available at  
//  http://www.landis-ii.org/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Util;
using Landis.PlugIns;
using Landis.Util;
using System.Collections.Generic;

namespace Landis.Extension.Succession.Century.AgeOnlyDisturbances
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

            //EditableParameterDataset dataset = new EditableParameterDataset();
            ParameterDataset dataset = new ParameterDataset();
            const string DeadBiomassReductions = "DeadBiomassReductions";
            
            ParseTable(dataset.CohortReductions,
                       "CohortBiomassReductions",
                       DeadBiomassReductions);
            
            ParseTable(dataset.PoolReductions,
                       DeadBiomassReductions,
                       null);

            return dataset; //.GetComplete();
        }

        //---------------------------------------------------------------------

        private void ParseTable(IPercentageTable table,
                                string                  tableName,
                                string                  nextTableName)
        {
            ReadName(tableName);
            
            UI.WriteLine("      Reading {0}.", tableName);

            InputVar<string> disturbance = new InputVar<string>("Disturbance");
            InputVar<Percentage> woodPercentage = new InputVar<Percentage>("Woody");
            InputVar<Percentage> foliarPercentage = new InputVar<Percentage>("Non-Woody");
            string lastColumn = "the " + foliarPercentage.Name + " column";

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

                PoolPercentages percentages;
                if (disturbance.Value.Actual == defaultName) {
                    defaultFound = true;
                    percentages = table.Default;
                }
                else {
                    PlugInType disturbanceType = new PlugInType("disturbance:" + disturbance.Value.Actual);
                    
                    //if(disturbance.Value.Actual == "Fire" || disturbance.Value.Actual == "fire")
                    //    throw new InputValueException(disturbance.Value.Actual,
                    //                              "\"{0}\" is not an allowable disturbance type, line {1}",
                    //                              disturbance.Value.Actual,
                    //                              lineNum);
                    
                    //percentages = table[disturbanceType];
                    table[disturbanceType] = new PoolPercentages();
                    percentages = table[disturbanceType];
                    UI.WriteLine("         Adding {0}...", disturbanceType);
                }

                ReadValue(woodPercentage, currentLine);
                percentages.Wood = woodPercentage.Value;

                ReadValue(foliarPercentage, currentLine);
                percentages.Foliar = foliarPercentage.Value;

                CheckNoDataAfter(lastColumn, currentLine);
                GetNextLine();
            }

            if (! defaultFound)
                throw NewParseException("Missing the \"{0}\" row in the percentage table",
                                        defaultName);
        }
    }
}
