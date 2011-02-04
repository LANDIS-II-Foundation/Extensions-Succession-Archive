//  Copyright 2009 Conservation Biology Institute
//  Authors:  Robert M. Scheller
//  License:  Available at  
//  http://www.landis-ii.org/developers/LANDIS-IISourceCodeLicenseAgreement.pdf
using Landis.Biomass;  

namespace Landis.Extension.Succession.Century.AgeOnlyDisturbances
{
    /// <summary>
    /// The public interface for the module that handles age-only disturbances.
    /// </summary>
    public static class Module
    {
        private static IParameterDataset parameters;

        //---------------------------------------------------------------------

        /// <summary>
        /// The collection of biomass parameters for age-only disturbances.
        /// </summary>
        internal static IParameterDataset Parameters
        {
            get {
                return parameters;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Initializes the module.
        /// </summary>
        /// <param name="filename">
        /// The path of the file with the biomass parameters for age-only
        /// disturbances.  null if no file was specified by user.
        /// </param>
        public static void Initialize(string filename)
        {
            if (filename != null) {
                UI.WriteLine("   Loading biomass parameters for age-only disturbances from file \"{0}\" ...", filename);
                DatasetParser parser = new DatasetParser();
                parameters = Data.Load<IParameterDataset>(filename, parser);

                Cohort.AgeOnlyDeathEvent += Events.CohortDied;
                SiteCohorts.AgeOnlyDisturbanceEvent += Events.SiteDisturbed;
            }
            else {
                parameters = null;
                Cohort.AgeOnlyDeathEvent += NoParameters.CohortDied;
                SiteCohorts.AgeOnlyDisturbanceEvent += NoParameters.SiteDisturbed;
            }
        }
    }
}
