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

            // ReferenceIntersector to detect
            ReferenceIntersector ri = new ReferenceIntersector(
                selectedFaces.Select(x => doc.GetElement(x.Reference).Id).ToList(), // Gets the Id of the element owning the face
                // TODO: Cannot get an ElementId from a Face
                FindReferenceTarget.Face,
                doc.ActiveView as View3D
                );

            // Compute and collect all the new points that will be added to the topography
            IList<XYZ> newPoints = new List<XYZ>();
            IList<XYZ> topoPointsToDelete = new List<XYZ>();

            foreach (Face face in selectedFaces)
            {
                // Projects the topography points into the surface in order to
                // detect what points are above or below the surface to delete them
                // using the ReferenceIntersector
                Surface surface = face.GetSurface();

                foreach (XYZ point in topoPoints)
                {
                    try
                    {
                        ReferenceWithContext riResult = ri.FindNearest(point, new XYZ(point.X, point.Y, -1));

                        if (riResult == null)
                        {
                            riResult = ri.FindNearest(point, new XYZ(point.X, point.Y, 1));

                            if (riResult == null)
                            {
                                continue;
                            }
                        }

                        if (!topoPointsToDelete.Contains(point))
                        {
                            topoPointsToDelete.Add(point);
                        }
                    }
                    catch (ArgumentNullException)
                    {
                        continue;
                    }
                }

                // Gets the points from parametrized positions along the face
                for (double u = 0; u <= 1; u += 0.05)
                {
                    for (double v = 0; v <= 1; v += 0.05)
                    {
                        UV evaluationParameter = new UV(u, v);
                        XYZ point = face.Evaluate(evaluationParameter);

                        // If the Z component of the normal at the point is positive, do not include it
                        if (face.ComputeNormal(evaluationParameter).Z > 0)
                        {
                            continue;
                        }

                        newPoints.Add(point);
                    }
                }

            }

            // Deletes the collected points from the topography
            selectedTopo.DeletePoints(topoPointsToDelete);

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
