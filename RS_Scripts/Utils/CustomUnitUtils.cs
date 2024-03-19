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
        public static double ConvertInternalUnitsToMillimeters(double valueInInternalUnits)
        {
            double valueInMillimeters = UnitUtils.ConvertFromInternalUnits(valueInInternalUnits, UnitTypeId.Millimeters);

            return valueInMillimeters;
        }

        public static double ConvertMillimetersToInternalUnits(double valueInMillimeters)
        {
            double valueInInternalUnits = UnitUtils.ConvertToInternalUnits(valueInMillimeters, UnitTypeId.Millimeters);

            return valueInInternalUnits;
        }
    }
}
