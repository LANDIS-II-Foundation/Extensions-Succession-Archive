using Landis.Cohorts;
using Landis.Landscape;

namespace Landis.AgeOnly.Succession
{
	public class LandscapeCohorts
		: ILandscapeCohorts<AgeCohort.ICohort>
	{
		private ISiteVar<AgeCohort.SiteCohorts> cohorts;

		//---------------------------------------------------------------------

		public ISiteCohorts<AgeCohort.ICohort> this[Site site]
		{
			get {
				return cohorts[site];
			}
		}

		//---------------------------------------------------------------------

		public LandscapeCohorts(ISiteVar<AgeCohort.SiteCohorts> cohorts)
		{
			this.cohorts = cohorts;
		}
	}
}
