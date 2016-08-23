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
		private double[,] establishProbs;

		//---------------------------------------------------------------------

		public int Timestep
		{
			get {
				return timestep;
			}
		}

		//---------------------------------------------------------------------

		public double[,] EstablishProbabilities
		{
			get {
				return establishProbs;
			}
		}

		//---------------------------------------------------------------------

		public Parameters(int       timestep,
		                  double[,] establishProbabilities)
		{
			this.timestep = timestep;
			this.establishProbs = establishProbabilities;
		}
	}
}
