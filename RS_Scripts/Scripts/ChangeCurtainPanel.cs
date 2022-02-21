using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
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
            TaskDialog.Show("Show", "I work! 1");
            return Result.Succeeded;
        }
    }
}
