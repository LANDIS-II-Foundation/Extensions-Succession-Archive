using Edu.Wisc.Forest.Flel.Util;
using Landis.Biomass.Succession.ClimateChange;
using NUnit.Framework;
using System.Collections.Generic;

namespace Landis.Test.Biomass.Succession.ClimateChange
{
    [TestFixture]
    public class ParametersParser_Test
        : BiomassParametersParser_Tester
    {
        private ParametersParser parser;
        private LineReader reader;

        //---------------------------------------------------------------------

        [TestFixtureSetUp]
        public void Init()
        {
            parser = new ParametersParser(Data.EcoregionDataset, Data.SpeciesDataset);
        }

        //---------------------------------------------------------------------

        [Test]
        public void GoodFile()
        {
            ReadAndCheckParameters("GoodFile.txt");
        }

        //---------------------------------------------------------------------

        private string MakeRelativePath(string filename)
        {
            return System.IO.Path.Combine("climate-change", filename);
        }

        //---------------------------------------------------------------------

        private FileLineReader OpenFile(string filename)
        {
            return Data.OpenFile(MakeRelativePath(filename));
        }

        //---------------------------------------------------------------------

        private void ReadAndCheckParameters(string filename)
        {
            IParameters parameters;
            try {
                reader = OpenFile(filename);
                parameters = parser.Parse(reader);
            }
            finally {
                reader.Close();
            }

            try {
                //  Now that we know the data file is properly formatted, read
                //  data from it and compare it against parameter object.
                reader = OpenFile(filename);
                InputLine = new InputLine(reader);

                Assert.AreEqual(parser.LandisDataValue, ReadInputVar<string>("LandisData"));

                CheckBiomassParameters(parameters, null);
                //  Note: The null parameter above indicates that no parameter
                //  or table are expected after the biomass parameters.
            }
            finally {
                InputLine = null;
                reader.Close();
            }
        }

        //---------------------------------------------------------------------

        private void TryParse(string filename)
        {
            int? errorLineNum = Testing.FindErrorMarker(Data.MakeInputPath(MakeRelativePath(filename)));
            try {
                reader = OpenFile(filename);
                IParameters parameters = parser.Parse(reader);
            }
            catch (System.Exception e) {
                Data.Output.WriteLine();
                Data.Output.WriteLine(e.Message.Replace(Data.Directory, Data.DirPlaceholder));
                LineReaderException lrExc = e as LineReaderException;
                if (lrExc != null && errorLineNum.HasValue)
                    Assert.AreEqual(errorLineNum.Value, lrExc.LineNumber);
                throw;
            }
            finally {
                reader.Close();
            }
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void LandisData_WrongValue()
        {
            TryParse("LandisData-WrongValue.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void MinRelBiomass_Missing()
        {
            TryParse("MinRelBiomass-Missing.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void MinRelBiomass_MissingEcoregions()
        {
            TryParse("MinRelBiomass-MissingEcoregions.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void MinRelBiomass_UnknownEcoregion()
        {
            TryParse("MinRelBiomass-UnknownEcoregion.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void MinRelBiomass_InactiveEcoregion()
        {
            TryParse("MinRelBiomass-InactiveEcoregion.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void MinRelBiomass_RepeatedEcoregion()
        {
            TryParse("MinRelBiomass-RepeatedEcoregion.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void MinRelBiomass_MissingRows()
        {
            TryParse("MinRelBiomass-MissingRows.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void MinRelBiomass_FirstRowNotClass1()
        {
            TryParse("MinRelBiomass-FirstRowNotClass1.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void MinRelBiomass_MissingValue()
        {
            TryParse("MinRelBiomass-MissingValue.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void MinRelBiomass_ExtraText()
        {
            TryParse("MinRelBiomass-ExtraText.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void MinRelBiomass_BadValue()
        {
            TryParse("MinRelBiomass-BadValue.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void MinRelBiomass_BelowMin()
        {
            TryParse("MinRelBiomass-BelowMin.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void MinRelBiomass_AboveMax()
        {
            TryParse("MinRelBiomass-AboveMax.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void MinRelBiomass_ExtraRow()
        {
            TryParse("MinRelBiomass-ExtraRow.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void MinRelBiomass_MissingRow4()
        {
            TryParse("MinRelBiomass-MissingRow4.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void BiomassParms_Missing()
        {
            TryParse("BiomassParms-Missing.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void BiomassParms_WrongName()
        {
            TryParse("BiomassParms-WrongName.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void BiomassParms_UnknownSpecies()
        {
            TryParse("BiomassParms-UnknownSpecies.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void BiomassParms_RepeatedSpecies()
        {
            TryParse("BiomassParms-RepeatedSpecies.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void BiomassParms_ExtraText()
        {
            TryParse("BiomassParms-ExtraText.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void LeafLongevity_Missing()
        {
            TryParse("LeafLongevity-Missing.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void LeafLongevity_BadValue()
        {
            TryParse("LeafLongevity-BadValue.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void LeafLongevity_BelowMin()
        {
            TryParse("LeafLongevity-BelowMin.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void LeafLongevity_AboveMax()
        {
            TryParse("LeafLongevity-AboveMax.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void WoodyDecayRate_Missing()
        {
            TryParse("WoodyDecayRate-Missing.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void WoodyDecayRate_BadValue()
        {
            TryParse("WoodyDecayRate-BadValue.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void WoodyDecayRate_BelowMin()
        {
            TryParse("WoodyDecayRate-BelowMin.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void WoodyDecayRate_AboveMax()
        {
            TryParse("WoodyDecayRate-AboveMax.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void MortCurveShapeParm_Missing()
        {
            TryParse("MortCurveShapeParm-Missing.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void MortCurveShapeParm_BadValue()
        {
            TryParse("MortCurveShapeParm-BadValue.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void MortCurveShapeParm_BelowMin()
        {
            TryParse("MortCurveShapeParm-BelowMin.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void MortCurveShapeParm_AboveMax()
        {
            TryParse("MortCurveShapeParm-AboveMax.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void EstablishProb_Missing()
        {
            TryParse("EstablishProb-Missing.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void EstablishProb_UnknownEcoregion()
        {
            TryParse("EstablishProb-UnknownEcoregion.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void EstablishProb_InactiveEcoregion()
        {
            TryParse("EstablishProb-InactiveEcoregion.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void EstablishProb_RepeatedEcoregion()
        {
            TryParse("EstablishProb-RepeatedEcoregion.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void EstablishProb_UnknownSpecies()
        {
            TryParse("EstablishProb-UnknownSpecies.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void EstablishProb_RepeatedSpecies()
        {
            TryParse("EstablishProb-RepeatedSpecies.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void EstablishProb_MissingValue()
        {
            TryParse("EstablishProb-MissingValue.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void EstablishProb_BadValue()
        {
            TryParse("EstablishProb-BadValue.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void EstablishProb_BelowMin()
        {
            TryParse("EstablishProb-BelowMin.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void EstablishProb_AboveMax()
        {
            TryParse("EstablishProb-AboveMax.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void EstablishProb_ExtraText()
        {
            TryParse("EstablishProb-ExtraText.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void MaxANPP_Missing()
        {
            TryParse("MaxANPP-Missing.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void MaxANPP_UnknownEcoregion()
        {
            TryParse("MaxANPP-UnknownEcoregion.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void MaxANPP_InactiveEcoregion()
        {
            TryParse("MaxANPP-InactiveEcoregion.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void MaxANPP_RepeatedEcoregion()
        {
            TryParse("MaxANPP-RepeatedEcoregion.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void MaxANPP_UnknownSpecies()
        {
            TryParse("MaxANPP-UnknownSpecies.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void MaxANPP_RepeatedSpecies()
        {
            TryParse("MaxANPP-RepeatedSpecies.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void MaxANPP_MissingValue()
        {
            TryParse("MaxANPP-MissingValue.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void MaxANPP_BadValue()
        {
            TryParse("MaxANPP-BadValue.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void MaxANPP_BelowMin()
        {
            TryParse("MaxANPP-BelowMin.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void MaxANPP_AboveMax()
        {
            TryParse("MaxANPP-AboveMax.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void MaxANPP_ExtraText()
        {
            TryParse("MaxANPP-ExtraText.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void LeafDecayRate_Missing()
        {
            TryParse("LeafDecayRate-Missing.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void LeafDecayRate_UnknownEcoregion()
        {
            TryParse("LeafDecayRate-UnknownEcoregion.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void LeafDecayRate_InactiveEcoregion()
        {
            TryParse("LeafDecayRate-InactiveEcoregion.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void LeafDecayRate_RepeatedEcoregion()
        {
            TryParse("LeafDecayRate-RepeatedEcoregion.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void LeafDecayRate_UnknownSpecies()
        {
            TryParse("LeafDecayRate-UnknownSpecies.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void LeafDecayRate_RepeatedSpecies()
        {
            TryParse("LeafDecayRate-RepeatedSpecies.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void LeafDecayRate_MissingValue()
        {
            TryParse("LeafDecayRate-MissingValue.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void LeafDecayRate_BadValue()
        {
            TryParse("LeafDecayRate-BadValue.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void LeafDecayRate_BelowMin()
        {
            TryParse("LeafDecayRate-BelowMin.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void LeafDecayRate_AboveMax()
        {
            TryParse("LeafDecayRate-AboveMax.txt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void LeafDecayRate_ExtraText()
        {
            TryParse("LeafDecayRate-ExtraText.txt");
        }
    }
}
