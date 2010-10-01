//  Copyright 2007-2010 University of Nevada, Portland State University
//  Authors:  Sarah Ganschow, Robert M. Scheller
//  License:  Available at
//  http://www.landis-ii.org/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Landis;
using Landis.Ecoregions;
using Landis.Landscape;
using Landis.Species;

using System;
using System.Collections.Generic;

namespace Landis.Biomass.NuCycling.Succession
{
    /// <summary>
    /// Variables for all landscape sites.
    /// </summary>
    public static class SiteVars
    {
        private static ISiteVar<SiteCohorts> cohorts;
        private static ISiteVar<double> availableN;
        private static ISiteVar<Pool> unavailable;
        private static ISiteVar<Pool> fineRoots;
        private static ISiteVar<Pool> coarseRoots;
        private static ISiteVar<PoolD> woodyDebris;
        private static ISiteVar<List<PoolD>> litter;
        private static ISiteVar<List<PoolD>> deadFineRoots;

        //Used to sum new annual litter prior to adding to main litter pool.
        private static ISiteVar<List<PoolD>> litterAdd;
        private static ISiteVar<List<PoolD>> deadFineRootsAdd;

        //Used to remove new annual litter after addition to main litter pool.
        private static ISiteVar<List<PoolD>> removeLitter;
        private static ISiteVar<List<PoolD>> removeDeadFineRoots;

        private static ISiteVar<SoilOrganicMatter> soilOrganicMatter;
        private static ISiteVar<MineralSoil> mineralSoil;
        private static ISiteVar<Charcoal> charcoal;
        private static ISiteVar<Rock> rock;
        private static ISiteVar<byte> fireSeverity;

        public static ISiteVar<double> TotalWoodBiomass;
        public static ISiteVar<double> PrevYearMortality;
        public static ISiteVar<double> CurrentYearMortality;


        //---------------------------------------------------------------------

        /// <summary>
        /// Initializes the module.
        /// </summary>
        public static void Initialize()
        {
            cohorts             = Model.Core.Landscape.NewSiteVar<SiteCohorts>();
            availableN          = Model.Core.Landscape.NewSiteVar<double>();
            unavailable         = Model.Core.Landscape.NewSiteVar<Pool>();
            fineRoots           = Model.Core.Landscape.NewSiteVar<Pool>();
            coarseRoots         = Model.Core.Landscape.NewSiteVar<Pool>();
            woodyDebris         = Model.Core.Landscape.NewSiteVar<PoolD>();
            litter              = Model.Core.Landscape.NewSiteVar<List<PoolD>>();
            deadFineRoots       = Model.Core.Landscape.NewSiteVar<List<PoolD>>();
            litterAdd           = Model.Core.Landscape.NewSiteVar<List<PoolD>>();
            deadFineRootsAdd    = Model.Core.Landscape.NewSiteVar<List<PoolD>>();
            removeLitter        = Model.Core.Landscape.NewSiteVar<List<PoolD>>();
            removeDeadFineRoots = Model.Core.Landscape.NewSiteVar<List<PoolD>>();
            soilOrganicMatter   = Model.Core.Landscape.NewSiteVar<SoilOrganicMatter>();
            mineralSoil         = Model.Core.Landscape.NewSiteVar<MineralSoil>();
            charcoal            = Model.Core.Landscape.NewSiteVar<Charcoal>();
            rock                = Model.Core.Landscape.NewSiteVar<Rock>();
            //fireSeverity        = Model.Core.Landscape.NewSiteVar<int>();

            TotalWoodBiomass    = Model.Core.Landscape.NewSiteVar<double>();
            CurrentYearMortality = Model.Core.Landscape.NewSiteVar<double>();
            PrevYearMortality = Model.Core.Landscape.NewSiteVar<double>();
            fireSeverity = Model.Core.GetSiteVar<byte>("Fire.Severity");



            // Enable interactions with (almost) any fire extension:
            /*if (Model.Core.GetSiteVar<int>("Fire.Severity") == null)
            {
                Console.Write("Fire Severity not cu");
            }
            else
            {
                Console.Write("Real value");
                fireSeverity = Model.Core.GetSiteVar<int>("Fire.SeverityX");
            }*/

            foreach (ActiveSite site in Model.Core.Landscape)
            {
                //  site cohorts are initialized by the PlugIn.InitializeSite method
                availableN[site]            = new double();
                unavailable[site]           = new Pool();
                fineRoots[site]             = new Pool();
                coarseRoots[site]           = new Pool();
                woodyDebris[site]           = new PoolD();
                litter[site]                = new List<PoolD>();
                deadFineRoots[site]         = new List<PoolD>();
                litterAdd[site]             = new List<PoolD>();
                deadFineRootsAdd[site]      = new List<PoolD>();
                removeLitter[site]          = new List<PoolD>();
                removeDeadFineRoots[site]   = new List<PoolD>();
                soilOrganicMatter[site]     = new SoilOrganicMatter();
                mineralSoil[site]           = new MineralSoil();
                charcoal[site]              = new Charcoal();
                rock[site]                  = new Rock();
            }
        }

        //---------------------------------------------------------------------

        public static double ComputeWoodBiomass(ActiveSite site)
        {
            double woodBiomass = 0;
            if (SiteVars.Cohorts[site] != null)
                foreach (ISpeciesCohorts speciesCohorts in SiteVars.Cohorts[site])
                    foreach (ICohort cohort in speciesCohorts)
                        woodBiomass += cohort.WoodBiomass;
            return woodBiomass;
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// Biomass cohorts for the landscape's sites.
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
        /// Available nitrogen for the landscape's sites.
        /// </summary>
        public static ISiteVar<double> AvailableN
        {
            get
            {
                return availableN;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Unavailable CNP
        /// </summary>
        public static ISiteVar<Pool> Unavailable
        {
            get
            {
                return unavailable;
            }
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// The dead non-woody pools for the landscape's sites.
        /// </summary>
        public static ISiteVar<Pool> FineRoots
        {
            get
            {
                return fineRoots;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The dead woody pools for the landscape's sites.
        /// </summary>
        public static ISiteVar<Pool> CoarseRoots
        {
            get
            {
                return coarseRoots;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The intact dead woody pools for the landscape's sites.
        /// </summary>
        public static ISiteVar<PoolD> WoodyDebris
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
        public static ISiteVar<List<PoolD>> Litter
        {
            get
            {
                return litter;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The dead non-woody pools for the landscape's sites.
        /// </summary>
        public static ISiteVar<List<PoolD>> DeadFineRoots
        {
            get
            {
                return deadFineRoots;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The dead non-woody pools for the landscape's sites. BAD FIX
        /// </summary>
        public static ISiteVar<List<PoolD>> LitterAdd
        {
            get
            {
                return litterAdd;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The dead non-woody pools for the landscape's sites. BAD FIX
        /// </summary>
        public static ISiteVar<List<PoolD>> DeadFineRootsAdd
        {
            get
            {
                return deadFineRootsAdd;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The dead non-woody pools for the landscape's sites to be removed (reached limit value).
        /// </summary>
        public static ISiteVar<List<PoolD>> RemoveLitter
        {
            get
            {
                return removeLitter;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The dead non-woody pools for the landscape's sites to be removed (reached limit value).
        /// </summary>
        public static ISiteVar<List<PoolD>> RemoveDeadFineRoots
        {
            get
            {
                return removeDeadFineRoots;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The Soil Organic Matter for the landscape's sites.
        /// </summary>
        public static ISiteVar<SoilOrganicMatter> SoilOrganicMatter
        {
            get
            {
                return soilOrganicMatter;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The Mineral Soil for the landscape's sites.
        /// </summary>
        public static ISiteVar<MineralSoil> MineralSoil
        {
            get
            {
                return mineralSoil;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The charcoal minerals for the landscape's sites.
        /// </summary>
        public static ISiteVar<Charcoal> Charcoal
        {
            get
            {
                return charcoal;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The rock minerals for the landscape's sites.
        /// </summary>
        public static ISiteVar<Rock> Rock
        {
            get
            {
                return rock;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The fire severity for the landscape's sites.
        /// </summary>
        public static ISiteVar<byte> FireSeverity
        {
            get
            {
                return fireSeverity;
            }

            set
            {
                fireSeverity = value;
            }
        }
    }
}
