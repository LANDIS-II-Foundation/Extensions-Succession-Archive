//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Robert M. Scheller, James B. Domingo

using Wisc.Flel.GeospatialModeling.Landscapes;
using Landis.Core;
using Landis.Library.BiomassCohorts;
using System.Collections.Generic;

namespace Landis.Extension.Succession.Biomass
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

            cohorts = PlugIn.ModelCore.Landscape.NewSiteVar<SiteCohorts>();
            woodyDebris     = PlugIn.ModelCore.Landscape.NewSiteVar<Pool>();
            litter          = PlugIn.ModelCore.Landscape.NewSiteVar<Pool>();
            percentShade    = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            lightTrans      = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            ag_npp          = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            //lai         = PlugIn.ModelCore.Landscape.NewSiteVar<double>();

            foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
            {
                //  site cohorts are initialized by the PlugIn.InitializeSite method
                woodyDebris[site] = new Pool();
                litter[site] = new Pool();
            }

            PlugIn.ModelCore.RegisterSiteVar(SiteVars.Cohorts, "Succession.Cohorts");
            PlugIn.ModelCore.RegisterSiteVar(SiteVars.WoodyDebris, "Succession.WoodyDebris");
            PlugIn.ModelCore.RegisterSiteVar(SiteVars.Litter, "Succession.Litter");

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
