using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
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
            var selectedTopo = SelectTopo(doc, sel);

            // Get the points from the toposurface
            IList<XYZ> topoPoints = selectedTopo.GetPoints();

            // [OPTIONAL] Get the boundary of each subregion to store them in memory and recreate them in the new toposurface
            // TODO

            // Get boundaries of faces to remove points within those boundaries
            // TODO

            // Compute and collect all the new points that will be added to the topography
            IList<XYZ> newPoints = new List<XYZ>();
            foreach (Face face in selectedFaces)
            {
                foreach (CurveLoop curveLoop in face.GetEdgesAsCurveLoops())
                {
                    // Evaluate points throughout all the surface of the face, for all topologies
                    // TODO

                    //foreach (Curve curve in curveLoop)
                    //{
                    //    for (double i = 0.01; i <= 0.99; i = i + 0.05)
                    //    {
                    //        newPoints.Add(curve.Evaluate(i, true));
                    //    }
                    //}
                }
            }

            // Cleaning points that share the same X and Y coordinates. Prioritize removing points in a higher position
            // TODO

            // Adding points to the topography

            return Result.Succeeded;
        }

        private IList<Face> SelectFaces(Document doc, Selection sel)
        {
            TaskDialog.Show("Instructions", "Please select the faces you want the topography to conform to...");

            try
            {
                IList<Reference> selectedFaces = sel.PickObjects(ObjectType.Face, "Please select the face(s)");

                IList<Face> faces = selectedFaces.Select(x => doc.GetElement(x).GetGeometryObjectFromReference(x) as Face).ToList();
                IList<Face> nonVerticalFaces = faces.Select(x => x).Where(x => x.ComputeNormal(new UV(0.5, 0.5)).Z != 0).ToList();

                return nonVerticalFaces;
            }
            catch (OperationCanceledException)
            {
                TaskDialog.Show("Message", "The operation was canceled by the user");
                return null;
            }

        }

        private TopographySurface SelectTopo(Document doc, Selection sel)
        {
            TaskDialog.Show("Instructions", "Please select the toposurface (the main toposurface, no subregions or pads)...");

            Reference surfaceRef = sel.PickObject(ObjectType.Element, "Please select the toposurface");

            Element surfaceElem = doc.GetElement(surfaceRef);

            TopographySurface surface = surfaceElem as TopographySurface;

            return surface;
        }
    }
}
