//  Copyright 2006 University of Wisconsin
//  Authors:  
//      Jane Foster
//      Robert M. Scheller
//  Version 1.0
//  License:  Available at  
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

//using Landis.Biomass;
using Landis.Landscape;

namespace Landis.Biomass.Succession
{
    ///<summary>
    /// Site Variables for a disturbance plug-in that simulates Biological Agents.
    /// </summary>
    public static class SiteVars
    {
        private static ISiteVar<int> insectDefoliation;
        //private static ISiteVar<int> percentDefoliationLastYear;
        //private static ISiteVar<int> percentDefoliationTwoYearsAgo;
        //private static ISiteVar<int> percentDefoliationThreePlusYearsAgo;

        //---------------------------------------------------------------------

        public static void Initialize()
        {
            insectDefoliation  = Model.Core.GetSiteVar<int>("Insect.Defoliation");
        }

        //---------------------------------------------------------------------

        public static ISiteVar<int> InsectDefoliation
        {
            get {
                return insectDefoliation;
            }
        }
    }
}
