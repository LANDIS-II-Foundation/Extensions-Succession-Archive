//  Copyright 2007-2010 University of Nevada, Portland State University
//  Authors:  Sarah Ganschow, Robert M. Scheller
//  License:  Available at
//  http://www.landis-ii.org/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Util;
using Landis.Ecoregions;
using Landis.Landscape;
using Landis.Species;

using System;

namespace Landis.Biomass.NuCycling.Succession
{
    public class EcoregionData
    {
        //  Minimum relative biomass for each shade class in each ecoregion
        public static Ecoregions.AuxParm<Percentage>[] MinRelativeBiomass;

        //Annual nitrogen deposition to the ecoregion
        public static Ecoregions.AuxParm<int> DepositionN;

        //Annual phosphorus deposition to the ecoregion
        public static Ecoregions.AuxParm<int> DepositionP;

        //Actual evapotranspiration for the ecoregion
        public static Ecoregions.AuxParm<int> AET;

        //Rock weathering rate for release of mineral phosphorus
        public static Ecoregions.AuxParm<double> WeatheringP;
        public static Ecoregions.AuxParm<int> ActiveSiteCount;

        //---------------------------------------------------------------------
        public static void Initialize(IInputParameters parameters)
        {
            UpdateParameters(parameters);
            ActiveSiteCount = new Ecoregions.AuxParm<int>(Model.Core.Ecoregions);
            foreach (ActiveSite site in Model.Core.Landscape)
            {
                IEcoregion ecoregion = Model.Core.Ecoregion[site];
                ActiveSiteCount[ecoregion]++;
            }

        }

        //---------------------------------------------------------------------

        public static void UpdateParameters(DynamicChange.IInputParameters parameters)
        {
            DepositionN = parameters.DepositionN;
            DepositionP = parameters.DepositionP;
            AET = parameters.AET;
            WeatheringP = parameters.WeatheringP;
            MinRelativeBiomass = parameters.MinRelativeBiomass;

        }
    }
}
