//  Copyright 2007-2010 University of Nevada, Portland State University
//  Authors:  Sarah Ganschow, Robert M. Scheller.
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
    /// Charcoal pools are created by fire. Weathering, decomposition, and subsequent fire
    /// release nitrogen and phosphorus.
    ///
    /// Charcoal is included because of different decomposition times, microbial communities,
    /// and properties than soil organic matter (Pietkainen et al. 2000). With more information,
    /// a temporal effect of charcoal on decomposition and other processes could be included.
    /// </summary>
    public class Charcoal
    {
        private double contentC;
        private double contentN;
        private double contentP;
        private double weatheredMineralN;
        private double weatheredMineralP;

        //---------------------------------------------------------------------
        public Charcoal()
        {
            this.contentC = 0;
            this.contentN = 0;
            this.contentP = 0;
            this.weatheredMineralN = 0;
            this.weatheredMineralP = 0;
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Charcoal carbon.
        /// </summary>
        public double ContentC
        {
            get
            {
                return contentC;
            }
            set
            {
                contentC = value;
            }
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Charcoal nitrogen.
        /// </summary>
        public double ContentN
        {
            get
            {
                return contentN;
            }
            set
            {
                contentN = value;
            }
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Charcoal phosphorus.
        /// </summary>
        public double ContentP
        {
            get
            {
                return contentP;
            }
            set
            {
                contentP = value;
            }
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Mineral nitrogen released from weathered charcoal.
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
        /// Mineral phosphorus released from weathered charcoal.
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
        /// <summary>
        /// Weathering (i.e., physical decomposition) of charcoal carbon, nitrogen,
        /// and phosphorus. Charcoal C, N, and P and mineral soil N and P are
        /// updated. The weathering rate gives a half-life of 3,465 years (Lutzow
        /// et al. 2006 suggest a range of 500-10,000 years).
        /// </summary>
        public static void Weathering(Charcoal charcoal,
                                      MineralSoil mineralSoil)
        {
            double weatheringRate = 0.0002;

            charcoal.ContentC = Math.Max(charcoal.ContentC * Math.Exp(-1 * weatheringRate), 0);

            charcoal.WeatheredMineralN = charcoal.ContentN - (charcoal.ContentN *
                Math.Exp(-1 * weatheringRate));
            charcoal.ContentN = Math.Max(charcoal.ContentN - charcoal.WeatheredMineralN, 0);
            mineralSoil.ContentN += charcoal.WeatheredMineralN;

            charcoal.WeatheredMineralP = charcoal.ContentP - (charcoal.ContentP *
                Math.Exp(-1 * weatheringRate));
            charcoal.ContentP = Math.Max(charcoal.ContentP - charcoal.WeatheredMineralP, 0);
            mineralSoil.ContentP += charcoal.WeatheredMineralP;
        }
    }
}
