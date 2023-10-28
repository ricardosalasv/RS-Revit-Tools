using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS_Scripts.Scripts
{
    [Transaction(TransactionMode.Manual)]
    internal class WalkthroughsExporter : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            IEnumerable<View3D> view3Ds = new FilteredElementCollector(doc).OfClass(typeof(View3D)).Cast<View3D>().Where(x => x.ViewType == ViewType.Walkthrough);

            foreach (View3D view3D in view3Ds)
            {
                if (view3D.Name != "Int - Upper Floor") continue;

                var handles = view3D.GetDirectContext3DHandleOverrides().GetDirectContext3DHandleSettings(doc, view3D.Id);

                var frames = view3D.LookupParameter("Camera Position").AsInteger();

                string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + view3D.Name + ".png";
                //view3D.ExportImage(path, new ImageExportOptions() { FitDirection = FitDirectionType.Horizontal, HLRandWFViewsFileType = ImageFileType.PNG, ZoomType = ZoomFitType.FitToPage });
            }

            return Result.Succeeded;
        }
    }
}
