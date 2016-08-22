//  Copyright 2006 University of Wisconsin-Madison
//  Author: Jimm Domingo, FLEL

using Landis.PlugIns;
using System.Collections.Generic;

namespace Landis.Biomass.Succession.AgeOnlyDisturbances
{
    /// <summary>
    /// A table of pool percentages for age-only disturbances.
    /// </summary>
    public class PercentageTable
        : IPercentageTable
    {
        private IDictionary<PlugInType, PoolPercentages> percentages;
        private PoolPercentages defaultPercentages;

        //---------------------------------------------------------------------

        /// <summary>
        /// Gets the pair of percentages for a particular disturbance type.
        /// </summary>
        public PoolPercentages this[PlugInType disturbanceType]
        {
            get {
                PoolPercentages poolPercentages;
                if (percentages.TryGetValue(disturbanceType, out poolPercentages))
                    return poolPercentages;
                return defaultPercentages;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public PercentageTable(IDictionary<PlugInType, PoolPercentages> percentages,
                               PoolPercentages                          defaultPercentages)
        {
            this.percentages = percentages;
            this.defaultPercentages = defaultPercentages;
        }
    }
}
