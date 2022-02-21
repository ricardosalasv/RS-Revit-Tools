using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace RS_Scripts.UI
{
    internal class CreateUIElements
    {
        public static void Create(UIControlledApplication application)
        {
            // Getting running assembly location
            string assemblyPath = Assembly.GetExecutingAssembly().Location;

            // Creating the Ribbon tab and Panel
            application.CreateRibbonTab("RS Scripts");
            RibbonPanel UtilitiesPanel = application.CreateRibbonPanel("RS Scripts", "Utilities");
            RibbonPanel SitePanel = application.CreateRibbonPanel("RS Scripts", "Site");

            // Pushbuttons
            // Topo To Face
            var topoToFacePushButton = new PushButtonData("topoToFacePushButton", "Topo To Face", assemblyPath, "RS_Scripts.Scripts.TopoToFace");
            var topoToFaceIconPath = GetIconUriFromAssemblyLocation(assemblyPath, "icon_TopoToFace.png");
            var topoToFaceIcon = new BitmapImage(topoToFaceIconPath);
            topoToFacePushButton.LargeImage = topoToFaceIcon;

            // Change Curtain Panel
            var changeCurtainPanelPushButton = new PushButtonData("changeCurtainPanelPushButton", "Change Curtain Panel", assemblyPath, "RS_Scripts.Scripts.ChangeCurtainPanel");
            var changeCurtainPanelIconPath = GetIconUriFromAssemblyLocation(assemblyPath, "icon_ChangeCurtainPanel.png");
            var changeCurtainPanelIcon = new BitmapImage(changeCurtainPanelIconPath);
            changeCurtainPanelPushButton.LargeImage = changeCurtainPanelIcon;

            UtilitiesPanel.AddItem(changeCurtainPanelPushButton);
            SitePanel.AddItem(topoToFacePushButton);
        }

        public static Uri GetIconUriFromAssemblyLocation(string assemblyPath, string iconName)
        {
            Uri iconPath = new Uri($"{assemblyPath.Replace(Path.GetFileName(assemblyPath), "")}{iconName}", UriKind.Absolute);

            return iconPath;
        }
    }
}
