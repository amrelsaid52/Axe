using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProperPlacement.RevitContext.Manager
{
    internal class CurvesManager
    {
        public static List<Curve> CreateLines(List<XYZ> pointsList)
        {
            List<Curve> lines = new List<Curve>();
            //Check for nullable and check if the count is even (if not even i won't be able to create last line)
            if(pointsList == null ||pointsList.Count % 2 !=0) return lines;

            
            for (int i = 0; i < pointsList.Count; i+=2)
            {
                lines.Add(Line.CreateBound(pointsList[i],pointsList[i+1]));
            }
            return lines;
        }


        public static CurveLoop CreateCurveLoop(List<Curve> curveList) 
        {
            CurveLoop curves= null;
            try
            {
                curves = CurveLoop.Create(curveList);
            }
            catch (Exception ex) 
            {
                SortCurvesContiguous(curveList);

                try
                {

                    curves = CurveLoop.Create(curveList);
                }
                catch 
                {
                
                }
            }
            return curves;
        }
        const double _inch = 1.0 / 12.0;
        const double _sixteenth = _inch / 16.0;
        static Curve CreateReversedCurve(Curve orig)
        {


            if (orig is Autodesk.Revit.DB.Line)
            {
                return orig.CreateReversed();
            }
            else if (orig is Arc)
            {
                return orig.CreateReversed();
            }
            else
            {
                throw new Exception(
                  "CreateReversedCurve - Unreachable");
            }
        }
        private static void SortCurvesContiguous(IList<Curve> curves)
        {
            double _precision1 = 1.0 / 12.0 / 16.0; // around 0.00520833
            double _precision2 = 0.001; // limit for CurveLoop.Create(...)

            int n = curves.Count;

            // Walk through each curve (after the first)
            // to match up the curves in order

            for (int i = 0; i < n; ++i)
            {
                //Curve curve = curves;

                XYZ beginPoint = curves[i].GetEndPoint(0);
                XYZ endPoint = curves[i].GetEndPoint(1);

                XYZ p, q;

                // Find curve with start point = end point

                bool found = (i + 1 >= n);

                for (int j = i + 1; j < n; ++j)
                {
                    p = curves[j].GetEndPoint(0);
                    q = curves[j].GetEndPoint(1);

                    // If there is a match end->start,
                    // this is the next curve
                    // double x= p.DistanceTo(endPoint);
                    if (p.DistanceTo(endPoint) < _precision1)
                    {
                        if (p.DistanceTo(endPoint) > _precision2)
                        {
                            XYZ intermediate = new XYZ((endPoint.X + p.X) / 2.0, (endPoint.Y + p.Y) / 2.0, (endPoint.Z + p.Z) / 2.0);

                            curves[i] = Line.CreateBound(beginPoint, intermediate);

                            curves[j] = Line.CreateBound(intermediate, q);
                        }

                        if (i + 1 != j)
                        {
                            Curve tmp = curves[i + 1];
                            curves[i + 1] = curves[j];
                            curves[j] = tmp;
                        }
                        found = true;
                        break;
                    }

                    // If there is a match end->end,
                    // reverse the next curve

                    if (q.DistanceTo(endPoint) < _precision1)
                    {
                        if (q.DistanceTo(endPoint) > _precision2)
                        {
                            XYZ intermediate = new XYZ((endPoint.X + q.X) / 2.0, (endPoint.Y + q.Y) / 2.0, (endPoint.Z + q.Z) / 2.0);

                            curves[i] = Line.CreateBound(beginPoint, intermediate);

                            curves[j] = Line.CreateBound(p, intermediate);
                        }

                        if (i + 1 == j)
                        {
                            curves[i + 1] = CreateReversedCurve(curves[j]);
                        }
                        else
                        {
                            Curve tmp = curves[i + 1];
                            curves[i + 1] = CreateReversedCurve(curves[j]);
                            curves[j] = tmp;
                        }
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    throw new Exception("SortCurvesContiguous :" + " non-contiguous input curves");
                }
            }
        }
    }
}
