//  Copyright 2007-2010 Portland State University
//  Authors:  Robert M. Scheller
//  License:  Available at
//  http://www.landis-ii.org/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Util;
using System;

using Landis.Ecoregions;
using Landis.Landscape;
using Landis.Species;
using System.IO;

namespace Landis.Extension.Succession.Century
{
    /// <summary>
    /// </summary>
    public class WoodLayer
    {

        //---------------------------------------------------------------------------
        public static void Decompose(ActiveSite site)
        {
        //lock(site){

            double wood2c = SiteVars.SurfaceDeadWood[site].Carbon;

            double anerb = SiteVars.AnaerobicEffect[site];

            //....LARGE WOOD....
            if (wood2c > 0.0000001)
            {
                //Compute total C flow out of large wood
                double totalCFlow = wood2c
                                    * SiteVars.DecayFactor[site]
                                    * SiteVars.SurfaceDeadWood[site].DecayValue
                                    * System.Math.Exp(-1 * OtherData.LigninDecayEffect * SiteVars.SurfaceDeadWood[site].FractionLignin)
                                    * OtherData.MonthAdjust;

                //UI.WriteLine("Decompose wood.  C={0:0.00}, Cflow={1:0.00}, DF={2:0.00}.", wood2c, totalCFlow, SiteVars.DecayFactor[site]);
                //UI.WriteLine("  R/C={0}/{1}.  Dead Wood={2:0.0}, CFlow={3:0.00}, DecayFactor={4:0.00}, DecayValue={5:0.0}, fractionLignin={6:0.00}.", site.Location.Row, site.Location.Column, wood2c, totalCFlow, SiteVars.DecayFactor[site], SiteVars.SurfaceDeadWood[site].DecayValue, SiteVars.SurfaceDeadWood[site].FractionLignin);

                // Decompose large wood into SOM1 and SOM2 with CO2 loss.
                SiteVars.SurfaceDeadWood[site].DecomposeLignin(totalCFlow, site);
            }


                //....COARSE ROOTS (SoilDeadWood)....
            double wood3c = SiteVars.SoilDeadWood[site].Carbon;

            if (wood3c > 0.0000001)
            {
                //Compute total C flow out of coarse roots.
                double ligninEffect = System.Math.Exp(-1 * OtherData.LigninDecayEffect * SiteVars.SoilDeadWood[site].FractionLignin);
                double totalCFlow = wood3c
                                    * SiteVars.DecayFactor[site]
                                    * SiteVars.SoilDeadWood[site].DecayValue
                                    * ligninEffect
                                    * anerb
                                    * OtherData.MonthAdjust;



                //UI.WriteLine("  Coarse Root={0:0.0}, CFlow={1:0.00}, DecayFactor={2:0.00}, DecayValue={3:0.0}, ligninEffect={4:0.00}, anerb={5:0.00}.", wood3c, totalCFlow, SiteVars.DecayFactor[site], SiteVars.SoilDeadWood[site].DecayValue, ligninEffect, anerb);

                SiteVars.SoilDeadWood[site].DecomposeLignin(totalCFlow, site);
            }
        }

        public static void PartitionResidue(
                            double inputMass,
                            double inputDecayValue,
                            double inputCNratio,
                            double fracLignin,
                            LayerName name,
                            LayerType type,
                            ActiveSite site)
        {
            double directAbsorb = 0.0;
            double ratioCNtotal = 0.0;
            double totalNitrogen = 0.0;

            // from dry matter to C, 0.5 ratio
            double totalC = inputMass * 0.47;

            if (totalC < 0.0000001)
                return;

            // ...For each mineral element..
            // ...Compute amount of element in residue.
            double Npart = totalC / inputCNratio;

            //UI.WriteLine("tC={0}, inputCNratio={1}, name={2}, type={3}.", totalC, inputCNratio, name, type);

            // ...Direct absorption of mineral element by residue
            //      (mineral will be transferred to donor compartment
            //      and then partitioned into structural and metabolic
            //      using flow routines.)

            // ...If minerl(SRFC,iel) is negative then directAbsorb = zero.
            if (SiteVars.MineralN[site] <=  0.0)
                directAbsorb  = 0.0;
            else
                directAbsorb = OtherData.FractionSurfNAbsorbed
                                * SiteVars.MineralN[site]
                                * System.Math.Max(totalC / OtherData.ResidueMaxDirectAbsorb, 1.0);


            // ...If C/N ratio is too low, transfer just enough to make
            //       C/N of residue = damrmn
            if (Npart + directAbsorb  <= 0.0)
                ratioCNtotal = 0.0;
            else
                ratioCNtotal = totalC / (Npart + directAbsorb);

            if (ratioCNtotal < OtherData.MinResidueCN )
                directAbsorb  = (totalC / OtherData.MinResidueCN) - Npart;

            if (directAbsorb  < 0.0)
                directAbsorb  = 0.0;

            if(directAbsorb > SiteVars.MineralN[site])
                directAbsorb = SiteVars.MineralN[site];

            SiteVars.MineralN[site] -= directAbsorb;

            totalNitrogen = directAbsorb + Npart;

            Layer layer;

            if((int) name == (int) LayerName.Wood)
                layer = SiteVars.SurfaceDeadWood[site];
            else
                layer = SiteVars.SoilDeadWood[site];

            layer.Carbon += totalC;
            layer.Nitrogen += totalNitrogen;

            // ...Adjust lignin and decay rates in Structural Layers
            layer.AdjustLignin(totalC, fracLignin);
            layer.AdjustDecayRate(totalC,  inputDecayValue);

            return;

        //}
        }

    }
}
