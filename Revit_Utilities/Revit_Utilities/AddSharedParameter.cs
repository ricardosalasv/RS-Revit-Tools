using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using MoreLinq;
using Revit_Utilities.lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_Utilities
{
    [Transaction(TransactionMode.Manual)]
    public class AddSharedParameter : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Getting the Application
            UIApplication uiapp = commandData.Application;
            Application app = uiapp.Application;

            // Getting the current document
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Getting all the instances in the model
            var allFamilyInstances = new FilteredElementCollector(doc).OfClass(typeof(FamilyInstance)).ToElements();

            // Excluding mullions from the selection
            var filteredFamilyInstances = allFamilyInstances.Where(x => x.GetType() != typeof(Mullion));

            // Getting the symbol of each instance
            var familySymbols = from FamilyInstance x in filteredFamilyInstances
                                select x.Symbol;

            // Getting the families from the distinct symbols
            var families = from x in familySymbols
                           select x.Family;

            // Getting the distinct families
            var distinctFamilies = families.DistinctBy(x => x.Id.IntegerValue);
            distinctFamilies = distinctFamilies.Where(x => x.IsEditable == true);
            distinctFamilies = distinctFamilies.Where(x => x.GetType() == typeof(Family));

            var NamesOfFamilies = from x in distinctFamilies
                                  select x.Name;

            // Getting the current Shared Parameters file
            DefinitionFile spFile = app.OpenSharedParameterFile();

            // Getting the groups of parameters withing the SP file
            DefinitionGroups parameterGroups = spFile.Groups;
            DefinitionGroup generalGroup = parameterGroups.get_Item("General");
            DefinitionGroup caseworkGroup = parameterGroups.get_Item("Casework");
            DefinitionGroup LFGroup = parameterGroups.get_Item("Lighting Fixtures");
            DefinitionGroup SEGroup = parameterGroups.get_Item("Specialty Equipment");

            // Getting the parameters within the selected group
            Definitions generalParameters = generalGroup.Definitions;
            Definitions caseworkParameters = caseworkGroup.Definitions;
            Definitions LFParameters = LFGroup.Definitions;
            Definitions SEParameters = SEGroup.Definitions;

            // Getting the parameter to be added to doors and windows families
            IList<ExternalDefinition> dwMaterialParameters = new List<ExternalDefinition>();
            dwMaterialParameters.Add(generalParameters.get_Item("Frame Material") as ExternalDefinition);
            dwMaterialParameters.Add(generalParameters.get_Item("Handle Material") as ExternalDefinition);
            dwMaterialParameters.Add(generalParameters.get_Item("Panel Material") as ExternalDefinition);
            dwMaterialParameters.Add(generalParameters.get_Item("Glazing Material") as ExternalDefinition);

            // Getting the parameter to be added to casework families
            IList<ExternalDefinition> caseworkMaterialParameters = new List<ExternalDefinition>();
            caseworkMaterialParameters.Add(generalParameters.get_Item("Frame Material") as ExternalDefinition);
            caseworkMaterialParameters.Add(generalParameters.get_Item("Handle Material") as ExternalDefinition);
            caseworkMaterialParameters.Add(generalParameters.get_Item("Panel Material") as ExternalDefinition);
            caseworkMaterialParameters.Add(generalParameters.get_Item("Body Material") as ExternalDefinition);
            caseworkMaterialParameters.Add(caseworkParameters.get_Item("Countertop Material") as ExternalDefinition);
            caseworkMaterialParameters.Add(caseworkParameters.get_Item("Backsplash Material") as ExternalDefinition);

            // Getting the parameter to be added to Lighting Fixture families
            IList<ExternalDefinition> LFMaterialParameters = new List<ExternalDefinition>();
            LFMaterialParameters.Add(generalParameters.get_Item("Secondary Material") as ExternalDefinition);
            LFMaterialParameters.Add(LFParameters.get_Item("Light Source Material") as ExternalDefinition);
            LFMaterialParameters.Add(generalParameters.get_Item("Body Material") as ExternalDefinition);

            // Getting the parameter to be added to Electrical Devices families
            IList<ExternalDefinition> EDMaterialParameters = new List<ExternalDefinition>();
            EDMaterialParameters.Add(generalParameters.get_Item("Body Material") as ExternalDefinition);
            EDMaterialParameters.Add(generalParameters.get_Item("Display Material") as ExternalDefinition);

            // Getting the parameter to be added to Plumbing Fixtures families
            IList<ExternalDefinition> PFMaterialParameters = new List<ExternalDefinition>();
            PFMaterialParameters.Add(generalParameters.get_Item("Body Material") as ExternalDefinition);

            // Getting the parameter to be added to Specialty Equipment families
            IList<ExternalDefinition> SEMaterialParameters = new List<ExternalDefinition>();
            SEMaterialParameters.Add(generalParameters.get_Item("Body Material") as ExternalDefinition);
            SEMaterialParameters.Add(generalParameters.get_Item("Button Material") as ExternalDefinition);
            SEMaterialParameters.Add(generalParameters.get_Item("Glazing Material") as ExternalDefinition);

            TransactionGroup tg = new TransactionGroup(doc, "Add Shared Parameter to Families");
            tg.Start();

            // List to store opened family editors for later imports into the main document
            IList<Document> modifiedFamilies = new List<Document>();

            foreach (var family in distinctFamilies)
            {

                if (family.FamilyCategory.Name == "Doors" || family.FamilyCategory.Name == "Windows")
                {

                    Document familyEditor = Helpers.AddParameterToFamily(doc, family, dwMaterialParameters);
                    modifiedFamilies.Add(familyEditor);

                }

                if (family.FamilyCategory.Name == "Casework")
                {

                    Document familyEditor = Helpers.AddParameterToFamily(doc, family, caseworkMaterialParameters);
                    modifiedFamilies.Add(familyEditor);

                }

                if (family.FamilyCategory.Name == "Lighting Fixtures")
                {

                    Document familyEditor = Helpers.AddParameterToFamily(doc, family, LFMaterialParameters);
                    modifiedFamilies.Add(familyEditor);

                }

                if (family.FamilyCategory.Name == "Plumbing Fixtures")
                {

                    Document familyEditor = Helpers.AddParameterToFamily(doc, family, PFMaterialParameters);
                    modifiedFamilies.Add(familyEditor);

                }

                if (family.FamilyCategory.Name == "Specialty Equipment")
                {

                    Document familyEditor = Helpers.AddParameterToFamily(doc, family, SEMaterialParameters);
                    modifiedFamilies.Add(familyEditor);

                }

            }

            foreach (Document family in modifiedFamilies)
            {

                try
                {
                    // Loading family back to the main document
                    family.LoadFamily(doc, new loadOptions());

                    // Closing family editor
                    family.Close(false);
                    family.Dispose();
                }
                catch (Exception ex)
                {
                    continue;
                }
                
            }

            tg.Assimilate();

            return Result.Succeeded;
        }
        
    }

    // Load Family Options
    public class loadOptions : IFamilyLoadOptions
    {
        public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
        {   
            overwriteParameterValues = false;
            return true;
        }

        public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, out FamilySource source, out bool overwriteParameterValues)
        {
            source = FamilySource.Project;
            overwriteParameterValues = false;

            return true;
        }
    }
}
