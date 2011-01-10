using Landis.Species;
using System.Collections.Generic;

namespace Landis.AgeOnly
{
	public class SiteCohorts
		: ISiteCohorts, IEnumerable<ISpecies>
	{
		private List<ISpeciesCohorts> cohorts;
		
		//---------------------------------------------------------------------

		public IEnumerable<ISpecies> SpeciesPresent
		{
			get {
				return this;
			}
		}

		//---------------------------------------------------------------------

		IEnumerator<ISpecies> IEnumerable<ISpecies>.GetEnumerator()
		{
			foreach (ISpeciesCohorts speciesCohorts in cohorts)
				yield return speciesCohorts.Species;
		}

		//---------------------------------------------------------------------

		public SiteCohorts(SiteInitialization.ISiteCohorts initialSiteCohorts)
		{
			cohorts = new List<ISpeciesCohorts>();
			foreach (SiteInitialization.ISpeciesCohorts speciesCohorts in initialSiteCohorts.BySpecies)
				cohorts.Add(new SpeciesCohorts(speciesCohorts));
		}

		//---------------------------------------------------------------------

		public void Age(int          deltaTime,
		                CohortMethod senescenceMethod)
		{
			//  Go through list of species cohorts from back to front so that
			//	a removal does not mess up the loop.
			for (int i = cohorts.Count - 1; i >= 0; --i) {
				if (cohorts[i].Age(deltaTime, senescenceMethod))
					cohorts.RemoveAt(i);
			}
		}
	}
}
