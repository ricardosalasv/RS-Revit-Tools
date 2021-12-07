using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_Utilities.lib
{
    static internal class Helpers
    {
        public static Document AddParameterToFamily(Document sourceDoc,
            Family family, 
            IList<ExternalDefinition> setOfParameters)
        {

            // Opening the family in a background family editor
            Document familyEditor = sourceDoc.EditFamily(family);
            var familyManager = familyEditor.FamilyManager;

            // Adding the parameter to the family
            Transaction fett = new Transaction(familyEditor, "Add Parameter");

            fett.Start();

            // Checks if parameter already exists in family
            FamilyParameterSet parametersInFamily = familyManager.Parameters;

            IList<FamilyParameter> parametersToRemove = new List<FamilyParameter>();
            foreach (ExternalDefinition sharedParameter in setOfParameters)
            {
                bool parameterAdded = false;

                foreach (FamilyParameter parameter in parametersInFamily)
                {
                    
                    // If the parameter exists and is not shared, remove it and add the shared parameter
                    if (parameter.Definition.Name == sharedParameter.Name && !parameter.IsShared)
                    {

                        // Adding parameter to list of parameters to be deleted
                        familyManager.RenameParameter(parameter, $"{parameter.Definition.Name}_deprecated");
                        parametersToRemove.Add(parameter);

                        familyManager.AddParameter(sharedParameter, BuiltInParameterGroup.PG_MATERIALS, false);
                        parameterAdded = true;
                        break;

                    }
                    else if (parameter.Definition.Name == sharedParameter.Name && parameter.IsShared)
                    {
                        parameterAdded = true;
                    }

                }

                if (parameterAdded)
                {
                    continue;
                }
                else
                {
                    familyManager.AddParameter(sharedParameter, BuiltInParameterGroup.PG_MATERIALS, false);
                }
            }

            // Removing substituted parameters in postponed process
            foreach (FamilyParameter parameter in parametersToRemove)
            {
                familyManager.RemoveParameter(parameter);
            }

            fett.Commit();

            return familyEditor;
        }
    }
}
