using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using AXE.RevitContext.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace AXE.RevitContext.Manager
{
    internal class RoomManager
    {
        static SpatialElementBoundaryOptions options = new SpatialElementBoundaryOptions()
               {
                    SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.CoreCenter

               };

        /// <summary>
        /// This Method gets the only segment that is connected to the bathroom
        /// </summary>
        /// <param name="bathrooms">All Bathrooms int the model</param>
        /// <param name="selectedWall">The picked wall to place the element on</param>
        /// <param name="selectedBathroom">the selected bathroom that is connected to the bathroom as an output</param>
        /// <returns></returns>
        public static Curve GetConnectedCurve(List<Room> bathrooms, Wall selectedWall, out Room selectedBathroom)
        {
            Curve curve = null;
            selectedBathroom = null;

            foreach (var bathroom in bathrooms)
            {
                //Assume it is only one list but should take the outer boundary 
                IList<BoundarySegment> boundarySegments = bathroom.GetBoundarySegments(options).FirstOrDefault();

                IsWallIsBoundaryOfRoom(boundarySegments, selectedWall, out curve);
                if (curve != null) { selectedBathroom = bathroom; break; }
            }

            return curve;
        }

        public static XYZ GetDoorLocation(Room bathroom)
        {
            XYZ doorLocation = null;

            IList<BoundarySegment> boundarySegments = bathroom.GetBoundarySegments(options).FirstOrDefault();

            foreach (var item in boundarySegments)
            {
                if (item.ElementId == ElementId.InvalidElementId) continue;
                //Get Wall

                Wall wall = bathroom.Document.GetElement(item.ElementId) as Wall;

                //Collect Doors in that wall
                List<FamilyInstance> doors = wall.GetDependentElements(new ElementCategoryFilter(BuiltInCategory.OST_Doors)).Select(s=>bathroom.Document.GetElement(s)).Cast<FamilyInstance>().ToList();

                //Find which door is associated with bathroom
                doorLocation = doors.Where(S=>S.Room.Name .StartsWith (GlobalData.BathroomName)).Select(s=>(s.Location as LocationPoint).Point).FirstOrDefault();

                if (doorLocation != null) break;

            }

            return doorLocation;
        }

        public static PointLocation GetPointLocationInRoom(XYZ doorPoint, XYZ insertionPoint)
        {
            PointLocation point = new PointLocation
            {
                HzDirection = insertionPoint.X >= doorPoint.X ? HorizontalDirection.Right : HorizontalDirection.Left,
                VrDirection = insertionPoint.Y >= doorPoint.Y ? VerticalDirection.Top : VerticalDirection.Bottom
            };

            return point;
        }

        private static void IsWallIsBoundaryOfRoom(IList<BoundarySegment> boundarySegments, Wall selectedWall, out Curve curve)
        {
            curve = null;
            foreach (var item in boundarySegments)
            {
                if (item.ElementId == selectedWall.Id)
                {
                    curve = item.GetCurve(); break;
                }
            }
        }


    }
}
