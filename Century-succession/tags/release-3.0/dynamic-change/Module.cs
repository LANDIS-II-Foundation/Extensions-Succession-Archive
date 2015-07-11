//  Copyright 2007-2010 Portland State University, University of Wisconsin-Madison
//  Author: Robert Scheller, Ben Sulman

using Landis.Library.Succession;
using System.Collections.Generic;

namespace Landis.Extension.Succession.Century.Dynamic
{
    /// <summary>
    /// The public interface for the module that handles updates to the biomass
    /// parameters due to climate change.
    /// </summary>
    public static class Module
    {
        private static List<ParametersUpdate> parameterUpdates;
        private static int indexOfNextUpdate;
        private static ParametersUpdate nextUpdate;

        //---------------------------------------------------------------------

        /// <summary>
        /// Initializes the module.
        /// </summary>
        /// <param name="parameterUpdates">
        /// The list of parameter updates.  The year and the file for each
        /// update has been specified.
        /// </param>
        /// <remarks>
        /// The file for each update is parsed for the set of biomass
        /// parameters.
        /// </remarks>
        public static void Initialize(List<ParametersUpdate> parameterUpdates)
        {
            Module.parameterUpdates = parameterUpdates;
            if (parameterUpdates.Count > 0) {
                PlugIn.ModelCore.Log.WriteLine("   Loading biomass parameters for climate change:");
                ParametersParser parser = new ParametersParser();
                foreach (ParametersUpdate update in parameterUpdates) {
                    update.LoadParameters(parser);
                    PlugIn.ModelCore.Log.WriteLine("  Read parameters for year {0} from file \"{1}\"",
                                 update.Year, update.File);
                }

                indexOfNextUpdate = 0;
                nextUpdate = parameterUpdates[indexOfNextUpdate];
            }
            else {
                nextUpdate = null;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Checks to see if the biomass parameters should be updated for the
        /// current time step.
        /// </summary>
        public static void CheckForUpdate()
        {
            while (nextUpdate != null && nextUpdate.Year <= PlugIn.ModelCore.CurrentTime) {
                //  Update various biomass parameters
                //Reproduction.ChangeEstablishProbabilities(Util.ToArray<double>(nextUpdate.Parameters.EstablishProbability));

                //Update Input Data:
                SpeciesData.ChangeParameters(nextUpdate.Parameters);
                EcoregionData.ChangeParameters(nextUpdate.Parameters);

                PlugIn.ModelCore.Log.WriteLine("   Updated biomass parameters for climate change");

                indexOfNextUpdate++;
                if (indexOfNextUpdate < parameterUpdates.Count)
                    nextUpdate = parameterUpdates[indexOfNextUpdate];
                else
                    nextUpdate = null;
            }
        }

    }
}
