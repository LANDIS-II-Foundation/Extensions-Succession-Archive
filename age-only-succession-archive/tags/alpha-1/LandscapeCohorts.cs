using Landis.Landscape;

namespace Landis.AgeOnly.Succession
{
	public class LandscapeCohorts
		: Landis.ILandscapeCohorts<AgeOnly.ICohort>
	{
		public ISiteCohorts<AgeOnly.ICohort> this[Site site]
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
