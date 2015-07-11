//  Copyright 2006 University of Wisconsin-Madison
//  Author: Jimm Domingo, FLEL

using Landis.PlugIns;

namespace Landis.Biomass.Succession.AgeOnlyDisturbances
{
    /// <summary>
    /// A collection of biomass parameters for age-only disturbances.
    /// </summary>
    public interface IParameterDataset
    {
        /// <summary>
        /// A table of percentages that a disturbance reduce a cohort's
        /// inputs to the dead pools.
        /// </summary>
        IPercentageTable CohortReductions
        {
            get;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// A table of percentages that a disturbances reduces a site's dead
        /// pools.
        /// </summary>
        IPercentageTable PoolReductions
        {
            get;
        }
    }
}
