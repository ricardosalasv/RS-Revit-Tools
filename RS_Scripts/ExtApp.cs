using Autodesk.Revit.UI;
using RS_Scripts.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS_Scripts
{
    public class ExtApp : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            // Creating all the UI Elements related to this app
            CreateUIElements.Create(application);

            return Result.Succeeded;
        }
    }
}
