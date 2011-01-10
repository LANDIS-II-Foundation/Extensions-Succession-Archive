using Landis.Landscape;

namespace Landis.AgeOnly.Succession
{
	public class LandscapeCohorts
		: Landis.ILandscapeCohorts<AgeCohort.ICohort>
	{
		public ISiteCohorts<AgeCohort.ICohort> this[Site site]
		{
			get {
				return SiteVars.Cohorts[site];
			}
		}

		//---------------------------------------------------------------------

		public LandscapeCohorts()
		{
		}
	}
}
