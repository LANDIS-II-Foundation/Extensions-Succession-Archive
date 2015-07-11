//  Copyright 2009 Conservation Biology Institute
//  Authors:  Robert M. Scheller
//  License:  Available at  
//  http://www.landis-ii.org/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Util;
using System.Text.RegularExpressions;

namespace Landis.Extension.Succession.Century.Dynamic
{
    /// <summary>
    /// Methods for validating input values for climate change.
    /// </summary>
    public static class InputValidation
    {
        private static ParseMethod<int> parseInt;
        private static bool initialized = false;
        private static int startYear;
        private static int endYear;

        //---------------------------------------------------------------------

        static InputValidation()
        {
            parseInt = InputValues.GetParseMethod<int>();
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Initializes this module.
        /// </summary>
        public static void Initialize()
        {
            //if (start > end)
            //    throw new System.ArgumentException("Start year is > end year");

            startYear = PlugIn.ModelCore.StartTime;
            endYear = PlugIn.ModelCore.EndTime;
            initialized = true;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Reads a year or a year expression.
        /// </summary>
        /// A year expression is an expression that refers to a year by its
        /// relation to either the scenario's starting or ending year.  The
        /// valid format for a year expression is:
        /// 
        /// <pre>
        ///    start
        ///    start+<i>integer</i>
        ///    end
        ///    end-<i>integer</i>
        /// </pre>
        /// </remarks>
        public static InputValue<int> ReadYear(StringReader reader,
                                               out int      index)
        {
            CheckForInitialization();
            TextReader.SkipWhitespace(reader);
            index = reader.Index;
            string word = TextReader.ReadWord(reader);
            return new InputValue<int>(ParseYear(word), word);
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Parses a word for a year, handling year expressions.
        /// </summary>
        /// <remarks>
        /// A year expression is an expression that refers to a year by its
        /// relation to either the scenario's starting or ending year.  The
        /// valid format for a year expression is:
        /// 
        /// <pre>
        ///    start
        ///    start+<i>integer</i>
        ///    end
        ///    end-<i>integer</i>
        /// </pre>
        /// </remarks>
        public static int ParseYear(string word)
        {
            CheckForInitialization();
            if (word == "")
                throw new InputValueException(); // Missing value

            int year;
            bool isExpression;
            if (word.StartsWith("start")) {
                year = ParseYearExpression(word, "start", "+");
                isExpression = true;
            }
            else if (word.StartsWith("end")) {
                year = ParseYearExpression(word, "end", "-");
                isExpression = true;
            }
            else {
                year = parseInt(word);
                isExpression = false;
            }

            string description = isExpression ? "The expression's result (= year {0})"
                                              : "Year {0}";
            CheckYear(year, word, description);
            return year;
        }

        //---------------------------------------------------------------------

        public static int ParseYearExpression(string expression,
                                              string initialKeyword,
                                              string expectedSign)
        {
            int year = (initialKeyword == "start" ? startYear : endYear);
            string restOfExpr = expression.Substring(initialKeyword.Length);
            if (restOfExpr == "")
                return new InputValue<int>(year, expression);

            if (! restOfExpr.StartsWith(expectedSign))
                throw NewParseYearException(expression,
                                            "Missing \"{0}\" and integer after \"{1}\"",
                                            expectedSign, initialKeyword);
            string offset = restOfExpr.Substring(expectedSign.Length);
            if (offset == "")
                throw NewParseYearException(expression,
                                            "Missing integer after the \"{0}\"",
                                            expectedSign);
            if (! Regex.IsMatch(offset, @"^\d+$"))
                throw NewParseYearException(expression,
                                            "\"{0}\" is not a valid integer",
                                            offset);

            int multiplier = (expectedSign == "-") ? -1 : 1;
            return year + (multiplier * parseInt(offset));
        }

        //---------------------------------------------------------------------

        private static InputValueException NewParseYearException(string          expression,
                                                                 string          message,
                                                                 params object[] mesgArgs)
        {
            string errorMessage = string.Format("\"{0}\" is not a valid year or year expression", expression);
            string errorDetails = string.Format(message, mesgArgs);
            return new InputValueException(expression, errorMessage, new MultiLineText(errorDetails));
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Checks if the year of a climate change is valid.
        /// </summary>
        private static void CheckYear(int    year,
                                      string yearAsString,
                                      string description)
        {
            if (year < startYear)
                throw new InputValueException(yearAsString,
                                              description + " is before the scenario's first year ({1})",
                                              year, startYear);
            if (year > endYear)
                throw new InputValueException(yearAsString,
                                              description + " is after the scenario's last year ({1})",
                                              year, endYear);
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Checks if the path of the parameter file for a climate change is
        /// valid.
        /// </summary>
        public static void CheckPath(InputValue<string> path)
        {
            CheckForInitialization();
            if (path.Actual.Trim(null).Length == 0)
                throw new InputValueException(path.String,
                                              "{0} is not a valid path.",
                                              path.String);
        }

        //---------------------------------------------------------------------

        private static void CheckForInitialization()
        {
            if (! initialized)
                throw new System.InvalidOperationException();
        }
    }
}
