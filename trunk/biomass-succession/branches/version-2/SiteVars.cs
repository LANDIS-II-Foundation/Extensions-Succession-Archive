//  Copyright 2005-2010 Portland State University
//  Authors:  Robert M. Scheller
//  License:  Available at
//  http://www.landis-ii.org/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

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
        private static ISiteVar<double> capacityReduction;

        //---------------------------------------------------------------------

        /// <summary>
        /// Initializes the module.
        /// </summary>
        public static void Initialize()
        {

            cohorts             = Model.Core.Landscape.NewSiteVar<SiteCohorts>();
            woodyDebris         = Model.Core.Landscape.NewSiteVar<Pool>();
            litter              = Model.Core.Landscape.NewSiteVar<Pool>();

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
            get {
                return woodyDebris;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The dead non-woody pools for the landscape's sites.
        /// </summary>
        public static ISiteVar<Pool> Litter
        {
            get {
                return litter;
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

    }
}
