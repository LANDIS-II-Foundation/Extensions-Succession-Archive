//  Copyright 2006 University of Wisconsin-Madison
//  Author: Jimm Domingo, FLEL

using Edu.Wisc.Forest.Flel.Util;
using Landis.PlugIns;
using System.Collections.Generic;

namespace Landis.Biomass.Succession.AgeOnlyDisturbances
{
    /// <summary>
    /// A editable table of pool percentages for age-only disturbances.
    /// </summary>
    public class EditablePercentageTable
        : IEditable<IPercentageTable>
    {
        private IDictionary<PlugInType, EditablePoolPercentages> percentages;
        private EditablePoolPercentages defaultPercentages;

        //---------------------------------------------------------------------

        /// <summary>
        /// Gets the pair of percentages for a particular disturbance type.
        /// </summary>
        /// <remarks>
        /// If the disturbance type is not in the table, a new empty entry is
        /// added to the table.
        /// </remarks>
        public EditablePoolPercentages this[PlugInType disturbanceType]
        {
            get {
                EditablePoolPercentages poolPercentages;
                if (percentages.TryGetValue(disturbanceType, out poolPercentages))
                    return poolPercentages;

                poolPercentages = new EditablePoolPercentages();
                percentages[disturbanceType] = poolPercentages;
                return poolPercentages;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The default pair of percentages for disturbance types that do not
        /// have an entry in the table.
        /// </summary>
        public EditablePoolPercentages Default
        {
            get {
                return defaultPercentages;
            }
        }

        //---------------------------------------------------------------------

        public bool IsComplete
        {
            get {
                ICollection<EditablePoolPercentages> entryValues = percentages.Values;
                foreach (EditablePoolPercentages entryValue in entryValues)
                    if (! entryValue.IsComplete)
                        return false;
                return defaultPercentages.IsComplete;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public EditablePercentageTable()
        {
            this.percentages = new Dictionary<PlugInType, EditablePoolPercentages>();
            this.defaultPercentages = new EditablePoolPercentages();
        }

        //---------------------------------------------------------------------

        public IPercentageTable GetComplete()
        {
            if (IsComplete) {
                Dictionary<PlugInType, PoolPercentages> percentageDict;
                percentageDict = new Dictionary<PlugInType, PoolPercentages>();
                foreach (KeyValuePair<PlugInType, EditablePoolPercentages> entry in percentages)
                    percentageDict[entry.Key] = entry.Value.GetComplete();

                return new PercentageTable(percentageDict,
                                           defaultPercentages.GetComplete());
            }
            else
                return null;
        }
    }
}
