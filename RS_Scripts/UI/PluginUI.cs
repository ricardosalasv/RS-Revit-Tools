using Autodesk.Revit.UI;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace RS_Scripts.UI
{
    internal class PluginUI
    {
        public static void Build(UIControlledApplication application)
        {
            // Getting running assembly location
            string assemblyPath = Assembly.GetExecutingAssembly().Location;

            // Creating the Ribbon tab and Panel
            application.CreateRibbonTab("RS Scripts");
            RibbonPanel CurtainPanelsPanel = application.CreateRibbonPanel("RS Scripts", "Curtain Panels");
            RibbonPanel SitePanel = application.CreateRibbonPanel("RS Scripts", "Site");
            RibbonPanel ExporterPanel = application.CreateRibbonPanel("RS Scripts", "Exporters");
            RibbonPanel ElementsPanel = application.CreateRibbonPanel("RS Scripts", "Elements");

            // Pushbuttons
            // Topo To Face
            var topoToFacePushButtonData = new PushButtonData("topoToFacePushButton", "Topo To Face", assemblyPath, "RS_Scripts.Scripts.TopoToFace");
            var topoToFaceIconPath = GetIconUriFromAssemblyLocation(assemblyPath, "icon_TopoToFace.png");
            var topoToFaceIcon = new BitmapImage(topoToFaceIconPath);
            topoToFacePushButtonData.LargeImage = topoToFaceIcon;

            // Change Curtain Panel
            var changeCurtainPanelPushButtonData = new PushButtonData("changeCurtainPanelPushButton", "Change Curtain Panel", assemblyPath, "RS_Scripts.Scripts.ChangeCurtainPanel");
            var changeCurtainPanelIconPath = GetIconUriFromAssemblyLocation(assemblyPath, "icon_ChangeCurtainPanel.png");
            var changeCurtainPanelIcon = new BitmapImage(changeCurtainPanelIconPath);
            changeCurtainPanelPushButtonData.LargeImage = changeCurtainPanelIcon;

            var changeCurtainPanelComboBoxData = new ComboBoxData("changeCurtainPanelComboBox")
            {
                ToolTip = "Select the curtain panel type you want to use to replace the existing one."
            };

            // Resize element
            var resizeElementPushButtonData = new PushButtonData("resizeElementsPushButton", "Resize Element", assemblyPath, "RS_Scripts.Scripts.ResizeElement");
            var resizeElementIconPath = GetIconUriFromAssemblyLocation(assemblyPath, "icon_ResizeElement.png");
            var resizeElementIcon = new BitmapImage(resizeElementIconPath);
            resizeElementPushButtonData.LargeImage = resizeElementIcon;

            // Export Walkthrough
            var exportWalkthroughsPushButtonData = new PushButtonData("exportWalkthroughsPushButton", "Export Walkthroughs", assemblyPath, "RS_Scripts.Scripts.WalkthroughsExporter");
            var exportWalkthroughsIconPath = GetIconUriFromAssemblyLocation(assemblyPath, "icon_ChangeCurtainPanel.png");
            var exportWalkthroughsIcon = new BitmapImage(exportWalkthroughsIconPath);
            exportWalkthroughsPushButtonData.LargeImage = changeCurtainPanelIcon;

            ExporterPanel.AddItem(exportWalkthroughsPushButtonData);

            CurtainPanelsPanel.AddItem(changeCurtainPanelPushButtonData);
            InitializedUIElements.ChangeCurtainPanelComboBox = CurtainPanelsPanel.AddItem(changeCurtainPanelComboBoxData) as ComboBox;

            SitePanel.AddItem(topoToFacePushButtonData);
        }

        public static Uri GetIconUriFromAssemblyLocation(string assemblyPath, string iconName)
        {
            Uri iconPath = new Uri($"{assemblyPath.Replace(Path.GetFileName(assemblyPath), "")}{iconName}", UriKind.Absolute);

            return iconPath;
        }
    }
}
