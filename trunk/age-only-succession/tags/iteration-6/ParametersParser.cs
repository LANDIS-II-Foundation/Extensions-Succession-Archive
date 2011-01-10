using Landis.Util;
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
				return "Age-Only Succession";
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

			CheckNoDataAfter("the " + timestep.Name + " parameter");

			return parameters.GetComplete();
		}
	}
}
