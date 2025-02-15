using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using AXE.RevitContext.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AXE.RevitContext.Manager
{
    internal class RoomManager
    {
        static SpatialElementBoundaryOptions options = new SpatialElementBoundaryOptions()
        {
            SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Finish

        };

        public static CurveLoop GetLowerBoundaries(Room room)
        {
            CurveLoop loop = null;
            foreach (var item in room.ClosedShell)
            {
                foreach (Face item1 in (item as Solid).Faces)
                {
                    if (item1 is PlanarFace planar)
                    {
                        if (planar.FaceNormal.Z < 0)
                        {
                            loop = planar.GetEdgesAsCurveLoops().FirstOrDefault();
                        }
                    }
                }
            }
            return loop;
        }


        public static List<ThresholdPoints> GetDoorLocation(Room room)
        {
            List<ThresholdPoints> thresholds = new List<ThresholdPoints>();

            //FamilyInstance door = null;
            Wall wall = null;
            BoundarySegment boundary = null;
            IList<BoundarySegment> boundarySegments = room.GetBoundarySegments(options).FirstOrDefault();
            var ditinctBoundaries = boundarySegments.GroupBy(s => s.ElementId).Select(s => s.FirstOrDefault());
            foreach (var item in ditinctBoundaries)
            {
                if (item.ElementId == ElementId.InvalidElementId) continue;
                //Get Wall

                //if(wall!= null && wall.Id == item.ElementId)
                //{
                //    ThresholdPoints thresholdPoints = new ThresholdPoints();
                //    thresholdPoints.WallStartPoint = (boundary.GetCurve() as Line).GetEndPoint(0);
                //    thresholdPoints.WallEndPoint = (boundary.GetCurve() as Line).GetEndPoint(1);
                //    thresholds.Add(thresholdPoints);
                //    break;
                //}
                wall = room.Document.GetElement(item.ElementId) as Wall;

                //Collect Doors in that wall
                List<FamilyInstance> allDoors = wall.GetDependentElements(new ElementCategoryFilter(BuiltInCategory.OST_Doors)).Select(s => room.Document.GetElement(s)).Cast<FamilyInstance>().ToList();

                //Find which door is associated with bathroom

                var doorsTo = allDoors.Where(S => S.Room.Number.Equals(room.Number)).ToList();


                var doorsFrom = allDoors.Where(S => S.FromRoom?.Number.Equals(room.Number) ?? false).ToList();

                List<FamilyInstance> doors = new List<FamilyInstance>();

                if (doorsTo.Any()) doors.AddRange(doorsTo);
                //if(doorsFrom.Any()) doors.AddRange(doorsFrom);

                boundary = item;

                if (doors.Count > 0)
                {
                    foreach (var door in doors)
                    {

                        ThresholdPoints thresholdPoints = new ThresholdPoints();
                        thresholdPoints.Segmented = true;
                        double doorWidth = door.Symbol.get_Parameter(BuiltInParameter.DOOR_WIDTH).AsDouble();


                        XYZ doorLocation = (door.Location as LocationPoint).Point;
                        XYZ dir = wall.Orientation.CrossProduct(XYZ.BasisZ);

                        thresholdPoints.DoorStartPoint = doorLocation + dir * (doorWidth / 2);
                        thresholdPoints.DoorEndPoint = doorLocation - dir * (doorWidth / 2);

                        thresholdPoints.WallPartStartPoint = CurvesHelper.ProjectPointOnLine(thresholdPoints.DoorStartPoint, boundary.GetCurve() as Line);
                        thresholdPoints.WallPartEndPoint = CurvesHelper.ProjectPointOnLine(thresholdPoints.DoorEndPoint, boundary.GetCurve() as Line);



                      var ListOfLines=  boundarySegments.Where(s => s.ElementId == wall.Id).Select((s=>s.GetCurve() as Line)).ToList();
                        List<XYZ> points= new List<XYZ>();
                        foreach (var line in ListOfLines)
                        {
                            points.Add(line.GetEndPoint(0));
                            points.Add(line.GetEndPoint(1));
                        }
                            double xRange = points.Max(p => p.X) - points.Min(p => p.X);
                        double yRange = points.Max(p => p.Y) - points.Min(p => p.Y);

                        // Sort based on the dominant axis
                        List<XYZ> sortedPoints = xRange >= yRange
                        ? points.OrderBy(p => p.X).ToList()  // Sort by X if more horizontal
                            : points.OrderBy(p => p.Y).ToList(); // Sort by Y if more vertical

                        XYZ p1 = sortedPoints.First();
                        XYZ p2 = sortedPoints.Last();

                       // var p1= (boundary.GetCurve() as Line).GetEndPoint(0);
                       //var p2= (boundary.GetCurve() as Line).GetEndPoint(1);
                        thresholdPoints.WallEndPoint = thresholdPoints.WallPartStartPoint.DistanceTo(p1) 
                            < thresholdPoints.WallPartEndPoint.DistanceTo(p1) ? p1: p2;
                        thresholdPoints.WallStartPoint = thresholdPoints.WallPartStartPoint.DistanceTo(p1)
                            < thresholdPoints.WallPartEndPoint.DistanceTo(p1) ? p2 : p1;


                        thresholds.Add(thresholdPoints);

                    }
                }
                else
                {
                    //ThresholdPoints thresholdPoints = new ThresholdPoints();
                    //thresholdPoints.WallStartPoint = (boundary.GetCurve() as Line).GetEndPoint(0);
                    //thresholdPoints.WallEndPoint = (boundary.GetCurve() as Line).GetEndPoint(1);
                    //thresholds.Add(thresholdPoints);
                }
               

            }

            return thresholds;


        }

        public static List<XYZ> GetRoomPoints(Room room)
        {
            List<XYZ> result = new List<XYZ>();
            IList<BoundarySegment> boundarySegments = room.GetBoundarySegments(options).FirstOrDefault();

            foreach (var item in boundarySegments)
            {
                result.Add(item.GetCurve().GetEndPoint(0));
                //result.Add( item.GetCurve().GetEndPoint(1));
            }

            return result;
        }
    }
}
