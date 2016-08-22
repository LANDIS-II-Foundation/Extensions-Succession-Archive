//  Copyright 2006 University of Wisconsin-Madison
//  Author: Jimm Domingo, FLEL

using Edu.Wisc.Forest.Flel.Util;

namespace Landis.Biomass.Succession.AgeOnlyDisturbances
{
    /// <summary>
    /// A editable pair of percentage parameters for the two dead pools.
    /// </summary>
    public class EditablePoolPercentages
        : IEditable<PoolPercentages>
    {
        private InputValue<Percentage> woody;
        private InputValue<Percentage> nonWoody;

        //---------------------------------------------------------------------

        /// <summary>
        /// The percentage associated with the dead woody pool.
        /// </summary>
        public InputValue<Percentage> Woody
        {
            get {
                return woody;
            }

            set {
                ValidatePercentage(value);
                woody = value;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The percentage associated with the dead non-woody pool.
        /// pool.
        /// </summary>
        public InputValue<Percentage> NonWoody
        {
            get {
                return nonWoody;
            }

            set {
                ValidatePercentage(value);
                nonWoody = value;
            }
        }

        //---------------------------------------------------------------------

        private void ValidatePercentage(InputValue<Percentage> percentage)
        {
            if (percentage.Actual < 0.0 || percentage.Actual > 1.0)
                throw new InputValueException(percentage.String,
                                              "Value must be between 0% and 100%");
        }

        //---------------------------------------------------------------------

        public bool IsComplete
        {
            get {
                return (woody != null) && (nonWoody != null);
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public EditablePoolPercentages()
        {
        }

        //---------------------------------------------------------------------

        public PoolPercentages GetComplete()
        {
            if (IsComplete)
                return new PoolPercentages(woody.Actual, nonWoody.Actual);
            else
                return null;
        }
    }
}
