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
    /// <summary>
    /// Rock mineral pool (only phosphorus in most systems).
    /// Weathering must occur to release mineral phosphorus.
    /// </summary>
    public class Rock : Pool
    {
        private double weatheredMineralP;

        //---------------------------------------------------------------------

        public Rock()
        {
            this.weatheredMineralP = 0;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Mineral P weathered from rock.
        /// </summary>
        public double WeatheredMineralP
        {
            get
            {
                return weatheredMineralP;
            }
            set
            {
                weatheredMineralP = value;
            }
        }

        //---------------------------------------------------------------------

        public static void Initialize(IInputParameters parameters)
        {
            foreach (ActiveSite site in Model.Core.Landscape)
            {
                IEcoregion ecoregion = Model.Core.Ecoregion[site];

                SiteVars.Rock[site].ContentP = (parameters.InitialMineralP[ecoregion] * 0.99);
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Weathering of rock phosphorus (rate is user-defined), increasing
        /// mineral phosphorus and reducing rock phosphorus.
        /// </summary>
        public static void Weathering(IEcoregion ecoregion,
                                      Rock rock,
                                      MineralSoil mineralSoil)
        {
            rock.WeatheredMineralP = rock.ContentP * EcoregionData.WeatheringP[ecoregion];
            rock.ContentP = Math.Max(rock.ContentP - rock.WeatheredMineralP, 0);
            mineralSoil.ContentP += rock.WeatheredMineralP;
        }
    }
}
