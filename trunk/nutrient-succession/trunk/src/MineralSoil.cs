//  Copyright 2007-2010 University of Nevada, Portland State University
//  Authors:  Sarah Ganschow, Robert M. Scheller
//  License:  Available at
//  http://www.landis-ii.org/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Util;

using Landis;
using Landis.Ecoregions;
using Landis.Landscape;
using Landis.Species;

using System;

namespace Landis.Biomass.NuCycling.Succession
{
    /// <summary>
    /// Mineral soil pools for nitrogen and phosphorus.
    /// </summary>
    public class MineralSoil
    {
        private double leachedN;
        private double contentN;
        private double contentP;

        //---------------------------------------------------------------------

        public MineralSoil()
        {
            this.leachedN = 0.0;
            this.contentN = 0.0;
            this.contentP = 0.0;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Leached mineral nitrogen content.
        /// </summary>
        public double LeachedN
        {
            get
            {
                return leachedN;
            }
            set
            {
                leachedN = value;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Mineral nitrogen content.
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
        /// Mineral phosphorus content.
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

        public static void Initialize(IInputParameters parameters)
        {
            foreach (ActiveSite site in Model.Core.Landscape)
            {
                IEcoregion ecoregion = Model.Core.Ecoregion[site];
                SiteVars.MineralSoil[site].ContentN = parameters.InitialMineralN[ecoregion];
                SiteVars.MineralSoil[site].ContentP = (parameters.InitialMineralP[ecoregion] * 0.01);
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Calculate nitrogen leached from the system (nitrification implied).
        /// If Min N pool > 50 kg/ha, leach excess; otherwise, minimal amount.
        /// P is assumed to be conservative. Verburg and Johnson 2001.
        /// </summary>
        public static void Leaching(ActiveSite site)
        {
            SiteVars.MineralSoil[site].LeachedN = Math.Max(SiteVars.MineralSoil[site].ContentN - 50.0, 0.1);
            SiteVars.MineralSoil[site].ContentN -= SiteVars.MineralSoil[site].LeachedN;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Add nitrogen and phosphorus deposition to the mineral pools.
        /// </summary>
        public static void Deposition(IEcoregion ecoregion,
                                      MineralSoil mineralSoil)
        {
            mineralSoil.ContentN += EcoregionData.DepositionN[ecoregion];
            mineralSoil.ContentP += EcoregionData.DepositionP[ecoregion];
        }
    }
}
