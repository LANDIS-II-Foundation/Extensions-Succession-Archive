using Landis.Species;
using System.Collections.Generic;

namespace Landis.AgeOnly
{
	public interface ISiteCohorts
	{
		IEnumerable<ISpecies> SpeciesPresent
		{
			get;
		}

		//---------------------------------------------------------------------

		void Age(int          deltaTime,
		         CohortMethod senescenceMethod);
	}
}
