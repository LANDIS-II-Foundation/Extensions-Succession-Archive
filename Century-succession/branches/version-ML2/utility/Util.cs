//  Copyright 2007-2010 Portland State University, University of Wisconsin-Madison
//  Author: Robert Scheller, Ben Sulman

using Landis.Core;
using Landis.SpatialModeling;
using Landis.Library.LeafBiomassCohorts;

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
            T[,] array = new T[PlugIn.ModelCore.Ecoregions.Count, PlugIn.ModelCore.Species.Count];
            foreach (ISpecies species in PlugIn.ModelCore.Species) {
                foreach (IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions) {
                    array[ecoregion.Index, species.Index] = table[species][ecoregion];
                }
            }
            return array;
        }
    }
}
