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

            var selectedTopo = SelectTopo(doc, sel);
            var selectedFaces = SelectFaces(doc, sel, out IList<ElementId> selectedFacesIds);

            // Get the points from the toposurface
            IList<XYZ> topoPoints = selectedTopo.GetPoints();

            // ReferenceIntersector to detect
            ReferenceIntersector ri = new ReferenceIntersector(
                selectedFacesIds,
                FindReferenceTarget.All,
                doc.ActiveView as View3D
                );

            // Compute and collect all the new points that will be added to the topography
            IList<XYZ> newPoints = new List<XYZ>();
            IList<XYZ> topoPointsToDelete = new List<XYZ>();

            foreach (Face face in selectedFaces)
            {
                foreach (XYZ point in topoPoints)
                {
                    try
                    {
                        XYZ origin = new XYZ(point.X, point.Y, 9999);
                        ReferenceWithContext riResult = ri.FindNearest(origin, XYZ.BasisZ.Negate());

                        if (riResult == null)
                        {
                            continue;
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
                XYZ pointToAdd = null;
                double resolution = 0.2;

                double maxDimension = GetFaceMaximumDimension(face, out double uDim, out double vDim);
                for (double u = uDim * -1; u <= uDim; u += resolution)
                {
                    for (double v = vDim * -1; v <= vDim; v += resolution)
                    {
                        pointToAdd = EvaluateAndAddTopoPoint(new UV(u, v), face);

                        if (pointToAdd == null) continue;

                        newPoints.Add(pointToAdd);
                    }
                }
                // --- Adding points to last border and corner limited by precision ---
            }

            // Cleaning points that share the same X and Y coordinates. Prioritize removing points in a higher position
            IList<XYZ> cleanedPoints = newPoints.OrderBy(p => p.Z)
                .GroupBy(p => new { p.X, p.Y })
                .Select(grp => grp.First())
                .ToList();

            TopographyEditScope topoEdit = new TopographyEditScope(doc, "Conform Topo to Faces");
            topoEdit.Start(selectedTopo.Id);

            Transaction tt = new Transaction(doc, "Edit Topography");
            tt.Start();

            // Deletes the collected points from the topography
            if (topoPointsToDelete.Count > 0)
            {
                selectedTopo.DeletePoints(topoPointsToDelete);
            }

            // Adding points to the topography
            selectedTopo.AddPoints(cleanedPoints);

            tt.Commit();
            topoEdit.Commit(new TopoEditFailurePreprocessor());

            return Result.Succeeded;
        }

        private double GetFaceMaximumDimension(Face face, out double u, out double v)
        {
            double max = 0;

            BoundingBoxUV bb = face.GetBoundingBox();

            u = bb.Max.U - bb.Min.U;
            v = bb.Max.V - bb.Min.V;

            max = u > v ? u : v;

            max = max == 0 ? 50 : max;

            return max;
        }

        private IList<Face> SelectFaces(Document doc, Selection sel, out IList<ElementId> facesIds)
        {
            //TaskDialog.Show("Instructions", "Please select the faces you want the topography to conform to...");

            try
            {
                IList<Reference> selectedFaces = sel.PickObjects(ObjectType.Face, "Please select the face(s)");

                facesIds = selectedFaces.GroupBy(x => x.ElementId.IntegerValue)
                    .Select(grp => grp.First().ElementId).ToList(); // Select distinct ElementIds

                IList<Face> faces = selectedFaces.Select(x => doc.GetElement(x).GetGeometryObjectFromReference(x) as Face).ToList();
                IList<Face> nonVerticalFaces = faces.Select(x => x).Where(x => x.ComputeNormal(new UV(0.5, 0.5)).Z != 0).ToList();

                return nonVerticalFaces;
            }
            catch (OperationCanceledException)
            {
                facesIds = null;
                TaskDialog.Show("Message", "The operation was canceled by the user");
                return null;
            }

        }

        private TopographySurface SelectTopo(Document doc, Selection sel)
        {
            //TaskDialog.Show("Instructions", "Please select the toposurface (the main toposurface, no subregions or pads)...");

            Reference surfaceRef = sel.PickObject(ObjectType.Element, "Please select the toposurface");

            Element surfaceElem = doc.GetElement(surfaceRef);

            TopographySurface surface = surfaceElem as TopographySurface;

            return surface;
        }

        private XYZ EvaluateAndAddTopoPoint(UV parameters, Face face)
        {
            XYZ point = face.Evaluate(parameters);

            // If the Z component of the normal at the point is positive, do not include it
            if (!face.IsInside(parameters))
            {
                return null;
            }
            else if (face.ComputeNormal(parameters).Z >= 0)
            {
                return null;
            }
            else
            {
                return point;
            }
        }
    }

    class TopoEditFailurePreprocessor : IFailuresPreprocessor
    {
        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
        {
            return FailureProcessingResult.Continue;
        }
    }
}
