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
using Autodesk.Revit.Attributes;
using RS_Scripts.Views;

namespace RS_Scripts.Scripts
{
    [Transaction(TransactionMode.Manual)]
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
                InputForm form = new InputForm();
                form.ShowDialog();

                input = form.UserData;

                if (input == null)
                {
                    Console.WriteLine("Invalid input. Please try again.");
                    continue;
                }

                // Validate with regex ^\d*, ?\d*$
                if (Regex.Match(input, "^\\d*(, ?\\d*)?$").Success)
                {
                    break;
                }
            }

            // Split input
            string width = input.Split(',')[0];
            string heightLength = "";

            try
            {
                heightLength = input.Split(',')[1];
            }
            catch (Exception) { }

            using (Transaction tt = new Transaction(doc, "Resize element"))
            {
                tt.Start();
                FamilySymbol familySymbol = ((FamilyInstance)element).Symbol;
                double originalWidth = CustomUnitUtils.ConvertInternalUnitsToMillimeters(doc, familySymbol.LookupParameter("Width").AsDouble());

                double originalHeight = 9999;
                double originalLength = 9999;
                try
                {
                    originalHeight = CustomUnitUtils.ConvertInternalUnitsToMillimeters(doc, familySymbol.LookupParameter("Height").AsDouble());

                    if (originalHeight == 9999) throw new Exception();
                }
                catch
                {
                    originalLength = CustomUnitUtils.ConvertInternalUnitsToMillimeters(doc, familySymbol.LookupParameter("Length").AsDouble());
                }

                string newName = familySymbol.Name.Replace(originalWidth.ToString(), width);
                newName = newName[0] == '0' ? newName.Substring(1) : newName;
                newName = String.IsNullOrEmpty(heightLength) ? newName : newName.Replace(originalLength.ToString(), heightLength)
                    .Replace(originalHeight.ToString(), heightLength);
                    
                
                FamilySymbol newSymbol = familySymbol.Duplicate(newName) as FamilySymbol;

                newSymbol.LookupParameter("Width").Set(CustomUnitUtils.ConvertMillimetersToInternalUnits(doc, double.Parse(width)));
                if (!String.IsNullOrEmpty(heightLength))
                {
                    try
                    {
                        newSymbol.LookupParameter("Height").Set(CustomUnitUtils.ConvertMillimetersToInternalUnits(doc, double.Parse(heightLength)));
                    }
                    catch
                    {
                        newSymbol.LookupParameter("Length").Set(CustomUnitUtils.ConvertMillimetersToInternalUnits(doc, double.Parse(heightLength)));
                    }
                }

                ((FamilyInstance)element).Symbol = newSymbol;

                tt.Commit();
            }

            return Result.Succeeded;
        }
    }
}
