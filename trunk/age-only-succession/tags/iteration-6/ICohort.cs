using Landis.Species;

namespace Landis.AgeOnly
{
	/// <summary>
	/// A species cohort with only age information.
	/// </summary>
	public interface ICohort
	{
		/// <summary>
		/// The cohort's species.
		/// </summary>
		ISpecies Species
		{
			get;
		}

		//---------------------------------------------------------------------

		/// <summary>
		/// The cohort's age (years).
		/// </summary>
		int Age
		{
			get;
		}
	}
}
