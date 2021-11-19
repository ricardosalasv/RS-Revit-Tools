using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using MoreLinq;
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
            Document doc = uiapp.ActiveUIDocument.Document;

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
            DefinitionGroup identityDataGroup = parameterGroups.get_Item("Identity Data");

            // Getting the parameters within the selected group
            Definitions parameters = identityDataGroup.Definitions;

            // Getting the parameter to be added to each family
            ExternalDefinition elementNameParam = parameters.get_Item("Element Name") as ExternalDefinition;

            TransactionGroup tg = new TransactionGroup(doc, "Add Shared Parameter to Families");
            tg.Start();

            // List to store opened family editors for later imports into the main document
            IList<Document> modifiedFamilies = new List<Document>();

            foreach (var family in distinctFamilies)
            {
                // Opening the family in a background family editor
                Document familyEditor = doc.EditFamily(family);
                var familyManager = familyEditor.FamilyManager;

                // Checks if parameter already exists in family
                FamilyParameterSet parametersInFamily = familyManager.Parameters;

                bool parameterExists = false;
                foreach (FamilyParameter parameter in parametersInFamily)
                {
                    if (parameter.Definition.Name == elementNameParam.Name)
                    {
                        parameterExists = true;
                        break;
                    }
                }
                    
                // If the parameter already exists, skip this family
                if (parameterExists)
                {
                    continue;
                }

                // Adding the parameter to the family
                Transaction fett = new Transaction(familyEditor, "Add Parameter");
                fett.Start();
                familyManager.AddParameter(elementNameParam, BuiltInParameterGroup.PG_IDENTITY_DATA, false);
                fett.Commit();

                // Storing current editor for later import
                modifiedFamilies.Add(familyEditor);

            }

            foreach (Document family in modifiedFamilies)
            {

                try
                {
                    // Loading family back to the main document
                    family.LoadFamily(doc, new loadOptions());

                    // Closing family editor
                    family.Close(false);
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
