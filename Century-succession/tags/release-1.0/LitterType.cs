//  Copyright 2008 Conservation Biology Institute
//  Authors:  Robert M. Scheller
//  License:  Available at  
//  http://www.landis-ii.org/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Grids;
using Landis.AgeCohort;
using Landis.Landscape;
using Landis.PlugIns;
using Landis.Species;
using Landis.Util;
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
