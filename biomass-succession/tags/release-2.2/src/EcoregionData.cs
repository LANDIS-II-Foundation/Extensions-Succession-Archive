//  Copyright 2007 University of Nevada, University of Wisconsin
//  Authors:  Sarah Ganschow, Robert M. Scheller, James B. Domingo
//  License:  Available at  
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Util;
using System;

using Landis.Ecoregions;
using Landis.Landscape;
using Landis.Species;

namespace Landis.Biomass.Succession
{
    public class EcoregionData
    {

        //user-defined by ecoregion
        public static Ecoregions.AuxParm<int> AET;
        
        //  Minimum relative biomass for each shade class in each ecoregion
        public static Ecoregions.AuxParm<Percentage>[] MinRelativeBiomass;

        //  Maximum biomass at any site in each ecoregion
        public static Ecoregions.AuxParm<int> B_MAX;


        //---------------------------------------------------------------------
        public static void Initialize(IParameters parameters)
        {
            MinRelativeBiomass  = parameters.MinRelativeBiomass; 
            B_MAX               = new Ecoregions.AuxParm<int>(Model.Core.Ecoregions);

            ChangeParameters(parameters);
            
            
        }
        public static void ChangeParameters(ClimateChange.IParameters parameters)
        {
            AET = parameters.AET;
            
            //  Fill in B_MAX array
            foreach (IEcoregion ecoregion in Model.Core.Ecoregions) 
            {
                int largest_B_MAX_Spp = 0;
                foreach (ISpecies species in Model.Core.Species) 
                {
                    largest_B_MAX_Spp = Math.Max(largest_B_MAX_Spp, SpeciesData.B_MAX_Spp[species][ecoregion]);
                }
                B_MAX[ecoregion] = largest_B_MAX_Spp;
            }
            
         
        }
        
        
    }
}
