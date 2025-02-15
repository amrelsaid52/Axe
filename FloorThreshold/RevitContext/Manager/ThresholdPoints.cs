using Autodesk.Revit.DB;
using SectionCreator.RevitContext.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SectionCreator.RevitContext.Manager
{
    internal class ThresholdPoints
    {
        public ThresholdPoints(XYZ doorStartPoint, XYZ doorEndPoint, XYZ wallPartStartPoint, XYZ wallPartEndPoint)
        {
            DoorStartPoint = doorStartPoint;
            DoorEndPoint = doorEndPoint;
            WallPartStartPoint = wallPartStartPoint;
            WallPartEndPoint = wallPartEndPoint;
        }
        public ThresholdPoints() { }

        public XYZ DoorStartPoint { get; set; }
        public XYZ DoorEndPoint { get; set; }

        public XYZ WallPartStartPoint { get; set; }

        public XYZ WallPartEndPoint { get; set; }

        public XYZ WallStartPoint { get; set; }

        public XYZ WallEndPoint { get; set; }
       
        public Position DoorPosition { get; set; }

        public bool Segmented { get; set; }

        public List<XYZ> SortPoints()
        {
            List<XYZ> doorPoints = new List<XYZ>
            {
                WallPartStartPoint,
                DoorEndPoint,
                DoorStartPoint,
                WallPartEndPoint
            };

            if (DoorPosition.Equals(Position.Right) || DoorPosition.Equals(Position.Bottom))
            {
                return CurvesHelper.SortXYZClockwise(doorPoints, XYZ.BasisZ);

            }
            else if (DoorPosition.Equals(Position.Left) || DoorPosition.Equals(Position.Bottom))
            {
                return CurvesHelper.SortXYZClockwise(doorPoints,- XYZ.BasisZ);

            }
            else
            {
                return CurvesHelper.SortXYZClockwise(doorPoints, XYZ.BasisZ);

            }
        }
        public List<XYZ> GetPoints()
        {
            return new List<XYZ>
            {
                WallPartStartPoint,
                DoorStartPoint,
                DoorEndPoint,
                WallPartEndPoint
            };



        }
        public List<XYZ> GetPoint(bool flag)
        {
            if (!Segmented)
            {
                return new List<XYZ> { WallStartPoint, WallEndPoint };
            }
            else
            {
                return new List<XYZ>
                {
                    WallStartPoint,WallPartStartPoint,
                    WallPartStartPoint,DoorStartPoint,
                    DoorStartPoint,DoorEndPoint,
                    DoorEndPoint,WallPartEndPoint,
                    WallPartEndPoint,WallEndPoint,

                };
            }
        }
        public XYZ GetCenter()
        {
            var roomPoints = new List<XYZ>() {

                WallPartStartPoint,
                DoorStartPoint,
                DoorEndPoint,
                WallPartEndPoint

                };

            double centerX = roomPoints.Average(p => p.X);
            double centerY = roomPoints.Average(p => p.Y);

            return new XYZ(Math.Round(centerX, 3), Math.Round(centerY, 3), 0);
        }

        public List<Curve> CreateCurves()
        {
            if (!Segmented)
            {
                return new List<Curve> { Line.CreateBound(WallStartPoint, WallEndPoint) };
            }
            else
            {
                return new List<Curve>
                {
                    Line.CreateBound(WallStartPoint,WallPartStartPoint),
                    Line.CreateBound(WallPartStartPoint,DoorStartPoint),
                    Line.CreateBound(DoorStartPoint,DoorEndPoint),
                    Line.CreateBound(DoorEndPoint,WallPartEndPoint),
                    Line.CreateBound(WallPartEndPoint,WallEndPoint),

                };
            }
        }

    }
    internal enum Position
    {
        Right,
        Left,
        Top, Bottom
    }
   
}
