//  Copyright 2007 University of Nevada, University of Wisconsin
//  Authors:  Sarah Ganschow, Robert M. Scheller, James B. Domingo
//  License:  Available at
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Util;

namespace Landis.Biomass.NuCycling.Succession
{
    public interface ISeverity
    {
        //byte Number { get; set;}
        double LitterReduction { get;set;}
        double WoodyDebrisReduction { get;set;}
    }

    /// <summary>
    /// Definition of a fire severity.
    /// </summary>
    public class Severity
        : ISeverity
    {
        //private byte number;
        private double litterReduction;
        private double woodyDebrisReduction;

        //---------------------------------------------------------------------

        /// <summary>
        /// The severity's number (between 1 and 254).
        /// </summary>
        /*public byte Number
        {
            get
            {
                return number;
            }
        }*/

        //---------------------------------------------------------------------

        public double LitterReduction
        {
            get
            {
                return litterReduction;
            }
            set
            {
                if (value < 0.0 || value > 1.0)
                        throw new InputValueException(value.ToString(), "Value must be between 0 and 1");
                litterReduction = value;
            }
        }

        //---------------------------------------------------------------------

        public double WoodyDebrisReduction
        {
            get
            {
                return woodyDebrisReduction;
            }
            set
            {
                if (value < 0.0 || value > 1.0)
                        throw new InputValueException(value.ToString(), "Value must be between 0 and 1.0");
                woodyDebrisReduction = value;
            }
        }

        //---------------------------------------------------------------------

        public Severity()
        {
        }
        //---------------------------------------------------------------------
/*
        public Severity(byte number,
                        double litterReduction,
                        double woodyDebrisReduction)
        {
            this.number = number;
            this.litterReduction = litterReduction;
            this.woodyDebrisReduction = woodyDebrisReduction;
        }*/
    }
}
