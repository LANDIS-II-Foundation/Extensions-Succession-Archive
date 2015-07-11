using Landis.Cohorts;
using Landis.Ecoregions;
using Landis.InitialCommunities;
using Landis.Landscape;
using Landis.Species;
using System.Collections.Generic;

namespace Landis.AgeOnly.Succession
{
	public class PlugIn
		: Landis.Succession.BaseComponent, PlugIns.ISuccession,
	                                       Cohorts.ISuccession<AgeCohort.ICohort>
	{
		private ISiteVar<AgeCohort.SiteCohorts> cohorts;
	    private LandscapeCohorts landscapeCohorts;

		//---------------------------------------------------------------------

		public string Name
		{
			get {
				return "Age-only Succession";
			}
		}

		//---------------------------------------------------------------------

		public ILandscapeCohorts<AgeCohort.ICohort> Cohorts
		{
			get {
				return landscapeCohorts;
			}
		}

		//---------------------------------------------------------------------

		public PlugIn()
		{
		}

		//---------------------------------------------------------------------

		public void Initialize(string dataFile,
		                       int    startTime)
		{
			ParametersParser parser = new ParametersParser();
			IParameters parameters = Landis.Data.Load<IParameters>(dataFile,
			                                                       parser);

			//	Cohorts must be created before the base class is initialized
			//	because the base class' reproduction module uses
			//	Model.GetSuccession<...>().Cohorts in its Initialization
			//	method.
			cohorts = Model.Landscape.NewSiteVar<AgeCohort.SiteCohorts>();
			landscapeCohorts = new LandscapeCohorts(cohorts);
			AgeCohort.Cohort.SenescenceDeath = Landis.Succession.Reproduction.CheckForResprouting;

			base.Initialize(parameters.Timestep,
			                parameters.EstablishProbabilities,
			                startTime,
			                parameters.SeedAlgorithm,
			                AddNewCohort);
		}

		//---------------------------------------------------------------------

		public void AddNewCohort(ISpecies   species,
		                         ActiveSite site)
		{
			cohorts[site].AddNewCohort(species);
		}

		//---------------------------------------------------------------------

		protected override void InitializeSite(ActiveSite site,
		                                       ICommunity initialCommunity)
		{
			cohorts[site] = new AgeCohort.SiteCohorts(initialCommunity.Cohorts);
		}

		//---------------------------------------------------------------------

		protected override void AgeCohorts(ActiveSite site,
		                                   ushort     years,
		                                   int?       successionTimestep)
		{
			cohorts[site].Grow(years, site, successionTimestep);
		}

		//---------------------------------------------------------------------

		public override byte ComputeShade(ActiveSite site)
		{
			byte shade = 0;
			foreach (AgeCohort.SpeciesCohorts speciesCohorts in cohorts[site]) {
				ISpecies species = speciesCohorts.Species;
				if (species.ShadeTolerance > shade)
					shade = species.ShadeTolerance;
			}
			return shade;
		}

		//---------------------------------------------------------------------

		public void CheckForResprouting(AgeCohort.ICohort cohort,
		                                ActiveSite        site)
		{
			Landis.Succession.Reproduction.CheckForResprouting(cohort, site);
		}

		//---------------------------------------------------------------------

		public void CheckForPostFireRegen(AgeCohort.ICohort cohort,
		                                  ActiveSite        site)
		{
			Landis.Succession.Reproduction.CheckForPostFireRegen(cohort, site);
		}
	}
}
