//  Copyright 2007-2010 University of Nevada, Portland State University
//  Authors:  Sarah Ganschow, Robert M. Scheller
//  License:  Available at
//  http://www.landis-ii.org/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Util;
using System;
using Landis.Ecoregions;
using Landis.Landscape;
using Landis.Species;

namespace Landis.Biomass.NuCycling.Succession
{
    /// <summary>
    /// Unavailable CHARCOAL mineral pools
    /// Weathering\decomposition must occur to release minerals.
    /// Included because different microbial communities/properties than SOM (Pietkainen et al. 2000)
    /// NEED TO DETERMINE PROPORTION ABOVE- VERSUS BELOWGROUND (I.E., CHAR V SOOT)
    /// NEED TO TRACK TIME SINCE LAST FIRE!!!!!!!!!
    ///
    /// </summary>
    public class Unavailable : Pool
    {
        private double weatheredMineralN;
        private double weatheredMineralP;

        //---------------------------------------------------------------------
        public Unavailable()
        {
            this.weatheredMineralN = 0;
            this.weatheredMineralP = 0;
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Mineral N weathered from unavailable sources.
        /// </summary>
        public double WeatheredMineralN
        {
            get
            {
                return weatheredMineralN;
            }
            set
            {
                weatheredMineralN = value;
            }
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Mineral P weathered from unavailable sources.
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

        /// <summary>.
        /// Need to get increase of unavailable nutrients (primarily caused
        /// by fire events).
        /// Amounts weathered N/P need to be used elsewhere.
        /// </summary>

        /// <summary>
        /// Weathering of unavailable C and mineral N and P.
        /// Unavailable content updated to reflect weathering.
        /// </summary>
        public void Weathering(ActiveSite site)
        {
            //Constant 0.0002 gives charcoal half-life of 3465 years (Lutzow et al. 2006--range 500-10000 yr)
            double weatheringRate = 0.0002;
            SiteVars.Unavailable[site].ContentC = (SiteVars.Unavailable[site].ContentC * Math.Exp(-1 * weatheringRate));
            SiteVars.Unavailable[site].ContentC = Math.Max(SiteVars.Unavailable[site].ContentC, 0);

            WeatheredMineralN = SiteVars.Unavailable[site].ContentN - (SiteVars.Unavailable[site].ContentN * Math.Exp(-1 * weatheringRate));
            SiteVars.Unavailable[site].ContentN -= WeatheredMineralN;
            SiteVars.Unavailable[site].ContentN = Math.Max(SiteVars.Unavailable[site].ContentN, 0);

            WeatheredMineralP = SiteVars.Unavailable[site].ContentP - (SiteVars.Unavailable[site].ContentP * Math.Exp(-1 * weatheringRate));
            SiteVars.Unavailable[site].ContentP -= WeatheredMineralP;
            SiteVars.Unavailable[site].ContentP = Math.Max(SiteVars.Unavailable[site].ContentP, 0);
        }

    }
}
