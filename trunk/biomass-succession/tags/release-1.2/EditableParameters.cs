using Edu.Wisc.Forest.Flel.Util;

using Landis.Ecoregions;
using Landis.Species;
using Landis.Succession;

using System.Collections.Generic;

namespace Landis.Biomass.Succession
{
    /// <summary>
    /// Editable set of parameters for biomass succession.
    /// </summary>
    public class EditableParameters
        : ClimateChange.EditableParameters, IEditable<IParameters>
    {
        private InputValue<int> timestep;
        private InputValue<SeedingAlgorithms> seedAlg;
        private InputValue<string> ageOnlyDisturbanceParms;
        private List<ClimateChange.ParametersUpdate> climateChangeUpdates;

        //---------------------------------------------------------------------

        /// <summary>
        /// Timestep (years)
        /// </summary>
        public InputValue<int> Timestep
        {
            get {
                return timestep;
            }

            set {
                if (value != null) {
                    if (value.Actual < 0)
                        throw new InputValueException(value.String,
                                                      "Timestep must be > or = 0");
                }
                timestep = value;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Seeding algorithm
        /// </summary>
        public InputValue<SeedingAlgorithms> SeedAlgorithm
        {
            get {
                return seedAlg;
            }

            set {
                seedAlg = value;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Path to the optional file with the biomass parameters for age-only
        /// disturbances.
        /// </summary>
        public InputValue<string> AgeOnlyDisturbanceParms
        {
            get {
                return ageOnlyDisturbanceParms;
            }

            set {
                if (value != null) {
                    string path = value.Actual;
                    if (path.Trim(null).Length == 0)
                        throw new InputValueException(path,
                                                      "\"{0}\" is not a valid path.",
                                                      path);
                }
                ageOnlyDisturbanceParms = value;
            }
        }

        //---------------------------------------------------------------------

        public List<ClimateChange.ParametersUpdate> ClimateChangeUpdates
        {
            get {
                return climateChangeUpdates;
            }
        }

        //---------------------------------------------------------------------

        public override bool IsComplete
        {
            get {
                object[] parameters = new object[]{ timestep,
                                                    seedAlg };
                foreach (object parameter in parameters)
                    if (parameter == null)
                        return false;
                return base.IsComplete;
            }
        }

        //---------------------------------------------------------------------

        public EditableParameters(Ecoregions.IDataset ecoregionDataset,
                                  Species.IDataset    speciesDataset)
            : base(ecoregionDataset,
                   speciesDataset)
        {
            this.climateChangeUpdates = new List<ClimateChange.ParametersUpdate>();
        }

        //---------------------------------------------------------------------

        public new IParameters GetComplete()
        {
            if (this.IsComplete) {
                string ageOnlyDistParmFile = null;
                if (ageOnlyDisturbanceParms != null)
                    ageOnlyDistParmFile = ageOnlyDisturbanceParms.Actual;

                ClimateChange.IParameters climateChangeParams = base.GetComplete();
                return new Parameters(timestep.Actual,
                                      seedAlg.Actual,
                                      climateChangeParams.MinRelativeBiomass,
                                      climateChangeParams.LeafLongevity,
                                      climateChangeParams.WoodyDecayRate,
                                      climateChangeParams.MortCurveShapeParm,
                                      climateChangeParams.EstablishProbability,
                                      climateChangeParams.MaxANPP,
                                      climateChangeParams.LeafLitterDecayRate,
                                      ageOnlyDistParmFile,
                                      climateChangeUpdates);
            }
            else
                return null;
        }
    }
}
