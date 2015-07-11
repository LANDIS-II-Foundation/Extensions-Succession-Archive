//  Copyright 2009 Conservation Biology Institute
//  Authors:  Robert M. Scheller
//  License:  Available at  
//  http://www.landis-ii.org/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Landis.PlugIns;
using System.Collections.Generic;

namespace Landis.Extension.Succession.Century.AgeOnlyDisturbances
{
    /// <summary>
    /// A table of pool percentages for age-only disturbances.
    /// </summary>
    public interface IPercentageTable
    {
        /// <summary>
        /// Gets the pair of percentages for a particular disturbance type.
        /// </summary>
        PoolPercentages this[PlugInType disturbanceType]
        {
            get;set;
        }
        
        PoolPercentages Default {get;}
    }

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
                //UI.WriteLine("   Trying to acquire pool percentages for {0}.", disturbanceType);
                if (percentages.TryGetValue(disturbanceType, out poolPercentages))
                    return poolPercentages;

                return defaultPercentages;
            }
            set {
                //PoolPercentages poolPercentages;
                percentages[disturbanceType] = value;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The default pair of percentages for disturbance types that do not
        /// have an entry in the table.
        /// </summary>
        public PoolPercentages Default
        {
            get {
                return defaultPercentages;
            }
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
/*        public PercentageTable(IDictionary<PlugInType, PoolPercentages> percentages,
                               PoolPercentages                          defaultPercentages)
        {
            this.percentages = percentages;
            this.defaultPercentages = defaultPercentages;
        }*/
        //---------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public PercentageTable()
        {
            this.percentages = new Dictionary<PlugInType, PoolPercentages>();
            this.defaultPercentages = new PoolPercentages();
        }

    }
}
