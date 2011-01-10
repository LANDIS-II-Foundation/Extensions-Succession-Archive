using Landis;
using Landis.Ecoregions;
using Landis.InitialCommunities;
using Landis.Landscape;
using Landis.Species;
using System.Collections.Generic;

namespace Landis.AgeOnly.Succession
{
	public class PlugIn
		: Landis.Succession.BaseComponent, PlugIns.ISuccession,
	                                       Cohorts.ISuccession<AgeOnly.ICohort>
	{
	    private LandscapeCohorts landscapeCohorts;

		//---------------------------------------------------------------------

		public string Name
		{
			get {
				return "Age-only Succession";
			}
		}

		//---------------------------------------------------------------------

		public ILandscapeCohorts<AgeOnly.ICohort> Cohorts
		{
			get {
				return landscapeCohorts;
			}
		}

		//---------------------------------------------------------------------

		public PlugIn()
		{
			landscapeCohorts = new LandscapeCohorts();
		}

		//---------------------------------------------------------------------

		public void Initialize(string dataFile,
		                       int    startTime)
		{
			ParametersParser parser = new ParametersParser();
			IParameters parameters = Landis.Data.Load<IParameters>(dataFile,
			                                                       parser);

			base.Initialize(parameters.Timestep,
			                parameters.EstablishProbabilities,
			                startTime);
			SiteVars.Initialize();
			AgeOnly.Cohort.Died = Landis.Succession.Reproduction.CohortDeath;
		}

		//---------------------------------------------------------------------

		protected override void InitializeSite(ActiveSite site,
		                                       ICommunity initialCommunity)
		{
			SiteVars.Cohorts[site] = new AgeOnly.SiteCohorts(initialCommunity.Cohorts.BySpecies);
		}

		//---------------------------------------------------------------------

		protected override void AgeCohorts(ActiveSite site,
		                                   ushort     years,
		                                   int?       successionTimestep)
		{
			SiteVars.Cohorts[site].Grow(years, site, successionTimestep);
		}

		//---------------------------------------------------------------------

		public override byte ComputeShade(ActiveSite site)
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
