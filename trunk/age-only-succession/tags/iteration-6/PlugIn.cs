using Landis;
using Landis.Landscape;
using Landis.Species;
using System.Collections.Generic;

namespace Landis.AgeOnly.Succession
{
	public class PlugIn
		: Landis.ISuccession, Landis.PlugIn.IPlugIn
	{
		private int timestep;

		//---------------------------------------------------------------------

		public int Timestep
		{
			get {
				return timestep;
			}
		}

		//---------------------------------------------------------------------

		public DisperseSeedMethod DisperseSeed
		{
			get {
				// TODO - either return a local method or move this to scenario file
				return null;
			}
		}

		//---------------------------------------------------------------------

		public void Initialize(string initDataFile)
		{
			ParametersParser parser = new ParametersParser();
			IParameters parameters = Landis.Data.Load<IParameters>(initDataFile,
			                                                       parser);
			timestep = parameters.Timestep;
		}

		//---------------------------------------------------------------------

		public void InitializeSite(ActiveSite                site,
		                           SiteInitialization.IClass initialSiteClass)
		{
			SiteVars.Cohorts[site] = new SiteCohorts(initialSiteClass.SiteCohorts);
		}

		//---------------------------------------------------------------------

		public void AgeCohorts(ActiveSite site,
		                       int        deltaTime)
		{
			SiteVars.Cohorts[site].Age(deltaTime, CohortSenescence);
		}

		//---------------------------------------------------------------------

		public void CohortSenescence(ICohort cohort)
		{
			// TODO - log the event?  tally some sort of statistics?
		}

		//---------------------------------------------------------------------

		public byte ComputeShade(ActiveSite site)
		{
			byte shade = 0;
			foreach (ISpecies species in SiteVars.Cohorts[site].SpeciesPresent) {
				if (species.ShadeTolerance > shade)
					shade = species.ShadeTolerance;
			}
			return shade;
		}
	}
}
