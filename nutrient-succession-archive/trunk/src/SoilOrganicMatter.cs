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
    /// Soil organic matter (SOM) pool.
    /// Mass = C fraction + N fraction + P fraction + other(inexplicit).
    /// </summary>
    public class SoilOrganicMatter : Pool
    {
        private double mineralizedN;
        private double mineralizedP;

        //User-defined single DECOMPOSITION RATE by ecoregion
        public static Ecoregions.AuxParm<double> DecayRateSOM;

        //---------------------------------------------------------------------

        public SoilOrganicMatter()
        {
            this.mineralizedN = 0;
            this.mineralizedP = 0;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Amount of nitrogen mineralized (to be added to mineral N)
        /// </summary>
        public double MineralizedN
        {
            get
            {
                return mineralizedN;
            }
            set
            {
                mineralizedN = value;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Amount of phosphorus mineralized (to be added to mineral P)
        /// </summary>
        public double MineralizedP
        {
            get
            {
                return mineralizedP;
            }
            set
            {
                mineralizedP = value;
            }
        }

        //---------------------------------------------------------------------

        public static void Initialize(IInputParameters parameters)
        {
            UpdateParameters(parameters);

            foreach (ActiveSite site in Model.Core.Landscape)
            {
                IEcoregion ecoregion = Model.Core.Ecoregion[site];

                SiteVars.SoilOrganicMatter[site].ContentC = parameters.InitialSOMC[ecoregion];
                SiteVars.SoilOrganicMatter[site].Mass = parameters.InitialSOMMass[ecoregion];
                SiteVars.SoilOrganicMatter[site].ContentN = parameters.InitialSOMN[ecoregion];
                SiteVars.SoilOrganicMatter[site].ContentP = parameters.InitialSOMP[ecoregion];
            }
        }

        //---------------------------------------------------------------------

        public static void UpdateParameters(DynamicChange.IInputParameters parameters)
        {
            DecayRateSOM = parameters.DecayRateSOM;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Decomposition of soil organic matter and mineralization of N/P.
        /// C:N ratio should be rough average of C:Ns at limit value transfers.
        /// </summary>
        public static void Decompose(IEcoregion ecoregion,
                                     ActiveSite site)
        {
            double k = DecayRateSOM[ecoregion];

            SiteVars.SoilOrganicMatter[site].Mass = SiteVars.SoilOrganicMatter[site].Mass * Math.Exp(-1 * k);
            SiteVars.SoilOrganicMatter[site].ContentC = SiteVars.SoilOrganicMatter[site].ContentC * Math.Exp(-1 * k);

            SiteVars.SoilOrganicMatter[site].MineralizedN = SiteVars.SoilOrganicMatter[site].ContentN -
                (SiteVars.SoilOrganicMatter[site].ContentN * Math.Exp(-1 * k));
            SiteVars.SoilOrganicMatter[site].ContentN = Math.Max(SiteVars.SoilOrganicMatter[site].ContentN -
                SiteVars.SoilOrganicMatter[site].MineralizedN, 0);
            SiteVars.MineralSoil[site].ContentN += SiteVars.SoilOrganicMatter[site].MineralizedN;

            SiteVars.SoilOrganicMatter[site].MineralizedP = SiteVars.SoilOrganicMatter[site].ContentP -
                (SiteVars.SoilOrganicMatter[site].ContentP * Math.Exp(-1 * k));
            SiteVars.SoilOrganicMatter[site].ContentP = Math.Max(SiteVars.SoilOrganicMatter[site].ContentP -
                SiteVars.SoilOrganicMatter[site].MineralizedP, 0);
            SiteVars.MineralSoil[site].ContentP += SiteVars.SoilOrganicMatter[site].MineralizedP;
        }
    }
}
