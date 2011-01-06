using Landis.Cohorts;
using Landis.Landscape;

namespace Landis.Biomass.Succession
{
    public class LandscapeCohorts
        : ILandscapeCohorts<Biomass.ICohort>,
          ILandscapeCohorts<AgeCohort.ICohort>
    {
        private ISiteVar<Biomass.SiteCohorts> cohorts;

        //---------------------------------------------------------------------

        public ISiteCohorts<Biomass.ICohort> this[Site site]
        {
            get {
                return cohorts[site];
            }
        }

        //---------------------------------------------------------------------

        ISiteCohorts<AgeCohort.ICohort> ILandscapeCohorts<AgeCohort.ICohort>.this[Site site]
        {
            get {
                return cohorts[site];
            }
        }

        //---------------------------------------------------------------------

        public LandscapeCohorts(ISiteVar<Biomass.SiteCohorts> cohorts)
        {
            this.cohorts = cohorts;
        }
    }
}
