//  Copyright 2009-2010 Portland State University, Conservation Biology Institute
//  Authors:  Robert M. Scheller

using Landis.Core;
using System.Collections.Generic;
using System.IO;
using System;
//using climate_generator;
using System.Collections;
using System.Data.OleDb;
using System.Data;
using System.Linq;



namespace Landis.Library.Climate
{

    public class Climate
    {
        private static Dictionary<int, IClimateRecord[,]> allData;
        private static IClimateRecord[,] timestepData;
        private static ICore modelCore;
        private static System.Data.DataTable annualPDSI;
        private static double[] landscapAnnualPDSI;

        public Climate()
        {
        }

        //---------------------------------------------------------------------

        public static ICore ModelCore
        {
            get
            {
                return modelCore;
            }
        }

        public static System.Data.DataTable AnnualPDSI
        {
            get
            {
                return annualPDSI;
            }
        }

        public static double[] LandscapAnnualPDSI
        {
            get
            {
                return landscapAnnualPDSI;
            }
            set
            {
                landscapAnnualPDSI = value;
            }

        }
        public static Dictionary<int, IClimateRecord[,]> AllData
        {
            get
            {
                return allData;
            }
        }
        //---------------------------------------------------------------------
        public static IClimateRecord[,] TimestepData
        {
            get
            {
                return timestepData;
            }
            set
            {
                timestepData = value;
            }
        }

        public static void Write(IEcoregionDataset ecoregionDataset)
        {
            foreach (IEcoregion ecoregion in ecoregionDataset)
            {
                for (int i = 0; i < 12; i++)
                {
                    ModelCore.Log.WriteLine("Eco={0}, Month={1}, AvgMinTemp={2:0.0}, AvgMaxTemp={3:0.0}, StdDevTemp={4:0.0}, AvgPpt={5:0.0}, StdDevPpt={6:0.0}.",
                        ecoregion.Index, i + 1,
                        TimestepData[ecoregion.Index, i].AvgMinTemp,
                        TimestepData[ecoregion.Index, i].AvgMaxTemp,
                        TimestepData[ecoregion.Index, i].StdDevTemp,
                        TimestepData[ecoregion.Index, i].AvgPpt,
                        TimestepData[ecoregion.Index, i].StdDevPpt
                        );
                }
            }

        }
        //---------------------------------------------------------------------
        public static void Initialize(string filename, bool writeOutput, ICore mCore)
        {
            modelCore = mCore;
            ModelCore.Log.WriteLine("   Loading weather data from file \"{0}\" ...", filename);
            ClimateParser parser = new ClimateParser();
            allData = ModelCore.Load<Dictionary<int, IClimateRecord[,]>>(filename, parser);
            modelCore = mCore;

            timestepData = allData[0]; //time step zero!

            //timestepData = allData[1];
            //TimestepData[1,11].AvgMinTemp,  //should get ecoregion (index=1), month 11, time step 1

            if (writeOutput)
                Write(Climate.ModelCore.Ecoregions);

        }

        public static void GenerateClimate_GetPDSI(int startYear, int endYear)
        {
            string outputFilePath = @"PDSI_BaseBDA_Genrated_Climate.csv";
            File.WriteAllText(outputFilePath, String.Empty);

            foreach (IEcoregion ecoregion in Climate.ModelCore.Ecoregions)
            {
                AnnualClimate[] acs;
                int numOfYears = endYear - startYear + 1;
                acs = new AnnualClimate[numOfYears];

                //foreach time step it should be called

                for (int i = startYear; i <= endYear; i++)
                {
                    acs[i - startYear] = new AnnualClimate(ecoregion, 0, i, Landis.Extension.Succession.Century.EcoregionData.Latitude[ecoregion]); // Latitude should be given
                    //Console.WriteLine(ac.MonthlyTemp[0].ToString() + "\n");
                    //Console.WriteLine(ac.MonthlyPrecip[0].ToString() + "\n");
                }


                double[] mon_T_normal = new double[12];//new double[12] { 19.693, 23.849, 34.988, 49.082, 60.467, 70.074, 75.505, 73.478, 64.484, 52.634, 36.201, 24.267 };
                IClimateRecord[] climateRecs = new ClimateRecord[12];

                //If timestep is 0 then calculate otherwise get the mon_T_normal for timestep 0

                Climate.TimestepData = allData[0];
                for (int mo = 0; mo < 12; mo++)
                {
                    climateRecs[mo] = Climate.TimestepData[ecoregion.Index, mo];

                    mon_T_normal[mo] = (climateRecs[mo].AvgMinTemp + climateRecs[mo].AvgMinTemp) / 2;
                }
                double AWC = Landis.Extension.Succession.Century.EcoregionData.FieldCapacity[ecoregion] - Landis.Extension.Succession.Century.EcoregionData.WiltingPoint[ecoregion];
                double latitude = Landis.Extension.Succession.Century.EcoregionData.Latitude[ecoregion];
                new PDSI_Calculator().CalculatePDSI(acs, mon_T_normal, AWC, latitude, outputFilePath, UnitSystem.metrics);
            }
        }

        public static void GetPDSI(int startYear)
        {
            string outputFilePath = @"PDSI_BaseBDA.csv";
            File.WriteAllText(outputFilePath, String.Empty);
            Climate.annualPDSI = new System.Data.DataTable();//final list of annual PDSI values
            foreach (IEcoregion ecoregion in Climate.ModelCore.Ecoregions)
            {
                if (ecoregion.Active)
                {
                    if (true)//(ecoregion.Index == 0)
                    {
                        AnnualClimate[] acs;
                        int numOfYears = allData.Count - 1; //-1 is because we dont want the timestep 0
                        acs = new AnnualClimate[numOfYears];
                        int timestepIndex = 0;

                        double[] mon_T_normal = new double[12];//new double[12] { 19.693, 23.849, 34.988, 49.082, 60.467, 70.074, 75.505, 73.478, 64.484, 52.634, 36.201, 24.267 };
                        IClimateRecord[] climateRecs = new ClimateRecord[12];

                        //If timestep is 0 then calculate otherwise get the mon_T_normal for timestep 0

                        Climate.TimestepData = allData[0];
                        for (int mo = 0; mo < 12; mo++)
                        {
                            climateRecs[mo] = Climate.TimestepData[ecoregion.Index, mo];
                            mon_T_normal[mo] = (climateRecs[mo].AvgMinTemp + climateRecs[mo].AvgMinTemp) / 2;
                        }

                        foreach (KeyValuePair<int, IClimateRecord[,]> timeStep in allData)
                        {
                            if (timeStep.Key != 0)
                            {
                                acs[timestepIndex] = new AnnualClimate(ecoregion, timeStep.Key, startYear + timeStep.Key, Landis.Extension.Succession.Century.EcoregionData.Latitude[ecoregion]); // Latitude should be given
                                timestepIndex++;
                            }
                        }
                        double AWC = Landis.Extension.Succession.Century.EcoregionData.FieldCapacity[ecoregion] - Landis.Extension.Succession.Century.EcoregionData.WiltingPoint[ecoregion];
                        double latitude = Landis.Extension.Succession.Century.EcoregionData.Latitude[ecoregion];
                        new PDSI_Calculator().CalculatePDSI(acs, mon_T_normal, AWC, latitude, outputFilePath, UnitSystem.metrics);
                    }
                }
            }



            int numberOftimeStaps = 0;
            int numberOfEcoregions = 0;

            int index = 0;
            double ecoAverage = 0;

            //List<int> levels = Climate.AnnualPDSI.AsEnumerable().Select(al => al.Field<int>("TimeStep")).Distinct().ToList().Max();
            //numberOftimeStaps = levels.Max();
            numberOftimeStaps = Climate.AnnualPDSI.AsEnumerable().Select(al => al.Field<int>("TimeStep")).Distinct().ToList().Max();

            //List<int> ecos = Climate.AnnualPDSI.AsEnumerable().Select(a2 => a2.Field<int>("Ecorigion")).Distinct().ToList().Max();
            //numberOfEcoregions = ecos.Max();
            numberOfEcoregions = Climate.AnnualPDSI.AsEnumerable().Select(a2 => a2.Field<int>("Ecorigion")).Distinct().ToList().Max();

            Climate.LandscapAnnualPDSI = new double[numberOftimeStaps];

            for (int timeStep = 1; timeStep <= numberOftimeStaps; timeStep++)
            {
                index = timeStep;
                for (int eco = 1; eco <= numberOfEcoregions; eco++)
                {
                    if (index <= Climate.AnnualPDSI.Rows.Count && (Int32)Climate.AnnualPDSI.Rows[index - 1][0] == timeStep && (Int32)Climate.AnnualPDSI.Rows[index - 1][1] == eco)
                    {
                        ecoAverage += (double)Climate.AnnualPDSI.Rows[index - 1][2];// get the valuse of annualPDSI

                        if (eco == numberOfEcoregions)
                        {
                            ecoAverage = ecoAverage / numberOfEcoregions;
                            Climate.LandscapAnnualPDSI[timeStep - 1] = ecoAverage;
                            //Can be printed
                            //file.WriteLine(timeStep + ", " + ecoAverage) ;

                            ecoAverage = 0;
                        }
                    }
                    index = index + numberOftimeStaps;
                }
            }






        }

        public static void GetPDSI_Test()
        {
            IEcoregion ecoregion = Climate.ModelCore.Ecoregions[0];
            //here:
            string outputFilePath = @"C:\Program Files\LANDIS-II\v6\examples\base-BDA_1\PDSI_BaseBDA_Test.csv";
            File.WriteAllText(outputFilePath, String.Empty);
            int startYear = 1893, endYear = 1897;
            AnnualClimate[] acs;
            if (endYear > startYear)
            {
                int numOfYears = endYear - startYear + 1;
                acs = new AnnualClimate[numOfYears];


                double[] mon_T_normal = new double[12] { 19.693, 23.849, 34.988, 49.082, 60.467, 70.074, 75.505, 73.478, 64.484, 52.634, 36.201, 24.267 };
                IClimateRecord[] climateRecs = new ClimateRecord[12];

                //Climate.TimestepData = allData[0];
                //for (int mo = 0; mo < 12; mo++)
                //{
                //    climateRecs[mo] = Climate.TimestepData[ecoregion.Index, mo];
                //    //mon_T_normal[mo] = (climateRecs[mo].AvgMinTemp + climateRecs[mo].AvgMinTemp) / 2;
                //}

                acs[0] = new AnnualClimate(ecoregion, 1893, 0);
                acs[0].MonthlyTemp = new double[] { 14.371, 14.000, 26.435, 44.250, 54.645, 70.683, 73.355, 69.323, 63.600, 48.806, 32.867, 19.161 };
                acs[0].MonthlyPrecip = new double[] { 0.610, 1.500, 1.730, 4.050, 1.950, 0.790, 3.020, 2.570, 1.430, 0.850, 1.260, 2.350 };

                acs[1] = new AnnualClimate(ecoregion, 1894, 0);
                acs[1].MonthlyTemp = new double[] { 12.705, 14.979, 37.984, 49.700, 61.209, 71.463, 77.935, 74.312, 65.283, 51.516, 34.767, 29.548 };
                acs[1].MonthlyPrecip = new double[] { 0.700, 0.550, 0.580, 4.240, 2.430, 1.150, 0.580, 1.480, 0.550, 1.760, 0.050, 1.000 };

                acs[2] = new AnnualClimate(ecoregion, 1895, 0);
                acs[2].MonthlyTemp = new double[] { 12.519, 17.964, 33.994, 54.506, 60.411, 66.172, 70.548, 69.622, 65.288, 44.795, 32.433, 23.333 };
                acs[2].MonthlyPrecip = new double[] { 0.650, 0.540, 0.520, 3.980, 2.380, 6.240, 2.320, 3.920, 4.770, 0.060, 1.040, 0.000 };

                acs[3] = new AnnualClimate(ecoregion, 1896, 0);
                acs[3].MonthlyTemp = new double[] { 23.258, 27.397, 26.425, 48.833, 62.790, 68.054, 71.365, 70.677, 57.991, 46.355, 21.154, 28.597 };
                acs[3].MonthlyPrecip = new double[] { 0.250, 0.270, 1.670, 5.680, 6.240, 7.740, 5.550, 1.660, 1.810, 3.230, 3.850, 0.230 };

                acs[4] = new AnnualClimate(ecoregion, 1897, 0);
                acs[4].MonthlyTemp = new double[] { 13.758, 20.179, 26.613, 46.700, 59.016, 66.533, 74.032, 67.928, 71.617, 54.613, 32.450, 18.686 };
                acs[4].MonthlyPrecip = new double[] { 2.500, 0.540, 3.010, 4.480, 0.980, 5.820, 3.780, 1.600, 1.010, 1.940, 0.910, 2.950 };



                //for (int i = startYear; i <= endYear; i++)
                //{
                //    acs[i - startYear] = new AnnualClimate(ecoregion, i, 0); // Latitude should be given
                //    //Console.WriteLine(ac.MonthlyTemp[0].ToString() + "\n");
                //    //Console.WriteLine(ac.MonthlyPrecip[0].ToString() + "\n");
                //}



                //for (int mo = 0; mo < 12; mo++)
                //{
                //    climateRecs[mo] = Climate.TimestepData[ecoregion.Index, mo];
                //    mon_T_normal[mo] = (climateRecs[mo].AvgMinTemp + climateRecs[mo].AvgMinTemp) / 2;
                //}

                double AWC = 0.3;//Landis.Extension.Succession.Century.EcoregionData.FieldCapacity[ecoregion] - Landis.Extension.Succession.Century.EcoregionData.WiltingPoint[ecoregion];
                double latitude = 42.60;//Landis.Extension.Succession.Century.EcoregionData.Latitude[ecoregion];
                new PDSI_Calculator().CalculatePDSI(acs, mon_T_normal, AWC, latitude, outputFilePath, UnitSystem.USCustomaryUnits);

            }




        }


        /// <summary>
        /// Converts USGS Data to Input climate Data 
        /// </summary>
        public static void Convert_USGS_to_ClimateData(Period period, string climateFile)
        {
            string path = climateFile;
            StreamReader sreader;
            // monthly and daily climates should be filled before in order to chack weather input climatefile can be processed as daily or monthly
            //List<string> montlyClimates;
            //List<string> DailyClimate;
            if (period == Period.Daily)
            {

                //string path = @"D:\PSU\Landis_II\amin-branch\USGS_Data\Hayhoe_Climate_Data1.csv";
                sreader = new StreamReader(path);
                string line;
                string[] fields;
                string tempScenarioName = "";
                DataTable _dataTableDataByTime = new DataTable();
                int numberOfAllEcorigions = 0;
                line = sreader.ReadLine();
                fields = line.Split(',');
                tempScenarioName = fields[0].Substring(1, fields[0].LastIndexOf("t") - 2);
                line = sreader.ReadLine();
                fields = line.Split(',');
                //int totalRows = 0;
                //string[,] wholedata;
                string CurrentScenarioName = "";

                string CurrentScenarioType = "";
                Dictionary<string, double[]> century_climate_Dic = new Dictionary<string, double[]>();

                //string currentT;
                //string currentSTD;
                //string currentPart = "";
                //int totalRow = 0;
                string key = "";
                int IndexMax_MeanT = 0;
                //int IndexMax_MaxT = 1;
                int IndexMax_Var = 1;
                int IndexMax_STD = 2;
                int IndexMin_MeanT = 3;
                //int IndexMin_MaxT = 5;
                int IndexMin_Var = 4;
                int IndexMin_STD = 5;
                int IndexPrcp_MeanT = 6;
                //int IndexPrcp_MaxT = 9;
                int IndexPrcp_Var = 7;
                int IndexPrcp_STD = 8;

                //bool firstFlag = false;
                string currentYear = "";
                int currentMonth = 1;
                int tempEco = 1;
                double AverageMax = 0;

                //double AverageMaxSTD = 0;
                double AverageMin = 0;
                //double AverageMinSTD = 0;
                double AveragePrecp = 0;

                double AverageSTDT =0 ;
                double StdDevPpt = 0;
                //double AveragePrecSTD = 0;
                int numberOfDays = 0;
                double[] tempSum = new double[31];
                double[] tempPrp = new double[31];
                //double sums = 0;
                //double prpSums = 0;
                //double stdTemp = 0;
                //double stdPrp = 0;
                bool emptytxt = false;
                int updatedIndex = 0;

                foreach (string field in fields)
                {
                    if (field != "" && Convert.ToInt16(field) > numberOfAllEcorigions)
                    {
                        numberOfAllEcorigions = Convert.ToInt16(field);
                    }
                }
                //12 beacuse for each ecoriogn we need Max_MinT,Max_MaxT,Max_Var Max_Std, Min_MinT,Min_MaxT,Min_Var Min_Std, Prcp_MinT,Prcp_MaxT,Prcp_Var Prcp_Std
                int dicSize = numberOfAllEcorigions * 9;
                sreader.Close();
                StreamReader reader = new StreamReader(path);

                while (reader.Peek() >= 0)
                {
                    line = reader.ReadLine();
                    fields = line.Split(',');
                    foreach (string field in fields)
                    {
                        if (field.Contains("#"))
                        {
                            //tempScenarioName = CurrentScenarioName;
                            if (field.Contains("tmax") || field.Contains("tmin"))
                            {
                                CurrentScenarioName = field.Substring(1, field.LastIndexOf("t") - 2);
                                if (field.Contains("tmax"))
                                    CurrentScenarioType = "tmax";
                                if (field.Contains("tmin"))
                                    CurrentScenarioType = "tmin";
                            }
                            if (field.Contains("pr"))
                            {
                                CurrentScenarioName = field.Substring(1, field.LastIndexOf("p") - 2);
                                CurrentScenarioType = "pr";

                            }

                            //if (tempScenarioName != CurrentScenarioName)// firstFlag == false)
                            //{
                            //    tempScenarioName = CurrentScenarioName;
                            //    //firstFlag = true;
                            //}



                            //line = reader.ReadLine();
                            //fields = line.Split(',');

                        }



                    }

                    if (fields[0] == string.Empty && !fields[0].Contains("#"))
                    {
                        line = reader.ReadLine();
                        fields = line.Split(',');

                        if (fields[0].Contains("TIME"))
                        {
                            line = reader.ReadLine();
                            fields = line.Split(',');

                            //now fill array 
                            //Get the lenght of array according to the number of ecorigions/
                            //

                        }
                    }
                    if (CurrentScenarioName == tempScenarioName && !fields[0].Contains("#"))
                    {

                        key = fields[0].ToString();
                        if (CurrentScenarioType.Contains("max"))
                        {
                            IndexMax_MeanT = 0;
                            //IndexMax_MaxT = 1;
                            IndexMax_Var = 1;
                            IndexMax_STD = 2;
                            //int indexofSTD = 0;
                            //indexofSTD = fields.Length - (numberOfAllEcorigions);

                            century_climate_Dic.Add(key, new double[dicSize]);//{ currentT, currentSTD, 0, 0, 0, 0 });

                            //set index of max and maxSTD for each ecorigion
                            for (int i = 1; i <= numberOfAllEcorigions; i++)
                            {
                                //currentT = fields[i];
                                //if (indexofSTD < 26)
                                //{
                                
                                century_climate_Dic[key].SetValue(Convert.ToDouble(fields[i]), IndexMax_MeanT);
                                updatedIndex += i + numberOfAllEcorigions;
                                //century_climate_Dic[key].SetValue(Convert.ToDouble(fields[updatedIndex]), IndexMax_MaxT);
                                //updatedIndex +=  numberOfAllEcorigions;
                                century_climate_Dic[key].SetValue(Convert.ToDouble(fields[updatedIndex]), IndexMax_Var);
                                updatedIndex += numberOfAllEcorigions;
                                century_climate_Dic[key].SetValue(Convert.ToDouble(fields[updatedIndex]), IndexMax_STD);
                                IndexMax_MeanT = IndexMax_MeanT + 9;
                                //IndexMax_MaxT = IndexMax_MaxT + 12;
                                IndexMax_Var = IndexMax_Var + 9;
                                IndexMax_STD = IndexMax_STD + 9;
                                updatedIndex = 0;

                                //indexofSTD++;
                                //}

                            }
                        }
                        if (CurrentScenarioType.Contains("min"))
                        {
                            IndexMin_MeanT = 3;
                            //IndexMin_MaxT = 5;
                            IndexMin_Var = 4;
                            IndexMin_STD = 5;
                            //int indexofSTD = 0;
                            //indexofSTD = fields.Length - (numberOfAllEcorigions);

                            // century_climate_Dic.Add(key, new double[dicSize]);//{ currentT, currentSTD, 0, 0, 0, 0 });

                            //set index of max and maxSTD for each ecorigion
                            for (int i = 1; i <= numberOfAllEcorigions; i++)
                            {
                                //currentT = fields[i];
                                //if (indexofSTD < 26)
                                //{
                                //currentSTD = fields[indexofSTD];
                                century_climate_Dic[key].SetValue(Convert.ToDouble(fields[i]), IndexMin_MeanT);
                                updatedIndex += i + numberOfAllEcorigions;
                                //century_climate_Dic[key].SetValue(Convert.ToDouble(fields[updatedIndex]), IndexMin_MaxT);
                                //updatedIndex += numberOfAllEcorigions;
                                century_climate_Dic[key].SetValue(Convert.ToDouble(fields[updatedIndex]), IndexMin_Var);
                                updatedIndex += numberOfAllEcorigions;
                                century_climate_Dic[key].SetValue(Convert.ToDouble(fields[updatedIndex]), IndexMin_STD);

                                
                                //century_climate_Dic[key].SetValue(Convert.ToDouble(currentSTD), IndexSTD);
                                IndexMin_MeanT = IndexMin_MeanT + 9;
                                //IndexMin_MaxT = IndexMin_MaxT + 12;
                                IndexMin_Var = IndexMin_Var + 9;
                                IndexMin_STD = IndexMin_STD + 9;
                                updatedIndex = 0;
                                //    IndexSTD = IndexSTD + 6;
                                //    indexofSTD++;
                                //}

                            }
                        }
                        if (CurrentScenarioType.Contains("pr"))
                        {
                            IndexPrcp_MeanT = 6;
                            //IndexPrcp_MaxT = 9;
                            IndexPrcp_Var = 7;
                            IndexPrcp_STD = 8;

                            //IndexSTD = 5;
                            //int indexofSTD = 0;
                            //indexofSTD = fields.Length - (numberOfAllEcorigions);

                            // century_climate_Dic.Add(key, new double[dicSize]);//{ currentT, currentSTD, 0, 0, 0, 0 });

                            //set index of max and maxSTD for each ecorigion
                            for (int i = 1; i <= numberOfAllEcorigions; i++)
                            {
                                //currentT = fields[i];
                                //if (indexofSTD < 26)
                                //{
                                //currentSTD = fields[indexofSTD];
                                century_climate_Dic[key].SetValue(Convert.ToDouble(fields[i]), IndexPrcp_MeanT);
                                updatedIndex += i + numberOfAllEcorigions;
                                //century_climate_Dic[key].SetValue(Convert.ToDouble(fields[updatedIndex]), IndexPrcp_MaxT);
                                //updatedIndex += numberOfAllEcorigions;
                                century_climate_Dic[key].SetValue(Convert.ToDouble(fields[updatedIndex]), IndexPrcp_Var);
                                updatedIndex += numberOfAllEcorigions;
                                century_climate_Dic[key].SetValue(Convert.ToDouble(fields[updatedIndex]), IndexPrcp_STD);


                                //century_climate_Dic[key].SetValue(Convert.ToDouble(currentSTD), IndexSTD);
                                IndexPrcp_MeanT = IndexPrcp_MeanT +9;
                                //IndexPrcp_MaxT = IndexPrcp_MaxT + 12;
                                IndexPrcp_Var = IndexPrcp_Var + 9;
                                IndexPrcp_STD = IndexPrcp_STD + 9;
                                updatedIndex = 0;
                                //IndexSTD = IndexSTD + 6;
                                //indexofSTD++;
                            }

                        }

                    }

                    if (CurrentScenarioName != tempScenarioName || reader.EndOfStream)
                    {
                        //tempScenarioName = CurrentScenarioName;
                        //Print file for one scenario then clear dictionary to use for another scenario

                        //Daily peiod
                        string centuryPath = @"C:\Program Files\LANDIS-II\v6\examples\base-BDA_1\Century_Climate_Inputs_Monthly.txt";
                        //int AverageMaxT = 0;
                        //int AverageMaxSTD = 1;
                        //int AverageMinT = 2;
                        //int AverageMinSTD = 3;
                        //int AveragePrec = 4;
                        //int AveragePrecSTD = 5;
                        IndexMax_MeanT = 0;
                        //IndexMax_MaxT = 1;
                        IndexMax_Var = 1;
                        IndexMax_STD = 2;
                        IndexMin_MeanT = 3;
                        //IndexMin_MaxT = 5;
                        IndexMin_Var = 4;
                        IndexMin_STD = 5;
                        IndexPrcp_MeanT = 6;
                        //IndexPrcp_MaxT = 9;
                        IndexPrcp_Var = 7;
                        IndexPrcp_STD = 8;


                        //int AverageMaxT = 0;
                        //int AverageMaxSTD = 1;
                        //int AverageMinT = 1;
                        //int AverageMinSTD = 3;
                        //int AveragePrec = 2;
                        //int AveragePrecSTD = 5;
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(centuryPath, emptytxt))
                        {
                            file.WriteLine("LandisData" + " \"Climate Data\" \n");
                            file.WriteLine("ClimateTable \n");
                            //file.WriteLine(tempScenarioName + "\n");
                            file.WriteLine(">>Eco" + "\t" + "Time" + "\t" + "Month" + "\t" + "AvgMinT" + "\t" + "AvgMaxT" + "\t" + "StdDevT" + "\t" + "AvgPpt" + "\t" + "StdDev" + "\t" + "PAR" + "\n");
                            file.WriteLine(">>Name" + "\t" + "Step" + "\t" + "\t" + "(C)" + "\t" + "(C)" + "\t" + "(k)" + "\t" + "(cm)" + "\t" + "Ppt" + "\t" + "µmol m-2 s-1" + "\n");
                            //file.WriteLine(">>Eco" + "\t" + "Time" + "\t" + "\t" + "AvgMaxT" + "\t" + "StdMaxT" + "\t" + "AvgMinT" + "\t" + "StdMinT" + "\t" + "AvgPpt" + "\t" + "StdDev" + "\n");
                            // file.WriteLine(">>Name" + "\t" + "Step" + "\t" + "\t" + "(C)" + "\t" + "(C)" + "\t" + "(C)" + "\t" + "(C)" + "\t" + "(C)" + "\t" + "(C)" + "\n");
                            //initialize currentYear and month
                            currentYear = century_climate_Dic.First().Key.Substring(0, 4).ToString();
                            currentMonth = Convert.ToInt16(century_climate_Dic.First().Key.Substring(5, 2).ToString());
                            tempEco = 1;

                            for (int i = 1; i <= numberOfAllEcorigions; i++)
                            {
                                foreach (KeyValuePair<string, double[]> row in century_climate_Dic)
                                {

                                    //file.WriteLine("eco" + i.ToString() + "\t" + row.Key.Remove(10) + "\t" + Math.Round(row.Value[AverageMaxT], 2) +  "\t" + Math.Round(row.Value[AverageMinT], 2) +  "\t" + Math.Round(row.Value[AveragePrec], 2)  + "\n");
                                    //file.WriteLine("eco" + i.ToString() + "\t" + row.Key.Remove(10) + "\t" + Math.Round(row.Value[AverageMaxT], 2) + "\t" + Math.Round(row.Value[AverageMaxSTD], 2) + "\t" + Math.Round(row.Value[AverageMinT], 2) + "\t" + Math.Round(row.Value[AverageMinSTD], 2) + "\t" + Math.Round(row.Value[AveragePrec], 2) + "\t" + Math.Round(row.Value[AveragePrecSTD], 2) + "\n");

                                    if (currentYear == row.Key.Substring(0, 4).ToString())
                                    {

                                        if (currentMonth == Convert.ToInt16(row.Key.Substring(5, 2)))
                                        {

                                            //(row.Value[IndexMax_MaxT] + row.Value[IndexMax_MeanT])/2
                                            //AverageMin += (row.Value[IndexMin_MaxT] + row.Value[IndexMin_MeanT]) / 2;
                                            //AverageMax += (row.Value[IndexMax_MaxT] + row.Value[IndexMax_MeanT]) / 2;
                                            //AveragePrecp += (row.Value[IndexPrcp_MaxT] + row.Value[IndexPrcp_MeanT]) / 2;
                                            //AverageSTDT += (row.Value[IndexMax_Var] + row.Value[IndexMin_Var]) / 2;
                                            //AverageMaxSTD += Math.Round(Convert.ToDouble(row.Value[2]), 2);
                                            AverageMin += Math.Round(row.Value[IndexMin_MeanT], 2);
                                            AverageMax += Math.Round(row.Value[IndexMax_MeanT], 2);
                                            AveragePrecp += Math.Round(row.Value[IndexPrcp_MeanT], 2);
                                            AverageSTDT +=  Math.Round((row.Value[IndexMax_Var] + row.Value[IndexMin_Var]) / 2,2);
                                            StdDevPpt += Convert.ToDouble(row.Value[IndexPrcp_Var]);

                                            //AverageMinSTD += Math.Round(Convert.ToDouble(row.Value[4]), 2);
                                            //AveragePrecp += Math.Round(row.Value[AveragePrec], 2);
                                            //AveragePrecSTD += Math.Round(Convert.ToDouble(row.Value[6]), 2);

                                            //Calculating STD of Tempeture
                                            //tempSum[numberOfDays] = (row.Value[AverageMaxT] + row.Value[AverageMinT]) / 2;
                                            //stdTemp = 0;
                                            //stdPrp = 0;


                                            //Calculating STD of Prp
                                            //tempPrp[numberOfDays] = row.Value[AveragePrec];

                                            numberOfDays++;

                                        }


                                        else
                                        {
                                            //for (int j = 0; j < numberOfDays; j++)
                                            //{
                                            //    sums += Math.Pow((tempSum[j] - (((AverageMax / numberOfDays) + (AverageMin / numberOfDays)) / 2)), 2);
                                            //    prpSums += Math.Pow(tempPrp[j] - (AveragePrec / numberOfDays), 2);
                                            //}

                                            //stdTemp = Math.Sqrt(sums / (numberOfDays - 1));
                                            //stdPrp = Math.Sqrt(prpSums / (numberOfDays - 1));
                                            file.WriteLine("eco" + i.ToString() + "\t" + currentYear + "\t" + currentMonth + "\t" + Math.Round(AverageMin / numberOfDays, 2) + "\t" + Math.Round(AverageMax / numberOfDays, 2) + "\t" + Math.Round(Math.Sqrt(AverageSTDT/numberOfDays), 2) + "\t" + Math.Round(AveragePrecp / numberOfDays, 2) + "\t" + Math.Round(Math.Sqrt(StdDevPpt), 2) + "\t" + "0.0" + "\n");
                                            //file.WriteLine("eco1" + "\t" + currentYear + "\t" + currentMonth + "\t" + Math.Round(AverageMax / numberOfDays, 2) + "\t" + Math.Round(AverageMaxSTD / numberOfDays, 2) + "\t" + Math.Round(AverageMinT / numberOfDays, 2) + "\t" + Math.Round(AverageMinSTD / numberOfDays, 2) + "\t" + Math.Round(AveragePrec / numberOfDays, 2) + "\t" + Math.Round(AveragePrecSTD / numberOfDays, 2) + "\n");
                                            //tempMonth = currentMonth;
                                            currentMonth = Convert.ToInt16(row.Key.Substring(5, 2));
                                            //if (tempMonth != currentMonth)

                                            AverageMax = 0;
                                            //AverageMaxSTD = 0;
                                            AverageMin = 0;
                                            //AverageMinSTD = 0;
                                            AveragePrecp = 0;
                                            //AveragePrecSTD = 0;
                                            AverageSTDT = 0;
                                            StdDevPpt = 0;

                                            numberOfDays = 0;
                                            AverageMin += Math.Round(row.Value[IndexMin_MeanT], 2);
                                            AverageMax += Math.Round(row.Value[IndexMax_MeanT], 2);
                                            AveragePrecp += Math.Round(row.Value[IndexPrcp_MeanT], 2);
                                            AverageSTDT += Math.Round((row.Value[IndexMax_Var] + row.Value[IndexMin_Var]) / 2, 2);
                                            StdDevPpt += Convert.ToDouble(row.Value[IndexPrcp_Var]);        
                                            //sums = 0;
                                            //stdTemp = 0;
                                            //prpSums = 0;
                                            //stdPrp = 0;
                                            numberOfDays++;
                                        }

                                    }
                                    else
                                    {
                                        //If ecorigion has been changed
                                        if (tempEco != i && currentMonth == 12)
                                            file.WriteLine("eco" + tempEco.ToString() + "\t" + currentYear + "\t" + currentMonth + "\t" + Math.Round(AverageMin / numberOfDays, 2) + "\t" + Math.Round(AverageMax / numberOfDays, 2) + "\t" + Math.Round(Math.Sqrt(AverageSTDT / numberOfDays), 2) + "\t" + Math.Round(AveragePrecp / numberOfDays, 2) + "\t" + Math.Round(Math.Sqrt(StdDevPpt), 2) + "\t" + "0.0" + "\n");

                                        else if (currentMonth == 12)
                                            file.WriteLine("eco" + i.ToString() + "\t" + currentYear + "\t" + currentMonth + "\t" + Math.Round(AverageMin / numberOfDays, 2) + "\t" + Math.Round(AverageMax / numberOfDays, 2) + "\t" + Math.Round(Math.Sqrt(AverageSTDT / numberOfDays), 2) + "\t" + Math.Round(AveragePrecp / numberOfDays, 2) + "\t" + Math.Round(Math.Sqrt(StdDevPpt), 2) + "\t" + "0.0" + "\n");

                                        //file.WriteLine("eco1" + "\t" + currentYear + "\t" + currentMonth + "\t" + Math.Round(AverageMaxT / numberOfDays, 2) + "\t" + Math.Round(AverageMaxSTD / numberOfDays, 2) + "\t" + Math.Round(AverageMinT / numberOfDays, 2) + "\t" + Math.Round(AverageMinSTD / numberOfDays, 2) + "\t" + Math.Round(AveragePrec / numberOfDays, 2) + "\t" + Math.Round(AveragePrecSTD / numberOfDays, 2) + "\n");

                                        currentYear = row.Key.Substring(0, 4).ToString();
                                        tempEco = i;
                                        currentMonth = 1;
                                        AverageMax = 0;
                                        //AverageMaxSTD = 0;
                                        AverageMin = 0;
                                        //AverageMinSTD = 0;
                                        AveragePrecp = 0;
                                        //AveragePrecSTD = 0;

                                        AverageSTDT = 0;
                                        StdDevPpt = 0;

                                        numberOfDays = 0;
                                        AverageMin += Math.Round(row.Value[IndexMin_MeanT], 2);
                                        AverageMax += Math.Round(row.Value[IndexMax_MeanT], 2);
                                        AveragePrecp += Math.Round(row.Value[IndexPrcp_MeanT], 2);
                                        AverageSTDT += Math.Round((row.Value[IndexMax_Var] + row.Value[IndexMin_Var]) / 2, 2);
                                        StdDevPpt += Convert.ToDouble(row.Value[IndexPrcp_Var]);         
                                        //sums = 0;
                                        //stdTemp = 0;
                                        //prpSums = 0;
                                        //stdPrp = 0;
                                        numberOfDays++;
                                    }


                                }

                                IndexMax_MeanT = IndexMax_MeanT + 9;
                                //IndexMax_MaxT = IndexMax_MaxT + 12;
                                IndexMax_Var = IndexMax_Var + 9;
                                IndexMax_STD = IndexMax_STD + 9;
                                IndexMin_MeanT = IndexMin_MeanT + 9;
                                //IndexMin_MaxT = IndexMin_MaxT + 12;
                                IndexMin_Var =  IndexMin_Var + 9;
                                IndexMin_STD =  IndexMin_STD + 9;
                                IndexPrcp_MeanT =  IndexPrcp_MeanT + 9;
                                //IndexPrcp_MaxT = IndexPrcp_MaxT + 12;
                                IndexPrcp_Var = IndexPrcp_Var + 9;
                                IndexPrcp_STD = IndexPrcp_STD + 9;
                            }
                            file.WriteLine("eco" + numberOfAllEcorigions.ToString() + "\t" + currentYear + "\t" + currentMonth + "\t" + Math.Round(AverageMin / numberOfDays, 2) + "\t" + Math.Round(AverageMax / numberOfDays, 2) + "\t" + Math.Round(Math.Sqrt(AverageSTDT / numberOfDays), 2) + "\t" + Math.Round(AveragePrecp / numberOfDays, 2) + "\t" + Math.Round(Math.Sqrt(StdDevPpt), 2) + "\t" + "0.0" + "\n");


                        }


                        //If file contains more than one scenario then these setting will be needed
                        century_climate_Dic.Clear();
                        emptytxt = true;
                        tempScenarioName = CurrentScenarioName;

                    }
                }

             


            }
            else if (period == Period.Monthly)
            {
                

                    //string path = @"D:\PSU\Landis_II\amin-branch\USGS_Data\Hayhoe_Climate_Data1.csv";
                    sreader = new StreamReader(path);
                    string line;
                    string[] fields;
                    string tempScenarioName = "";
                    DataTable _dataTableDataByTime = new DataTable();
                    int numberOfAllEcorigions = 0;
                    line = sreader.ReadLine();
                    fields = line.Split(',');
                    tempScenarioName = fields[0].Substring(1, fields[0].LastIndexOf("t") - 2);
                    line = sreader.ReadLine();
                    fields = line.Split(',');
                    //int totalRows = 0;
                    //string[,] wholedata;
                    string CurrentScenarioName = "";

                    string CurrentScenarioType = "";
                    Dictionary<string, double[]> century_climate_Dic = new Dictionary<string, double[]>();

                    //string currentT;
                    //string currentSTD;
                    //string currentPart = "";
                    //int totalRow = 0;
                    string key = "";
                    int IndexMax_MeanT = 0;
                    //int IndexMax_MaxT = 1;
                    int IndexMax_Var = 1;
                    int IndexMax_STD = 2;
                    int IndexMin_MeanT = 3;
                    //int IndexMin_MaxT = 5;
                    int IndexMin_Var = 4;
                    int IndexMin_STD = 5;
                    int IndexPrcp_MeanT = 6;
                    //int IndexPrcp_MaxT = 9;
                    int IndexPrcp_Var = 7;
                    int IndexPrcp_STD = 8;

                    //bool firstFlag = false;
                    string currentYear = "";
                    int currentMonth = 1;
                    int tempEco = 1;
                    double AverageMax = 0;

                    //double AverageMaxSTD = 0;
                    double AverageMin = 0;
                    //double AverageMinSTD = 0;
                    double AveragePrecp = 0;

                    double AverageSTDT = 0;
                    double StdDevPpt = 0;
                    //double AveragePrecSTD = 0;
                    int numberOfDays = 0;
                    double[] tempSum = new double[31];
                    double[] tempPrp = new double[31];
                    //double sums = 0;
                    //double prpSums = 0;
                    //double stdTemp = 0;
                    //double stdPrp = 0;
                    bool emptytxt = false;
                    int updatedIndex = 0;

                    foreach (string field in fields)
                    {
                        if (field != "" && Convert.ToInt16(field) > numberOfAllEcorigions)
                        {
                            numberOfAllEcorigions = Convert.ToInt16(field);
                        }
                    }
                    //12 beacuse for each ecoriogn we need Max_MinT,Max_MaxT,Max_Var Max_Std, Min_MinT,Min_MaxT,Min_Var Min_Std, Prcp_MinT,Prcp_MaxT,Prcp_Var Prcp_Std
                    int dicSize = numberOfAllEcorigions * 9;
                    sreader.Close();
                    StreamReader reader = new StreamReader(path);

                    while (reader.Peek() >= 0)
                    {
                        line = reader.ReadLine();
                        fields = line.Split(',');
                        foreach (string field in fields)
                        {
                            if (field.Contains("#"))
                            {
                                //tempScenarioName = CurrentScenarioName;
                                if (field.Contains("tmax") || field.Contains("tmin"))
                                {
                                    CurrentScenarioName = field.Substring(1, field.LastIndexOf("t") - 2);
                                    if (field.Contains("tmax"))
                                        CurrentScenarioType = "tmax";
                                    if (field.Contains("tmin"))
                                        CurrentScenarioType = "tmin";
                                }
                                if (field.Contains("pr"))
                                {
                                    CurrentScenarioName = field.Substring(1, field.LastIndexOf("p") - 2);
                                    CurrentScenarioType = "pr";

                                }

                                //if (tempScenarioName != CurrentScenarioName)// firstFlag == false)
                                //{
                                //    tempScenarioName = CurrentScenarioName;
                                //    //firstFlag = true;
                                //}



                                //line = reader.ReadLine();
                                //fields = line.Split(',');

                            }



                        }

                        if (fields[0] == string.Empty && !fields[0].Contains("#"))
                        {
                            line = reader.ReadLine();
                            fields = line.Split(',');

                            if (fields[0].Contains("TIME"))
                            {
                                line = reader.ReadLine();
                                fields = line.Split(',');

                                //now fill array 
                                //Get the lenght of array according to the number of ecorigions/
                                //

                            }
                        }
                        if (CurrentScenarioName == tempScenarioName && !fields[0].Contains("#"))
                        {

                            key = fields[0].ToString();
                            if (CurrentScenarioType.Contains("max"))
                            {
                                IndexMax_MeanT = 0;
                                //IndexMax_MaxT = 1;
                                IndexMax_Var = 1;
                                IndexMax_STD = 2;
                                //int indexofSTD = 0;
                                //indexofSTD = fields.Length - (numberOfAllEcorigions);

                                century_climate_Dic.Add(key, new double[dicSize]);//{ currentT, currentSTD, 0, 0, 0, 0 });

                                //set index of max and maxSTD for each ecorigion
                                for (int i = 1; i <= numberOfAllEcorigions; i++)
                                {
                                    //currentT = fields[i];
                                    //if (indexofSTD < 26)
                                    //{

                                    century_climate_Dic[key].SetValue(Convert.ToDouble(fields[i]), IndexMax_MeanT);
                                    updatedIndex += i + numberOfAllEcorigions;
                                    //century_climate_Dic[key].SetValue(Convert.ToDouble(fields[updatedIndex]), IndexMax_MaxT);
                                    //updatedIndex +=  numberOfAllEcorigions;
                                    century_climate_Dic[key].SetValue(Convert.ToDouble(fields[updatedIndex]), IndexMax_Var);
                                    updatedIndex += numberOfAllEcorigions;
                                    century_climate_Dic[key].SetValue(Convert.ToDouble(fields[updatedIndex]), IndexMax_STD);
                                    IndexMax_MeanT = IndexMax_MeanT + 9;
                                    //IndexMax_MaxT = IndexMax_MaxT + 12;
                                    IndexMax_Var = IndexMax_Var + 9;
                                    IndexMax_STD = IndexMax_STD + 9;
                                    updatedIndex = 0;

                                    //indexofSTD++;
                                    //}

                                }
                            }
                            if (CurrentScenarioType.Contains("min"))
                            {
                                IndexMin_MeanT = 3;
                                //IndexMin_MaxT = 5;
                                IndexMin_Var = 4;
                                IndexMin_STD = 5;
                                //int indexofSTD = 0;
                                //indexofSTD = fields.Length - (numberOfAllEcorigions);

                                // century_climate_Dic.Add(key, new double[dicSize]);//{ currentT, currentSTD, 0, 0, 0, 0 });

                                //set index of max and maxSTD for each ecorigion
                                for (int i = 1; i <= numberOfAllEcorigions; i++)
                                {
                                    //currentT = fields[i];
                                    //if (indexofSTD < 26)
                                    //{
                                    //currentSTD = fields[indexofSTD];
                                    century_climate_Dic[key].SetValue(Convert.ToDouble(fields[i]), IndexMin_MeanT);
                                    updatedIndex += i + numberOfAllEcorigions;
                                    //century_climate_Dic[key].SetValue(Convert.ToDouble(fields[updatedIndex]), IndexMin_MaxT);
                                    //updatedIndex += numberOfAllEcorigions;
                                    century_climate_Dic[key].SetValue(Convert.ToDouble(fields[updatedIndex]), IndexMin_Var);
                                    updatedIndex += numberOfAllEcorigions;
                                    century_climate_Dic[key].SetValue(Convert.ToDouble(fields[updatedIndex]), IndexMin_STD);


                                    //century_climate_Dic[key].SetValue(Convert.ToDouble(currentSTD), IndexSTD);
                                    IndexMin_MeanT = IndexMin_MeanT + 9;
                                    //IndexMin_MaxT = IndexMin_MaxT + 12;
                                    IndexMin_Var = IndexMin_Var + 9;
                                    IndexMin_STD = IndexMin_STD + 9;
                                    updatedIndex = 0;
                                    //    IndexSTD = IndexSTD + 6;
                                    //    indexofSTD++;
                                    //}

                                }
                            }
                            if (CurrentScenarioType.Contains("pr"))
                            {
                                IndexPrcp_MeanT = 6;
                                //IndexPrcp_MaxT = 9;
                                IndexPrcp_Var = 7;
                                IndexPrcp_STD = 8;

                                //IndexSTD = 5;
                                //int indexofSTD = 0;
                                //indexofSTD = fields.Length - (numberOfAllEcorigions);

                                // century_climate_Dic.Add(key, new double[dicSize]);//{ currentT, currentSTD, 0, 0, 0, 0 });

                                //set index of max and maxSTD for each ecorigion
                                for (int i = 1; i <= numberOfAllEcorigions; i++)
                                {
                                    //currentT = fields[i];
                                    //if (indexofSTD < 26)
                                    //{
                                    //currentSTD = fields[indexofSTD];
                                    century_climate_Dic[key].SetValue(Convert.ToDouble(fields[i]), IndexPrcp_MeanT);
                                    updatedIndex += i + numberOfAllEcorigions;
                                    //century_climate_Dic[key].SetValue(Convert.ToDouble(fields[updatedIndex]), IndexPrcp_MaxT);
                                    //updatedIndex += numberOfAllEcorigions;
                                    century_climate_Dic[key].SetValue(Convert.ToDouble(fields[updatedIndex]), IndexPrcp_Var);
                                    updatedIndex += numberOfAllEcorigions;
                                    century_climate_Dic[key].SetValue(Convert.ToDouble(fields[updatedIndex]), IndexPrcp_STD);


                                    //century_climate_Dic[key].SetValue(Convert.ToDouble(currentSTD), IndexSTD);
                                    IndexPrcp_MeanT = IndexPrcp_MeanT + 9;
                                    //IndexPrcp_MaxT = IndexPrcp_MaxT + 12;
                                    IndexPrcp_Var = IndexPrcp_Var + 9;
                                    IndexPrcp_STD = IndexPrcp_STD + 9;
                                    updatedIndex = 0;
                                    //IndexSTD = IndexSTD + 6;
                                    //indexofSTD++;
                                }

                            }

                        }

                        if (CurrentScenarioName != tempScenarioName || reader.EndOfStream)
                        {
                            //tempScenarioName = CurrentScenarioName;
                            //Print file for one scenario then clear dictionary to use for another scenario

                            //Daily peiod
                            string centuryPath = @"C:\Program Files\LANDIS-II\v6\examples\base-BDA_1\Century_Climate_Inputs_PRISM_Monthly.txt";
                            //int AverageMaxT = 0;
                            //int AverageMaxSTD = 1;
                            //int AverageMinT = 2;
                            //int AverageMinSTD = 3;
                            //int AveragePrec = 4;
                            //int AveragePrecSTD = 5;
                            IndexMax_MeanT = 0;
                            //IndexMax_MaxT = 1;
                            IndexMax_Var = 1;
                            IndexMax_STD = 2;
                            IndexMin_MeanT = 3;
                            //IndexMin_MaxT = 5;
                            IndexMin_Var = 4;
                            IndexMin_STD = 5;
                            IndexPrcp_MeanT = 6;
                            //IndexPrcp_MaxT = 9;
                            IndexPrcp_Var = 7;
                            IndexPrcp_STD = 8;


                            //int AverageMaxT = 0;
                            //int AverageMaxSTD = 1;
                            //int AverageMinT = 1;
                            //int AverageMinSTD = 3;
                            //int AveragePrec = 2;
                            //int AveragePrecSTD = 5;
                            using (System.IO.StreamWriter file = new System.IO.StreamWriter(centuryPath, emptytxt))
                            {
                                file.WriteLine("LandisData" + " \"Climate Data\" \n");
                                file.WriteLine("ClimateTable \n");
                                //file.WriteLine(tempScenarioName + "\n");
                                file.WriteLine(">>Eco" + "\t" + "Time" + "\t" + "Month" + "\t" + "AvgMinT" + "\t" + "AvgMaxT" + "\t" + "StdDevT" + "\t" + "AvgPpt" + "\t" + "StdDev" + "\t" + "PAR" + "\n");
                                file.WriteLine(">>Name" + "\t" + "Step" + "\t" + "\t" + "(C)" + "\t" + "(C)" + "\t" + "(k)" + "\t" + "(cm)" + "\t" + "Ppt" + "\t" + "µmol m-2 s-1" + "\n");
                                //file.WriteLine(">>Eco" + "\t" + "Time" + "\t" + "\t" + "AvgMaxT" + "\t" + "StdMaxT" + "\t" + "AvgMinT" + "\t" + "StdMinT" + "\t" + "AvgPpt" + "\t" + "StdDev" + "\n");
                                // file.WriteLine(">>Name" + "\t" + "Step" + "\t" + "\t" + "(C)" + "\t" + "(C)" + "\t" + "(C)" + "\t" + "(C)" + "\t" + "(C)" + "\t" + "(C)" + "\n");
                                //initialize currentYear and month
                                currentYear = century_climate_Dic.First().Key.Substring(0, 4).ToString();
                                currentMonth = Convert.ToInt16(century_climate_Dic.First().Key.Substring(5, 2).ToString());
                                tempEco = 1;

                                for (int i = 1; i <= numberOfAllEcorigions; i++)
                                {
                                    foreach (KeyValuePair<string, double[]> row in century_climate_Dic)
                                    {

                                        //file.WriteLine("eco" + i.ToString() + "\t" + row.Key.Remove(10) + "\t" + Math.Round(row.Value[AverageMaxT], 2) +  "\t" + Math.Round(row.Value[AverageMinT], 2) +  "\t" + Math.Round(row.Value[AveragePrec], 2)  + "\n");
                                        //file.WriteLine("eco" + i.ToString() + "\t" + row.Key.Remove(10) + "\t" + Math.Round(row.Value[AverageMaxT], 2) + "\t" + Math.Round(row.Value[AverageMaxSTD], 2) + "\t" + Math.Round(row.Value[AverageMinT], 2) + "\t" + Math.Round(row.Value[AverageMinSTD], 2) + "\t" + Math.Round(row.Value[AveragePrec], 2) + "\t" + Math.Round(row.Value[AveragePrecSTD], 2) + "\n");

                                        if (currentYear == row.Key.Substring(0, 4).ToString())
                                        {

                                            if (currentMonth == Convert.ToInt16(row.Key.Substring(5, 2)))
                                            {

                                                //(row.Value[IndexMax_MaxT] + row.Value[IndexMax_MeanT])/2
                                                //AverageMin += (row.Value[IndexMin_MaxT] + row.Value[IndexMin_MeanT]) / 2;
                                                //AverageMax += (row.Value[IndexMax_MaxT] + row.Value[IndexMax_MeanT]) / 2;
                                                //AveragePrecp += (row.Value[IndexPrcp_MaxT] + row.Value[IndexPrcp_MeanT]) / 2;
                                                //AverageSTDT += (row.Value[IndexMax_Var] + row.Value[IndexMin_Var]) / 2;
                                                //AverageMaxSTD += Math.Round(Convert.ToDouble(row.Value[2]), 2);
                                                AverageMin += Math.Round(row.Value[IndexMin_MeanT], 2);
                                                AverageMax += Math.Round(row.Value[IndexMax_MeanT], 2);
                                                AveragePrecp += Math.Round(row.Value[IndexPrcp_MeanT], 2);
                                                AverageSTDT += Math.Round((row.Value[IndexMax_Var] + row.Value[IndexMin_Var]) / 2, 2);
                                                StdDevPpt += Convert.ToDouble(row.Value[IndexPrcp_STD]);

                                                //AverageMinSTD += Math.Round(Convert.ToDouble(row.Value[4]), 2);
                                                //AveragePrecp += Math.Round(row.Value[AveragePrec], 2);
                                                //AveragePrecSTD += Math.Round(Convert.ToDouble(row.Value[6]), 2);

                                                //Calculating STD of Tempeture
                                                //tempSum[numberOfDays] = (row.Value[AverageMaxT] + row.Value[AverageMinT]) / 2;
                                                //stdTemp = 0;
                                                //stdPrp = 0;


                                                //Calculating STD of Prp
                                                //tempPrp[numberOfDays] = row.Value[AveragePrec];

                                                numberOfDays++;

                                            }


                                            else
                                            {
                                                //for (int j = 0; j < numberOfDays; j++)
                                                //{
                                                //    sums += Math.Pow((tempSum[j] - (((AverageMax / numberOfDays) + (AverageMin / numberOfDays)) / 2)), 2);
                                                //    prpSums += Math.Pow(tempPrp[j] - (AveragePrec / numberOfDays), 2);
                                                //}

                                                //stdTemp = Math.Sqrt(sums / (numberOfDays - 1));
                                                //stdPrp = Math.Sqrt(prpSums / (numberOfDays - 1));
                                                file.WriteLine("eco" + i.ToString() + "\t" + currentYear + "\t" + currentMonth + "\t" + Math.Round(AverageMin / numberOfDays, 2) + "\t" + Math.Round(AverageMax / numberOfDays, 2) + "\t" + Math.Round(Math.Sqrt(AverageSTDT / numberOfDays), 2) + "\t" + Math.Round(AveragePrecp / numberOfDays, 2) + "\t" + Math.Round(StdDevPpt, 2) + "\t" + "0.0" + "\n");
                                                //file.WriteLine("eco1" + "\t" + currentYear + "\t" + currentMonth + "\t" + Math.Round(AverageMax / numberOfDays, 2) + "\t" + Math.Round(AverageMaxSTD / numberOfDays, 2) + "\t" + Math.Round(AverageMinT / numberOfDays, 2) + "\t" + Math.Round(AverageMinSTD / numberOfDays, 2) + "\t" + Math.Round(AveragePrec / numberOfDays, 2) + "\t" + Math.Round(AveragePrecSTD / numberOfDays, 2) + "\n");
                                                //tempMonth = currentMonth;
                                                currentMonth = Convert.ToInt16(row.Key.Substring(5, 2));
                                                //if (tempMonth != currentMonth)

                                                AverageMax = 0;
                                                //AverageMaxSTD = 0;
                                                AverageMin = 0;
                                                //AverageMinSTD = 0;
                                                AveragePrecp = 0;
                                                //AveragePrecSTD = 0;
                                                AverageSTDT = 0;
                                                StdDevPpt = 0;

                                                numberOfDays = 0;
                                                AverageMin += Math.Round(row.Value[IndexMin_MeanT], 2);
                                                AverageMax += Math.Round(row.Value[IndexMax_MeanT], 2);
                                                AveragePrecp += Math.Round(row.Value[IndexPrcp_MeanT], 2);
                                                AverageSTDT += Math.Round((row.Value[IndexMax_Var] + row.Value[IndexMin_Var]) / 2, 2);
                                                StdDevPpt += Convert.ToDouble(row.Value[IndexPrcp_STD]);
                                                //sums = 0;
                                                //stdTemp = 0;
                                                //prpSums = 0;
                                                //stdPrp = 0;
                                                numberOfDays++;
                                            }

                                        }
                                        else
                                        {
                                            //If ecorigion has been changed
                                            if (tempEco != i && currentMonth == 12)
                                                file.WriteLine("eco" + tempEco.ToString() + "\t" + currentYear + "\t" + currentMonth + "\t" + Math.Round(AverageMin / numberOfDays, 2) + "\t" + Math.Round(AverageMax / numberOfDays, 2) + "\t" + Math.Round(Math.Sqrt(AverageSTDT / numberOfDays), 2) + "\t" + Math.Round(AveragePrecp / numberOfDays, 2) + "\t" + Math.Round(StdDevPpt, 2) + "\t" + "0.0" + "\n");


                                            else if (currentMonth == 12)
                                                file.WriteLine("eco" + i.ToString() + "\t" + currentYear + "\t" + currentMonth + "\t" + Math.Round(AverageMin / numberOfDays, 2) + "\t" + Math.Round(AverageMax / numberOfDays, 2) + "\t" + Math.Round(Math.Sqrt(AverageSTDT / numberOfDays), 2) + "\t" + Math.Round(AveragePrecp / numberOfDays, 2) + "\t" + Math.Round(StdDevPpt, 2) + "\t" + "0.0" + "\n");


                                            //file.WriteLine("eco1" + "\t" + currentYear + "\t" + currentMonth + "\t" + Math.Round(AverageMaxT / numberOfDays, 2) + "\t" + Math.Round(AverageMaxSTD / numberOfDays, 2) + "\t" + Math.Round(AverageMinT / numberOfDays, 2) + "\t" + Math.Round(AverageMinSTD / numberOfDays, 2) + "\t" + Math.Round(AveragePrec / numberOfDays, 2) + "\t" + Math.Round(AveragePrecSTD / numberOfDays, 2) + "\n");

                                            currentYear = row.Key.Substring(0, 4).ToString();
                                            tempEco = i;
                                            currentMonth = 1;
                                            AverageMax = 0;
                                            //AverageMaxSTD = 0;
                                            AverageMin = 0;
                                            //AverageMinSTD = 0;
                                            AveragePrecp = 0;
                                            //AveragePrecSTD = 0;

                                            AverageSTDT = 0;
                                            StdDevPpt = 0;

                                            numberOfDays = 0;
                                            AverageMin += Math.Round(row.Value[IndexMin_MeanT], 2);
                                            AverageMax += Math.Round(row.Value[IndexMax_MeanT], 2);
                                            AveragePrecp += Math.Round(row.Value[IndexPrcp_MeanT], 2);
                                            AverageSTDT += Math.Round((row.Value[IndexMax_Var] + row.Value[IndexMin_Var]) / 2, 2);
                                            StdDevPpt += Convert.ToDouble(row.Value[IndexPrcp_STD]);
                                            //sums = 0;
                                            //stdTemp = 0;
                                            //prpSums = 0;
                                            //stdPrp = 0;
                                            numberOfDays++;
                                        }


                                    }

                                    IndexMax_MeanT = IndexMax_MeanT + 9;
                                    //IndexMax_MaxT = IndexMax_MaxT + 12;
                                    IndexMax_Var = IndexMax_Var + 9;
                                    IndexMax_STD = IndexMax_STD + 9;
                                    IndexMin_MeanT = IndexMin_MeanT + 9;
                                    //IndexMin_MaxT = IndexMin_MaxT + 12;
                                    IndexMin_Var = IndexMin_Var + 9;
                                    IndexMin_STD = IndexMin_STD + 9;
                                    IndexPrcp_MeanT = IndexPrcp_MeanT + 9;
                                    //IndexPrcp_MaxT = IndexPrcp_MaxT + 12;
                                    IndexPrcp_Var = IndexPrcp_Var + 9;
                                    IndexPrcp_STD = IndexPrcp_STD + 9;
                                }
                                file.WriteLine("eco" + numberOfAllEcorigions.ToString() + "\t" + currentYear + "\t" + currentMonth + "\t" + Math.Round(AverageMin / numberOfDays, 2) + "\t" + Math.Round(AverageMax / numberOfDays, 2) + "\t" + Math.Round(Math.Sqrt(AverageSTDT / numberOfDays), 2) + "\t" + Math.Round(AveragePrecp / numberOfDays, 2) + "\t" + Math.Round(StdDevPpt, 2) + "\t" + "0.0" + "\n");



                            }


                            //If file contains more than one scenario then these setting will be needed
                            century_climate_Dic.Clear();
                            emptytxt = true;
                            tempScenarioName = CurrentScenarioName;

                        }
                    }




                
            }
            //while (sreader.Peek() >= 0)
            //{
            //    if (_dataTableDataByTime.Columns.Count == 0)
            //    {
            //        foreach (string field in fields)
            //        {
            //             //will add default names like "Column1", "Column2", and so on
            //            _dataTableDataByTime.Columns.Add();
            //        }
            //}
            //_dataTableDataByTime.Rows.Add(fields);

            //}

            //string tableName = "Hayhoe_Climate_Data_1";
            //OleDbConnection dbConnection = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path + ";Extended Properties=Excel 12.0;");
            //Exception excelFileReadingException = null;
            //try
            //{
            //dbConnection.Open();
            //OleDbDataAdapter dbAdapter =
            //    new OleDbDataAdapter
            //        ("SELECT * FROM [original$]", dbConnection);
            //dbAdapter.Fill(_dataTableDataByTime);
            //string currentT;
            //string currentSTD;
            //string currentPart = "";
            ////Dictionary<int, string[]> century_climate_Dic = new Dictionary<int, string[]>();
            //int totalRow = 0;
            //string key = "";
            //int IndexT = 0;
            //int IndexSTD = 0;

            //for (int i = 0; i < _dataTableDataByTime.Rows.Count; i++)
            //{
            //    key = _dataTableDataByTime.Rows[i][0].ToString();
            //    if (key.Contains("TIMESTEP"))
            //    {
            //        if (currentPart == "" || currentPart =="pr")
            //        {
            //            currentPart = "max";
            //            IndexT = 1;
            //            IndexSTD = 2;
            //            //i++;
            //        }
            //        else if (currentPart == "max")
            //        {
            //            currentPart = "min";
            //            IndexT = 3;
            //            IndexSTD = 4;
            //        }
            //        else if (currentPart == "min")
            //        {
            //            currentPart = "pr";
            //            IndexT = 5;
            //            IndexSTD = 6;
            //        }
            //    }
            //    else
            //    {
            //        if (_dataTableDataByTime.Rows[i][0].ToString().Trim() != string.Empty)
            //        {
            //            //now should fetch the mean column and STD column
            //            if (_dataTableDataByTime.Rows[i][1] !="" && _dataTableDataByTime.Rows[i][2] != "")
            //            {
            //                currentT = _dataTableDataByTime.Rows[i][1].ToString();
            //                currentSTD = _dataTableDataByTime.Rows[i][2].ToString();

            //                if (currentPart == "max")
            //                {
            //                    //insert new item 
            //                    century_climate_Dic.Add(i, new string[7]);//{ currentT, currentSTD, 0, 0, 0, 0 });
            //                    century_climate_Dic[i].SetValue(key, 0);
            //                    century_climate_Dic[i].SetValue(currentT, IndexT);
            //                    century_climate_Dic[i].SetValue(currentSTD, IndexSTD);
            //                }
            //                else if (currentPart == "min")
            //                {
            //                    century_climate_Dic[i].SetValue(key, 0);
            //                    century_climate_Dic[i].SetValue(currentT, IndexT);
            //                    century_climate_Dic[i].SetValue(currentSTD, IndexSTD);

            //                }
            //                else if (currentPart == "pr")
            //                {
            //                    century_climate_Dic[i].SetValue(key, 0);
            //                    century_climate_Dic[i].SetValue(currentT, IndexT);
            //                    century_climate_Dic[i].SetValue(currentSTD, IndexSTD);
            //                }
            //            }

            //        }

            //    }

            //}

            //Now Dictionary is ready to print in txt file
            //Daily peiod
            //string centuryPath = @"C:\Program Files\LANDIS-II\v6\examples\base-BDA_1\Century_Climate_Inputs_1.txt";
            //using (System.IO.StreamWriter file = new System.IO.StreamWriter(centuryPath, false))
            //{
            //    file.WriteLine("LandisData" + "Climate Data" + "\n");
            //    file.WriteLine("ClimateTable \n");
            //    file.WriteLine(">>Eco" + "\t" + "Time" + "\t" + "\t" + "AvgMaxT" + "\t" + "StdMaxT" + "\t" + "AvgMinT" + "\t" + "StdMinT" + "\t" + "AvgPpt" + "\t" + "StdDev" + "\n");
            //    file.WriteLine(">>Name" + "\t" + "Step" + "\t" + "\t" + "(C)" + "\t" + "(C)" + "\t" + "(C)" + "\t" + "(C)" + "\t" + "(C)" + "\t" + "(C)" + "\n");
            //    foreach (KeyValuePair<int, string[]> row in century_climate_Dic)
            //    {
            //        file.WriteLine("eco1" + "\t" + row.Value[0].Remove(10) + "\t" + Math.Round(Convert.ToDouble(row.Value[1]), 2) + "\t" + Math.Round(Convert.ToDouble(row.Value[2]), 2) + "\t" + Math.Round(Convert.ToDouble(row.Value[3]), 2) + "\t" + Math.Round(Convert.ToDouble(row.Value[4]), 2) + "\t" + Math.Round(Convert.ToDouble(row.Value[5]), 2) + "\t" + Math.Round(Convert.ToDouble(row.Value[6]), 2) + "\n");
            //    }

            //}

            // should apply later
            //monthly period
            //string currentYear = "";
            //int currentMonth = 1;
            //int tempMonth = 1;
            //double AverageMaxT = 0;
            //double AverageMaxSTD = 0;
            //double AverageMinT = 0;
            //double AverageMinSTD = 0;
            //double AveragePrec = 0;
            //double AveragePrecSTD = 0;
            //int numberOfDays = 0;
            //string centuryPathMonthly = @"C:\Program Files\LANDIS-II\v6\examples\base-BDA_1\Century_Climate_Inputs_2.txt";
            //using (System.IO.StreamWriter file = new System.IO.StreamWriter(centuryPathMonthly, false))
            //{
            //    file.WriteLine("LandisData" + "Climate Data" + "\n");
            //    file.WriteLine("ClimateTable \n");
            //    file.WriteLine(">>Eco" + "\t" + "Year" + "\t" + "Month" + "\t" + "AvgMaxT" + "\t" + "StdMaxT" + "\t" + "AvgMinT" + "\t" + "StdMinT" + "\t" + "AvgPpt" + "\t" + "StdDev" + "\n");
            //    file.WriteLine(">>Name" + "\t" + " " + "\t" + " " + "\t" + "(C)" + "\t" + "(C)" + "\t" + "(C)" + "\t" + "(C)" + "\t" + "(C)" + "\t" + "(C)" + "\n");

            //    foreach (KeyValuePair<int, string[]> row in century_climate_Dic)
            //    {
            //        if (currentYear == row.Value[0].Substring(0, 4).ToString())
            //        {

            //            if (currentMonth == Convert.ToInt16(row.Value[0].Substring(5, 2)))
            //            {
            //                AverageMaxT += Math.Round(Convert.ToDouble(row.Value[1]), 2);
            //                AverageMaxSTD += Math.Round(Convert.ToDouble(row.Value[2]), 2);
            //                AverageMinT += Math.Round(Convert.ToDouble(row.Value[3]), 2);
            //                AverageMinSTD += Math.Round(Convert.ToDouble(row.Value[4]), 2);
            //                AveragePrec += Math.Round(Convert.ToDouble(row.Value[5]), 2);
            //                AveragePrecSTD += Math.Round(Convert.ToDouble(row.Value[6]), 2);
            //                numberOfDays++;
            //            }
            //            else
            //            {

            //                file.WriteLine("eco1" + "\t" + currentYear + "\t" + currentMonth + "\t" + Math.Round(AverageMaxT / numberOfDays, 2) + "\t" + Math.Round(AverageMaxSTD / numberOfDays, 2) + "\t" + Math.Round(AverageMinT / numberOfDays, 2) + "\t" + Math.Round(AverageMinSTD / numberOfDays, 2) + "\t" + Math.Round(AveragePrec / numberOfDays, 2) + "\t" + Math.Round(AveragePrecSTD / numberOfDays, 2) + "\n");
            //                //tempMonth = currentMonth;
            //                currentMonth = Convert.ToInt16(row.Value[0].Substring(5, 2));
            //                //if (tempMonth != currentMonth)

            //                AverageMaxT = 0;
            //                AverageMaxSTD = 0;
            //                AverageMinT = 0;
            //                AverageMinSTD = 0;
            //                AveragePrec = 0;
            //                AveragePrecSTD = 0;
            //                numberOfDays = 0;
            //                AverageMaxT += Math.Round(Convert.ToDouble(row.Value[1]), 2);
            //                AverageMaxSTD += Math.Round(Convert.ToDouble(row.Value[2]), 2);
            //                AverageMinT += Math.Round(Convert.ToDouble(row.Value[3]), 2);
            //                AverageMinSTD += Math.Round(Convert.ToDouble(row.Value[4]), 2);
            //                AveragePrec += Math.Round(Convert.ToDouble(row.Value[5]), 2);
            //                AveragePrecSTD += Math.Round(Convert.ToDouble(row.Value[6]), 2);
            //                numberOfDays++;
            //            }

            //        }
            //        else
            //        {
            //            if (currentMonth == 12)
            //                file.WriteLine("eco1" + "\t" + currentYear + "\t" + currentMonth + "\t" + Math.Round(AverageMaxT / numberOfDays, 2) + "\t" + Math.Round(AverageMaxSTD / numberOfDays, 2) + "\t" + Math.Round(AverageMinT / numberOfDays, 2) + "\t" + Math.Round(AverageMinSTD / numberOfDays, 2) + "\t" + Math.Round(AveragePrec / numberOfDays, 2) + "\t" + Math.Round(AveragePrecSTD / numberOfDays, 2) + "\n");

            //            currentYear = row.Value[0].Substring(0, 4).ToString();
            //            currentMonth = 1;
            //            AverageMaxT = 0;
            //            AverageMaxSTD = 0;
            //            AverageMinT = 0;
            //            AverageMinSTD = 0;
            //            AveragePrec = 0;
            //            AveragePrecSTD = 0;
            //            numberOfDays = 0;
            //            AverageMaxT += Math.Round(Convert.ToDouble(row.Value[1]), 2);
            //            AverageMaxSTD += Math.Round(Convert.ToDouble(row.Value[2]), 2);
            //            AverageMinT += Math.Round(Convert.ToDouble(row.Value[3]), 2);
            //            AverageMinSTD += Math.Round(Convert.ToDouble(row.Value[4]), 2);
            //            AveragePrec += Math.Round(Convert.ToDouble(row.Value[5]), 2);
            //            AveragePrecSTD += Math.Round(Convert.ToDouble(row.Value[6]), 2);
            //            numberOfDays++;
            //        }

            //    }
            //}




            ///-------------------------------------
            //}

            //catch (Exception ex)
            //{
            //    excelFileReadingException = ex;
            //    //MessageBox.Show("Import Failed: Could not read the excel file or excel file was not found!");
            //}
            //finally
            //{
            //    dbConnection.Close();
            //}

        }


    }

}
