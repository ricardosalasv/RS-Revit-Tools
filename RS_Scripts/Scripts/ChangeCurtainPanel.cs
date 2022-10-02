using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using RS_Scripts.lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS_Scripts.Scripts
{
    [Transaction(TransactionMode.Manual)]
    internal class ChangeCurtainPanel : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            var currentSelection = commandData.Application.ActiveUIDocument.Selection.GetElementIds();

            if (currentSelection.Count == 0)
            {
                TaskDialog.Show("Information", "You must have selected at least one curtain panel to perform the change.");

                return Result.Succeeded;
            }

            Transaction tt = new Transaction(doc, "Replace curtain panel");
            tt.Start();

            // Creating temporary 3D view to run the ReferenceIntersector within
            Temp3DView.Create(doc);

            // Getting the type of panel that will replace the existing one
            IList<Element> panelTypes = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Doors)
                .WhereElementIsElementType()
                .ToElements();

            FamilySymbol selectedPanelType = panelTypes.Select(x => x as FamilySymbol).Where(x => x.Name.ToLower() == "m_door-curtain-wall-single-glass").First();

            // Instancing a ReferenceIntersector to detect the bottom mullion in case the selected curtain panel is a door
            ElementFilter elementFilter = new ElementCategoryFilter(BuiltInCategory.OST_CurtainWallMullions);

            ReferenceIntersector ri = new ReferenceIntersector(elementFilter, FindReferenceTarget.All, Temp3DView.Active);

            foreach (var id in currentSelection)
            {
                var element = doc.GetElement(id);

                if (element is Panel)
                {
                    Panel panel = element as Panel;

                    // Getting the centroid of the current panel
                    XYZ centroid = Helpers.GetCentroid(panel);

                    if (centroid == null)
                    {
                        TaskDialog.Show("Error", "An error occured while calculating the centroid of one of the panels.");
                        continue;
                    }

                    if (panel.Pinned) panel.Pinned = false;

                    panel.Symbol = selectedPanelType;

                    if (panel.Category.Name.ToLower() == "doors")
                    {
                        ReferenceWithContext riResult = ri.FindNearest(centroid, XYZ.BasisZ.Negate());

                        Mullion mullion = null;

                        try
                        {
                            mullion = doc.GetElement(riResult.GetReference()) as Mullion;
                        }
                        catch (Exception)
                        {
                            continue;
                        }

                        if (mullion.Pinned) mullion.Pinned = false;

                        doc.Delete(mullion.Id);
                    }
                }
            }

            Temp3DView.Terminate(doc);

            tt.Commit();

            return Result.Succeeded;
        }
    }
}
