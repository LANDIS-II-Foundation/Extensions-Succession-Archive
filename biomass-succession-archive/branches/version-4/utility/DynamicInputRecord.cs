//  Copyright 2009 Conservation Biology Institute
//  Authors:  Robert M. Scheller
//  License:  Available at
//  http://www.landis-ii.org/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using System.Collections.Generic;

namespace Landis.Biomass.Succession
{
    /// <summary>
    /// Values for each ecoregion x species combination.
    /// </summary>
    public interface IDynamicInputRecord
    {

        double ProbEst{get;set;}
        int ANPP_MAX_Spp {get;set;}
        int B_MAX_Spp {get;set;}
    }

    public class DynamicInputRecord
    : IDynamicInputRecord
    {

        private double probEst;
        private int anpp_max_spp;
        private int b_max_spp;

        public double ProbEst
        {
            get {
                return probEst;
            }
            set {
                probEst = value;
            }
        }

        public int ANPP_MAX_Spp
        {
            get {
                return anpp_max_spp;
            }
            set {
                anpp_max_spp = value;
            }
        }

        public int B_MAX_Spp
        {
            get {
                return b_max_spp;
            }
            set {
                b_max_spp = value;
            }
        }




        public DynamicInputRecord()
        {
        }

    }
}
