using Landis;
using Landis.Landscape;

namespace Landis.AgeOnly.Succession
{
	public static class SiteVars
	{
		private static ISiteVar<ISiteCohorts<AgeOnly.ICohort>> cohorts;

		//---------------------------------------------------------------------

		public static ISiteVar<ISiteCohorts<AgeOnly.ICohort>> Cohorts
		{
			get {
				return cohorts;
			}
		}

		//---------------------------------------------------------------------

		internal static void Initialize()
		{
			cohorts = Model.Landscape.NewSiteVar<ISiteCohorts<AgeOnly.ICohort>>();
		}
	}
}
