using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using AXE.RevitContext.Manager;
using AXE.RevitContext.Utilities;
using System;
using System.Linq;

namespace AXE
{
    [Transaction(TransactionMode.Manual)]
    public class SectionCreatorCommand : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIDocument uIDocument = commandData.Application.ActiveUIDocument;
                Document document = uIDocument.Document;

               


                    using (Transaction transaction = new Transaction(document, "Create Section"))
                    {
                        transaction.Start();

                        try
                        {
                            SectionManager.CreateSectionView(document);
                            transaction.Commit();

                        }
                        catch (Exception ex)
                        {
                            transaction.RollBack();
                            message = ex.Message;
                        }
                    }

                
               










                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}
