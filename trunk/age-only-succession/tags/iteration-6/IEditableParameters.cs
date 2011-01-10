using Landis.Util;

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
	}
}
