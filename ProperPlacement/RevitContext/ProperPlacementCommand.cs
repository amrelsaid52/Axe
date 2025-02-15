using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using SectionCreator.RevitContext.Manager;
using SectionCreator.RevitContext.Manager;
using SectionCreator.RevitContext.Utilities;
using System;
using System.Linq;

namespace SectionCreator
{
    [Transaction(TransactionMode.Manual)]
    public class ProperPlacementCommand : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIDocument uIDocument = commandData.Application.ActiveUIDocument;
                Document document = uIDocument.Document;

                //Select Wall

                Reference refe = uIDocument.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, new WallSelectionFilter(), "Pick a Wall");
                Wall selectedWall = document.GetElement(refe) as Wall;

                //check if wall has Bathroom besides

                //Enhancement ==> should make possible names like powder and etc..
                RoomFilter roomFilter = new RoomFilter();
                var collector = new FilteredElementCollector(document).WherePasses(roomFilter);
                var bathrooms = collector.WhereElementIsNotElementType().Cast<Room>()
                    .Where(r => r.Name.Contains(GlobalData.BathroomName)).ToList();

                Curve connectedCurve = RoomManager.GetConnectedCurve(bathrooms, selectedWall, out Room selectedBathroom);

                if (connectedCurve != null)
                {
                    //Get Door Location
                    XYZ doorLocation = RoomManager.GetDoorLocation(selectedBathroom);

                    //Select Point
                    XYZ insertionPoint = CurvesManager.GetInsertionPoint(connectedCurve, doorLocation);



                    //Place Family Instance

                    //Placement Direction
                   PointLocation pointLocation= RoomManager.GetPointLocationInRoom(doorLocation,insertionPoint);


                    FamilySymbol familySymbol = new FilteredElementCollector(document).OfClass(typeof(FamilySymbol)).WhereElementIsElementType()
                        .Cast<FamilySymbol>().FirstOrDefault(s=>s.Name == GlobalData.InsertedObjectName);
                  

                    using (Transaction transaction = new Transaction(document, "Place Element"))
                    {
                        transaction.Start();

                        try
                        {
                            if (!familySymbol.IsActive) familySymbol.Activate();

                            
                          FamilyInstance placedFamilyInstance=  document.Create.NewFamilyInstance(insertionPoint, familySymbol, selectedWall, StructuralType.NonStructural);

                          double dis=  UnitUtils.ConvertToInternalUnits(559, UnitTypeId.Millimeters);
                          FamilyInstanceManager.AdjustFamilyInstanceFlipping(placedFamilyInstance,pointLocation);
                          FamilyInstanceManager.AdjustFamilyInstanceLocation(placedFamilyInstance,pointLocation,dis);
                            transaction.Commit();

                        }
                        catch (Exception ex)
                        {
                            transaction.RollBack();
                            message = ex.Message;
                        }
                    }

                }
                else
                {
                    TaskDialog.Show("Placement Error", "Can not find Bathroom connected to this wall");
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
