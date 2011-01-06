//  Copyright 2006 University of Wisconsin-Madison
//  Author: Jimm Domingo, FLEL

using Edu.Wisc.Forest.Flel.Util;

namespace Landis.Biomass.Succession.AgeOnlyDisturbances
{
    /// <summary>
    /// A pair of percentage parameters for the two dead pools.
    /// </summary>
    public class PoolPercentages
    {
        private Percentage woody;
        private Percentage nonWoody;

        //---------------------------------------------------------------------

        /// <summary>
        /// The percentage associated with the dead woody pool.
        /// </summary>
        public Percentage Woody
        {
            get {
                return woody;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The percentage associated with the dead non-woody pool.
        /// pool.
        /// </summary>
        public Percentage NonWoody
        {
            get {
                return nonWoody;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public PoolPercentages(Percentage woody,
                               Percentage nonWoody)
        {
            this.woody = woody;
            this.nonWoody = nonWoody;
        }
    }
}
