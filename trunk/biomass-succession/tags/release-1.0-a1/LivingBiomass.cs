using Edu.Wisc.Forest.Flel.Util;

using Landis.Cohorts;
using Landis.Ecoregions;
using Landis.Landscape;
using Landis.Species;

using System;

namespace Landis.Biomass.Succession
{
    /// <summary>
    /// Calculations for aboveground living biomass
    /// </summary>
    /// <remarks>
    /// References:
    /// <list type="">
    ///   <item>
    ///   Scheller, R. M., Mladenoff, D. J., 2004.  A forest growth and biomass
    ///   module for a landscape simulation model, LANDIS:  Design, validation,
    ///   and application. Ecological Modelling 180(1):211-229.
    ///   </item>
    ///   <item>
    ///   Brown, S. L., Schroeder, P. E., 1999.  Spatial patterns of
    ///   aboveground production and mortality of woody biomass for eastern
    ///   U.S. forests.  Ecol. Appl. 9:968-980.
    ///   </item>
    /// </list>
    /// </remarks>
    public class LivingBiomass
    {
        private static ISiteVar<SiteCohorts> cohorts;
        private static int successionTimestep;

        //  Minimum relative biomass for each shade class in each ecoregion
        private static Ecoregions.AuxParm<Percentage>[] minRelativeBiomass;

        //  Leaf longevity for each species
        protected static Species.AuxParm<double> LeafLongevity;

        //  Bole decay rate for each species
        protected static Species.AuxParm<double> WoodyDecayRate;

        //  Shape parameter for the mortality curve for each species
        protected static Species.AuxParm<double> MortCurveShapeParm;

        //  Establishment probability for each species in each ecoregion
        protected static Species.AuxParm<Ecoregions.AuxParm<double>> EstablishProbability;

        //  Leaf litter decay rate for each species in each ecoregion
        protected static Species.AuxParm<Ecoregions.AuxParm<double>> LeafLitterDecayRate;

        //  Maximum ANPP for each species in each ecoregion
        protected static Species.AuxParm<Ecoregions.AuxParm<int>> ANPP_MAX_i;

        //  Maximum possible biomass for each species in each ecoregion
        protected static Species.AuxParm<Ecoregions.AuxParm<int>> B_MAX_i;

        //  Maximum biomass at any site in each ecoregion
        protected static Ecoregions.AuxParm<int> B_MAX;

        //---------------------------------------------------------------------

        public static void Initialize(IParameters           parameters,
                                      ISiteVar<SiteCohorts> cohorts)
        {
            minRelativeBiomass = parameters.MinRelativeBiomass; // FIXME

            LeafLongevity = parameters.LeafLongevity;
            WoodyDecayRate = parameters.WoodyDecayRate;
            MortCurveShapeParm = parameters.MortCurveShapeParm;

            ANPP_MAX_i = parameters.MaxANPP;
            EstablishProbability = parameters.EstablishProbability;
            LeafLitterDecayRate = parameters.LeafLitterDecayRate;

            B_MAX_i = new Species.AuxParm<Ecoregions.AuxParm<int>>(Model.Species);
            foreach (ISpecies species in Model.Species) {
                B_MAX_i[species] = new Ecoregions.AuxParm<int>(Model.Ecoregions);
                foreach (IEcoregion ecoregion in Model.Ecoregions) {
                    B_MAX_i[species][ecoregion] = ComputeMaxBiomass(ANPP_MAX_i[species][ecoregion]);
                }
            }

            B_MAX = new Ecoregions.AuxParm<int>(Model.Ecoregions);
            foreach (IEcoregion ecoregion in Model.Ecoregions) {
                int largest_ANPP_MAX_i = 0;
                foreach (ISpecies species in Model.Species) {
                    largest_ANPP_MAX_i = Math.Max(largest_ANPP_MAX_i,
                                                  ANPP_MAX_i[species][ecoregion]);
                }
                B_MAX[ecoregion] = ComputeMaxBiomass(largest_ANPP_MAX_i);
            }

            LivingBiomass.cohorts = cohorts;
            successionTimestep = parameters.Timestep;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Computes the maximum biomass from a maximum ANPP.  Units: Mg / ha
        /// </summary>
        /// <remarks>
        /// Equation 2 in Scheller and Mladenoff, 2004.  Estimated from Brown
        /// and Schroeder, 1999.
        /// </remarks>
        public static int ComputeMaxBiomass(int ANPP_MAX)
        {
            return ANPP_MAX * 30;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Computes the actual biomass at a site.  The biomass is the total
        /// of all the site's cohorts except young ones.  The total is limited
        /// to being no more than the site's maximum biomass less the previous
        /// year's mortality at the site.
        /// </summary>
        public static double ActualSiteBiomass(SiteCohorts    siteCohorts,
                                               ActiveSite     site,
                                               out IEcoregion ecoregion)
        {
            int youngBiomass;
            int totalBiomass = Cohorts.ComputeBiomass(siteCohorts, out youngBiomass);
            double B_ACT = totalBiomass - youngBiomass;

            int lastMortality = siteCohorts.PrevYearMortality;
            ecoregion = Model.SiteVars.Ecoregion[site];
            B_ACT = Math.Min( B_MAX[ecoregion] - lastMortality, B_ACT);

            return B_ACT;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Computes the shade at a site.
        /// </summary>
        /// <remarks>
        /// For details, see section 3.2.6 of Scheller and Mladenoff, 2004.
        /// </remarks>
        public static byte ComputeShade(ActiveSite site)
        {
            IEcoregion ecoregion;
            double B_ACT = ActualSiteBiomass(cohorts[site], site, out ecoregion);

            //  Relative living biomass (ratio of actual to maximum site
            //  biomass).
            double B_AM = B_ACT / B_MAX[ecoregion];

            for (byte shade = 5; shade >= 1; shade--) {
                if (B_AM >= minRelativeBiomass[shade][ecoregion])
                    return shade;
            }
            return 0;
        }
    }
}
