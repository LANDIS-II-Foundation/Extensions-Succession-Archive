//  Copyright 2009 Conservation Biology Institute
//  Authors:  Robert M. Scheller
//  License:  Available at  
//  http://www.landis-ii.org/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Util;
using Landis.Biomass;  

namespace Landis.Extension.Succession.Century.AgeOnlyDisturbances
{
    /// <summary>
    /// The event handlers when no biomass parameters have been provided by
    /// the user.  
    /// </summary>
    public static class NoParameters
    {
        public static void CohortDied(object         sender,
                                      DeathEventArgs eventArgs)
        {
            ThrowException();
        }

        //---------------------------------------------------------------------

        public static void SiteDisturbed(object               sender,
                                         DisturbanceEventArgs eventArgs)
        {
            ThrowException();
        }

        //---------------------------------------------------------------------

        private static void ThrowException()
        {
            string[] mesg = new string[] {
                "Error:  An age-only disturbance has occurred, but no biomass",
                "        parameters were provided for age-only disturbances."
            };
            throw new MultiLineException(new MultiLineText(mesg));
        }
    }
}
