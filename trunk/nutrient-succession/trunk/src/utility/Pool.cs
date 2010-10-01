//  Copyright 2005 University of Nevada, University of Wisconsin
//  Authors:  Sarah Ganschow, Robert M. Scheller, James B. Domingo
//  License:  Available at  
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Util;

using Landis;
using Landis.Ecoregions;
using Landis.Landscape;
using Landis.Species;

using System;

namespace Landis.Biomass.NuCycling.Succession
{
    /// <summary>
    /// The pool of dead biomass at a site.
    /// </summary>
    public class Pool
    {
        private double mass;
        private double contentC;
        private double contentN;
        private double contentP;

        //---------------------------------------------------------------------
        
        public Pool()
        {
            this.mass = 0;
            this.contentC = 0;
            this.contentN = 0;
            this.contentP = 0;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Litter pool biomass.
        /// </summary>
        public double Mass
        {
            get
            {
                return mass;
            }
            set
            {
                mass = value;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Litter pool carbon content.
        /// </summary>
        public double ContentC
        {
            get
            {
                return contentC;
            }
            set
            {
                contentC = value;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Litter pool nitrogen content.
        /// </summary>
        public double ContentN
        {
            get
            {
                return contentN;
            }
            set
            {
                contentN = value;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Litter pool phosphorus content.
        /// </summary>
        public double ContentP
        {
            get
            {
                return contentP;
            }
            set
            {
                contentP = value;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Adds some dead biomass (and associated C/N/P contents) to the pool.
        /// </summary>
        public void AddMass(double inputMass,
                            double inputPercentC,
                            double inputPercentN,
                            double inputPercentP)
        {
            double totalMass = mass + inputMass;

            if (totalMass == 0)
            {
                contentC = 0;
                contentN = 0;
                contentP = 0;
            }

            else
            {
                contentC += (inputMass * inputPercentC);
                contentN += (inputMass * inputPercentN);
                contentP += (inputMass * inputPercentP);
            }

            mass = totalMass;
        }

        //---------------------------------------------------------------------
        
        /// <summary>
        /// Reduces the pool's biomass by a specified percentage.
        /// </summary>
        public double ReducePercentage(double percentage)
        {
            if (percentage < 0.0 || percentage > 1.0)
                throw new ArgumentException("Percentage must be between 0% and 100%");
            double reduction = (mass * percentage);
            mass -= (mass * percentage);
            contentC -= (contentC * percentage);
            contentN -= (contentN * percentage);
            contentP -= (contentP * percentage);

            return reduction;
        }

        //---------------------------------------------------------------------
        

    }
}
