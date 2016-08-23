
//using Landis.Cohorts;
using Landis.Ecoregions;
using Landis.Species;
using Wisc.Flel.GeospatialModeling.Landscapes;
using Landis.Library.BaseCohorts;
using Landis.Extension.Succession.AgeOnly;


namespace age_only_successsion
{
    public static class SiteVars
    {
        private static ISiteVar<SiteCohorts> cohorts;

        //---------------------------------------------------------------------

        public static void Initialize()
        {
            //timeOfLastEvent = Model.Core.Landscape.NewSiteVar<int>();

            cohorts = Model.Core.Landscape.NewSiteVar<SiteCohorts>();

            //if (cohorts.ActiveSiteValues.HasAge())
            
            Model.Core.RegisterSiteVar(SiteVars.Cohorts, "Succession.Cohorts");

        }

        //---------------------------------------------------------------------

        public static ISiteVar<SiteCohorts> Cohorts
        {
            get
            {
                return cohorts;
            }
        }

        public static bool MaturePresent(ISpecies species,
                                         Site site)
        {
            return SiteVars.Cohorts[site].IsMaturePresent(species);
        }
        
    }
}

