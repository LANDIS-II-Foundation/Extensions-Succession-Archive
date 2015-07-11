using Edu.Wisc.Forest.Flel.Util;

using Landis.Ecoregions;
using Landis.Species;

namespace Landis.AgeOnly.Succession
{
	/// <summary>
	/// Editable set of parameters for age-only succession.
	/// </summary>
	public interface IEditableParameters
		: IEditable<IParameters>
	{
		/// <summary>
		/// Timestep (years)
		/// </summary>
		InputValue<int> Timestep
		{
			get;
			set;
		}

		//---------------------------------------------------------------------

		void SetProbability(IEcoregion ecoregion,
		                    ISpecies   species,
		                    double     probability);
	}
}
