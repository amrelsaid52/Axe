using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using FloorThreshold.RevitContext.Helpers;
using FloorThreshold.RevitContext.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FloorThreshold
{
    [Transaction(TransactionMode.Manual)]
    public class FloorThresholdCommand : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIDocument uIDocument = commandData.Application.ActiveUIDocument;
                Document document = uIDocument.Document;


                //Get All Rooms
                RoomFilter filter = new RoomFilter();

                var rooms = new FilteredElementCollector(document).WherePasses(filter).WhereElementIsNotElementType()
                              .Cast<Room>().ToList();


                using (Transaction transaction = new Transaction(document, "Create Floors"))
                {
                    transaction.Start();
                    try
                    {
                        foreach (var room in rooms)
                        {
                            List<ThresholdPoints> thresholdPoints = RoomManager.GetDoorLocation(room);

                            if (thresholdPoints.Count == 0) continue;

                            //Get Room Points
                            List<XYZ> roomPoints = RoomManager.GetRoomPoints(room);


                            foreach (var roomPoint in thresholdPoints)
                            {
                                //roomPointsSorted.Add(roomPoint.GetCenter());
                                roomPoints.AddRange(roomPoint.GetPoints());
                            }

                            double centerX = roomPoints.Average(p => p.X);
                            double centerY = roomPoints.Average(p => p.Y);

                            XYZ centroid = new XYZ(centerX, centerY, 0);

                            // Sort points by angle relative to centroid (clockwise order)
                            var sortedPoints = roomPoints
                                .OrderBy(p => Math.Atan2(p.Y - centroid.Y, p.X - centroid.X))
                                .ToList();

                            List<XYZ> roomPointsSorted = CurvesHelper.SortXYZClockwise(roomPoints, XYZ.BasisZ);
                            //List<XYZ> roomPointsSorted = CurvesHelper.ComputeRectilinearHull(roomPoints);
                            //List<XYZ> roomPointsSorted = CurvesHelper.FindNearestNeighborPath(roomPoints);
                            // List<XYZ> roomPointsSorted = sortedPoints; 

                           
                            //for (global::System.Int32 i = 0; i < roomPointsSorted.Count; i++)
                            //{
                            //    if (roomPointsSorted[i].X - roomPointsSorted[(i + 1) % roomPointsSorted.Count].X >= 0.01 |
                            //        roomPointsSorted[i].Y - roomPointsSorted[(i + 1) % roomPointsSorted.Count].Y >= 0.01)
                            //    {
                            //        Swap<XYZ>(roomPointsSorted, roomPointsSorted[i], roomPointsSorted[(i + 1) % roomPointsSorted.Count]);
                            //    }
                            //}

                            //List<XYZ> roomPointsSorted = CurvesHelper.SortByNearestNeighbor(roomPoints);
                            //List<XYZ> roomPointsSorted = CurvesHelper.SortByNearestNeighbor(roomPoints);
                            //List<XYZ> roomPointsSorted = CurvesHelper.ConvexHull(roomPoints);
                            // List<XYZ> roomPointsSorted = roomPoints;
                            //foreach (var item in thresholdPoints)
                            //{


                            //    int index = sortedPoints.Select((p, index1) => new { Point = p, Index = index1 })
                            //        .FirstOrDefault(s => s.Point.IsAlmostEqualTo(item.GetCenter())).Index; // Find index of target point

                            //    if (index != -1)
                            //    {
                            //        sortedPoints.RemoveAt(index); // Remove the target point
                            //        sortedPoints.InsertRange(index, item.SortPoints()); // Insert the new range at the same index
                            //    }
                            //}

                            //List<Curve> curves = new List<Curve>();
                            //foreach (var item in thresholdPoints)
                            //{
                            //  curves.AddRange(  item.CreateCurves());

                            //}
                            //List<XYZ> sortedPoints = new List<XYZ>();
                            //foreach (var item in thresholdPoints)
                            //{
                            //    sortedPoints.AddRange(item.GetPoint(true));

                            //}
                            //Draw Floor

                            CurveLoop cl = CurvesHelper.CreateCurveLoop(sortedPoints);
                            //CurvesHelper.SortCurvesContiguous(curves);
                            //CurveLoop cl = CurveLoop.Create(curves);

                            var floorType = new FilteredElementCollector(document).OfCategory(BuiltInCategory.OST_Floors).WhereElementIsElementType()
                           .FirstOrDefault();
                            var level = new FilteredElementCollector(document).OfCategory(BuiltInCategory.OST_Levels).WhereElementIsNotElementType()
                                .FirstOrDefault();
                            try
                            {

                                Floor.Create(document, new List<CurveLoop>() { cl }, floorType.Id, level.Id);
                            }
                            catch (Exception e)
                            {

                            }
                        }

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
        public static void Swap<T>(List<T> list, T value1, T value2)
        {
            int index1 = list.IndexOf(value1);
            int index2 = list.IndexOf(value2);

            if (index1 != -1 && index2 != -1) // Ensure both elements exist
            {
                (list[index1], list[index2]) = (list[index2], list[index1]); // Swap using tuple
            }
        }
    }
}
