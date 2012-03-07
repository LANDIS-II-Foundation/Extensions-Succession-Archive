//  Copyright 2007-2010 Portland State University, University of Wisconsin-Madison
//  Author: Robert Scheller, Ben Sulman

using Landis.Core;
using Landis.SpatialModeling;
using Edu.Wisc.Forest.Flel.Util;

namespace Landis.Extension.Succession.Century
{
    /// <summary>
    /// </summary>
    public class SoilLayer 
    {
        
        public static void Decompose(ActiveSite site)
        {
            //PlugIn.ModelCore.Log.WriteLine("SiteVars.SOM2[site].Nitrogen = {0:0.00}", SiteVars.SOM2[site].Nitrogen);
            //PlugIn.ModelCore.Log.WriteLine("SiteVars.MineralN = {0:0.00}", SiteVars.MineralN[site]);
            
            IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];
            
            //---------------------------------------------------------------------
            // Surface SOM1 decomposes to SOM2 with CO2 lost to respiration.
            
            double som1c_surface = SiteVars.SOM1surface[site].Carbon;  
                                
            if (som1c_surface > 0.0000001)
            {
                // Determine C/N ratios for flows to som2
                double radds1 = OtherData.SurfaceActivePoolCNIntercept 
                    + OtherData.SurfaceActivePoolCNSlope * ((som1c_surface / SiteVars.SOM1surface[site].Nitrogen) 
                    - OtherData.MinCNSurfMicrobes);
                
                double ratioCNtoSOM2 = (som1c_surface / SiteVars.SOM1surface[site].Nitrogen) + radds1;
                ratioCNtoSOM2 = System.Math.Max(ratioCNtoSOM2, OtherData.SurfaceActivePoolCNMinimum);
        
                //Compute total C flow out of surface microbes.
                double totalCflow = som1c_surface
                    * SiteVars.DecayFactor[site]
                    * EcoregionData.DecayRateSurf[ecoregion]
                    * OtherData.MonthAdjust
                    * OtherData.LitterParameters[(int)LayerType.Surface].DecayRateMicrobes;
                    
                // If decomposition can occur, schedule flows associated with respiration
                // and decomposition
                if (SiteVars.SOM1surface[site].DecomposePossible(ratioCNtoSOM2, SiteVars.MineralN[site]))
                {   
                    
                    //CO2 loss - Compute and schedule respiration flows.
                    double co2loss = totalCflow * OtherData.P1CO2_Surface;
                    double netCFlow = totalCflow - co2loss;
                    SiteVars.SOM1surface[site].Respiration(co2loss, site);

                    // Decompose Surface SOM1 to SOM2
                    SiteVars.SOM1surface[site].TransferCarbon(SiteVars.SOM2[site], netCFlow);
                    SiteVars.SOM1surface[site].TransferNitrogen(SiteVars.SOM2[site], netCFlow, som1c_surface, ratioCNtoSOM2, site);

                }
            }


            //---------------------------------------------------------------------
            // Soil SOM1 decomposes to SOM2 and SOM3 with CO2 loss and leaching
            
            double som1c_soil = SiteVars.SOM1soil[site].Carbon;
            //PlugIn.ModelCore.Log.WriteLine("SOM1soil[site].Carbon={0:0.00}", som1c_soil);
            //PlugIn.ModelCore.Log.WriteLine("SiteVars.MineralN = {0:0.00} - pre SOM1.", SiteVars.MineralN[site]);
          
            if (som1c_soil > 0.0000001)
            {
            
                //Determine C/N ratios for flows to som2
                double ratioCNtoSOM2  = Layer.BelowgroundDecompositionRatio(site,
                                            OtherData.MinCNenterSOM2, 
                                            OtherData.MaxCNenterSOM2,
                                            OtherData.MinContentN_SOM2);

                //Compute total C flow out of soil microbes.
                //Added impact of soil anaerobic conditions -rm 12/91
                double textureEffect = OtherData.TextureEffectIntercept 
                                        + OtherData.TextureEffectSlope * EcoregionData.PercentSand[ecoregion];
                
                double anerb = SiteVars.AnaerobicEffect[site];

                //PlugIn.ModelCore.Log.WriteLine("SiteVars.DecayFactor = {0:0.00}, SoilDecayRateMicrobes = {1:0.00}, texture = {2:0.00}, anerb = {3:0.00}, MonthAdjust = {4:0.00}.",
                double totalCflow = som1c_soil 
                            * SiteVars.DecayFactor[site]
                            * OtherData.LitterParameters[(int) LayerType.Soil].DecayRateMicrobes
                            * EcoregionData.DecayRateSOM1[ecoregion] 
                            * textureEffect  
                            * anerb
                            * OtherData.MonthAdjust;

                // If soil SOM1 can decompose to SOM2, it will also go to SOM3.
                // If it can't go to SOM2, it can't decompose at all.

                //If decomposition can occur,
                //PlugIn.ModelCore.Log.WriteLine("SiteVars.MineralN = {0:0.00} - SOM1: before DecomposePossible.", SiteVars.MineralN[site]);
                if (SiteVars.SOM1soil[site].DecomposePossible(ratioCNtoSOM2, SiteVars.MineralN[site]))
                {   
                    //CO2 Loss - Compute and schedule respiration flows
                    double P1CO2_Soil = OtherData.P1CO2_Soil_Intercept + OtherData.P1CO2_Soil_Slope * EcoregionData.PercentSand[ecoregion];

                    double co2loss = totalCflow * P1CO2_Soil;
                    double netCFlow = totalCflow - co2loss;
                    SiteVars.SOM1soil[site].Respiration(co2loss, site);
 
                    //Decompose Soil SOM1 to SOM3
                    //The fraction of totalCflow that goes to SOM3 is a function of
                    //clay content.
                    double clayEffect = OtherData.PS1S3_Intercept + (OtherData.PS1S3_Slope * EcoregionData.PercentClay[ecoregion]);
                    
                    double cFlowS1S3 = netCFlow * clayEffect * (1.0 + OtherData.AnaerobicImpactSlope * (1.0 - anerb));

                    //PlugIn.ModelCore.Log.WriteLine("cFlowS1S3={0:0.00}, totalCflow={1:0.00}, clayEffect={2:0.00}.", cFlowS1S3,totalCflow,clayEffect);

                    //Compute and schedule N, P, and S flows and update mineralization
                    //accumulators
                    double ratioCNto3 = Layer.BelowgroundDecompositionRatio(site,
                                            OtherData.MinCNenterSOM3, 
                                            OtherData.MaxCNenterSOM3,
                                            OtherData.MinContentN_SOM3);
                     
                    //Partition and schedule C and N flows 
                    SiteVars.SOM1soil[site].TransferCarbon(SiteVars.SOM3[site], cFlowS1S3);
                    SiteVars.SOM1soil[site].TransferNitrogen(SiteVars.SOM3[site], cFlowS1S3, som1c_soil, ratioCNto3, site);
                     
                    // Leaching of Organics
                    // This only occurs when the water flow out of water layer 2
                    // exceeds a critical value.  Use the same C/N, C/P, and C/S
                    // ratios as for the flow to SOM3.

                    double cLeached = 0.0;  // Carbon leached to a stream
                    
                    if(SiteVars.WaterMovement[site] > 0.0)  //index of water movement that indicates saturation (amov)
                    {
                    
                        double leachTextureEffect = OtherData.OMLeachIntercept + OtherData.OMLeachSlope * EcoregionData.PercentSand[ecoregion];
                        double linten = System.Math.Min(1.0 - ((OtherData.OMLeachWater - SiteVars.WaterMovement[site])  / OtherData.OMLeachWater), 1.0);
                        cLeached = netCFlow * leachTextureEffect * linten;
                
                        //Partition and schedule C flows 
                        SiteVars.SOM1soil[site].TransferCarbon(SiteVars.Stream[site], cLeached);

                        // Compute and schedule N flows and update mineralization accumulators
                        // Need to use the ratio for som1 for organic leaching
                        double ratioCNof1 = som1c_soil / SiteVars.SOM1soil[site].Nitrogen;
                        double orgflow = cLeached / ratioCNof1;
                        
                        SiteVars.SOM1soil[site].Nitrogen -= orgflow; 
                        SiteVars.Stream[site].Nitrogen += orgflow;

                        //PlugIn.ModelCore.Log.WriteLine("SOM1 soil flow to stream.  Water Movement = {0}", SiteVars.WaterMovement[site]);
                    }

                    //Decompose Soil SOM1 to SOM2.
                    //SOM2 gets what's left of totalCflow.
                    double cFlowS1S2 = netCFlow - cFlowS1S3 - cLeached;

                    //Partition and schedule C and N flows 
                    SiteVars.SOM1soil[site].TransferCarbon(SiteVars.SOM2[site], cFlowS1S2);
                    SiteVars.SOM1soil[site].TransferNitrogen(SiteVars.SOM2[site], cFlowS1S2, som1c_soil, ratioCNtoSOM2, site);

                }  
            } 


            //---------------------------------------------------------------------
            //**********SOM2 decomposes to soil SOM1 and SOM3 with CO2 loss**********
            // PlugIn.ModelCore.Log.WriteLine("    Decompose SOM2...");

            double som2c = SiteVars.SOM2[site].Carbon;
            //PlugIn.ModelCore.Log.WriteLine("som2c={0:0.00}.", som2c);
            //PlugIn.ModelCore.Log.WriteLine("SiteVars.MineralN = {0:0.00} - pre SOM2.", SiteVars.MineralN[site]);
          
            if (som2c > 0.0000001)
            {
                // Determine C/N ratios for flows to SOM1
                double ratioCNto1 = Layer.BelowgroundDecompositionRatio(site,
                                        OtherData.MinCNenterSOM1, 
                                        OtherData.MaxCNenterSOM1,
                                        OtherData.MinContentN_SOM1);
                
                double anerb = SiteVars.AnaerobicEffect[site];  

                //Compute total C flow out of SOM2C
                //Added impact of soil anaerobic conditions -rm 12/91
               
                double totalCflow = som2c 
                                * SiteVars.DecayFactor[site] 
                                * EcoregionData.DecayRateSOM2[ecoregion] 
                                * anerb
                                * OtherData.MonthAdjust;
                //PlugIn.ModelCore.Log.WriteLine("SiteVars.SOM2DecayFactor = {0:0.00}, DecayRateSOM2 = {1:0.00}, anerb = {2:0.00}, MonthAdjust = {3:0.00}.", SiteVars.DecayFactor[site], EcoregionData.DecayRateSOM2[ecoregion], anerb, OtherData.MonthAdjust);

                //If SOM2 can decompose to SOM1, it will also go to SOM3.
                //If it can't go to SOM1, it can't decompose at all.

                if (SiteVars.SOM2[site].DecomposePossible(ratioCNto1, SiteVars.MineralN[site]))
                {
                
                    //CO2 loss - Compute and schedule respiration flows
                    double co2loss = totalCflow * OtherData.FractionSOM2toCO2;
                    double netCFlow = totalCflow - co2loss;
                    SiteVars.SOM2[site].Respiration(co2loss, site);

                    // -----------------------------------------------
                    // Decompose SOM2 to SOM3, SOM3 gets what's left of totalCflow.
                    // Added impact of soil anaerobic conditions
                    double clayEffect = OtherData.PS2S3_Intercept + OtherData.PS2S3_Slope * EcoregionData.PercentClay[ecoregion];
                    double cFlowS2S3 = netCFlow * clayEffect * (1.0 + OtherData.AnaerobicImpactSlope * (1.0 - anerb));

                    //Compute and schedule C and N flows and update mineralization accumulators
                    double ratioCNto3 = Layer.BelowgroundDecompositionRatio(site,
                                            OtherData.MinCNenterSOM3, 
                                            OtherData.MaxCNenterSOM3,
                                            OtherData.MinContentN_SOM3);
                    
                    //Partition and schedule C and N flows 
                    SiteVars.SOM2[site].TransferCarbon(SiteVars.SOM3[site], cFlowS2S3);
                    SiteVars.SOM2[site].TransferNitrogen(SiteVars.SOM3[site], cFlowS2S3, som2c, ratioCNto3, site);
                   
                    // -----------------------------------------------
                    // Decompose SOM2 to SOM1
                    // Added impact of soil anaerobic conditions
                    double cFlowS2S1 = netCFlow - cFlowS2S3;

                    // Compute and schedule N and C flows and update mineralization accumulators
                    ratioCNto1 = Layer.BelowgroundDecompositionRatio(site,
                                        OtherData.MinCNenterSOM1, 
                                        OtherData.MaxCNenterSOM1,
                                        OtherData.MinContentN_SOM1);

                    //Partition and schedule C and N flows 
                    SiteVars.SOM2[site].TransferCarbon(SiteVars.SOM1soil[site], cFlowS2S1);
                    SiteVars.SOM2[site].TransferNitrogen(SiteVars.SOM1soil[site], cFlowS2S1, som2c, ratioCNto1, site);
                    
                }
                
            }

            //---------------------------------------------------------------------
            // SOM3 decomposes to soil SOM1 with CO2 loss
           
            double som3c = SiteVars.SOM3[site].Carbon; 
            
            if (som3c > 0.0000001)
            {
                //Determine C/N ratios for flows to SOM1.
                double ratioCNto1 = Layer.BelowgroundDecompositionRatio(site,
                                        OtherData.MinCNenterSOM1, 
                                        OtherData.MaxCNenterSOM1,
                                        OtherData.MinContentN_SOM1);
                 
                double anerb = SiteVars.AnaerobicEffect[site];  

                //Compute total C flow out of SOM3C
                double totalCflow = som3c
                                * SiteVars.DecayFactor[site]
                                * EcoregionData.DecayRateSOM3[ecoregion]
                                * anerb 
                                * OtherData.MonthAdjust;


                //If decomposition can occur,
                if (SiteVars.SOM3[site].DecomposePossible(ratioCNto1, SiteVars.MineralN[site]))
                {
                    //CO2 loss - Compute and schedule respiration flows.
                    double co2loss = totalCflow * OtherData.FractionSOM3toCO2 * anerb;
                    double netCFlow = totalCflow - co2loss;
                    SiteVars.SOM3[site].Respiration(co2loss, site);

                    //Decompose SOM3 to soil SOM1
                    double cFlowS3S1 = netCFlow;

                    //Partition and schedule C and N flows 
                    SiteVars.SOM3[site].TransferCarbon(SiteVars.SOM1soil[site], cFlowS3S1);
                    SiteVars.SOM3[site].TransferNitrogen(SiteVars.SOM1soil[site], cFlowS3S1, som3c, ratioCNto1, site);
                            
                }
            }
        }
    }
}
