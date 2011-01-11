//  Copyright 2005 University of Nevada, University of Wisconsin
//  Authors:  Sarah Ganschow, Robert M. Scheller, James B. Domingo
//  License:  Available at
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Landis;
using Landis.Ecoregions;
using Landis.Landscape;
using Landis.Species;
using System.Collections.Generic;

namespace Landis.Biomass.Succession
{
    /// <summary>
    /// The pools of dead biomass for the landscape's sites.
    /// </summary>
    public static class SiteVars
    {
        private static ISiteVar<SiteCohorts> cohorts;
        private static ISiteVar<Pool> woodyDebris;
        private static ISiteVar<Pool> litter;

        //private static ISiteVar<double> lai;
        private static ISiteVar<double> percentShade;
        private static ISiteVar<double> lightTrans;
        private static ISiteVar<double> capacityReduction;

        private static ISiteVar<double> ag_npp;


        //---------------------------------------------------------------------

        /// <summary>
        /// Initializes the module.
        /// </summary>
        public static void Initialize()
        {

            cohorts         = Model.Core.Landscape.NewSiteVar<SiteCohorts>();
            woodyDebris     = Model.Core.Landscape.NewSiteVar<Pool>();
            litter          = Model.Core.Landscape.NewSiteVar<Pool>();
            percentShade    = Model.Core.Landscape.NewSiteVar<double>();
            lightTrans      = Model.Core.Landscape.NewSiteVar<double>();
            ag_npp          = Model.Core.Landscape.NewSiteVar<double>();
            //lai         = Model.Core.Landscape.NewSiteVar<double>();

            foreach (ActiveSite site in Model.Core.Landscape)
            {
                //  site cohorts are initialized by the PlugIn.InitializeSite method
                woodyDebris[site] = new Pool();
                litter[site] = new Pool();
            }

            Model.Core.RegisterSiteVar(SiteVars.WoodyDebris, "Succession.WoodyDebris");
            Model.Core.RegisterSiteVar(SiteVars.Litter, "Succession.Litter");

        }

        //---------------------------------------------------------------------
        public static void ResetAnnualValues(Site site)
        {

            // Reset these accumulators to zero:
            SiteVars.AGNPP[site] = 0.0;
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// Biomass cohorts at each site.
        /// </summary>
        public static ISiteVar<SiteCohorts> Cohorts
        {
            get
            {
                return cohorts;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The intact dead woody pools for the landscape's sites.
        /// </summary>
        public static ISiteVar<Pool> WoodyDebris
        {
            get
            {
                return woodyDebris;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The dead non-woody pools for the landscape's sites.
        /// </summary>
        public static ISiteVar<Pool> Litter
        {
            get
            {
                return litter;
            }
        }


        //---------------------------------------------------------------------
        /*
        /// <summary>
        /// Leaf Area Index for the site.
        /// </summary>
        public static ISiteVar<double> LAI
        {
            get {
                return lai;
            }
        }
         */

        //---------------------------------------------------------------------

        /// <summary>
        /// Percent Shade (the inverse of percent transmittance)
        /// </summary>
        public static ISiteVar<double> PercentShade
        {
            get
            {
                return percentShade;
            }
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// Light transmittance
        /// </summary>
        public static ISiteVar<double> LightTrans
        {
            get
            {
                return lightTrans;
            }
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// </summary>
        public static ISiteVar<double> CapacityReduction
        {
            get {
                return capacityReduction;
            }
            set {
                capacityReduction = value;
            }
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// </summary>
        public static ISiteVar<double> AGNPP
        {
            get {
                return ag_npp;
            }
            set {
                ag_npp = value;
            }
        }
    }
}
