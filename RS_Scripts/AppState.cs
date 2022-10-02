using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using RS_Scripts.lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS_Scripts
{
    public static class AppState
    {
        public static UIControlledApplication UIControlledApplication { get; set; }
        public static ControlledApplication ControlledApplication { get; set; }
        public static UIApplication CurrentUIApplication { get; set; }
        public static string CurrentProjectFilename { get; set; }
        public static View3D QCView3D { get; set; }

        // This will store all initialized WPFWindows for Revit API External Events to work with
        //public static IList<Window> InitializedWPFWindows = new List<Window>();

        // This will store the External command that will be executed in the next raise of an External Event
        public static Func<bool> CommandToExecute { get; set; }

        #region Environment Methods
        public static void Initialize()
        {

        }

        //public static Window GetWindow(string title)
        //{
        //    Window window = null;
        //    try
        //    {
        //        window = InitializedWPFWindows.Select(x => x).Where(x => x.Title == title).First();
        //    }
        //    catch (Exception)
        //    {
        //        window = null;
        //    }

        //    return window;
        //}

        //public static void RelinquishAll(RevitAPIPostDocEventArgs args)
        //{
        //    Document doc = args.Document;

        //    if (Helpers.CheckIfDocumentIsRA(doc))
        //    {
        //        if (doc.IsWorkshared)
        //        {
        //            MainRelinquishMethod(doc);
        //        }
        //    }
        //}

        private static void MainRelinquishMethod(Document doc)
        {
            try
            {
                CurrentUIApplication.PostCommand(RevitCommandId.LookupPostableCommandId(PostableCommand.RelinquishAllMine));
            }
            catch (Exception ex)
            {
                var exc = ex.Message;
            }

            try
            {
                IList<Element> allElements = new FilteredElementCollector(doc).WhereElementIsNotElementType().ToElements();

                var borrowedElements = allElements.Select(x => x).Where(x => WorksharingUtils.GetCheckoutStatus(doc, x.Id) == CheckoutStatus.OwnedByCurrentUser);
            }
            catch (Exception ex)
            {
                var exc = ex.Message;
            }

            var projInfo = doc.ProjectInformation;

            var test = WorksharingUtils.GetCheckoutStatus(doc, projInfo.Id);
        }
        #endregion
    }
}
