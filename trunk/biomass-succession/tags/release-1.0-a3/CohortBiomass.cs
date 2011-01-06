using Edu.Wisc.Forest.Flel.Util;

using Landis.Ecoregions;
using Landis.Landscape;
using Landis.Species;

using System;

namespace Landis.Biomass.Succession
{
    /// <summary>
    /// Calculations for an individual cohort's biomass.
    /// </summary>
    /// <remarks>
    /// References:
    /// <list type="">
    ///     <item>
    ///     Crow, T. R., 1978.  Biomass and production in three contiguous
    ///     forests in northern Wisconsin. Ecology 59(2):265-273.
    ///     </item>
    ///     <item>
    ///     Niklas, K. J., Enquist, B. J., 2002.  Canonical rules for plant
    ///     organ biomass partitioning and annual allocation.  Amer. J. Botany
    ///     89(5): 812-819.
    ///     </item>
    /// </list>
    /// </remarks>
    public class CohortBiomass
        : LivingBiomass, Biomass.ICalculator
    {
        /// <summary>
        /// Computes the initial biomass for a cohort at a site.
        /// </summary>
        /// <remarks>
        /// Scheller, R. M. and Domingo, J. B. Biomass Succession (v1.0) for 
        /// LANDIS-II: User Guide.  Available online at
        /// http://landis.forest.wisc.edu/documentation.
        /// </remarks>
        public static ushort InitialBiomass(SiteCohorts siteCohorts,
                                            ActiveSite  site)
        {
            IEcoregion ecoregion;
            double B_ACT = ActualSiteBiomass(siteCohorts, site, out ecoregion);
            
            //  Initial biomass exponentially declines in response to
            //  competition.
            double initialBiomass = 0.025 * B_MAX[ecoregion] *
                                    Math.Exp(-1.6 * B_ACT / B_MAX[ecoregion]);

            //  Initial biomass cannot be less than 1.
            initialBiomass = Math.Max(1, initialBiomass);

            return (ushort) initialBiomass;
        }

        //---------------------------------------------------------------------

        //  Cohort's biomass
        private double B_ij;

        //  Cohort's species
        private ISpecies species;

        //  Site where cohort is located
        private ActiveSite site;

        //  Ecoregion where the cohort's site is located
        private IEcoregion ecoregion;

        //  Ratio of actual biomass to maximum biomass for the cohort.
        private double B_AP_ij;

        //  Maximum possible biomass for the cohort's species.
        private new double B_MAX_i;

        //  Potential biomass for the cohort -- the limit of the amount of
        //  biomass for the cohort based on the "growing space" available at
        //  the site.
        private double B_POT_ij;

        //  Ratio of potential biomass to maximum biomass for the cohort.
        private double B_PM_ij;

        //  Maximum ANPP for the cohort's species in the site's ecoregion
        private new double ANPP_MAX_i;

        //  Actual ANPP for the cohort
        private double ANPP_ACT_ij;

        //  Age-related mortality for the cohort
        private double M_AGE_ij;

        //  Cohort mortality due to stand biomass (stand development)
        private double M_BIO_ij;

        //  Total mortality for the cohort
        private double M_ij;

        //  Totaly mortality without annual leaf litter for the cohort
        private int M_noLeafLitter;

        //---------------------------------------------------------------------

        public int MortalityWithoutLeafLitter
        {
            get {
                return M_noLeafLitter;
            }
        }

        //---------------------------------------------------------------------

        public CohortBiomass()
        {
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Computes the change in a cohort's biomass due to Annual Net Primary
        /// Productivity (ANPP), age-related mortality (M_AGE), and development-
        /// related mortality (M_BIO).
        /// </summary>
        /// <param name="cohort">
        /// The cohort whose biomass the change is to be computed for.
        /// </param>
        /// <param name="site">
        /// The site where the cohort is located.
        /// </param>
        /// <param name="siteBiomass">
        /// The total biomass at the site.
        /// </param>
        /// <param name="prevYearSiteMortality">
        /// The total mortality at the site during the previous year.
        /// </param>
        /// <remarks>
        /// Mortality contributes to dead biomass pools.
        /// </remarks>
        public int ComputeChange(ICohort    cohort,
                                 ActiveSite site,
                                 int        siteBiomass,
                                 int        prevYearSiteMortality)
        {
            this.site = site;
            ecoregion = Model.SiteVars.Ecoregion[site];

            B_ij = cohort.Biomass;
            species = cohort.Species;

            B_MAX_i = LivingBiomass.B_MAX_i[species][ecoregion];
            ANPP_MAX_i = LivingBiomass.ANPP_MAX_i[species][ecoregion];
            ComputeActualANPP(siteBiomass, prevYearSiteMortality);

            //  M_AGE_ij: Age related mortality: wood only
            ComputeAgeMortality(cohort.Age);

            //  Age mortality is discounted from ANPP to prevent the over-
            //  estimation of mortality.
            ANPP_ACT_ij -= M_AGE_ij;
            //  Ensure that ANPP is not negative.
            ANPP_ACT_ij = Math.Max(0, ANPP_ACT_ij);

            //  M_BIO_ij: Growth-related mortality
            ComputeDevelopmentMortality();
            
            //  Age-related mortality is discounted from growth-related
            //  mortality to prevent the under-estimation of mortality.
            M_BIO_ij -= M_AGE_ij;
            //  Ensure that M_BIO_ij is not negative.
            M_BIO_ij = Math.Max(0, M_BIO_ij);

            //  Total mortality for the cohort
            M_ij = M_AGE_ij + M_BIO_ij;

            UpdateDeadBiomass();

            return (int) (ANPP_ACT_ij - M_ij);
        }

        //---------------------------------------------------------------------
        
        private void ComputeActualANPP(int siteBiomass,
                                       int prevYearSiteMortality)
        {
            //  Potential biomass, equation 3 in Scheller and Mladenoff, 2004
            B_POT_ij = Math.Max(0, B_MAX_i - siteBiomass) + B_ij;

            //  Species can use new space immediately
            B_POT_ij = Math.Max(B_POT_ij, prevYearSiteMortality + B_ij);

            //  Ratio of cohort's actual biomass to potential biomass
            B_AP_ij = B_ij / B_POT_ij;

            //  Ratio of cohort's potential biomass to maximum biomass.  The
            //  ratio cannot be exceed 1.
            B_PM_ij = B_POT_ij / B_MAX_i;
            B_PM_ij = Math.Min(1.0, B_PM_ij);

            //  Actual ANPP: equation (4) from Scheller & Mladenoff, 2004.
            //  Constants k1 and k2 control whether growth rate declines with
            //  age.
            const double k1 = 1.0;
            const double k2 = 1.0;
            ANPP_ACT_ij = ANPP_MAX_i *
                          (k1 * Math.E * Math.Pow(B_AP_ij, k2) * Math.Exp(-k1 * Math.Pow(B_AP_ij, k2))) *
                           B_PM_ij;

            //  Calculated actual ANPP can not exceed the limit set by the
            //  maximum ANPP times the ratio of potential to maximum biomass.
            ANPP_ACT_ij = Math.Min(ANPP_MAX_i * B_PM_ij, ANPP_ACT_ij);

            //  Actual ANPP cannot be less than 1 to ensure minimal growth
            ANPP_ACT_ij = Math.Max(1.0, ANPP_ACT_ij);
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Computes M_AGE_ij: the mortality caused by the aging of the cohort.
        /// </summary>
        /// <returns>
        /// Returns Woody biomass.  Leaf biomass not included.
        /// </returns>
        /// <remarks>
        /// See equation 6 in Scheller and Mladenoff, 2004.
        /// </remarks>
        private void ComputeAgeMortality(int age)
        {
            int max_age = species.Longevity;
            double d = MortCurveShapeParm[species];

            M_AGE_ij = Math.Exp((double) age / (double) max_age * d) / Math.Exp(d);

            if(M_AGE_ij > B_ij) M_AGE_ij = B_ij;          
            //M_AGE_ij = (double) B_ij * bio_fraction;
            //UI.WriteLine("d = {0}; Age= {1}; MaxAge = {2}; Age-Mortality = {3:0.00}; ", d, age, max_age, bio_fraction);
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Computes M_BIO_ij: the mortality caused by development processes,
        /// including self-thinning and loss of branches, twigs, etc.
        /// </summary>
        /// <returns>
        // Returns Woody biomass.  Leaf biomass not included.
        /// </returns>
        /// <remarks>
        /// See equation 5 in Scheller and Mladenoff, 2004.
        /// </remarks>
        private void ComputeDevelopmentMortality()
        {
            const double y0 = 0.01;
            const double r = 0.08;

            M_BIO_ij = ANPP_MAX_i *
                       (y0 / (y0 + (1 - y0) * Math.Exp(-r / y0 * B_AP_ij))) *
                       B_PM_ij;

            //  Mortality should not exceed the amount of living biomass
            M_BIO_ij = Math.Min(B_ij, M_BIO_ij);

            //  Mortality should not exceed potential growth rate
            M_BIO_ij = Math.Min(ANPP_MAX_i * B_PM_ij, M_BIO_ij);
        }

        //---------------------------------------------------------------------

        private void UpdateDeadBiomass()
        {
            Percentage nonwoodPercentage = ComputeNonwoodyPercentage();

            //  Assume that age-related mortality is divided proportionally
            //  between the woody mass and non-woody mass (Niklaus & Enquist,
            //  2002)
            double M_AGE_nonwood = M_AGE_ij * nonwoodPercentage;
            double M_AGE_wood = M_AGE_ij - M_AGE_nonwood;

            //  Approximately 35% of ANPP goes to creating leaves annually.
            //  pg. 817 Niklas and Enquist 2002.
            //  Applies to both angio and conifer.
            double ANPP_leaf = ANPP_ACT_ij * 0.35;
            
            //  Approximately 3.5% of ANPP goes to early leaf fall, bud scales,
            //  seed production, and herbivores (Crow 1978).
            double ANPP_other = ANPP_ACT_ij * 0.035;

            //  Assume that growth-related mortality is divided proportionally
            //  between the woody mass and non-woody mass. (Niklaus & Enquist,
            //  2002)  Do not include annual leaf loss here.
            double M_BIO_noLeafLitter = M_BIO_ij - ANPP_leaf - ANPP_other;
            double M_BIO_nonwood = M_BIO_noLeafLitter * nonwoodPercentage;
            //  Ensure non-woody mass is not negative
            M_BIO_nonwood = Math.Max(0, M_BIO_nonwood);
            double M_BIO_wood = M_BIO_noLeafLitter - M_BIO_nonwood;

            //  Mortality of woody components
            double M_wood = M_AGE_wood + M_BIO_wood;
            //  Mortality of non-woody components, including annual leaf and
            //  seed production
            double M_nonwood = M_AGE_nonwood + M_BIO_nonwood + ANPP_leaf + ANPP_other;

            //  Add mortality to dead biomass pools.
            Dead.Pools.AddBiomass((ushort) M_wood, (ushort) M_nonwood, species, site);

            //  Total mortality not including annual leaf litter
            M_noLeafLitter = (int) (M_wood + M_AGE_nonwood + M_BIO_nonwood);
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Computes the percentage of the cohort's biomass is leaf litter or
        /// other non-woody components.
        /// </summary>
        /// <remarks>
        /// Assumption is that remainder is coarse woody debris.
        /// </remarks>
        private Percentage ComputeNonwoodyPercentage()
        {
            //  Approximately 35% of all growth goes to creating leaves
            //  annually (pg. 817, Niklas and Enquist 2002)
            //  Applies to both angio and conifer.
            double B_leaf = ANPP_ACT_ij * 0.35 * LeafLongevity[species];

            //  Approximately 3.5% of aboveground production goes to early leaf
            //  fall, bud scales, seed production, and herbivores (Crow 1978).
            double B_nonwoody = B_leaf + (ANPP_ACT_ij * 0.035);

            //  Non-woody cannot be less than 2.5% or greater than 35% of total
            //  biomass for a cohort.
            B_nonwoody = Math.Max(B_nonwoody, B_ij * 0.025);
            B_nonwoody = Math.Min(B_nonwoody, B_ij * 0.35);
            
            return new Percentage(B_nonwoody / B_ij);
        }
    }
}
