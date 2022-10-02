using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS_Scripts.lib
{
    internal static class Helpers
    {
        public static XYZ GetCentroid(Element element)
        {
            XYZ centroid = null;

            try
            {
                BoundingBoxXYZ bb = element.get_BoundingBox(null);

                centroid = Line.CreateBound(bb.Min, bb.Max).Evaluate(0.5, true);
            }
            catch (Exception)
            {
                return null;
            }

            return centroid;
        }

        public static bool DocumentHasBeenSwitched(Document doc)
        {
            string filename = GetCurrentFilename(doc);

            if (AppState.CurrentProjectFilename != filename)
            {
                AppState.CurrentProjectFilename = filename;

                return true;
            }

            return false;
        }

        public static string GetCurrentFilename(Document doc)
        {
            string filename = doc.PathName;

            return filename;
        }
    }
}
