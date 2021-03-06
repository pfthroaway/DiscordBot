﻿using System.Globalization;

namespace ExtensionsCore.DataTypeHelpers
{
    /// <summary>Extension class to more easily parse Doubles.</summary>
    public static class DoubleHelper
    {
        /// <summary>Utilizes double.TryParse to easily parse a Double.</summary>
        /// <param name="obj">Object to be parsed</param>
        /// <returns>Parsed Double</returns>
        public static double Parse(object obj) => Parse(obj.ToString());

        /// <summary>Utilizes double.TryParse to easily parse a Double.</summary>
        /// <param name="text">Text to be parsed</param>
        /// <returns>Parsed Double</returns>
        public static double Parse(string text)
        {
            double.TryParse(text, NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out double temp);
            return temp;
        }
    }
}