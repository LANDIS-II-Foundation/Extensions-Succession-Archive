//  Copyright 2006 University of Wisconsin-Madison 
//  Author: Jimm Domingo, FLEL

using Landis.Succession;
using System.Collections.Generic;

namespace Landis.Biomass.Succession.ClimateChange
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
                UI.WriteLine("Loading biomass parameters for climate change:");
                ParametersParser parser = new ParametersParser(Model.Core.Ecoregions,
                                                               Model.Core.Species);
                foreach (ParametersUpdate update in parameterUpdates) {
                    update.LoadParameters(parser);
                    UI.WriteLine("  Read parameters for year {0} from file \"{1}\"",
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
            while (nextUpdate != null && nextUpdate.Year <= Model.Core.CurrentTime) {
                //  Update various biomass parameters
                //LivingBiomass.ChangeParameters(nextUpdate.Parameters);
                Reproduction.ChangeEstablishProbabilities(Util.ToArray<double>(nextUpdate.Parameters.EstablishProbability));

                //Update Input Data:
                SpeciesData.ChangeParameters(nextUpdate.Parameters);
                EcoregionData.ChangeParameters(nextUpdate.Parameters);

                UI.WriteLine("Updated biomass parameters for climate change");

                indexOfNextUpdate++;
                if (indexOfNextUpdate < parameterUpdates.Count)
                    nextUpdate = parameterUpdates[indexOfNextUpdate];
                else
                    nextUpdate = null;
            }
        }
    }
}
