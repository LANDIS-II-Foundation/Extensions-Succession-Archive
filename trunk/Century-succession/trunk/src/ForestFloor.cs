//  Copyright 2007-2010 Portland State University, University of Wisconsin-Madison
//  Author: Robert Scheller, Ben Sulman

using Landis.Core;
using Landis.SpatialModeling;
using Edu.Wisc.Forest.Flel.Util;

using System.Collections.Generic;


namespace Landis.Extension.Succession.Century
{
    /// <summary>
    /// Soil organic matter (SOM) pool.
    /// Mass = C Fraction + N Fraction + P Fraction + other(inexplicit).
    /// </summary>
    public class ForestFloor
    {

        /// <summary>
        /// Adds some biomass for a species to the WOOD litter pools at a site.
        /// </summary>
        public static void AddWoodLitter(double woodBiomass, 
                                    ISpecies   species,
                                    ActiveSite site)
        {
        
            if(woodBiomass > 0)
            WoodLayer.PartitionResidue(woodBiomass,  
                            FunctionalType.Table[SpeciesData.FuncType[species]].WoodDecayRate,
                            SpeciesData.WoodCN[species], 
                            SpeciesData.WoodLignin[species], 
                            LayerName.Wood,
                            LayerType.Surface,
                            site);
            
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Adds some biomass for a species to the foliar LITTER pools at a site.
        /// </summary>
        public static void AddFoliageLitter(double foliarBiomass, 
                                      ISpecies   species,
                                      ActiveSite site)
        {

            double inputDecayValue = 1.0;   // Decay value is calculated for surface/soil layers (leaf/fine root), 
                                            // therefore, this is just a dummy value.
                                            
            if(foliarBiomass > 0)
            {
                SiteVars.LitterfallC[site] += foliarBiomass * 0.47;
                
                LitterLayer.PartitionResidue(
                            foliarBiomass,
                            inputDecayValue,
                            SpeciesData.LeafLitterCN[species],
                            SpeciesData.LeafLignin[species],
                            OtherData.StructuralCN,
                            LayerName.Leaf,
                            LayerType.Surface,
                            site);
            }


        }

    }
}
