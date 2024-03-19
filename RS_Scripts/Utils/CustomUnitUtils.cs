using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace RS_Scripts.Utils
{
    internal class CustomUnitUtils
    {
        public static double ConvertInternalUnitsToMillimeters(Document doc, double valueInInternalUnits)
        {
            // Get the internal length units of the document
            DisplayUnitType displayUnitType = doc.GetUnits().GetFormatOptions(UnitType.UT_Length).DisplayUnits;

            // Convert the value from internal units to millimeters
            double valueInMillimeters = UnitUtils.ConvertFromInternalUnits(valueInInternalUnits, displayUnitType);

            return valueInMillimeters;
        }

        public static double ConvertMillimetersToInternalUnits(Document doc, double valueInMillimeters)
        {
            // Get the internal length units of the document
            DisplayUnitType displayUnitType = doc.GetUnits().GetFormatOptions(UnitType.UT_Length).DisplayUnits;

            // Convert the value from millimeters to internal units
            double valueInInternalUnits = UnitUtils.ConvertToInternalUnits(valueInMillimeters, displayUnitType);

            return valueInInternalUnits;
        }
    }
}
