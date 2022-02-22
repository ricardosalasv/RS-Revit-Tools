using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS_Scripts.Scripts
{
    [Transaction(TransactionMode.Manual)]
    internal class TopoToFace : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            Selection sel = commandData.Application.ActiveUIDocument.Selection;

            var selectedFaces = SelectFaces(doc, sel);

            return Result.Succeeded;
        }

        public IList<Reference> SelectFaces(Document doc, Selection sel)
        {
            try
            {
                IList<Reference> selectedFaces = sel.PickObjects(ObjectType.Face, "Please select the face(s)");

                IList<PlanarFace> faces = selectedFaces.Select(x => doc.GetElement(x).GetGeometryObjectFromReference(x) as PlanarFace).ToList();
                IList<PlanarFace> nonVerticalFaces = faces.Select(x => x).Where(x => x.FaceNormal.Z != 0).ToList();

                IList<XYZ> newPoints = new List<XYZ>();

                foreach (var face in nonVerticalFaces)
                {
                    foreach (CurveLoop curveLoop in face.GetEdgesAsCurveLoops())
                    {
                        foreach (Curve curve in curveLoop)
                        {
                            for (double i = 0.01; i <= 0.99; i = i + 0.05)
                            {
                                newPoints.Add(curve.Evaluate(i, true));
                            }
                        }
                    }
                }

                return selectedFaces;
            }
            catch (OperationCanceledException)
            {
                TaskDialog.Show("Message", "The operation was canceled by the user");
                return null;
            }

        }
    }
}
