//  Copyright 2009 Conservation Biology Institute
//  Authors:  Robert M. Scheller
//  License:  Available at
//  http://www.landis-ii.org/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Util;
using Landis.Util;
using Landis.Ecoregions;
using Landis.Species;

using System.Collections.Generic;
using System.Text;



namespace Landis.Biomass.Succession
{
    /// <summary>
    /// A parser that reads the tool parameters from text input.
    /// </summary>
    public class DynamicInputsParser
        : Landis.TextParser<Dictionary<int, IDynamicInputRecord[,]>>
    {

        private Ecoregions.IDataset ecoregionDataset;
        private Species.IDataset speciesDataset;

        //---------------------------------------------------------------------
        public override string LandisDataValue
        {
            get {
                return "Dynamic Input Data";
            }
        }

        //---------------------------------------------------------------------
        public DynamicInputsParser()
        {
            this.ecoregionDataset = Model.Core.Ecoregions;
            this.speciesDataset = Model.Core.Species;
        }

        //---------------------------------------------------------------------

        protected override Dictionary<int, IDynamicInputRecord[,]> Parse()
        {

            ReadLandisDataVar();

            Dictionary<int, IDynamicInputRecord[,]> allData = new Dictionary<int, IDynamicInputRecord[,]>();

            //const string nextTableName = "DynamicInputTable";


            //---------------------------------------------------------------------
            //Read in climate data:

            //ReadName(nextTableName);

            InputVar<int>    year       = new InputVar<int>("Time step for updating values");
            InputVar<string> ecoregionName = new InputVar<string>("Ecoregion Name");
            InputVar<string> speciesName = new InputVar<string>("Species Name");
            InputVar<double> pest = new InputVar<double>("Probability of Establishment");
            InputVar<int> anpp = new InputVar<int>("ANPP");
            InputVar<int> bmax = new InputVar<int>("Maximum Biomass");

            while (! AtEndOfInput)
            {
                StringReader currentLine = new StringReader(CurrentLine);

                ReadValue(year, currentLine);
                int yr = year.Value.Actual;

                if(!allData.ContainsKey(yr))
                {
                    IDynamicInputRecord[,] inputTable = new IDynamicInputRecord[speciesDataset.Count, ecoregionDataset.Count];
                    allData.Add(yr, inputTable);
                    UI.WriteLine("  Dynamic Input Parser:  Add new year = {0}.", yr);
                }

                ReadValue(ecoregionName, currentLine);

                IEcoregion ecoregion = GetEcoregion(ecoregionName.Value);

                ReadValue(speciesName, currentLine);

                ISpecies species = GetSpecies(speciesName.Value);

                IDynamicInputRecord dynamicInputRecord = new DynamicInputRecord();

                ReadValue(pest, currentLine);
                dynamicInputRecord.ProbEst = pest.Value;

                ReadValue(anpp, currentLine);
                dynamicInputRecord.ANPP_MAX_Spp = anpp.Value;

                ReadValue(bmax, currentLine);
                dynamicInputRecord.B_MAX_Spp = bmax.Value;

                allData[yr][species.Index, ecoregion.Index] = dynamicInputRecord;

                CheckNoDataAfter("the " + bmax.Name + " column",
                                 currentLine);

                GetNextLine();

            }

            return allData;
        }

        //---------------------------------------------------------------------

        private IEcoregion GetEcoregion(InputValue<string>      ecoregionName)
        {
            IEcoregion ecoregion = ecoregionDataset[ecoregionName.Actual];
            if (ecoregion == null)
                throw new InputValueException(ecoregionName.String,
                                              "{0} is not an ecoregion name.",
                                              ecoregionName.String);

            return ecoregion;
        }

        //---------------------------------------------------------------------

        private ISpecies GetSpecies(InputValue<string> speciesName)
        {
            ISpecies species = speciesDataset[speciesName.Actual];
            if (species == null)
                throw new InputValueException(speciesName.String,
                                              "{0} is not a recognized species name.",
                                              speciesName.String);

            return species;
        }


    }
}
