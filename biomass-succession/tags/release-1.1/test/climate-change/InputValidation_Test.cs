using Edu.Wisc.Forest.Flel.Util;
using Landis.Biomass.Succession.ClimateChange;
using NUnit.Framework;

namespace Landis.Test.Biomass.Succession.ClimateChange
{
    [TestFixture]
    public class InputValidation_Test
    {
        private const int startYear = 1900;
        private const int endYear =   2400;
        private const int duration = endYear - startYear;
        private InputVar<int> yearVar;

        //---------------------------------------------------------------------

        [TestFixtureSetUp]
        public void Init()
        {
            yearVar = new InputVar<int>("Year", InputValidation.ReadYear);
            InputValidation.Initialize(startYear, endYear);
        }

        //---------------------------------------------------------------------

        private void ReadAndCheckYear(string text,
                                       int    expectedYear,
                                       string expectedString)
        {
            TryReadYear(text);
            Assert.AreEqual(expectedYear, yearVar.Value.Actual);
            Assert.AreEqual(expectedString, yearVar.Value.String);
        }

        //---------------------------------------------------------------------

        private void TryReadYear(string text)
        {
            try {
                StringReader reader = new StringReader(text);
                yearVar.ReadValue(reader);
            }
            catch (System.Exception exc) {
                Data.Output.WriteLine(exc.Message);
                throw;
            }
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(InputVariableException))]
        public void ReadYear_Missing()
        {
            TryReadYear("");
        }

        //---------------------------------------------------------------------

        [Test]
        public void ReadYear_Start()
        {
            ReadAndCheckYear(" start", startYear, "start");
        }

        //---------------------------------------------------------------------

        [Test]
        public void ReadYear_StartPlus0()
        {
            ReadAndCheckYear(" start+00", startYear, "start+00");
        }

        //---------------------------------------------------------------------

        [Test]
        public void ReadYear_StartPlus1()
        {
            ReadAndCheckYear(" start+1", startYear+1, "start+1");
        }

        //---------------------------------------------------------------------

        [Test]
        public void ReadYear_StartPlusDuration()
        {
            string expr = string.Format("start+{0}", duration);
            ReadAndCheckYear("  " + expr, endYear, expr);
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(InputVariableException))]
        public void ReadYear_StartPlus()
        {
            TryReadYear("start+");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(InputVariableException))]
        public void ReadYear_StartPlusJunk()
        {
            TryReadYear("start+123MainSt");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(InputVariableException))]
        public void ReadYear_StartMinus()
        {
            TryReadYear("start-123");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(InputVariableException))]
        public void ReadYear_StartExprAfterEnd()
        {
            TryReadYear(string.Format("start+{0}", duration + 1));
        }

        //---------------------------------------------------------------------

        [Test]
        public void ReadYear_End()
        {
            ReadAndCheckYear("  end", endYear, "end");
        }

        //---------------------------------------------------------------------

        [Test]
        public void ReadYear_EndMinus0()
        {
            ReadAndCheckYear(" end-0", endYear, "end-0");
        }

        //---------------------------------------------------------------------

        [Test]
        public void ReadYear_EndMinus1()
        {
            ReadAndCheckYear("  end-001", endYear-1, "end-001");
        }

        //---------------------------------------------------------------------

        [Test]
        public void ReadYear_EndMinusDuration()
        {
            string expr = string.Format("end-{0}", duration);
            ReadAndCheckYear("  " + expr, startYear, expr);
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(InputVariableException))]
        public void ReadYear_EndMinus()
        {
            TryReadYear("end-");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(InputVariableException))]
        public void ReadYear_EndMinusJunk()
        {
            TryReadYear("end->here");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(InputVariableException))]
        public void ReadYear_EndPlus()
        {
            TryReadYear("end+70");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(InputVariableException))]
        public void ReadYear_EndExprBeforeStart()
        {
            TryReadYear(string.Format("end-{0}", duration + 1));
        }
    }
}
