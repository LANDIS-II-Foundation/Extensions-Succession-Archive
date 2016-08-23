//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.Core;
using Landis.SpatialModeling;
using Landis.SpatialModeling.CoreServices;
using Landis.Library.AgeOnlyCohorts;

namespace Landis.Extension.Succession.AgeOnly
{
    public static class SiteVars
    {
        private static ISiteVar<ISiteCohorts> cohorts;
        //private static ISiteVar<Library.AgeOnlyCohorts.SiteCohorts> baseCohorts;
        //private static BaseCohortsSiteVar baseCohortsSiteVar;

        //---------------------------------------------------------------------

        public static void Initialize()
        {

            cohorts = PlugIn.ModelCore.Landscape.NewSiteVar<ISiteCohorts>();
            //baseCohortsSiteVar = new BaseCohortsSiteVar(cohorts);

            //PlugIn.ModelCore.RegisterSiteVar(baseCohortsSiteVar, "Succession.AgeOnlyCohorts");
            PlugIn.ModelCore.RegisterSiteVar(cohorts, "Succession.AgeCohorts");
        }

        //---------------------------------------------------------------------

        public static ISiteVar<ISiteCohorts> Cohorts
        {
            get
            {
                return cohorts;
            }
            set
            {
                cohorts = value;
            }
        }

        public static bool MaturePresent(ISpecies species,
                                         Site site)
        {
            return SiteVars.Cohorts[site].IsMaturePresent(species);
        }

    }
}

