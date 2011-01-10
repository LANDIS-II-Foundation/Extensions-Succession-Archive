//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Robert M. Scheller, James B. Domingo

using Edu.Wisc.Forest.Flel.Util;

using Landis.Library.Succession;
using Landis.Core;

using System.Collections.Generic;

namespace Landis.Extension.Succession.AgeOnly
{
	/// <summary>
	/// A parser that reads age-only succession parameters from text input.
	/// </summary>
	public class InputParametersParser
		// : Landis.TextParser<IInputParameters>
        : TextParser<IInputParameters>
	{
        /*
		public override string LandisDataValue
		{
			get {
				return "Age-only Succession";
			}
		}*/

		//---------------------------------------------------------------------

		static InputParametersParser()
		{
			SeedingAlgorithmsUtil.RegisterForInputValues();
		}

		//---------------------------------------------------------------------

		public InputParametersParser()
		{
		}

		//---------------------------------------------------------------------

		protected override IInputParameters Parse()
		{
			// ReadLandisDataVar();

            InputVar<string> landisData = new InputVar<string>("LandisData");
            ReadVar(landisData);
            if (landisData.Value.Actual != PlugIn.ExtensionName)
                throw new InputValueException(landisData.Value.String, "The value is not \"{0}\"", PlugIn.ExtensionName);


			InputParameters parameters = new InputParameters();

			InputVar<int> timestep = new InputVar<int>("Timestep");
			ReadVar(timestep);
			parameters.Timestep = timestep.Value;

			InputVar<SeedingAlgorithms> seedAlg = new InputVar<SeedingAlgorithms>("SeedingAlgorithm");
			ReadVar(seedAlg);
			parameters.SeedAlgorithm = seedAlg.Value;

            //---------------------------------------------------------------------------------

            InputVar<string> initCommunities = new InputVar<string>("InitialCommunities");
            ReadVar(initCommunities);
            parameters.InitialCommunities = initCommunities.Value;

            InputVar<string> communitiesMap = new InputVar<string>("InitialCommunitiesMap");
            ReadVar(communitiesMap);
            parameters.InitialCommunitiesMap = communitiesMap.Value;

            //---------------------------------------------------------------------------------

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
				IEcoregion ecoregion = PlugIn.ModelCore.Ecoregions[ecoregionName.Value.Actual];
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
				//Species.ISpecies species = Model.Core.Species[speciesName.Value.Actual];
                ISpecies species = PlugIn.ModelCore.Species[speciesName.Value.Actual];
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

			return parameters; //.GetComplete();
		}
	}
}
