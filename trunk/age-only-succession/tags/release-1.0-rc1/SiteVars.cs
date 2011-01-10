using Landis;
using Landis.Landscape;

namespace Landis.AgeOnly.Succession
{
	public static class SiteVars
	{
		private static ISiteVar<ISiteCohorts<AgeCohort.ICohort>> cohorts;

		//---------------------------------------------------------------------

		public static ISiteVar<ISiteCohorts<AgeCohort.ICohort>> Cohorts
		{
			get {
				return cohorts;
			}
		}

		//---------------------------------------------------------------------

		internal static void Initialize()
		{
			cohorts = Model.Landscape.NewSiteVar<ISiteCohorts<AgeCohort.ICohort>>();
		}
	}
}
