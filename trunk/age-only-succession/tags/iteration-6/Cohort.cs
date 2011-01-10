using Landis.Species;

namespace Landis.AgeOnly
{
	public class Cohort
		: ICohort
	{
		private ISpecies species;
		private int age;

		//---------------------------------------------------------------------

		public ISpecies Species
		{
			get {
				return species;
			}
		}

		//---------------------------------------------------------------------

		public int Age
		{
			get {
				return age;
			}
		}

		//---------------------------------------------------------------------

		public Cohort(ISpecies species,
		              int      age)
		{
			this.species = species;
			this.age = age;
		}
	}
}
