using Landis.Util;

namespace Landis.AgeOnly.Succession
{
	/// <summary>
	/// The parameters for age-only succession.
	/// </summary>
	public class Parameters
		: IParameters
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

		public Parameters(int timestep)
		{
			this.timestep = timestep;
		}
	}
}
