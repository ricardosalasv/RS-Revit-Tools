using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS_Scripts.lib
{
    internal static class Temp3DView
    {
        public static View3D Active { get; set; }

        public static void Create(Document doc)
        {
            ViewFamilyType viewFamilyType = new FilteredElementCollector(doc)
                    .OfClass(typeof(ViewFamilyType))
                    .Where(x => ((ViewFamilyType)x).ViewFamily == ViewFamily.ThreeDimensional)
                    .First() as ViewFamilyType;

            Active = View3D.CreateIsometric(doc, viewFamilyType.Id);
        }

        public static void Terminate(Document doc)
        {
            doc.Delete(Active.Id);

            Active = null;
        }
    }
}
