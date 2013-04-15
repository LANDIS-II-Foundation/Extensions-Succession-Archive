using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Landis.Library.Climate
{
    public class PDSI_Calculator
    {
        protected AnnualClimate[] _AnnualClimates;
        private double[] mon_T_normal;
        private Potential[] _Potential;// = new Potential[12];
        public int Verbose { get; set; }
        protected AnnualClimate annClimate;
        protected int annClimateIndex;
        public PDSI_Calculator()
        {
            period_length = 1;
            num_of_periods = 12;
            Verbose = 1;
            bug = 0;
            output_mode = 0;
            tolerance = 0.00001;
            metric = 0;
            nadss = 0;
            setCalibrationStartYear = 0;
            setCalibrationEndYear = 0;

            int nCalibrationYears = totalyears;

            //in C++ PDSI mon_T_normal is read from the mon_T_normal text file 
            //mon_T_normal = new double[12] { 19.693, 23.849, 34.988, 49.082, 60.467, 70.074, 75.505, 73.478, 64.484, 52.634, 36.201, 24.267 };
        }

        //===========================================PDSI Attributes=====================================================
        public const double MISSING = -99.00;
        //int Verbose = ;
        // The variables for storing the starting year of calculation and the total number of years to calculate
        int startyear;
        int endyear;
        int totalyears;

        //preserve period_length and num_of_periods for multiple week PDSI's.
        int period_length = 1;        //set to 1 for monthly, otherwise, legth of period
        int num_of_periods = 12;//12;       I gave 1 so the PDSI calculation is not done for all 12 months of a year. //number of periods of period_length in a year.


        /* SG: Steve Goddard modifications */
        /* SG 6/5/06: add variables to allow user-defined calibration intervals */
        /* SG 6/5/06: The original Monthly PDSI should not support a calibration interval, so clear the vars */
        //int currentCalibrationStartYear = startyear;
        //int currentCalibrationEndYear = endyear;
        int nEndYearsToSkip = 0;
        int nStartYearsToSkip = 0;
        //int nCalibrationYears = totalyears;
        int nStartPeriodsToSkip = 0;
        int nEndPeriodsToSkip = 0;
        //int nCalibrationPeriods = nCalibrationYears * num_of_periods;


        // The variables used as flags to the pdsi class
        int bug;
        int output_mode;
        //int Verbose;
        int s_year;
        int e_year;
        int extra;
        int metric;
        bool south;//int south; //whethere the location is in Southern Hemisphere (If TLA is positive, we assume location is in Southern Hemisphere.
        int nadss;

        /* added on 9/21/05 to allow for user-define calibration start year (jdokulil) */
        int setCalibrationStartYear;
        int setCalibrationEndYear;
        int calibrationStartYear;
        int calibrationEndYear;
        /* end addition */




        // Various constants used in calculations
        double TLA; // The negative tangent of latitude is used in calculating PE
        double AWC; // The soils water capacity
        double I;   // Thornthwaites heat index
        double A;   // Thornthwaites exponent
        double tolerance; // The tolerance for various comparisons

        // The arrays used to read in the normal temp data, a years worth of 
        // actual temp data, and a years worth of precipitation data
        double[] TNorm = new double[12];
        double[] T = new double[12];
        double[] P = new double[12];

        // These variables are used in calculation to store the current period's
        // potential and actual water balance variables as well as the soil
        // moisture levels
        double ET;            // Actual evapotranspiration
        double R;             // Actual soil recharge 
        double L;             // Actual loss
        double RO;            // Actual runoff
        double PE;            // Potential evapotranspiration
        double PR;            // Potential soil recharge
        double PL;            // Potential Loss
        double PRO;           // Potential runoff
        double Su;            // Underlying soil moisture
        double Ss;            // Surface soil moisture

        // These arrays are used to store the monthly or weekly sums of the 8 key 
        // water balance variables and the precipitation
        double[] ETSum = new double[12];
        double[] RSum = new double[12];
        double[] LSum = new double[12];
        double[] ROSum = new double[12];
        double[] PESum = new double[12];
        double[] PRSum = new double[12];
        double[] PLSum = new double[12];
        double[] PROSum = new double[12];
        double[] PSum = new double[12];

        // These arrays store the monthly or weekly water balance coefficeints 
        double[] Alpha = new double[12];
        double[] Beta = new double[12];
        double[] Gamma = new double[12];
        double[] Delta = new double[12];

        // The CAFEC percipitation
        double Phat;

        // These variables are used in calculating the z index
        double d;     // Departure from normal for a period
        double[] D = new double[12]; // Sum of the absolute value of all d values by period
        double[] k = new double[12]; // Palmer's k' constant by period
        double K;     // The final K value for a period
        double Z;     // The z index for a period (Z=K*d)

        // These variables are used in calculating the PDSI from the Z
        // index.  They determine how much of an effect the z value has on 
        // the PDSI based on the climate of the region.  
        // They are calculated using CalcDurFact()
        double drym;
        double dryb;
        double wetm;
        double wetb;

        //these two variables weight the climate characteristic in the 
        //calibration process
        double dry_ratio;
        double wet_ratio;

        // The X variables are used in book keeping for the computation of
        // the pdsi
        double X1;    // Wet index for a month/week
        double X2;    // Dry index for a month/week
        double X3;    // Index for an established wet or dry spell
        double X;     // Current period's pdsi value before backtracking

        // These variables are used in calculating the probability of a wet
        // or dry spell ending
        double Prob;  // Prob=V/Q*100
        double V;     // Sumation of effective wetness/dryness
        double Q;     // Z needed for an end plus last period's V

        // These variables are statistical variables computed and output in 
        // Verbose mode
        double[] DSSqr = new double[12];
        double[] DEPSum = new double[12];
        double DKSum;
        double SD;
        double SD2;

        // linked lists to store X values for backtracking when computing X
        public Dictionary<int, double[]> XDic = new Dictionary<int, double[]>();//final list of PDSI values
        public Dictionary<int, double[]> PDSI_Dic = new Dictionary<int, double[]>();//final list of PDSI values
        public LinkedList<double> Xlist = new LinkedList<double>();//final list of PDSI values
        LinkedList<double> altX1 = new LinkedList<double>();//list of X1 values
        LinkedList<double> altX2 = new LinkedList<double>();//list of X2 values

        // These linked lists store the Z, Prob, and 3 X values for
        // outputing the Z index, Hydro Palmer, and Weighted Palmer
        LinkedList<double> XL1 = new LinkedList<double>();
        LinkedList<double> XL2 = new LinkedList<double>();
        LinkedList<double> XL3 = new LinkedList<double>();
        LinkedList<double> ProbL = new LinkedList<double>();
        LinkedList<double> ZIND = new LinkedList<double>();

        //===============================================================================================================
        //Methods called by SumAll()
        //-------------------------------------------------------

        // CalcPR calculates the Potential Recharge of the soil for one period of the 
        // year being examined.  PR = Soils Max Capacity - Soils Current Capacity or
        // AWC - (SU + Ss)
        //-----------------------------------------------------------------------------
        void CalcPR()
        {
            PR = AWC - (Su + Ss);
        }
        //-----------------------------------------------------------------------------
        // CalcPRO calculates the Potential Runoff for a given period of the year being
        // examined.  PRO = Potential Precip - PR. Palmer arbitrarily set the Potential
        // Precip to the AWC making PRO = AWC - (AWC - (Su + Ss)). This then simplifies
        // to PRO = Su + Ss
        //-----------------------------------------------------------------------------
        void CalcPRO()
        {
            PRO = Ss + Su;
        }
        //-----------------------------------------------------------------------------
        // CalcPL calculates the Potential Loss of moisture in the soil for a period of
        // one period of the year being examined. If the Ss capacity is enough to
        // handle all PE, PL is simple PE.  Otherwise, potential loss from Su occurs at
        // the rate of (PE-Ss)/AWC*Su.  This means PL = Su*(PE - Ss)/AWC + Ss
        //-----------------------------------------------------------------------------
        void CalcPL()
        {
            if (Ss >= PE)
                PL = PE;
            else
            {
                PL = ((PE - Ss) * Su) / (AWC) + Ss;
                if (PL > PRO)  // If PL>PRO then PL>water in the soil.  This isn't
                    PL = PRO;   // possible so PL is set to the water in the soil
            }
        }

        //-----------------------------------------------------------------------------
        // CalcActual calculates the actual values of evapotranspiration,soil recharge,
        // runoff, and soil moisture loss.  It also updates the soil moisture in both
        // layers for the next period depending on current weather conditions.
        //-----------------------------------------------------------------------------
        void CalcActual(int per)
        {
            double R_surface = 0.0;   // recharge of the surface layer
            double R_under = 0.0;    // recharge of the underlying layer
            double surface_L = 0.0;   // loss from surface layer
            double under_L = 0.0;    // loss from underlying layer
            double new_Su, new_Ss;    // new soil moisture values


            if (P[per] >= PE)
            {
                // The precipitation exceeded the maximum possible evapotranspiration
                // (excess moisture)
                ET = PE;   // Enough moisture for all potential evapotranspiration to occur
                L = 0.0;   // with no actual loss of soil moisture

                if ((P[per] - PE) > (1.0 - Ss))
                {
                    // The excess precip will recharge both layers. Note: (1.0 - SS) is the 
                    // amount of water needed to saturate the top layer of soil assuming it
                    // can only hold 1 in. of water.
                    R_surface = 1.0 - Ss;
                    new_Ss = 1.0;

                    if ((P[per] - PE - R_surface) < ((AWC - 1.0) - Su))
                    {
                        // The entire amount of precip can be absorbed by the soil (no runoff)
                        // and the underlying layer will receive whats left after the top layer
                        // Note: (AWC - 1.0) is the amount able to be stored in lower layer
                        R_under = (P[per] - PE - R_surface);
                        RO = 0.0;
                    }
                    else
                    {
                        // The underlying layer is fully recharged and some runoff will occur
                        R_under = (AWC - 1.0) - Su;
                        RO = P[per] - PE - (R_surface + R_under);
                    }
                    new_Su = Su + R_under;
                    R = R_surface + R_under;//total recharge
                }
                else
                {
                    // There is only enough moisture to recharge some of the top layer.
                    R = P[per] - PE;
                    new_Ss = Ss + R;
                    new_Su = Su;
                    RO = 0.0;
                }
            }// End of if(P[per] >= PE)
            else
            {
                // The evapotranspiration is greater than the precipitation received.  This
                // means some moisture loss will occur from the soil.
                if (Ss > (PE - P[per]))
                {
                    // The moisture from the top layer is enough to meet the remaining PE so 
                    // only the top layer losses moisture.
                    surface_L = PE - P[per];
                    under_L = 0.0;
                    new_Ss = Ss - surface_L;
                    new_Su = Su;
                }
                else
                {
                    // The top layer is drained, so the underlying layer loses moisture also.
                    surface_L = Ss;
                    under_L = (PE - P[per] - surface_L) * Su / AWC;
                    if (Su < under_L)
                        under_L = Su;
                    new_Ss = 0.0;
                    new_Su = Su - under_L;
                }
                R = 0;// No recharge occurs
                L = under_L + surface_L;// Total loss
                RO = 0.0;// No extra moisture so no runoff
                ET = P[per] + L;// Total evapotranspiration
            }
            Ss = new_Ss;//update soil moisture values
            Su = new_Su;
        }//end of CalcActual(int per)

        //-----------Is called from SumAll()----------------
        private void CalcMonPE(int month, int year)
        {
            double[] Phi = { -.3865982, -.2316132, -.0378180, .1715539, .3458803, .4308320, .3916645, .2452467, .0535511, -.15583436, -.3340551, -.4310691 };
            //these values of Phi[] come directly from the fortran program.
            int[] Days = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
            double Dum, Dk;
            int offset;
            if (south)
                offset = 6;
            else
                offset = 0;

            if (T[month] <= 32)
                PE = 0;
            else
            {
                Dum = Phi[(month + offset) % 12] * TLA;
                Dk = Math.Atan(Math.Sqrt(1 - Dum * Dum) / Dum);
                if (Dk < 0)
                    Dk += 3.141593;
                Dk = (Dk + .0157) / 1.57;
                if (T[month] >= 80)
                {
                    PE = (Math.Sin(T[month] / 57.3 - .166) - .76) * Dk;
                }
                else
                {
                    Dum = Math.Log(T[month] - 32);
                    PE = (Math.Exp(-3.863233 + A * 1.715598 - A * Math.Log(I) + A * Dum)) * Dk;
                }
            }
            // This calculation of leap year follows the FORTRAN program 
            // It does not take into account factors of 100 or 400
            /*
            if (year%4==0 && month==1)
              PE=PE*29;
            else
              PE=PE*Days[month];
            */
            //this calculation has been updated to accurately follow leap years
            if (month == 1)
            {
                if (year % 400 == 0)
                    PE = PE * 29;
                else if (year % 4 == 0 && year % 100 != 0)
                    PE = PE * 29;
                else
                    PE = PE * 28;
            }
            else
                PE = PE * Days[month];

        }

        //===============================================================================================================
        /// <summary>
        /// Calculates original PDSI (NOT Self-Calibrating PDSI) for a given month (1-12)
        /// </summary>
        /// <param name="annualClimate"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public void CalculatePDSI(AnnualClimate[] annualClimates, double[] historic_mon_Temp_Normal, double awc, double latitude, string outputFilePath, UnitSystem arsUnitSystem)
        {
            //double fieldCapacity = Landis.Extension.Succession.Century.EcoregionData.FieldCapacity[_AnnualClimates[0].Ecoregion]; //- Landis.Extension.Succession.Century.EcoregionData.WiltingPoint[_AnnualClimates[0].Ecoregion];
            _AnnualClimates = annualClimates;
            s_year = _AnnualClimates[0].Year;
            e_year = _AnnualClimates[_AnnualClimates.Count() - 1].Year;
            //AWC = awc;
            this.TLA = latitude;
            //---initial needs---------

            //in C++ PDSI mon_T_normal is read from the mon_T_normal text file 
            mon_T_normal = historic_mon_Temp_Normal;//new double[12] { 19.693, 23.849, 34.988, 49.082, 60.467, 70.074, 75.505, 73.478, 64.484, 52.634, 36.201, 24.267 };


            //Converting Farenheit temps to celsius by  Tf = (9/5)*Tc+32; formula
            //Converting precepitation from cm to inches by  in = cm * 0.39370 formula
            if (arsUnitSystem == UnitSystem.metrics)
            {
                for (int i = 0; i < mon_T_normal.Length; i++)
                {
                    mon_T_normal[i] = (9.0 / 5.0) * mon_T_normal[i] + 32.0;
                }

                //string temp1 = "";
                //string temp2 = "";

                for (int i = 0; i < _AnnualClimates.Length; i++)
                {
                    for (int m = 0; m < 12; m++)
                    {
                        //temp1 += _AnnualClimates[i].MonthlyTemp[m].ToString() + ", ";
                        _AnnualClimates[i].MonthlyTemp[m] = (9.0 / 5.0) * _AnnualClimates[i].MonthlyTemp[m] + 32.0;
                        _AnnualClimates[i].MonthlyPrecip[m] = _AnnualClimates[i].MonthlyPrecip[m] * 0.39370;
                        //temp2 += _AnnualClimates[i].MonthlyTemp[m].ToString() + ", ";
                    }
                    //temp1 += "\n";
                    //temp2 += "\n";
                }

                //awt is passed and used in GetParam(awt)
                double awc_temp = AWC;
                AWC = awc_temp * 0.39370;
                TLA = TLA * 0.39370;
            }
            //double fieldCapacity = Landis.Extension.Succession.Century.EcoregionData.FieldCapacity[_AnnualClimates[0].Ecoregion]; //- Landis.Extension.Succession.Century.EcoregionData.WiltingPoint[_AnnualClimates[0].Ecoregion];
            //the negative of the tangent of the latitude  of the station - TLA
            //TLA = 42.60; //Should be passed as a variable


            if (TLA > 0)
                south = true;
            else
                south = false;
            //--------------------------------------------------

            //Get 2 initial parameters fieldCapacity and TLA
            GetParam(awc);
            //double fieldCapacity = Landis.Extension.Succession.Century.EcoregionData.FieldCapacity[_AnnualClimates.Ecoregion];
            //the negative of the tangent of the latitude  of the station - TLA
            //TLA = 42.60; //Should be passed as a variable

            //------------------------
            //--------Calculation-----
            //------------------------
            //totalyears = HistoricClimate.endyear - HistoricClimate.startyear + 1;
            // Output seen only in maximum Verbose mode
            if (Verbose > 1)
                Console.WriteLine("processing station 1\n");
            // SumAll is called to compute the sums for the 8 water balance variables
            PDSI_SumAll();

            // This outputs those sums to the screen
            if (Verbose > 1)
            {
                // Console.WriteLine("STATION = %5d %18c SUMMATION OF MONTHLY VALUES OVER %4d YEARS\n", 0, ' ', totalyears);
                Console.WriteLine("%36c CALIBRATION YEARS:\n", ' ');
                Console.WriteLine("PER P S PR PE PL ET R L RO DEP\n\n");
            }
            for (int i = 0; i < num_of_periods; i++)
            {
                DEPSum[i] = ETSum[i] + RSum[i] - PESum[i] + ROSum[i];
                if (Verbose > 2)
                {
                    Console.WriteLine((period_length * i) + 1);
                    Console.WriteLine("{0} {1} {2} {3} {4}", PSum[i], PROSum[i], PRSum[i], PESum[i], PLSum[i]);
                    Console.WriteLine("{0} {1} {2} {3} {4}", ETSum[i], RSum[i], LSum[i], ROSum[i], DEPSum[i]);
                    Console.WriteLine("\n");
                }
                DSSqr[i] = 0;
            }

            // CalcWBCoef is then called to calculate alpha, beta, gamma, and delta
            CalcWBCoef();
            // Next Calcd is called to calculate the monthly departures from normal
            Calcd();
            // Finally CalcK is called to compute the K and Z values.  CalcX is called
            // within CalcK.
            CalcOrigK();

            string s = "";
            double annualPDSI = 0;
            string annPDSI = "";
            //double ecoAverage = 0;
            //foreach(KeyValuePair<int,double[]> item in XDic)
            //{
            //    for(int i=0; i<12; i++)
            //        s += Math.Round( ((double[])item.Value)[i], 2) + "\t";
            //    s += "\n";
            //}
            LinkedListNode<double> node = Xlist.Last;
            int j = 0;
            int stYear = _AnnualClimates[0].Year;
            //string outputFilePathAnnualPDSI = @"AnnualPDSI.csv";
            if(Climate.AnnualPDSI.Columns.Count == 0)
            {
                Climate.AnnualPDSI.Columns.Add("TimeStep", typeof(Int32));
                Climate.AnnualPDSI.Columns.Add("Ecorigion", typeof(Int32));
                Climate.AnnualPDSI.Columns.Add("AnnualPDSI", typeof(double));
            }
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(outputFilePath, true))
            {
                //using (System.IO.StreamWriter f = new System.IO.StreamWriter(outputFilePathAnnualPDSI, true))
                //{
                    //f.WriteLine("Time-step, Ecoregion, AnnualPDSI");

                    file.WriteLine("Ecoregion, Time-step, Year, mo.1, mo.2, mo.3, mo.4, mo.5, mo.6, mo.7, mo.8, mo.9, mo.10, mo.11, mo.12,");
                    for (int i = 0; i < Xlist.Count(); i += 12)
                    {
                        //if (i % 12 == 0)
                        //{
                        stYear = _AnnualClimates[j].Year;
                        s += "\r" + _AnnualClimates[j].Ecoregion.Name + ", " + _AnnualClimates[j].TimeStep + ", " + stYear.ToString() + ", " + "";
                        //Add s into file to save PDSI
                        j++;
                        //}
                        //if (stYear == annualClimates[0].Year)
                        //{
                        //    s += annualClimates[0].Year.ToString() + "\t";

                        //}
                        for (int m = 0; m < 12; m++)
                        {
                            annualPDSI += Math.Round(node.Value, 2);
                            if (m == 11)
                            {

                                annualPDSI = annualPDSI / 12;
                                System.Data.DataRow pRow = Climate.AnnualPDSI.NewRow();
                                pRow["TimeStep"] = _AnnualClimates[j - 1].TimeStep;
                                pRow["Ecorigion"] = _AnnualClimates[j - 1].Ecoregion.Name.Substring(3);
                                pRow["AnnualPDSI"] =  Math.Round(annualPDSI, 2);
                                Climate.AnnualPDSI.Rows.Add(pRow);
                                //annPDSI = "\r" + _AnnualClimates[j - 1].TimeStep + "," + _AnnualClimates[j - 1].Ecoregion.Name + "," + Math.Round(annualPDSI, 2) + ",";
                                //f.WriteLine(annPDSI);
                                //XDicAveragePDSI.Add(_AnnualClimates[j - 1].TimeStep,new string[2]{_AnnualClimates[j - 1].Ecoregion.Name, Math.Round(annualPDSI, 2).ToString()});
                                //XDicAveragePDSI.Add(_AnnualClimates[j - 1].TimeStep,new {ecoregion = _AnnualClimates[j - 1].Ecoregion.Name, annPdsi = Math.Round(annualPDSI, 2)});
                                annPDSI = "";
                                annualPDSI = 0; 
                            }


                            s += Math.Round(node.Value, 2) + ", ";
                            node = node.Previous;
                        }

                        s += "";
                        file.WriteLine(s);
                        s = "";
                        //if (j == _AnnualClimates.Length)
                        //{
                        //    //print average ecorigion for each ecorigion
                        //    ecoAverage = ecoAverage / _AnnualClimates.Length;
                        //    file.WriteLine("Ecoregion, Average");
                        //    file.WriteLine(_AnnualClimates[j - 1].Ecoregion.Name + "," + Math.Round(ecoAverage, 2));
                        //    ecoAverage = 0;

                        //}
                    }
                //}
            }


           

            //System.IO.File.WriteAllText(@"C:\Program Files\LANDIS-II\v6\examples\base-BDA_1\PDSI_BaseBDA.txt", s);
            //if(month == 0)
            //    XDic.Add(yearIndex,new double[12]);
            //((double[])XDic[yearIndex])[month] = newX;

            //return Xlist.ToArray()[month - 1];//Xlist[month-1].;
        }

        //Computes the sums for the 8 water balance variables
        private void PDSI_SumAll()
        {
            char[] Temp = new char[150], Precip = new char[150];
            int actyear;
            double DEP = 0;
            SD = 0;
            SD2 = 0;
            /* SG 6/5/06: add variable to support a calibration interval */
            int nCalibrationPeriodsLeft = 0;//nCalibrationPeriods; /* init periods left */        //******** I assigned 0 because we dont need it *******

            // Initializes the sums to 0;
            for (int i = 0; i < num_of_periods; i++)
            {
                ETSum[i] = 0;
                RSum[i] = 0;
                LSum[i] = 0;
                ROSum[i] = 0;
                PSum[i] = 0;
                PESum[i] = 0;
                PRSum[i] = 0;
                PLSum[i] = 0;
                PROSum[i] = 0;
            }

            totalyears = e_year - s_year + 1;
            //totalyears = e_year - s_year + 1;
            //totalyears = _AnnualClimates[e_year].Year - _AnnualClimates[s_year].Year;

            _Potential = new Potential[totalyears];
            // This loop runs to read in and calculate the values for all years
            for (int y = 1; y <= totalyears; y++)
            {
                // Get a year's worth of temperature and precipitation data
                // Also, get the current year from the temperature file.
                annClimate = _AnnualClimates[y - 1];
                annClimateIndex = y - 1;
                //if(Weekly){
                //  actyear=GetTemp(input_temp, T, 52);
                //  GetPrecip(input_prec, P, 52);
                //}
                //else{
                //for (int i = 0; i < 12; i++)
                //{
                //    T[i] = _AnnualClimates[year].MonthlyTemp[i];  //calculate T[i] from historic, not target climate
                //    P[i] = _AnnualClimates[year].MonthlyPrecip[i];  //calculate P[i] from historic, not target climate
                //}
                GetTemp(ref T, 12);//actyear = GetTemp(input_temp, T, 12);
                GetPrecip(ref P, 12);//GetPrecip(input_prec, P, 12);
                //}

                Potential pot = new Potential();

                // This loop runs for each per in the year
                for (int per = 0; per < num_of_periods; per++)
                {
                    if (P[per] >= 0 && T[per] != MISSING)
                    {
                        // calculate the Potential Evapotranspiration first
                        // because it's needed in later calculations
                        CalcMonPE(per, _AnnualClimates[y - 1].Year); //Year is the given actual year like 2012

                        CalcPR();         // calculate Potential Recharge, Potential Runoff,
                        CalcPRO();        // and Potential Loss
                        CalcPL();
                        CalcActual(per);  // Calculate Evapotranspiration, Recharge, Runoff,
                        // and Loss

                        // Calculates some statistical variables for output 
                        // to the screen in the most Verbose mode (Verbose > 1)
                        if (per > 4 && per < 8)
                        {
                            DEP = DEP + P[per] + L - PE;
                            if (per == 7)
                            {
                                SD = SD + DEP;
                                SD2 = SD2 + DEP * DEP;
                                DEP = 0;
                            }
                        }


                        /* SG 6/5/06: add code to support a calibration interval */
                        /* SG 6/4/06: Allow for user-defined calibration interval by not Summing during
                        **            years before the calibration interval or after the end of the calibration
                        **            interval.  
                        */
                        //if (nCalibrationPeriodsLeft > 0)
                        //{ /* Within the calibration interval, so update Sums */
                        // Reduce number of calibration years left by one for each year actually summed 
                        //nCalibrationPeriodsLeft--;

                        // Update the sums by adding the current water balance values
                        ETSum[per] += ET;
                        RSum[per] += R;
                        ROSum[per] += RO;
                        LSum[per] += L;
                        PSum[per] += P[per];
                        PESum[per] += PE;
                        PRSum[per] += PR;
                        PROSum[per] += PRO;
                        PLSum[per] += PL;
                        //}

                        ////P, PE, PR, PRO, and PL will be used later, so 
                        ////these variables need to be stored to an outside file
                        ////where they can be accessed at a later time
                        //fprintf(fout, "%5d %5d %10.6f ", actyear, (period_length * per) + 1, P[per]);
                        //fprintf(fout, "%10.6f %10.6f %10.6f %10.6f ", PE, PR, PRO, PL);
                        //fprintf(fout, "%10.6f\n", P[per] - PE);
                        pot.Year = _AnnualClimates[y - 1].Year;
                        pot.Period[per] = (period_length * per) + 1;
                        pot.P[per] = P[per];
                        pot.PE[per] = PE;
                        pot.PR[per] = PR;
                        pot.PRO[per] = PRO;
                        pot.PL[per] = PL;
                        pot.P_PE[per] = P[per] - PE;
                        //_Potential[year] = new Potential()  { Year = _AnnualClimates[year].Year, Period[per] = (period_length * per) + 1, P[per] = P[per], PE[per] = PE, PR[per] = PR, PRO[per] = PRO, PL[per] = PL, P_PE[per] = P[per] - PE };
                    }//matches if(P[per]>= 0 && T[per] != MISSING)
                    else
                    {
                        //fprintf(fout, "%5d %5d %f ", actyear, (period_length * per) + 1, MISSING);
                        //fprintf(fout, "%10.6f %10.6f %10.6f ", MISSING, MISSING, MISSING);
                        //fprintf(fout, "%10.6f %10.6f\n", MISSING, MISSING);
                        pot.Year = _AnnualClimates[y - 1].Year;
                        pot.Period[per] = (period_length * per) + 1;
                        pot.P[per] = MISSING;
                        pot.PE[per] = MISSING;
                        pot.PR[per] = MISSING;
                        pot.PRO[per] = MISSING;
                        pot.PL[per] = MISSING;
                        pot.P_PE[per] = MISSING;
                        //_Potential[year] = new Potential() { Year = _AnnualClimates[year].Year, Period = (period_length * per) + 1, P = MISSING, PE = MISSING, PR = MISSING, PRO = MISSING, PL = MISSING, P_PE = MISSING };
                    }
                }//end of period loop
                _Potential[y - 1] = pot;


            }

        }

        //-----------------------------------------------------------------------------
        //This function calculates the Thornthwaite heat index I for montly PDSI's.
        //-----------------------------------------------------------------------------
        private double CalcMonThornI()
        {
            double I = 0;
            int i = 0, j = 0;
            /*  float t[13];

                FILE *fin;
                char filename[150];
                if(strlen(input_dir)>1){
                  sprintf(filename,"%s%s",input_dir,"mon_T_normal");
                }
                else
                  strcpy(filename, "mon_T_normal");
                // The file containing the normal temperatures is opened for reading. 
              if ((fin=fopen(filename,"r")) == NULL) {
                if(verbose>1) { 
                  printf("Warning:  Failed opening file for normal temperatures.\n"); 
                  printf("          filename: %s\n",filename);
                }  
                if(strlen(input_dir)>1)  
                  sprintf(filename,"%s%s",input_dir,"T_normal");  
                else  
                  strcpy(filename,"T_normal");  
                if((fin=fopen(filename,"r"))==NULL){  
                  if(verbose > 0){ 
                    printf("Fatal Error: Failed to open file for normal temperatures.\n"); 
                    printf("             filename: %s\n",filename); 
                  } 
                  exit(0);  
                }  
              }

                // The monthly temperatures are read in to a temparary array.
                // This was done because the fscanf function was unable to handle an array
                // of type double with the %f.  There might be something that could be used
                // in place of the %f to get a double to work.
                fscanf(fin,"%f %f %f %f %f %f",&t[0],&t[1],&t[2],&t[3],&t[4],&t[5]);
                fscanf(fin,"%f %f %f %f %f %f",&t[6],&t[7],&t[8],&t[9],&t[10],&t[11]);
                //check to make sure file only had 12 entries
                if(fscanf(fin,"%f",&t[13]) != EOF){
                  printf("Warning: Normal Temperature file, %s, is the wrong format.\n",filename);
                  printf("         Results may not be accurate.\n");
                }
            */
            // Then we move the temperatures to the TNorm array and calclulate I
            for (i = 0; i < 12; i++)
            {
                if (metric == 1)
                    TNorm[i] = mon_T_normal[i] * (9.0 / 5.0) + 32;//TNorm[i] = t[i]*(9.0/5.0)+32;
                else
                    TNorm[i] = mon_T_normal[i];//TNorm[i]=t[i];
                // Prints the normal temperatures to the screen
                if (Verbose > 1)
                    Console.WriteLine("{0}", TNorm[i]);
                // Adds the modified temp to heat if the temp is above freezing
                if (TNorm[i] > 32)
                    I = I + Math.Pow((TNorm[i] - 32) / 9, 1.514);
            }
            // Prints a newline to the screen and closes the input file
            if (Verbose > 1)
                Console.WriteLine("\n");
            //  fclose(fin);
            return I;
        }



        //-----------------------------------------------------------------------------
        // CalcThornA calculates the Thornthwaite exponent a based on the heat index I.
        //-----------------------------------------------------------------------------
        private double CalcThornA(double I)
        {
            double A;
            A = 6.75 * (Math.Pow(I, 3)) / 10000000 - 7.71 * (Math.Pow(I, 2)) / 100000 + 0.0179 * I + 0.49;
            return A;
        }



        //-----------------------------------------------------------------------------
        // This function reads in the 2 initializing values of Su and TLA
        //-----------------------------------------------------------------------------
        private void GetParam(double awc)//(FILE * Param) 
        {
            //float scn1, scn2;
            double lat;
            AWC = awc;
            double PI = 3.1415926535;
            Core.IEcoregion TempEco = _AnnualClimates[0].Ecoregion;
            for (int i = 0; i < _AnnualClimates.Length; i++)
            {
                if (_AnnualClimates[i].Ecoregion != TempEco)
                {
                    throw new ApplicationException("The ecoregions of annual climates are not the same");
                }

            }
            //double fieldCapacity = Landis.Extension.Succession.Century.EcoregionData.FieldCapacity[_AnnualClimates[0].Ecoregion]; //- Landis.Extension.Succession.Century.EcoregionData.WiltingPoint[_AnnualClimates[0].Ecoregion];
            ////the negative of the tangent of the latitude  of the station - TLA
            //TLA = 42.60; //Should be passed as a variable


            //fscanf(Param,"%f %f",&scn1,&scn2);
            //AWC = fieldCapacity;//double(scn1);
            //TLA = double(scn2);
            if (metric == 1)
                AWC = AWC / 25.4;
            if (AWC <= 0)
            {
                throw new ApplicationException("Invalid value for AWC: " + Su);
                //exit(0);
            }
            Ss = 1.0;   //assume the top soil can hold 1 inch
            if (AWC < Ss)
            {
                //always assume the top layer of soil can 
                //hold at least the Ss value of 1 inch.
                AWC = Ss;
            }
            Su = AWC - Ss;
            if (Su < 0)
                Su = 0;
            if (nadss == 1)
            {
                if (TLA > 0)
                {
                    if (Verbose > 1)
                        Console.WriteLine("TLA is positive, assuming location is in Southern Hemisphere. TLA: {0}", TLA);
                    south = true;//1;
                    TLA = -TLA;
                }
                else
                    south = false;//0;
            }
            else
            {
                lat = TLA;
                TLA = -Math.Tan(PI * lat / 180);
                if (lat >= 0)
                {
                    if (Verbose > 1)
                        Console.WriteLine("TLA is positive, assuming location is in Southern Hemisphere. TLA: %f\n", TLA);
                    south = false;//0;
                }
                else
                {
                    south = true;//1;
                    TLA = -TLA;
                }
            }
            //if(Weekly)
            //  I=CalcWkThornI(); 
            //else if(Monthly || SCMonthly)
            I = CalcMonThornI();
            //else{
            //  if(verbose){
            //    printf("Error.  Invalid type of PDSI calculation\n");
            //    printf("Either the 'Weekly', 'Monthly', ");
            //    printf("or 'SCMonthly' flags must be set.\n");
            //  }
            //  exit(1);
            //}

            A = CalcThornA(I);
            if (Verbose > 1)
            {
                Console.WriteLine("AWC = {0}  TLA = {1}", AWC, TLA);
                Console.WriteLine("HEAT INDEX, THORNTHWAITE A: {0} {1}", I, A);
            }
        }



        //-----------------------------------------------------------------------------
        // This function reads in a years worth of data from file In and places those
        // values in array A.  It's been modified to average the input data to the 
        // correct time scale.  Because of this modification, it only works for 
        // temperature data; precip data must be summed, not averaged.
        //-----------------------------------------------------------------------------
        public void GetTemp(ref double[] A, int max) //int GetTemp(FILE *In, number *A, int max) 
        {
            double[] t = new double[12], t2 = new double[12];
            double temp;
            int i, j, year, read, bad_weeks;
            //char line[4096];
            //char letter;

            for (i = 0; i < 12; i++)
                A[i] = 0;

            //  fgets(line,4096,In);
            //  read=sscanf(line, "%d %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f",&year,&t[0],&t[1],&t[2],&t[3],&t[4],&t[5],&t[6],&t[7],&t[8],&t[9],&t[10],&t[11],&t[12],&t[13],&t[14],&t[15],&t[16],&t[17],&t[18],&t[19],&t[20],&t[21],&t[22],&t[23],&t[24],&t[25],&t[26],&t[27],&t[28],&t[29],&t[30],&t[31],&t[32],&t[33],&t[34],&t[35],&t[36],&t[37],&t[38],&t[39],&t[40],&t[41],&t[42],&t[43],&t[44],&t[45],&t[46],&t[47],&t[48],&t[49],&t[50],&t[51]);
            for (i = 0; i < max; i++)
                t[i] = annClimate.MonthlyTemp[i];
            //place values read into array t2 to be summarized
            //  if(read == max+1){
            //a full year's worth of data was read
            for (i = 0; i < max; i++)
                t2[i] = t[i];

            //  else{
            //check to see if it is the end of file
            //    if( (letter = fgetc(In)) != EOF ) {
            //    //it's not the end of the file
            //    //so place partial year's data at end of array
            //    for(i = 0 ; i < max - (read-1); i++)
            //      t2[i] = MISSING;
            //    for(i; i < max; i++)
            //      t2[i] = t[i - (max-read+1)];
            //    ungetc(letter, In);
            //  }
            //  else {
            //    //it's the end of the file, place partial year's data
            //    //at beginning on array
            //    for(i = 0; i < read - 1; i++)
            //      t2[i] = t[i];
            //    for(i; i < max; i++)
            //      t2[i] = MISSING;
            //  }
            //}
            for (i = 0; i < num_of_periods; i++)
            {
                bad_weeks = 0;
                temp = 0;
                for (j = 0; j < period_length; j++)
                {
                    if (t2[i * period_length + j] != MISSING)
                        temp += t2[i * period_length + j];
                    else
                        bad_weeks++;
                }
                if (bad_weeks < period_length)
                    A[i] = temp / (period_length - bad_weeks);
                else
                    A[i] = MISSING;
            }
            if (metric == 1)
            {
                for (i = 0; i < num_of_periods; i++)
                {
                    if (A[i] != MISSING)
                        A[i] = A[i] * (9.0 / 5.0) + 32;
                }
            }

            //return year;

        }
        //-----------------------------------------------------------------------------
        // This function is a modified version of GetTemp() function for precip only.
        //-----------------------------------------------------------------------------
        private void GetPrecip(ref double[] A, int max) //int GetPrecip(FILE *In, number *A, int max) 
        {
            double[] t = new double[12], t2 = new double[12];
            double temp;
            int i, j, year, read, bad_weeks;
            //  char line[4096];
            //  char letter;

            for (i = 0; i < 12; i++)
                A[i] = 0;

            for (i = 0; i < max; i++)
                t[i] = annClimate.MonthlyPrecip[i];


            for (i = 0; i < max; i++)
                t2[i] = t[i];

            /*
              fgets(line,4096,In);
              read=sscanf(line, "%d %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f",&year,&t[0],&t[1],&t[2],&t[3],&t[4],&t[5],&t[6],&t[7],&t[8],&t[9],&t[10],&t[11],&t[12],&t[13],&t[14],&t[15],&t[16],&t[17],&t[18],&t[19],&t[20],&t[21],&t[22],&t[23],&t[24],&t[25],&t[26],&t[27],&t[28],&t[29],&t[30],&t[31],&t[32],&t[33],&t[34],&t[35],&t[36],&t[37],&t[38],&t[39],&t[40],&t[41],&t[42],&t[43],&t[44],&t[45],&t[46],&t[47],&t[48],&t[49],&t[50],&t[51]);

             * 
             * 
              //place values read into array t2 to be summarized
              if(read == max+1){
                //a full year's worth of data was read
                for(i = 0; i < max; i++)
                  t2[i] = t[i];
              }
              else{
                //check to see if it is the end of file
                if( (letter = fgetc(In)) != EOF ) {
                  //it's not the end of the file
                  //so place partial year's data at end of array
                  for(i = 0 ; i < max - (read-1); i++)
                    t2[i] = MISSING;
                  for(i; i < max; i++)
                    t2[i] = t[i - (max-read+1)];
                  ungetc(letter, In);
                }
                else {
                  //it's the end of the file, place partial year's data
                  //at beginning on array
                  for(i = 0; i < read - 1; i++)
                    t2[i] = t[i];
                  for(i; i < max; i++)
                    t2[i] = MISSING;
                }
              }*/
            //now summaraize data in t2 into A
            for (i = 0; i < num_of_periods; i++)
            {
                bad_weeks = 0;
                temp = 0;
                for (j = 0; j < period_length; j++)
                {
                    if (t2[i * period_length + j] != MISSING)
                        temp += t2[i * period_length + j];
                    else
                        bad_weeks++;
                }
                if (bad_weeks < period_length)
                    A[i] = temp;
                else
                    A[i] = MISSING;
            }

            if (metric == 1)
            {
                for (i = 0; i < num_of_periods; i++)
                {
                    if (A[i] != MISSING)
                        A[i] = A[i] / 25.4;
                }
            }

            //return year;
        }




        //Calculates alpha, beta, gamma, and delta
        private void CalcWBCoef()
        {
            //FILE* wb;

            // The coefficients are calculated by per
            for (int per = 0; per < num_of_periods; per++)
            {
                //calculate alpha:
                if (PESum[per] != 0.0)
                    Alpha[per] = ETSum[per] / PESum[per];
                else if (ETSum[per] == 0.0)
                    Alpha[per] = 1.0;
                else
                    Alpha[per] = 0.0;

                //calculate beta:
                if (PRSum[per] != 0.0)
                    Beta[per] = RSum[per] / PRSum[per];
                else if (RSum[per] == 0.0)
                    Beta[per] = 1.0;
                else
                    Beta[per] = 0.0;

                //calculate gamma:
                if (PROSum[per] != 0.0)
                    Gamma[per] = ROSum[per] / PROSum[per];
                else if (ROSum[per] == 0.0)
                    Gamma[per] = 1.0;
                else
                    Gamma[per] = 0.0;

                //calculate delta:
                if (PLSum[per] != 0.0)
                    Delta[per] = LSum[per] / PLSum[per];
                else
                    Delta[per] = 0.0;
            }

            //if (extra == 1 || extra == 9)
            //{
            //    //output water balance coefficients
            //    wb = fopen("WB.tbl", "w");
            //    fprintf(wb, "PERIOD   ALPHA     BETA    GAMMA    DELTA\n");
            //    if (Verbose > 1)
            //        printf("\nPERIOD   ALPHA     BETA    GAMMA    DELTA\n");
            //    for (int i = 0; i < num_of_periods; i++)
            //    {
            //        fprintf(wb, "%3d %10.4f %8.4f %8.4f %8.4f \n", (period_length * i) + 1, Alpha[i], Beta[i], Gamma[i], Delta[i]);
            //        if (Verbose > 1)
            //            printf("%3d %10.4f %8.4f %8.4f %8.4f \n", (period_length * i) + 1, Alpha[i], Beta[i], Gamma[i], Delta[i]);
            //    }
            //    fclose(wb);
            //}
        }

        // Calculates the monthly departures from normal
        private void Calcd()
        {
            //FILE *fin;        // File pointer for the temp. input file potentials
            //FILE *fout;       // File pointer for the temp. output file dvalue
            int per;           // The period in question
            int yr;           // The year in question
            double p;         // The precip for that period
            float scn1, scn2, scn3, scn4, scn5, scn6; // Temp. variables for fscanf
            char letter = ' ';
            int i = 0;
            // These variables are used in calculating terminal outputs and are not 
            // important to the final PDSI
            double[] D_sum = new double[12];
            double[] DSAct = new double[12];
            double[] SPhat = new double[12];

            for (i = 0; i < 12; i++)
            {
                D_sum[i] = 0.0;
                DSAct[i] = 0.0;
                SPhat[i] = 0.0;
            }

            // The potentials file is opened for reading in the previously stored values
            //if((fin=fopen("potentials","r"))==NULL) {
            //  if(Verbose>0)
            //    printf ("Error opening temp file with all the potential values.\n");
            //  exit(1);
            //}

            //if((fout=fopen("dvalue","w")) == NULL) { 
            //  if(Verbose>0)
            //    printf ("Error opening temp file for d values.\n");
            //  exit(1);
            //}

            // This reads in the first line, which contains column headers.
            //while(letter != '\n')
            //  letter = fgetc(fin);
            // This reads in the values previously stored in "potentials"
            //while(fscanf(fin,"%d %d %f %f %f %f %f %f", &yr, &per, &scn1, &scn2, &scn3, &scn4, &scn5, &scn6) != EOF) 
            //{
            //per = (per - 1) / period_length;   //adjust the period # for use in arrays.

            for (int y = 0; y < _AnnualClimates.Length; y++)
            {
                for (per = 0; per < num_of_periods; per++)
                {
                    p = _Potential[y].P[per];//scn1;
                    PE = _Potential[y].PE[per];//scn2;
                    PR = _Potential[y].PR[per];//scn3;
                    PRO = _Potential[y].PRO[per];//scn4;
                    PL = _Potential[y].PL[per];//scn5;
                    //scn6 is P - PE, which can be ignored for calculations.

                    if (p != MISSING && PE != MISSING && PR != MISSING && PRO != MISSING && PL != MISSING)
                    {
                        // Then the calculations for Phat and d are done
                        Phat = (Alpha[per] * PE) + (Beta[per] * PR) + (Gamma[per] * PRO) - (Delta[per] * PL);
                        _Potential[y].d[per] = p - Phat;//d=p - Phat;

                        // The values of d are output to a temp file for later use.
                        //fprintf (fout, "%d %d %f\n", yr, (period_length*per)+1, d);

                        /* SG 6/5/06: Need to only update statistical values when in 
                        **            user defined calibration interval. When not used
                        **            nCalibrationYears==totalyears; hence no change then
                        */

                        //if (yr >= currentCalibrationStartYear && yr <= currentCalibrationEndYear)
                        //{
                        // D_sum is the sum of the absolute values of d
                        // and is used to find D
                        // D_sum[per] += abs(d);
                        if (_Potential[y].d[per] < 0.0)
                            D_sum[per] += -(_Potential[y].d[per]);
                        else
                            D_sum[per] += _Potential[y].d[per];


                        // The statistical values are updated
                        DSAct[per] += _Potential[y].d[per];
                        DSSqr[per] += _Potential[y].d[per] * _Potential[y].d[per];
                        SPhat[per] += Phat;
                    }
                    else
                    {
                        _Potential[y].d[per] = MISSING;//d = MISSING;
                        //    //fprintf (fout, "%d %d %f\n", yr, (period_length*per)+1, d);
                    }
                }

            }
            // If the user specifies, the various sums are output to the screen
            if (Verbose > 1)
            {
                Console.Write("\n{0} CHECK SUMS OF ESTIMATED VARIABLES\n\n", ' ');
                Console.Write("{0} {1} {2} {3}", "PER", "SCET", "SCR", "SCRO");
                Console.Write("{0} {1} {2}\n\n", "SCL", "SCP", "SCD");
            }
            for (i = 0; i < num_of_periods; i++)
            {
                if (Verbose > 1)
                {
                    Console.Write("{0}{1}{2}", (period_length * i) + 1, Alpha[i] * PESum[i], Beta[i] * PRSum[i]);
                    Console.Write("{0}{1}", Gamma[i] * PROSum[i], Delta[i] * PLSum[i]);
                    Console.Write("{0}{1}\n", SPhat[i], DSAct[i]);
                }
                // D becomes the mean of D_sum
                /* SG 6/5/06: changed totalyears to nCalibrationYears to support
                **            user defined calibration intervals. When not used
                **            nCalibrationYears==totalyears; hence no change then
                */

                D[i] = D_sum[i] / totalyears;//111// nCalibrationYears;
            }
            // The files are closed 
            //fclose(fin);
            //fclose(fout);
        }



        //Computes the K and Z values.  CalcX is called within CalcK.
        private void CalcOrigK()
        {
            int month, year;
            double sums;        //used to calc k
            float dtemp;
            DKSum = 0;

            //FILE * inputd; // File pointer for input file dvalue
            // The dvalue file is open for reading. 
            //if ((inputd = fopen("dvalue", "r")) == NULL)
            //{
            //    if (verbose > 0)
            //        printf("Error reading the file with d values.\n");
            //    exit(1);
            // Calculate k, which is K', or Palmer's second approximation of K
            for (int per = 0; per < num_of_periods; per++)
            //}
            {
                if (PSum[per] + LSum[per] == 0)
                    sums = 0;//prevent div by 0
                else
                    sums = (PESum[per] + RSum[per] + ROSum[per]) / (PSum[per] + LSum[per]);

                if (D[per] == 0)
                    k[per] = 0.5;//prevent div by 0
                else
                    k[per] = (1.5) * Math.Log10((sums + 2.8) / D[per]) + 0.5;

                DKSum += D[per] * k[per];
            }

            //if (Weekly)
            //{
            //    //set duration factors to CPC's 
            //    drym = 2.925;
            //    dryb = 0.075;
            //}
            //else
            //{
            // Set duration factors to Palmer's original duration factors
            drym = .309;
            dryb = 2.691;
            //}
            wetm = drym;
            wetb = dryb;


            // Initializes the book keeping indices used in finding the PDSI
            Prob = 0.0;
            X1 = 0.0;
            X2 = 0.0;
            X3 = 0.0;
            X = 0.0;
            V = 0.0;
            Q = 0.0;

            // open file point to bigTable.tbl if necessary
            //FILE* table;
            //if (extra == 2 || extra == 9)
            //{
            //    table = fopen("bigTable.tbl", "w");
            //    if (table == NULL)
            //    {
            //        if (verbose > 0)
            //            printf("Error opening file \"bigTable.tbl\"\n");
            //    }
            //    else
            //    {
            //        //write column headers 
            //        if (Weekly)
            //        {
            //            fprintf(table, "YEAR  WEEK      Z     %Prob     ");
            //            fprintf(table, "X1       X2      X3\n");
            //        }
            //        else
            //        {
            //            fprintf(table, "YEAR  MONTH     Z     %Prob     ");
            //            fprintf(table, "X1       X2      X3\n");
            //        }
            //    }
            //}
            //else
            //    table = NULL;
            // Reads in all previously calclulated d values and calculates Z
            // then calls CalcX to compute the corresponding PDSI value

            //while ((fscanf(inputd, "%d %d %f", &year, &month, &dtemp)) != EOF)
            //year = annClimate.Year;
            for (int y = 0; y < _AnnualClimates.Length; y++)
            {
                for (int per = 0; per < num_of_periods; per++)
                {
                    month = per; //Since we assumed each period is one month
                    //                PeriodList.insert(month);
                    //                YearList.insert(year);

                    d = _Potential[y].d[per];//d = dtemp;
                    //                month--; //In this code the month is the indes of month ie. 0..11 for a year 
                    //Console.WriteLine("####################################\n\n");
                    //Console.WriteLine(num_of_periods + " " + k.Count() + " " + month+ "\n\n");
                    //Console.WriteLine("####################################");   
                    K = (17.67 / DKSum) * k[month];
                    if (d != MISSING)
                        Z = d * K;
                    else
                        Z = MISSING;

                    //                ZIND.insert(Z);
                    //CalcOneX(table, month, year);
                    CalcOneX(month, y);
                }
            }
            //            fclose(inputd);
            //            if (table)
            //                fclose(table);
            // Now that all calculations have been done they can be output to the screen
            /*
            if (verbose > 1)
            {
                int i;
                if (Weekly)
                    printf("STATION = %5d %24c PARAMETERS AND MEANS OF WEEKLY VALUE FOR %d YEARS\n\n", 0, ' ', totalyears);
                else
                    printf("STATION = %5d %24c PA RAMETERS AND MEANS OF MONTHLY VALUE FOR %d YEARS\n\n", 0, ' ', totalyears);
                printf("%4s %8s %8s %8s %8s %8s %7s %8s", "MO", "ALPHA", "BETA", "GAMMA", "DELTA", "K", "P", "S");
                printf("%9s %8s %8s %8s %8s %8s %8s\n\n", "PR", "PE", "PL", "ET", "R", "L", "RO");
                for (i = 0; i < num_of_periods; i++)
                {
                    printf("%4d %8.4f %8.4f %8.4f %8.4f", (period_length * i) + 1, Alpha[i], Beta[i], Gamma[i], Delta[i]);
                    printf("%9.3f %8.2f %8.2f %8.2f", 17.67 / DKSum * k[i], PSum[i] / totalyears, PROSum[i] / totalyears, PRSum[i] / totalyears);
                    printf("%9.2f %8.2f %8.2f %8.2f", PESum[i] / totalyears, PLSum[i] / totalyears, ETSum[i] / totalyears, RSum[i] / totalyears);
                    printf("%9.2f %8.2f\n", LSum[i] / totalyears, ROSum[i] / totalyears);
                }
                printf("\n\n\n%4s %8s %8s %8s %8s %8s\n\n", "PER", "D-ABS", "SIG-D", "DEP", "S-DEP", "SIG-S");
                for (i = 0; i < num_of_periods; i++)
                {
                    printf("%4d %8.3f %8.2f %8.2f ", (period_length * i) + 1, D[i], sqrt(DSSqr[i] / (totalyears - 1)), DEPSum[i] / totalyears);
                    if (i == 7)
                    {
                        number E, DE;
                        E = SD / totalyears;
                        DE = sqrt((SD2 - E * SD) / (totalyears - 1));
                        printf("%8.2f %8.2f", E, DE);
                    }
                    printf("\n");
                }
            }
            */
        }


        //-----------------------------------------------------------------------------
        // This function calculates X, X1, X2, and X3
        //
        // X1 = severity index of a wet spell that is becoming "established"
        // X2 = severity index of a dry spell that is becoming "established"
        // X3 = severity index of any spell that is already "established"
        //
        // newX is the name given to the pdsi value for the current week.
        // newX will be one of X1, X2 and X3 depending on what the current 
        // spell is, or if there is an established spell at all.
        //-----------------------------------------------------------------------------
        private void CalcOneX(int month, int yearIndex)
        {

            double newV;    //These variables represent the values for 
            double newProb; //corresponding variables for the current period.
            //amin: I assigned 0 to avoid unassigend variable error
            double newX; newX = 0;  //They are kept seperate because many calculations
            double newX1; newX1 = 0;  //depend on last period's values.  
            double newX2; newX2 = 0;
            double newX3;
            double ZE;      //ZE is the Z value needed to end an established spell

            double m, b, c;

            int wd;        //wd is a sign changing flag.  It allows for use of the same
            //equations during both a wet or dry spell by adjusting the
            //appropriate signs.

            if (X3 >= 0)
            {
                m = wetm;
                b = wetb;
            }
            else
            {
                m = drym;
                b = dryb;
            }
            c = 1 - (m / (m + b));


            if (Z != MISSING)
            {
                // This sets the wd flag by looking at X3
                if (X3 >= 0) wd = 1;
                else wd = -1;
                // If X3 is 0 then there is no reason to calculate Q or ZE, V and Prob
                // are reset to 0;
                if (X3 == 0)
                {
                    newX3 = 0;
                    newV = 0;
                    newProb = 0;
                    ChooseX(ref newX, ref newX1, ref newX2, ref newX3, bug);
                }
                // Otherwise all calculations are needed.
                else
                {
                    newX3 = (c * X3 + Z / (m + b));
                    ZE = (m + b) * (wd * 0.5 - c * X3);
                    Q = ZE + V;
                    newV = Z - wd * (m * 0.5) + wd * Math.Min(wd * V + tolerance, 0);

                    if ((wd * newV) > 0)
                    {
                        newV = 0;
                        newProb = 0;
                        newX1 = 0;
                        newX2 = 0;
                        newX = newX3;
                        while (altX1.Count > 0)//(!altX1.is_empty())
                            altX1.RemoveFirst();//altX1.head_remove();
                        while (altX2.Count > 0)//(!altX2.is_empty())
                            altX2.RemoveFirst();//altX2.head_remove();
                    }
                    else
                    {
                        newProb = (newV / Q) * 100;
                        if (newProb >= 100 - tolerance)
                        {
                            newX3 = 0;
                            newV = 0;
                            newProb = 100;
                        }
                        ChooseX(ref newX, ref newX1, ref newX2, ref newX3, bug);
                    }
                }
                /*
                                if (table != NULL)
                                {
                                    //output stuff to a table
                                    //year, period, z, newProb, newX1, newX2, newX3
                                    fprintf(table, "%5d %5d %7.2f %7.2f ", year, period_number, Z, newProb);
                                    fprintf(table, "%7.2f %7.2f %7.2f\n", newX1, newX2, newX3);
                                }

                */
                //update variables for next month:
                V = newV;
                Prob = newProb;
                X1 = newX1;
                X2 = newX2;
                X3 = newX3;

                //-->
                //add newX to the list of pdsi values
                //if(month == 0)
                //    XDic.Add(yearIndex,new double[12]);
                //((double[])XDic[yearIndex])[month] = newX;

                Xlist.AddFirst(newX);////Xlist.insert(newX);
                XL1.AddFirst(X1);//XL1.insert(X1);
                XL2.AddFirst(X2);//XL2.insert(X2);
                XL3.AddFirst(X3);//XL3.insert(X3);
                ProbL.AddFirst(Prob);//ProbL.insert(Prob);

                //Xlist.AddFirst(newX);//Xlist.insert(newX);
                //XL1.AddFirst(X1);//XL1.insert(X1);
                //XL2.AddFirst(X2);//XL2.insert(X2);
                //XL3.AddFirst(X3);//XL3.insert(X3);
                //ProbL.AddFirst(Prob);//ProbL.insert(Prob);
            }
            else
            {
                //This month's data is missing, so output MISSING as PDSI.  
                //All variables used in calculating the PDSI are kept from 
                //the previous month.  Only the linked lists are changed to make
                //sure that if backtracking occurs, a MISSING value is kept 
                //as the PDSI for this month.

                /*
                                if (table != NULL)
                                {
                                    //output stuff to a table
                                    //year, period, z, newProb, newX1, newX2, newX3
                                    fprintf(table, "%5d %5d %7.2f %7.2f ", year, period_number, Z, MISSING);
                                    fprintf(table, "%7.2f %7.2f %7.2f\n", MISSING, MISSING, MISSING);
                                }
                */
                //if (month == 0)
                //    XDic.Add(yearIndex, new double[12]);
                //((double[])XDic[yearIndex])[month] = MISSING;

                Xlist.AddFirst(MISSING);////Xlist.insert(newX);

                XL1.AddFirst(MISSING);//XL1.insert(MISSING);
                XL2.AddFirst(MISSING);//XL2.insert(MISSING);
                XL3.AddFirst(MISSING);//XL3.insert(MISSING);
                ProbL.AddFirst(MISSING);//ProbL.insert(MISSING);
            }

        }


        private void ChooseX(ref double newX, ref double newX1, ref double newX2, ref double newX3, int bug)
        {
            double m, b;
            double wetc, dryc;

            if (X3 >= 0)
            {
                m = wetm;
                b = wetb;
            }
            else
            {
                m = drym;
                b = dryb;
            }

            wetc = 1 - (wetm / (wetm + wetb));
            dryc = 1 - (drym / (drym + wetb));

            newX1 = (wetc * X1 + Z / (wetm + wetb));
            if (newX1 < 0)
                newX1 = 0;
            newX2 = X2;

            if (bug == 0)
            {
                newX2 = (dryc * X2 + Z / (drym + dryb));
                if (newX2 > 0)
                    newX2 = 0;
            }

            if ((newX1 >= 0.5) && (newX3 == 0))
            {
                Backtrack(newX1, newX2);
                newX = newX1;
                newX3 = newX1;
                newX1 = 0;
            }
            else
            {
                newX2 = (dryc * X2 + Z / (drym + dryb));
                if (newX2 > 0)
                    newX2 = 0;

                if ((newX2 <= -0.5) && (newX3 == 0))
                {
                    Backtrack(newX2, newX1);
                    newX = newX2;
                    newX3 = newX2;
                    newX2 = 0;
                }
                else if (newX3 == 0)
                {
                    if (newX1 == 0)
                    {
                        Backtrack(newX2, newX1);
                        newX = newX2;
                    }
                    else if (newX2 == 0)
                    {
                        Backtrack(newX1, newX2);
                        newX = newX1;
                    }
                    else
                    {
                        altX1.AddFirst(newX1);//altX1.insert(newX1);
                        altX2.AddFirst(newX2);//altX2.insert(newX2);
                        newX = newX3;
                    }
                }

                else
                {
                    //store X1 and X2 in their linked lists for possible use later
                    altX1.AddFirst(newX1);//altX1.insert(newX1);
                    altX2.AddFirst(newX2);//altX2.insert(newX2);
                    newX = newX3;
                }
            }
        }//end of chooseX

        private void Backtrack(double X1, double X2)
        {
            double num1, num2;
            LinkedListNode<double> ptr = null;
            num1 = X1;
            while (altX1.Count > 0 && altX2.Count > 0)
            {
                if (num1 > 0)
                {
                    num1 = altX1.First(); altX1.RemoveFirst(); //num1=altX1.head_remove();
                    num2 = altX2.First(); altX2.RemoveLast(); //num2 = altX2.head_remove();
                }
                else
                {
                    num1 = altX2.Last(); altX2.RemoveLast(); //num1=altX2.head_remove();
                    num2 = altX1.Last(); altX1.RemoveLast(); //num2=altX1.head_remove();
                }
                if (-tolerance <= num1 && num1 <= tolerance)
                    num1 = num2;
                ptr = Xlist.SetNode<double>(num1, ptr);
                //Xlist = ptr.List;
            }
        }//end of backtrack()
    }
    //---------------------------
    class Potential
    {
        public Potential()
        {
            Period = new double[12];
            P = new double[12];
            PE = new double[12];
            PR = new double[12];
            PRO = new double[12];
            PL = new double[12];
            P_PE = new double[12];
            d = new double[12];
            x = new double[12];
        }
        public int Year { get; set; }
        public double[] Period { get; set; }
        public double[] P { get; set; }
        public double[] PE { get; set; }
        public double[] PR { get; set; }
        public double[] PRO { get; set; }
        public double[] PL { get; set; }
        public double[] P_PE { get; set; }

        public double[] d { get; set; }

        public double[] x { get; set; }
    }
}


namespace Landis.Library.Climate
{
    using System.Collections.Generic;
    public static class LinkedListExtentions
    {
        public static LinkedListNode<T> SetNode<T>(this LinkedList<T> linkedList, T value, LinkedListNode<T> set = null)
        {
            int error = 1;
            LinkedListNode<T> comparer;//node* comparer;

            if (set == null)
                set = linkedList.First;//set = head->next;
            comparer = linkedList.First;
            while (comparer != null)//while(comparer != head)
            {
                if (comparer == set)
                {
                    error = 0;
                    break;
                }
                comparer = comparer.Next;//comparer = comparer->next;
            }

            if (error == 1)
            {
                return null;
            }
            else
            {
                if (Convert.ToDouble(set.Value) != PDSI_Calculator.MISSING)
                {
                    set.Value = value;//set->key = x;
                    return set.Next;//return set->next;
                }
                else
                {
                    //if the node is MISSING, then don't replace
                    //that key.  instead, replace the first non-MISSING
                    //node you come to.
                    return SetNode(linkedList, value, set.Next);//return set_node(set->next, x);
                }
            }

            //set.Value = value;
            //return set;
        }
    }
}