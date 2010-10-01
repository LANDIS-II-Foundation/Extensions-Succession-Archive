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
    /// The pool of dead biomass at a site, including decomposition values.
    /// </summary>
    public class PoolD : Pool
    {
        private double decayValue;
        private double limitValue;
        private double initialMass;

        //---------------------------------------------------------------------
        
        public PoolD()
        {
            this.decayValue = 0.0;
            this.limitValue = 0.0;
            this.initialMass = 0.0;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Litter pool decay rate.
        /// </summary>
        public double DecayValue
        {
            get
            {
                return decayValue;
            }
            set
            {
                decayValue = value;
            }
        }

        //---------------------------------------------------------------------
        
        /// <summary>
        /// Litter pool mass loss limit value.
        /// </summary>
        public double LimitValue
        {
            get
            {
                return limitValue;
            }
            set
            {
                limitValue = value;
            }
        }

        //---------------------------------------------------------------------
        
        /// <summary>
        /// Initial mass of litter pool.
        /// </summary>
        public double InitialMass
        {
            get
            {
                return initialMass;
            }
            set
            {
                initialMass = value;
            }
        }

        //---------------------------------------------------------------------
        
        /// <summary>
        /// Adds some dead biomass (and associated C/N/P contents and decomposition
        /// rate) to the pool.
        /// </summary>
        /// <remarks>
        /// The pool's decomposition rate is adjusted by computing a weighted
        /// average of the its current decay rate and the decay rate 
        /// associated with the incoming biomass.
        /// </remarks>
        public void AddMass(double inputMass,
                            double inputPercentC,
                            double inputPercentN,
                            double inputPercentP,
                            double inputDecayValue)
        {
            double totalMass = Mass + inputMass;

            if (totalMass == 0)
            {
                ContentC = 0;
                ContentN = 0;
                ContentP = 0;
                DecayValue = 0;
            }

            else
            {
                ContentC += (inputMass * inputPercentC);
                ContentN += (inputMass * inputPercentN);
                ContentP += (inputMass * inputPercentP);
                DecayValue = ((Mass * DecayValue) + (inputMass * 
                    inputDecayValue)) / totalMass;
            }

            Mass = totalMass;
        }
    }
}
