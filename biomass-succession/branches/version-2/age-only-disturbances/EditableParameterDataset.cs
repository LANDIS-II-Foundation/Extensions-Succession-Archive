//  Copyright 2006 University of Wisconsin-Madison
//  Author: Jimm Domingo, FLEL

using Edu.Wisc.Forest.Flel.Util;

namespace Landis.Biomass.Succession.AgeOnlyDisturbances
{
    /// <summary>
    /// A editable collection of biomass parameters for age-only disturbances.
    /// </summary>
    public class EditableParameterDataset
        : IEditable<IParameterDataset>
    {
        private EditablePercentageTable cohortReductions;
        private EditablePercentageTable poolReductions;

        //---------------------------------------------------------------------

        /// <summary>
        /// An editable table of percentages that a disturbance reduce a
        /// cohort's inputs to the dead pools.
        /// </summary>
        public EditablePercentageTable CohortReductions
        {
            get {
                return cohortReductions;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// An editable table of percentages that a disturbances reduces a
        /// site's dead pools.
        /// </summary>
        public EditablePercentageTable PoolReductions
        {
            get {
                return poolReductions;
            }
        }

        //---------------------------------------------------------------------

        public bool IsComplete
        {
            get {
                return cohortReductions.IsComplete && poolReductions.IsComplete;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public EditableParameterDataset()
        {
            cohortReductions = new EditablePercentageTable();
            poolReductions = new EditablePercentageTable();
        }

        //---------------------------------------------------------------------

        public IParameterDataset GetComplete()
        {
            if (IsComplete)
                return new ParameterDataset(cohortReductions.GetComplete(),
                                            poolReductions.GetComplete());
            else
                return null;
        }
    }
}
