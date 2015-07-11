using Edu.Wisc.Forest.Flel.Util;

using Landis.Ecoregions;
using Landis.Species;

namespace Landis.Extension.Succession.Century.Dynamic
{
    /// <summary>
    /// A parser that reads biomass parameters affected by climate change from
    /// text input.
    /// </summary>
    public class ParametersParser
        : BiomassParametersParser<IParameters>
    {
        private Ecoregions.IDataset ecoregionDataset;
        private Species.IDataset speciesDataset;

        //---------------------------------------------------------------------

        public override string LandisDataValue
        {
            get {
                return "Biomass Succession - Climate Change";
            }
        }

        //---------------------------------------------------------------------

        static ParametersParser()
        {
        }

        //---------------------------------------------------------------------

        public ParametersParser(Ecoregions.IDataset ecoregionDataset,
                                Species.IDataset    speciesDataset)
            : base(ecoregionDataset,
                   speciesDataset)
        {
            this.ecoregionDataset = ecoregionDataset;
            this.speciesDataset = speciesDataset;
        }

        //---------------------------------------------------------------------

        protected override IParameters Parse()
        {
            ReadLandisDataVar();

            Parameters parameters = new Parameters(ecoregionDataset, speciesDataset);
            ParseBiomassParameters(parameters);
            return parameters; //.GetComplete();
        }
    }
}
