//  Copyright 2009 Conservation Biology Institute
//  Authors:  Robert M. Scheller
//  License:  Available at  
//  http://www.landis-ii.org/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Util;
using System;

using Landis.Ecoregions;
using Landis.Landscape;
using Landis.Species;

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
                            SpeciesData.FineRootLitterCN[species],
                            SpeciesData.FineRootLignin[species],
                            OtherData.StructuralCN,

                            LayerName.FineRoot,
                            LayerType.Soil,
                            site);
            
        }
        
        /// <summary>
        /// Calculate coarse and fine roots based on aboveground wood and leaf biomass.
        /// Coarse root:Aboveground Wood and Fine root:Foliar fom White et al. 2000/Niklas & Enquist 2002
        /// Averages of deciduous and conifer species
        /// </summary>
        public static double CalculateCoarseRoot(double wood)
        {
            return (wood * 0.25);
        }
        public static double CalculateFineRoot(double foliarBiomass)
        {
            return (foliarBiomass * 0.7);
        }
    }
}
