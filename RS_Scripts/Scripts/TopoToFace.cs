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

                IList<Face> faces = selectedFaces.Select(x => doc.GetElement(x).GetGeometryObjectFromReference(x) as Face).ToList();

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
