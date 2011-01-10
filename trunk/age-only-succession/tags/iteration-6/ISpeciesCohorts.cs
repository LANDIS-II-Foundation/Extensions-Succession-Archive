using Landis.Species;

namespace Landis.AgeOnly
{
	/// <summary>
	/// The cohorts for a particular species at a site.
	/// </summary>
	public interface ISpeciesCohorts
	{
		/// <summary>
		/// The species for the cohorts.
		/// </summary>
		ISpecies Species
		{
			get;
		}

		//---------------------------------------------------------------------

		/// <summary>
		/// Advances the age for all the cohorts by a specific amount of time.
		/// </summary>
		/// <param name="deltaTime">
		/// The amount of time to advance each age by (years).
		/// </param>
		/// <param name="senescenceMethod">
		/// The method that is called when a cohort dies due to senescence.
		/// </param>
		/// <returns>
		/// true if all the cohorts die off.
		/// </returns>
		bool Age(int          deltaTime,
		         CohortMethod senescenceMethod);
	}
}
