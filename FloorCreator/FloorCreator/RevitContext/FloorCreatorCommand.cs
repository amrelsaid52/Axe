using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using FloorThreshold.RevitContext.Helpers;
using FloorThreshold.RevitContext.Manager;
using FloorThreshold.RevitContext.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FloorThreshold
{
    [Transaction(TransactionMode.Manual)]
    public class FloorCreatorCommand : IExternalCommand
    {




        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIDocument uIDocument = commandData.Application.ActiveUIDocument;
                Document document = uIDocument.Document;

                //Get Points as UV
                List<UV> floorPointsUV = PointsHelper.GetPoints();
                //Convert to XYZ
                List<XYZ> floorPointsXYZ = floorPointsUV.Select(x => x.ToXYZ()).ToList();
                //Create Lines
                List<Curve> floorlines = CurvesManager.CreateLines(floorPointsXYZ);
                //Create CurveLoop
                CurveLoop curveLoop = CurvesManager.CreateCurveLoop(floorlines);
                //Create Floor
                if (curveLoop != null)
                {


                    var floorType = new FilteredElementCollector(document).OfCategory(BuiltInCategory.OST_Floors).WhereElementIsElementType()
                        .FirstOrDefault();
                    var level = new FilteredElementCollector(document).OfCategory(BuiltInCategory.OST_Levels).WhereElementIsNotElementType()
                        .FirstOrDefault();

                    using (Transaction transaction = new Transaction(document, "Create Floor"))
                    {
                        transaction.Start();

                        try
                        {
                            Floor.Create(document, new List<CurveLoop>() { curveLoop }, floorType.Id, level.Id);
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
                    TaskDialog.Show("Floor Creation Error", "Can't Create floor from these points");
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
