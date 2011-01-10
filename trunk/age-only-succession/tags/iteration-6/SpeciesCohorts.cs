using Landis.Species;
using System.Collections.Generic;

namespace Landis.AgeOnly
{
	/// <summary>
	/// The cohorts for a particular species at a site.
	/// </summary>
	public class SpeciesCohorts
		: ISpeciesCohorts
	{
		private ISpecies species;
		private List<int> ages;

		//---------------------------------------------------------------------

		public ISpecies Species
		{
			get {
				return species;
			}
		}

		//---------------------------------------------------------------------

		public SpeciesCohorts(SiteInitialization.ISpeciesCohorts initialCohorts)
		{
			species = initialCohorts.Species;
			ages = new List<int>();
			foreach (int age in initialCohorts.Ages)
				ages.Add(age);
		}

		//---------------------------------------------------------------------

		public bool Age(int          deltaTime,
		         		CohortMethod senescenceMethod)
		{
			//  Go backwards through list of ages, so the removal of an age
			//	doesn't mess up the loop.
			for (int i = ages.Count - 1; i >= 0; --i) {
				ages[i] += deltaTime;
				if (ages[i] > species.Longevity) {
					int age = ages[i];
					ages.RemoveAt(i);
					senescenceMethod(new Cohort(species, age));
				}
			}
			return ages.Count == 0;
		}
	}
}
