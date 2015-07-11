//  Copyright 2007-2010 Portland State University, University of Wisconsin-Madison
//  Author: Robert Scheller, Ben Sulman

using Landis.Core;
using Landis.SpatialModeling;
using Edu.Wisc.Forest.Flel.Util;

namespace Landis.Extension.Succession.Century.Dynamic
{
    /// <summary>
    /// A parser that reads biomass parameters affected by climate change from
    /// text input.
    /// </summary>
    public class ParametersParser
        : BiomassParametersParser<IParameters>
    {
        private IEcoregionDataset ecoregionDataset;
        private ISpeciesDataset speciesDataset;

        //---------------------------------------------------------------------

        public string LandisDataValue = "Biomass Succession - Climate Change";

        //---------------------------------------------------------------------

        static ParametersParser()
        {
        }

        //---------------------------------------------------------------------

        public ParametersParser()
            //: base(ecoregionDataset,
            //       speciesDataset)
        {
            this.ecoregionDataset = PlugIn.ModelCore.Ecoregions;
            this.speciesDataset = PlugIn.ModelCore.Species;
        }

        //---------------------------------------------------------------------

        protected override IParameters Parse()
        {
            InputVar<string> landisData = new InputVar<string>("LandisData");
            ReadVar(landisData);
            if (landisData.Value.Actual != LandisDataValue)
                throw new InputValueException(landisData.Value.String, "The value is not \"{0}\"", LandisDataValue);

            Parameters parameters = new Parameters(ecoregionDataset, speciesDataset);
            ParseBiomassParameters(parameters);
            return parameters; //.GetComplete();
        }
    }
}
