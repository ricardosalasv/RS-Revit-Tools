using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows.Input;
using RS_Scripts.Utils;

namespace RS_Scripts.Scripts
{
    public class ResizeElement : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            Selection sel = commandData.Application.ActiveUIDocument.Selection;

            if (sel.GetElementIds().Count != 1)
            {
                TaskDialog.Show("Error", "Please select only one element.");
                return Result.Failed;
            }

            Element element = doc.GetElement(sel.GetElementIds().First());

            if (!(element is FamilyInstance))
            {
                TaskDialog.Show("Error", "Select an instance");
                return Result.Failed;
            }

            string input = "";
            while (true)
            {
                Console.WriteLine("Specify new width and height/length for the element: ");
                input = Console.ReadLine();

                if (input == null)
                {
                    Console.WriteLine("Invalid input. Please try again.");
                    continue;
                }

                // Validate with regex ^\d*, ?\d*$
                if (Regex.Match(input, "^\\d*, ?\\d*$").Success)
                {
                    break;
                }
            }

            // Split input
            string width = input.Split(',')[0];
            string heightLength = input.Split(',')[1];

            using (Transaction tt = new Transaction(doc, "Resize element"))
            {
                tt.Start();
                FamilySymbol familySymbol = ((FamilyInstance)element).Symbol;
                double originalWidth = CustomUnitUtils.ConvertInternalUnitsToMillimeters(doc, familySymbol.LookupParameter("Width").AsDouble());

                double originalHeight = 0;
                double originalLength = 0;
                try
                {
                    originalHeight = CustomUnitUtils.ConvertInternalUnitsToMillimeters(doc, familySymbol.LookupParameter("Height").AsDouble());

                    if (originalHeight == 0) throw new Exception();
                }
                catch (Exception ex)
                {
                    originalLength = CustomUnitUtils.ConvertInternalUnitsToMillimeters(doc, familySymbol.LookupParameter("Length").AsDouble());
                }

                string newName = familySymbol.Name
                    .Replace(originalWidth.ToString(), width)
                    .Replace(originalLength.ToString(), heightLength)
                    .Replace(originalHeight.ToString(), heightLength);
                
                FamilySymbol newSymbol = familySymbol.Duplicate(newName) as FamilySymbol;

                ((FamilyInstance)element).Symbol = newSymbol;

                tt.Commit();
            }

            return Result.Succeeded;
        }
    }
}
