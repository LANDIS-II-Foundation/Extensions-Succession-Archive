//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Robert M. Scheller, James B. Domingo

using Wisc.Flel.GeospatialModeling.Landscapes;
using Landis.Core;
using Edu.Wisc.Forest.Flel.Util;

namespace Landis.Extension.Succession.Biomass
{
    public class EcoregionData
    {

        //user-defined by ecoregion
        public static Ecoregions.AuxParm<int> AET;

        //  Minimum relative biomass for each shade class in each ecoregion
        //public static Ecoregions.AuxParm<Percentage>[] MinRelativeBiomass;

        //  Maximum biomass at any site in each ecoregion
        public static Ecoregions.AuxParm<int> B_MAX;
        public static Ecoregions.AuxParm<int> ActiveSiteCount;


        //---------------------------------------------------------------------
        public static void Initialize(IInputParameters parameters)
        {
             AET = parameters.AET;  //FINISH LATER

            B_MAX               = new Ecoregions.AuxParm<int>(PlugIn.ModelCore.Ecoregions);
            ActiveSiteCount     = new Ecoregions.AuxParm<int>(PlugIn.ModelCore.Ecoregions);

            foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
            {
                IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];
                ActiveSiteCount[ecoregion]++;
            }
        }
        public static void UpdateB_MAX()//DynamicChange.IParameters parameters)
        {
            //AET = parameters.AET;

            //  Fill in B_MAX array
            foreach (IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions)
            {
                int largest_B_MAX_Spp = 0;
                foreach (ISpecies species in PlugIn.ModelCore.Species)
                {
                    largest_B_MAX_Spp = System.Math.Max(largest_B_MAX_Spp, SpeciesData.B_MAX_Spp[species][ecoregion]);
                }
                B_MAX[ecoregion] = largest_B_MAX_Spp;
            }


        }


    }
}
