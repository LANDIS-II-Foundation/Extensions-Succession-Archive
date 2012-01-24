//  Copyright 2007-2010 Portland State University, University of Wisconsin-Madison
//  Author: Robert Scheller, Ben Sulman

using Landis.Core;
using Landis.SpatialModeling;
using Landis.Library.LeafBiomassCohorts;
using System.Collections.Generic;


namespace Landis.Extension.Succession.Century
{


    
    public class LitterType
    {
        public double DecayRateStrucC;
        public double DecayRateMetabolicC;
        public double DecayRateMicrobes;

        //public static double maxStructuralC;    
        //---------------------------------------------------------------------
        /*public double DecayRateStrucC
        {
            get {
                return decayRateStrucC;
            }
        }
        //---------------------------------------------------------------------
        public double DecayRateMetabolicC
        {
            get {
                return decayRateMetabolicC;
            }
        }
        //---------------------------------------------------------------------
        public double DecayRateMicrobes
        {
            get {
                return decayRateMicrobes;
            }
        }
        //---------------------------------------------------------------------
        public double MaxStructuralC
        {
            get {
                return maxStructuralC;
            }
        }*/
        //---------------------------------------------------------------------

        /*public LitterType(
            double decayRateStrucC,
            double decayRateMetabolicC,
            double decayRateMicrobes,
            double maxStructuralC
            )
        {
            this.decayRateStrucC = decayRateStrucC;
            this.decayRateMetabolicC = decayRateMetabolicC;
            this.decayRateMicrobes = decayRateMicrobes;
            this.maxStructuralC = maxStructuralC;
        }*/

        //---------------------------------------------------------------------

        public LitterType()
        {
            this.DecayRateStrucC = 0.0;
            this.DecayRateMetabolicC = 0.0;
            this.DecayRateMicrobes = 0.0;
            //this.maxStructuralC = 0;
        }


    }
}
