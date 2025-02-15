using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FloorThreshold.RevitContext.Manager
{
    internal class CurvesManager
    {
        public static XYZ GetInsertionPoint(Curve curve, XYZ doorPoint)
        {
          XYZ  startPoint=  curve.GetEndPoint(0);
          XYZ endPoint=  curve.GetEndPoint(1);

            return startPoint.DistanceTo(doorPoint) >= endPoint.DistanceTo(doorPoint) ? startPoint : endPoint;
        }
    }
}
