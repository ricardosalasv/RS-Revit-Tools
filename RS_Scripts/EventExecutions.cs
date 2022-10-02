using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using RS_Scripts.lib;
using RS_Scripts.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS_Scripts
{
    public class EventExecutions
    {
        public static void Register()
        {
            AppState.ControlledApplication.DocumentSaving += new EventHandler<DocumentSavingEventArgs>(SavingExecutions);
            AppState.ControlledApplication.DocumentSaved += new EventHandler<DocumentSavedEventArgs>(SavedExecutions);
            AppState.ControlledApplication.DocumentSynchronizingWithCentral += new EventHandler<DocumentSynchronizingWithCentralEventArgs>(SynchronizingExecutions);
            AppState.ControlledApplication.DocumentSynchronizedWithCentral += new EventHandler<DocumentSynchronizedWithCentralEventArgs>(SynchronizedExecutions);
            AppState.ControlledApplication.DocumentOpened += new EventHandler<DocumentOpenedEventArgs>(OpenedExecutions);
            AppState.ControlledApplication.DocumentClosed += new EventHandler<DocumentClosedEventArgs>(ClosedExecutions);
            AppState.UIControlledApplication.Idling += new EventHandler<IdlingEventArgs>(IdlingExecutions);
        }

        // We are putting the actual methods as part of a wrapper method subscribed to each event to ensure a sequential execution
        private static void SavingExecutions(object sender, RevitAPIPreDocEventArgs args)
        {
        }

        private static void SavedExecutions(object sender, RevitAPIPostDocEventArgs args)
        {
        }

        private static void SynchronizingExecutions(object sender, RevitAPIPreDocEventArgs args)
        {
        }

        private static void SynchronizedExecutions(object sender, RevitAPIPostDocEventArgs args)
        {
        }

        private static void OpenedExecutions(object sender, DocumentOpenedEventArgs args)
        {
        }

        private static void ClosedExecutions(object sender, DocumentClosedEventArgs args)
        {
        }

        private static void IdlingExecutions(object sender, IdlingEventArgs args)
        {

        }

        // Methods in this class
        private static void ReplaceChangeCurtainPanelComboBox(IdlingEventArgs args)
        {
            Document doc = AppState.CurrentUIApplication.ActiveUIDocument.Document;
            if (Helpers.DocumentHasBeenSwitched(doc))
            {
                InitializedUIElements.ChangeCurtainPanelComboBox.GetItems();
            }
        }
    }
}
