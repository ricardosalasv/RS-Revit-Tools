using Autodesk.Revit.UI.Events;
using RS_Scripts.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;

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
            try
            {
                // Setting Revit Environment
                AppState.UIControlledApplication = application;
                AppState.ControlledApplication = application.ControlledApplication;

                // Registering the current UIApplication in AppState
                try
                {
                    application.Idling += SetUIApplication;
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Error", ex.Message);
                }

                // Build UI
                PluginUI.Build(application);

                // Registering events bindings
                EventExecutions.Register();
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.Message + ex.StackTrace);
            }
            

            return Result.Succeeded;
        }

        // Method to register the current UIApplication in AppState
        public static void SetUIApplication(object sender, IdlingEventArgs args)
        {
            AppState.UIControlledApplication.Idling -= SetUIApplication;

            AppState.CurrentUIApplication = sender as UIApplication;
        }
    }
}
