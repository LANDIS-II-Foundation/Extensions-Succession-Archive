//  Copyright 2007-2010 Portland State University, University of Wisconsin-Madison
//  Author: Robert Scheller, Ben Sulman, Melissa Lucash

using Landis.Core;
using Landis.SpatialModeling;
using Edu.Wisc.Forest.Flel.Util;
using Landis.Library.Succession;
using System;

namespace Landis.Extension.Succession.Century
{
    /// <summary>
    /// Fine and coarse roots.
    /// </summary>
    public class Roots
    {

        //---------------------------------------------------------------------

        /// <summary>
        /// Kills coarse roots and add the biomass to the Wood Debris pool.
        /// </summary>
        public static void AddCoarseRootLitter(double abovegroundWoodBiomass,
                                    ISpecies   species,
                                    ActiveSite site)
        {

            double coarseRootBiomass = CalculateCoarseRoot(abovegroundWoodBiomass); // Ratio above to below

            if(coarseRootBiomass > 0)
            WoodLayer.PartitionResidue(coarseRootBiomass,  
                            FunctionalType.Table[SpeciesData.FuncType[species]].WoodDecayRate,
                            SpeciesData.CoarseRootCN[species], 
                            SpeciesData.CoarseRootLignin[species], 
                            LayerName.CoarseRoot,
                            LayerType.Soil,
                            site);
            
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Kills fine roots and add the biomass to the Dead Fine Roots pool.
        /// </summary>
        public static void AddFineRootLitter(double abovegroundFoliarBiomass, 
                                      ISpecies   species,
                                      ActiveSite site)
        {
            double fineRootBiomass = CalculateFineRoot(abovegroundFoliarBiomass); 
            
            double inputDecayValue = 1.0;   // Decay value is calculated for surface/soil (leaf/fine root), 
                                            // therefore, this is just a dummy value.
            if(fineRootBiomass > 0)
            LitterLayer.PartitionResidue(
                            fineRootBiomass,
                            inputDecayValue,
                            SpeciesData.FineRootCN[species],
                            SpeciesData.FineRootLignin[species],
                            OtherData.StructuralCN,
                            LayerName.FineRoot,
                            LayerType.Soil,
                            site);
            
        }
        
        /// <summary>
        /// Calculate coarse and fine roots based on aboveground wood and leaf biomass.
        /// Coarse root:stem mass of loblolly pine from Albaugh et al 2006 based on multiple sites with loblolly pine.
        /// Fine root:foliar biomass estimated from Park et al. 2008 at HBEF and Sleepers River.
        /// Fine root production used an average of deciduous and conifer species since they didn't differ in Park et al. 2008.
        /// </summary>
        public static double CalculateCoarseRoot(double wood)
        {
            return (wood * 0.5); //V3 used 0.5, for aspen it 0.235
        }
        public static double CalculateFineRoot(double foliarBiomass)
        {
            return (foliarBiomass * 0.76); //v3 used 0.76, for aspen it 0.78
        }
    }
}
