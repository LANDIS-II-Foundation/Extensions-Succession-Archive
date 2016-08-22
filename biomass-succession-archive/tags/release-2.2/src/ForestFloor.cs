//  Copyright 2007 University of Nevada, University of Wisconsin
//  Authors:  Sarah Ganschow, Robert M. Scheller, James B. Domingo
//  License:  Available at  
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Util;
using System;
using System.Collections.Generic;

using Landis.Ecoregions;
using Landis.Landscape;
using Landis.Species;

namespace Landis.Biomass.Succession
{
    /// <summary>
    /// Soil organic matter (SOM) pool.
    /// Mass = C Fraction + N Fraction + P Fraction + other(inexplicit).
    /// </summary>
    public class ForestFloor
    {

        /// <summary>
        /// Adds some biomass for a species to the WOODY pools at a site.
        /// </summary>
        public static void AddWoody(double     woodyBiomass,
                                    ISpecies   species,
                                    ActiveSite site)
        {
        
            SiteVars.WoodyDebris[site].AddMass(woodyBiomass, 
                                    SpeciesData.WoodyDebrisDecay[species]);
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Adds some biomass for a species to the LITTER pools at a site.
        /// </summary>
        public static void AddLitter(double nonWoodyBiomass,
                                      ISpecies   species,
                                      ActiveSite site)
        {

            IEcoregion ecoregion = Model.Core.Ecoregion[site];
            double siteAET = (double)EcoregionData.AET[ecoregion]; 
            
            //Calculation of decomposition rate for species litter cohort
            // Decay rate from Meentemeyer 1978.  Ecology 59: 465-472.
            double leafKReg = (-0.5365 + (0.00241 * siteAET)) - (((-0.01586 + (0.000056 * siteAET)) * SpeciesData.LeafLignin[species] * 100));
            
            // From  Fan et al. 1998 Ecological Applications 8: 734-737:
            //double leafKReg = ((0.10015 * siteAET - 3.44618) - (0.01341 + 0.00147 * siteAET) *
            //SpeciesData.LeafLignin[species]) / 100;
            
            //UI.WriteLine("Decay rate for {0} within {1} = {2}.  LL = {3}.", species.Name, ecoregion.Name, leafKReg, SpeciesData.LeafLignin[species]);

            double decayValue = leafKReg;

            SiteVars.Litter[site].AddMass(nonWoodyBiomass, decayValue);

        }

    }
}
