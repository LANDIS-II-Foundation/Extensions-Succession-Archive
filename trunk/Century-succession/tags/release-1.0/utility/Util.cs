//  Copyright 2007 Conservation Biology Institute
//  Authors:  Robert M. Scheller
//  License:  Available at  
//  http://www.landis-ii.org/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Landis.Cohorts;
using Landis.Ecoregions;
using Landis.Landscape;
using Landis.Species;

namespace Landis.Extension.Succession.Century
{
    /// <summary>
    /// Utility methods.
    /// </summary>
    public static class Util
    {

        /// <summary>
        /// Converts a table indexed by species and ecoregion into a
        /// 2-dimensional array.
        /// </summary>
        public static T[,] ToArray<T>(Species.AuxParm<Ecoregions.AuxParm<T>> table)
        {
            T[,] array = new T[Model.Core.Ecoregions.Count, Model.Core.Species.Count];
            foreach (ISpecies species in Model.Core.Species) {
                foreach (IEcoregion ecoregion in Model.Core.Ecoregions) {
                    array[ecoregion.Index, species.Index] = table[species][ecoregion];
                }
            }
            return array;
        }
    }
}
