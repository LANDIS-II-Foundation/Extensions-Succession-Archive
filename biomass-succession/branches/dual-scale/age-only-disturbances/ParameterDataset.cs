//  Copyright 2006 University of Wisconsin-Madison
//  Author: Jimm Domingo, FLEL

namespace Landis.Biomass.Succession.AgeOnlyDisturbances
{
    /// <summary>
    /// A collection of biomass parameters for age-only disturbances.
    /// </summary>
    public class ParameterDataset
        : IParameterDataset
    {
        private IPercentageTable cohortReductions;
        private IPercentageTable poolReductions;

        //---------------------------------------------------------------------

        /// <summary>
        /// A table of percentages that a disturbance reduce a cohort's
        /// inputs to the dead pools.
        /// </summary>
        public IPercentageTable CohortReductions
        {
            get {
                return cohortReductions;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// A table of percentages that a disturbances reduces a site's dead
        /// pools.
        /// </summary>
        public IPercentageTable PoolReductions
        {
            get {
                return poolReductions;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ParameterDataset(IPercentageTable cohortReductions,
                                IPercentageTable poolReductions)
        {
            this.cohortReductions = cohortReductions;
            this.poolReductions = poolReductions;
        }
    }
}
