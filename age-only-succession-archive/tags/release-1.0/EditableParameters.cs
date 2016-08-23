using Edu.Wisc.Forest.Flel.Util;

using Landis.Ecoregions;
using Landis.Species;
using Landis.Succession;

namespace Landis.AgeOnly.Succession
{
	/// <summary>
	/// Editable set of parameters for age-only succession.
	/// </summary>
	public class EditableParameters
		: IEditableParameters
	{
		private InputValue<int> timestep;
		private InputValue<SeedingAlgorithms> seedAlg;
		private double[,] probabilities;

		//---------------------------------------------------------------------

		/// <summary>
		/// Timestep (years)
		/// </summary>
		public InputValue<int> Timestep
		{
			get {
				return timestep;
			}

			set {
				if (value != null) {
					if (value.Actual < 0)
						throw new InputValueException(value.String,
						                              "Timestep must be > or = 0");
				}
				timestep = value;
			}
		}

		//---------------------------------------------------------------------

		/// <summary>
		/// Seeding algorithm
		/// </summary>
		public InputValue<SeedingAlgorithms> SeedAlgorithm
		{
			get {
				return seedAlg;
			}

			set {
				seedAlg = value;
			}
		}

		//---------------------------------------------------------------------

		public bool IsComplete
		{
			get {
				object[] parameters = new object[]{ timestep,
				                                    seedAlg };
				foreach (object parameter in parameters)
					if (parameter == null)
						return false;
				return true;
			}
		}

		//---------------------------------------------------------------------

		public EditableParameters()
		{
			probabilities = new double[Model.Ecoregions.Count,
			                           Model.Species.Count];
		}

		//---------------------------------------------------------------------

		public void SetProbability(IEcoregion ecoregion,
		                           ISpecies   species,
		                           double     probability)
		{
			probabilities[ecoregion.Index, species.Index] = probability;
		}

		//---------------------------------------------------------------------

		public IParameters GetComplete()
		{
			if (this.IsComplete)
				return new Parameters(timestep.Actual,
				                      seedAlg.Actual,
				                      probabilities);
			else
				return null;
		}
	}
}
