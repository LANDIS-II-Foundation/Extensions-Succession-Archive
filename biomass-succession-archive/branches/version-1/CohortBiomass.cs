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
                                            ActiveSite  site,
                                            ISpecies species)
        {
            IEcoregion ecoregion;
            double B_ACT = ActualSiteBiomass(siteCohorts, site, out ecoregion);
            
            //  Initial biomass exponentially declines in response to
            //  competition.
            double initialBiomass = 0.025 * LivingBiomass.B_MAX_i[species][ecoregion] * //B_MAX[ecoregion] *
                                    Math.Exp(-1.6 * B_ACT / B_MAX[ecoregion]);

            //  Initial biomass cannot be less than 1.
            initialBiomass = Math.Max(1.0, initialBiomass);

            return (ushort) initialBiomass;
        }

        //  Ecoregion where the cohort's site is located
        private IEcoregion ecoregion;

        //  Ratio of actual biomass to maximum biomass for the cohort.
        private double B_AP;

        //  Ratio of potential biomass to maximum biomass for the cohort.
        private double B_PM;

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
        public int ComputeChange(ICohort    cohort,
                                 ActiveSite site,
                                 int        siteBiomass,
                                 int        prevYearSiteMortality)
        {
        
            ecoregion = Model.Core.Ecoregion[site];
            
            // First, calculate age-related mortality.
            // Age-related mortality will include woody and standing leaf biomass (=0 for deciduous trees).
            double mortalityAge = ComputeAgeMortality(cohort);
            
            double actualANPP = ComputeActualANPP(cohort, site, siteBiomass, prevYearSiteMortality);
            
            //  Age mortality is discounted from ANPP to prevent the over-
            //  estimation of mortality.  ANPP cannot be negative.
            actualANPP = Math.Max(0, actualANPP - mortalityAge);
                        
            //  Growth-related mortality
            double mortalityGrowth = ComputeGrowthMortality(cohort, site);

            //  Age-related mortality is discounted from growth-related
            //  mortality to prevent the under-estimation of mortality.  Cannot be negative.
            mortalityGrowth = Math.Max(0, mortalityGrowth - mortalityAge);
            
            //  Also ensure that growth mortality does not exceed actualANPP.
            mortalityGrowth = Math.Min(mortalityGrowth, actualANPP);
            
            //  Total mortality for the cohort
            double totalMortality = mortalityAge + mortalityGrowth;
            
            if(totalMortality > cohort.Biomass)
                throw new ApplicationException("Error: Mortality exceeds cohort biomass");

            int deltaBiomass = (int) (actualANPP - totalMortality);
            double newBiomass =  cohort.Biomass + (double) deltaBiomass;
            
            double totalLitter = UpdateDeadBiomass(cohort, actualANPP, totalMortality, site, newBiomass);

            //UI.WriteLine("Age={0}, ANPPact={1:0.0}, M={2:0.0}, litter={3:0.00}.", cohort.Age, actualANPP, totalMortality, totalLitter);
            
            return deltaBiomass;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Computes M_AGE_ij: the mortality caused by the aging of the cohort.
        /// See equation 6 in Scheller and Mladenoff, 2004.
        /// </summary>
        private double ComputeAgeMortality(ICohort cohort)
        {
            double max_age = (double) cohort.Species.Longevity;
            double d = MortCurveShapeParm[cohort.Species];

            double M_AGE = cohort.Biomass * Math.Exp((double) cohort.Age / max_age * d) / Math.Exp(d);
            
            M_AGE = Math.Min(M_AGE, cohort.Biomass);
                
            return M_AGE;
        }

        //---------------------------------------------------------------------
        
        private double ComputeActualANPP(ICohort    cohort,
                                         ActiveSite site,
                                         int        siteBiomass,
                                         int        prevYearSiteMortality)
        {
            double cohortBiomass = cohort.Biomass;

            double maxBiomass  = LivingBiomass.B_MAX_i[cohort.Species][ecoregion];
            double maxANPP     = LivingBiomass.ANPP_MAX_i[cohort.Species][ecoregion];

            //  Potential biomass, equation 3 in Scheller and Mladenoff, 2004
            double potentialBiomass = Math.Max(0, maxBiomass - siteBiomass) + cohortBiomass;

            //  Species can use new space immediately
            potentialBiomass = Math.Max(potentialBiomass, prevYearSiteMortality + cohortBiomass);

            //  Ratio of cohort's actual biomass to potential biomass
            B_AP = cohortBiomass / potentialBiomass;

            //  Ratio of cohort's potential biomass to maximum biomass.  The
            //  ratio cannot be exceed 1.
            B_PM = Math.Min(1.0, potentialBiomass / maxBiomass);

            //  Actual ANPP: equation (4) from Scheller & Mladenoff, 2004.
            //  Constants k1 and k2 control whether growth rate declines with
            //  age.  Set to default = 1.
            double actualANPP = maxANPP * Math.E * B_AP * Math.Exp(-1 * B_AP) * B_PM;
            
            // Calculated actual ANPP can not exceed the limit set by the
            //  maximum ANPP times the ratio of potential to maximum biomass.
            //  This down regulates actual ANPP by the available growing space.
            
            actualANPP = Math.Min(maxANPP * B_PM, actualANPP);

            return actualANPP;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The mortality caused by development processes,
        /// including self-thinning and loss of branches, twigs, etc.
        /// See equation 5 in Scheller and Mladenoff, 2004.
        /// </summary>
        private double ComputeGrowthMortality(ICohort cohort, ActiveSite site)
        {
            const double y0 = 0.01;
            const double r = 0.08;
            double maxANPP = LivingBiomass.ANPP_MAX_i[cohort.Species][ecoregion];

            double M_BIO = maxANPP *
                    (y0 / (y0 + (1 - y0) * Math.Exp(-r / y0 * B_AP))) *
                    B_PM;

            //  Mortality should not exceed the amount of living biomass
            M_BIO = Math.Min(cohort.Biomass, M_BIO);

            // Calculated actual ANPP can not exceed the limit set by the
            //  maximum ANPP times the ratio of potential to maximum biomass.
            //  This down regulates actual ANPP by the available growing space.
            
            M_BIO = Math.Min(maxANPP * B_PM, M_BIO);

            return M_BIO;
            
        }

        //---------------------------------------------------------------------

        private double UpdateDeadBiomass(ICohort cohort, double actualANPP, double totalMortality, ActiveSite site, double newBiomass)
        {
        
            ISpecies species     = cohort.Species;
            double leafLongevity = LeafLongevity[species];
            double cohortBiomass = newBiomass; // Mortality is for the current year's biomass.
            double leafFraction  = ComputeFractionANPPleaf(species);
            
            // First, deposit the a portion of the leaf mass directly onto the forest floor.
            // In this way, the actual amount of leaf biomass is added for the year.
            // In addition, add the equivalent portion of fine roots to the surface layer.
            
            // 0.8 was used to calibrate the model to steady-state Nitrogen.  Without this reduction, total N
            // increases by 0.038% each year.  
            
            double annualLeafANPP = actualANPP * leafFraction * 0.8;
            Dead.Pools.AddBiomass(0, (ushort) annualLeafANPP, species, site);
            
            // --------------------------------------------------------------------------------
            // The next section allocates mortality from standing (wood and leaf) biomass, i.e., 
            // biomass that has accrued from previous years' growth.

            // Subtract annual leaf growth as that was taken care of above.            
            totalMortality -= annualLeafANPP;
            
            // Assume that standing foliage is equal to this years annualLeafANPP * leaf longevity
            // minus this years leaf ANPP.  This assumes that actual ANPP has been relatively constant 
            // over the past 2 or 3 years (if coniferous).
            
            double standing_nonwood = (annualLeafANPP * leafLongevity) - annualLeafANPP; 
            double standing_wood    = Math.Max(0, cohortBiomass - standing_nonwood);
            
            double fractionStandingNonwood = standing_nonwood / cohortBiomass;
            
            //  Assume that the remaining mortality is divided proportionally
            //  between the woody mass and non-woody mass (Niklaus & Enquist,
            //  2002).   Do not include current years growth.
            double mortality_nonwood = Math.Max(0.0, totalMortality * fractionStandingNonwood) ;
            double mortality_wood    = Math.Max(0.0, totalMortality - mortality_nonwood);
            
            if(mortality_wood < 0 || mortality_nonwood < 0)
                throw new ApplicationException("Error: Woody input is < 0");
                
            //  Add mortality to dead biomass pools.
            Dead.Pools.AddBiomass((ushort) mortality_wood, (ushort) mortality_nonwood, species, site);

            //  Total mortality not including annual leaf litter
            M_noLeafLitter = (int) (mortality_wood);
            
            return (annualLeafANPP + mortality_nonwood + mortality_wood);

        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Computes the cohort's biomass that is leaf litter
        /// or other non-woody components.  Assumption is that remainder is woody.
        /// </summary>
        public static double ComputeStandingLeafBiomass(double ANPPactual, ICohort cohort)
        {

            double annualLeafFraction = ComputeFractionANPPleaf(cohort.Species);
            
            double annualFoliar = ANPPactual * annualLeafFraction;

            double B_nonwoody   = annualFoliar * LeafLongevity[cohort.Species];  
            
            //  Non-woody cannot be less than 2.5% or greater than leaf fraction of total
            //  biomass for a cohort.
            B_nonwoody = Math.Max(B_nonwoody, cohort.Biomass * 0.025);
            B_nonwoody = Math.Min(B_nonwoody, cohort.Biomass * annualLeafFraction);
            
            return B_nonwoody;
        }
        //---------------------------------------------------------------------

        public static double ComputeFractionANPPleaf(ISpecies species)
        {

            //  A portion of growth goes to creating leaves (Niklas and Enquist 2002).
            //  Approximate for angio and conifer:
            //  pg. 817, growth (G) ratios for leaf:stem (Table 4) = 0.54 or 35% leaf
            
            double leafFraction = 0.35;
            
            //  Approximately 3.5% of aboveground production goes to early leaf
            //  fall, bud scales, seed production, and herbivores (Crow 1978).
            //leafFraction += 0.035;
            
            return leafFraction;
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// Computes the percentage of a cohort's standing biomass that is non-woody.
        /// This method is designed for external disturbance calls that need to 
        /// estimate the amount of non-wood biomass.
        /// </summary>
        
        public Percentage ComputeNonWoodyPercentage(ICohort     cohort,
                                                    ActiveSite  site)
        {
            SiteCohorts siteCohorts = LivingBiomass.LandscapeCohorts[site];
            
            double actualANPP = ComputeActualANPP(cohort, site, siteCohorts.TotalBiomass,
                                siteCohorts.PrevYearMortality);
                                
            double mortalityAge = ComputeAgeMortality(cohort);
            
            //  Age mortality is discounted from ANPP to prevent the over-
            //  estimation of mortality.  ANPP cannot be negative.
            actualANPP = Math.Max(0, actualANPP - mortalityAge);
            
            return new Percentage(ComputeStandingLeafBiomass(actualANPP, cohort) / cohort.Biomass);
        }
        //---------------------------------------------------------------------
        /*
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
        private double B_MAX_Spp;

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

        // The percentage of a cohort that is foliar biomass.
        private Percentage standingFoliarPercentage;

        // The percentage of ANPP that is foliar biomass.
        private Percentage growthFoliarPercentage;

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
            ComputeAdjustedANPP(cohort, site, siteBiomass, prevYearSiteMortality);

            standingFoliarPercentage = ComputeStandingFoliarPercentage(ANPP_ACT_ij, B_ij, species);
            
            growthFoliarPercentage = ComputeGrowthFoliarPercentage(ANPP_ACT_ij, B_ij, species);

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

        /// <summary>
        /// Computes the Annual Net Primary Productivity (ANPP) for a cohort,
        /// adjusted for age-related mortality (M_AGE).
        /// </summary>
        private void ComputeAdjustedANPP(ICohort    cohort,
                                         ActiveSite site,
                                         int        siteBiomass,
                                         int        prevYearSiteMortality)
        {
            this.site = site;
            ecoregion = Model.Core.Ecoregion[site];

            B_ij = cohort.Biomass;
            species = cohort.Species;

            B_MAX_Spp = LivingBiomass.B_MAX_i[species][ecoregion];
            ANPP_MAX_i = LivingBiomass.ANPP_MAX_i[species][ecoregion];
            ComputeActualANPP(siteBiomass, prevYearSiteMortality);

            //  M_AGE_ij: Age related mortality: wood only
            ComputeAgeMortality(cohort.Age);

            //  Age mortality is discounted from ANPP to prevent the over-
            //  estimation of mortality.
            ANPP_ACT_ij -= M_AGE_ij;
            //  Ensure that ANPP is not negative.
            ANPP_ACT_ij = Math.Max(0, ANPP_ACT_ij);
        }

        //---------------------------------------------------------------------
        
        private void ComputeActualANPP(int siteBiomass,
                                       int prevYearSiteMortality)
        {
            //  Potential biomass, equation 3 in Scheller and Mladenoff, 2004
            B_POT_ij = Math.Max(0, B_MAX_Spp - siteBiomass) + B_ij;

            //  Species can use new space immediately
            B_POT_ij = Math.Max(B_POT_ij, prevYearSiteMortality + B_ij);

            //  Ratio of cohort's actual biomass to potential biomass
            B_AP_ij = B_ij / B_POT_ij;

            //  Ratio of cohort's potential biomass to maximum biomass.  The
            //  ratio cannot be exceed 1.
            B_PM_ij = B_POT_ij / B_MAX_Spp;
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

            M_AGE_ij = B_ij * Math.Exp((double) age / (double) max_age * d) / Math.Exp(d);

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
            if (standingFoliarPercentage > 1.0 || standingFoliarPercentage < 0.0)
                throw new ApplicationException("Error: nonwoodPercentage is NOT between 1.0 and 0.0");
                
            if (growthFoliarPercentage > 1.0 || growthFoliarPercentage < 0.0)
                throw new ApplicationException("Error: nonwoodPercentage is NOT between 1.0 and 0.0");

            double ANPP_nonwood   = ANPP_ACT_ij * growthFoliarPercentage;
            
            double percentFoliarNoANPP = standingFoliarPercentage - growthFoliarPercentage;

            //  Assume that age-related mortality is divided proportionally
            //  between the woody mass and non-woody mass (Niklaus & Enquist,
            //  2002).   Do not include current years growth.
            double M_AGE_nonwood      = Math.Max(0, M_AGE_ij * percentFoliarNoANPP);
            double M_AGE_wood         = Math.Max(0, M_AGE_ij - M_AGE_nonwood);
            

            //  Assume that growth-related mortality is divided proportionally
            //  between the woody mass and non-woody mass. (Niklaus & Enquist,
            //  2002)  Do not include annual leaf loss here.
            double M_BIO_nonwood      = Math.Max(0, M_BIO_ij * percentFoliarNoANPP);
            double M_BIO_wood         = Math.Max(0, M_BIO_ij - M_BIO_nonwood);

            //  Mortality of woody components
            double M_wood = M_AGE_wood + M_BIO_wood;
            
            //  Mortality of non-woody components, including annual leaf and
            //  seed production
            double M_nonwood = M_AGE_nonwood + M_BIO_nonwood + ANPP_nonwood;

            if(M_wood < 0 || M_nonwood < 0)
                throw new ApplicationException("Error: Woody input is < 0");

            //  Add mortality to dead biomass pools.
            Dead.Pools.AddBiomass((ushort) M_wood, (ushort) M_nonwood, species, site);

            //  Total mortality not including annual leaf litter
            M_noLeafLitter = (int) (M_wood);

            /*double nonwoodPercentage = (double) ComputeNonwoodyPercentage();
            
            if(nonwoodPercentage > 1.0 || nonwoodPercentage < 0)
                throw new ApplicationException("Error: nonwoodPercentage is NOT between 1.0 and 0.0");

            //  Assume that age-related mortality is divided proportionally
            //  between the woody mass and non-woody mass (Niklaus & Enquist,
            //  2002)
            double M_AGE_nonwood = M_AGE_ij * nonwoodPercentage;
            double M_AGE_wood = M_AGE_ij - M_AGE_nonwood;

            //UI.WriteLine("   M_AGE_nonwoody={0:0.00}, M_AGE_woody={1:0.00}.", M_AGE_nonwood, M_AGE_wood);


            //  Approximately 35% of ANPP goes to creating leaves annually.
            //  pg. 817 Niklas and Enquist 2002.
            //  Applies to both angio and conifer.
            double ANPP_leaf = ANPP_ACT_ij * 0.35;
            
            //  Approximately 3.5% of ANPP goes to early leaf fall, bud scales,
            //  seed production, and herbivores (Crow 1978).
            double ANPP_other = ANPP_ACT_ij * 0.035;
            
            //  Non-woody cannot be less than 2.5% or greater than 35% of total
            //  biomass for a cohort.
            double ANPP_litter = ANPP_leaf + ANPP_other;
            ANPP_litter = Math.Max(ANPP_litter, B_ij * 0.025);
            ANPP_litter = Math.Min(ANPP_litter, B_ij * 0.35);
            
            //  Assume that growth-related mortality is divided proportionally
            //  between the woody mass and non-woody mass. (Niklaus & Enquist,
            //  2002)  Do not include annual leaf loss here.
            double M_BIO_noLeafLitter = Math.Max(0, M_BIO_ij - ANPP_litter);
            double M_BIO_nonwood = M_BIO_noLeafLitter * nonwoodPercentage;

            //UI.WriteLine("   M_BIO_noLeaf={0:0.00}, M_BIO_nonwood={1:0.00}, M_BIO_ij={2:0.00}.", M_BIO_noLeafLitter, M_BIO_nonwood, M_BIO_ij);

            
            //  Ensure non-woody mass is not negative
            M_BIO_nonwood = Math.Max(0, M_BIO_nonwood);
            double M_BIO_wood = M_BIO_noLeafLitter - M_BIO_nonwood;

            //  Mortality of woody components
            double M_wood = M_AGE_wood + M_BIO_wood;
            //  Mortality of non-woody components, including annual leaf and
            //  seed production
            double M_nonwood = M_AGE_nonwood + M_BIO_nonwood + ANPP_leaf + ANPP_other;
            
            //UI.WriteLine("   Added Dead Biomass: Woody={0:0.00}, NonWoody={1:0.00}, Species={2:0.00}.", M_wood, M_nonwood, species.Name);

            if(M_wood < 0 || M_nonwood < 0)
                throw new ApplicationException("Error: Woody input is < 0");



        }
*/
        //---------------------------------------------------------------------

        /// <summary>
        /// Computes the percentage of the cohort's biomass that is leaf litter
        /// or other non-woody components.
        /// </summary>
        /// <remarks>
        /// Assumption is that remainder is coarse woody debris.
        /// </remarks>
        /*private Percentage ComputeNonwoodyPercentage()
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
            
            return new Percentage(B_nonwoody / (double) B_ij);
        }*/
        //---------------------------------------------------------------------
/*
        /// <summary>
        /// Computes the percentage of the cohort's biomass that is leaf litter
        /// or other non-woody components.  Assumption is that remainder is woody.
        /// </summary>
        public static Percentage ComputeStandingFoliarPercentage(double ANPPactual, double Bcohort, ISpecies species)
        {
        
            double annualFoliar = ComputeGrowthFoliarPercentage(ANPPactual, Bcohort, species);

            double B_nonwoody   = Math.Min(Bcohort, annualFoliar * LeafLongevity[species]);  
            
            return new Percentage(B_nonwoody / Bcohort);
        }
        //---------------------------------------------------------------------

        public static Percentage ComputeGrowthFoliarPercentage(double ANPPactual, double Bcohort, ISpecies species)
        {

            //  A portion of growth goes to creating leaves (Niklas and Enquist 2002).
            //  Approximate for angio and conifer:
            //  pg. 817, growth (G) ratios for leaf:stem:root
            //      are 32:50:9 for angiosperms (35% leaf) and 48:35:17 for conifers (48% leaf).
            
            double leafFraction = 0.35;
            
            if(LeafLongevity[species] > 1.0) leafFraction = 0.48;
            
            double B_leaf = ANPPactual * leafFraction;

            //  Approximately 3.5% of aboveground production goes to early leaf
            //  fall, bud scales, seed production, and herbivores (Crow 1978).
            double B_nonwoody = B_leaf + (ANPPactual * 0.035);

            //  Non-woody cannot be less than 2.5% or greater than (leaf fraction + seed fraction) of total
            //  biomass for a cohort.
            B_nonwoody = Math.Max(B_nonwoody, Bcohort * 0.025);
            B_nonwoody = Math.Min(B_nonwoody, Bcohort * (leafFraction + 0.025));
            B_nonwoody = Math.Min(B_nonwoody, Bcohort);
            
            return new Percentage(B_nonwoody / Bcohort);
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// Computes the percentage of a cohort's standing biomass that is non-woody.
        /// </summary>
        
        public Percentage ComputeNonWoodyPercentage(ICohort     cohort,
                                                    ActiveSite  site)
        {
            SiteCohorts siteCohorts = LivingBiomass.LandscapeCohorts[site];
            
            ComputeAdjustedANPP(cohort, site, siteCohorts.TotalBiomass,
                                siteCohorts.PrevYearMortality);
            
            return ComputeStandingFoliarPercentage(ANPP_ACT_ij, B_ij, cohort.Species);
        }*/

    }
}
