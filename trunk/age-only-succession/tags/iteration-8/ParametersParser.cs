using Edu.Wisc.Forest.Flel.Util;

using Landis.Util;
using Landis.Ecoregions;

using System.Collections.Generic;

namespace Landis.AgeOnly.Succession
{
	/// <summary>
	/// A parser that reads age-only succession parameters from text input.
	/// </summary>
	public class ParametersParser
		: Landis.TextParser<IParameters>
	{
		public override string LandisDataValue
		{
			get {
				return "Age-only Succession";
			}
		}

		//---------------------------------------------------------------------

		public ParametersParser()
		{
		}

		//---------------------------------------------------------------------

		protected override IParameters Parse()
		{
			ReadLandisDataVar();

			IEditableParameters parameters = new EditableParameters();

			InputVar<int> timestep = new InputVar<int>("Timestep");
			ReadVar(timestep);
			parameters.Timestep = timestep.Value;

			ReadName("EstablishProbabilities");

			//  Read ecoregion names as column headings
			InputVar<string> ecoregionName = new InputVar<string>("Ecoregion");
			if (AtEndOfInput)
				throw new InputVariableException(ecoregionName,
				                                 "Expected a line with the names of 1 or more active ecoregions.");
			List<IEcoregion> ecoregions = new List<IEcoregion>();
			StringReader currentLine = new StringReader(CurrentLine);
			TextReader.SkipWhitespace(currentLine);
			while (currentLine.Peek() != -1) {
				ReadValue(ecoregionName, currentLine);
				IEcoregion ecoregion = Model.Ecoregions[ecoregionName.Value.Actual];
				if (ecoregion == null)
					throw new InputValueException(ecoregionName.Value.String,
					                              "{0} is not an ecoregion name.",
					                              ecoregionName.Value.String);
				if (! ecoregion.Active)
					throw new InputValueException(ecoregionName.Value.String,
					                              "{0} is not an active ecoregion.",
					                              ecoregionName.Value.String);
				if (ecoregions.Contains(ecoregion))
					throw new InputValueException(ecoregionName.Value.String,
					                              "The ecoregion {0} appears more than once.",
					                              ecoregionName.Value.String);
				ecoregions.Add(ecoregion);
				TextReader.SkipWhitespace(currentLine);
			}
			GetNextLine();

			string lastEcoregion = ecoregions[ecoregions.Count-1].Name;

			//  Read species and their probabilities
			InputVar<string> speciesName = new InputVar<string>("Species");
			Dictionary <string, int> speciesLineNumbers = new Dictionary<string, int>();
			while (! AtEndOfInput) {
				currentLine = new StringReader(CurrentLine);

				ReadValue(speciesName, currentLine);
				Species.ISpecies species = Model.Species[speciesName.Value.Actual];
				if (species == null)
					throw new InputValueException(speciesName.Value.String,
					                              "{0} is not a species name.",
					                              speciesName.Value.String);
				int lineNumber;
				if (speciesLineNumbers.TryGetValue(species.Name, out lineNumber))
					throw new InputValueException(speciesName.Value.String,
					                              "The species {0} was previously used on line {1}",
					                              speciesName.Value.String, lineNumber);
				else
					speciesLineNumbers[species.Name] = LineNumber;

				foreach (IEcoregion ecoregion in ecoregions) {
					InputVar<double> probability = new InputVar<double>("Ecoregion " + ecoregion.Name);
					ReadValue(probability, currentLine);
					if (probability.Value.Actual < 0.0 || probability.Value.Actual > 1.0)
						throw new InputValueException(probability.Value.String,
						                              "{0} is not between 0.0 and 1.0",
						                              probability.Value.String);
					parameters.SetProbability(ecoregion, species, probability.Value.Actual);
				}

				CheckNoDataAfter("the Ecoregion " + lastEcoregion + " column",
				                 currentLine);
				GetNextLine();
			}

			return parameters.GetComplete();
		}
	}
}
